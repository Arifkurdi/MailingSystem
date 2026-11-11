using MailingSystem.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;

namespace MailingSystem.Tests.Controllers;

public class LanguageControllerTests
{
    private static LanguageController CreateController(DefaultHttpContext context)
    {
        var actionContext = new ActionContext(context, new RouteData(), new ControllerActionDescriptor());
        return new LanguageController
        {
            ControllerContext = new ControllerContext(actionContext),
            Url = new TestUrlHelper(actionContext)
        };
    }

    // Keep local-URL validation real; stub only fallback route generation, which needs MVC routing.
    private sealed class TestUrlHelper(ActionContext context) : UrlHelper(context)
    {
        public override string? Action(UrlActionContext actionContext) => "/Home/Index";
    }

    [Theory]
    [InlineData("en", "http", "")]
    [InlineData("ar", "https", "")]
    [InlineData("en", "https", "/mail")]
    [InlineData("ar", "http", "/mail")]
    public void SetLanguage_SetsCultureCookieAndPreservesLocalReturnUrl(
        string culture, string scheme, string pathBase)
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = scheme;
        context.Request.PathBase = pathBase;
        var controller = CreateController(context);
        var before = DateTimeOffset.UtcNow.AddYears(1);
        const string returnUrl = "/Home/Sent?page=2";

        var result = controller.SetLanguage(culture, returnUrl);

        Assert.Equal(returnUrl, Assert.IsType<LocalRedirectResult>(result).Url);
        var cookie = SetCookieHeaderValue.Parse(Assert.Single(context.Response.Headers.SetCookie)!);
        Assert.Equal(CookieRequestCultureProvider.DefaultCookieName, cookie.Name.ToString());
        var parsedCulture = CookieRequestCultureProvider.ParseCookieValue(
            Uri.UnescapeDataString(cookie.Value.ToString()));
        Assert.NotNull(parsedCulture);
        Assert.Equal(culture, Assert.Single(parsedCulture.Cultures).ToString());
        Assert.Equal(culture, Assert.Single(parsedCulture.UICultures).ToString());
        Assert.Equal(pathBase == "" ? "/" : pathBase, cookie.Path.ToString());
        Assert.True(cookie.HttpOnly);
        Assert.Equal(scheme == "https", cookie.Secure);
        Assert.Equal(Microsoft.Net.Http.Headers.SameSiteMode.Lax, cookie.SameSite);
        Assert.NotNull(cookie.Expires);
        // HTTP dates have second precision.
        Assert.InRange(cookie.Expires.Value, before.AddSeconds(-1), DateTimeOffset.UtcNow.AddYears(1));
    }

    [Theory]
    [InlineData("fr")]
    [InlineData("EN")]
    [InlineData("")]
    public void SetLanguage_UnsupportedCulture_ReturnsBadRequestWithoutSettingCookie(string culture)
    {
        var context = new DefaultHttpContext();
        var controller = CreateController(context);

        var result = controller.SetLanguage(culture, "/");

        Assert.IsType<BadRequestResult>(result);
        Assert.Equal(0, context.Response.Headers.SetCookie.Count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://evil.example/path")]
    [InlineData("//evil.example/path")]
    [InlineData("/\\evil.example/path")]
    public void SetLanguage_UnsafeOrMissingReturnUrl_RedirectsToHome(string? returnUrl)
    {
        var controller = CreateController(new DefaultHttpContext());

        var result = controller.SetLanguage("en", returnUrl);

        Assert.Equal("/Home/Index", Assert.IsType<LocalRedirectResult>(result).Url);
    }
}
