using System.Diagnostics;
using MailingSystem.Controllers;
using MailingSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MailingSystem.Tests.Controllers;

public class ErrorActionTests
{
    [Theory]
    [InlineData("Home", false)]
    [InlineData("Details", false)]
    [InlineData("Home", true)]
    [InlineData("Details", true)]
    public void Error_ReturnsTraceIdentifierOrCurrentActivityId(string controllerName, bool hasActivity)
    {
        // Isolate Activity.Current from the test runner's tracing context.
        var previous = Activity.Current;
        Activity.Current = null;
        try
        {
            using var activity = hasActivity ? new Activity("error-test").Start() : null;
            var context = new DefaultHttpContext { TraceIdentifier = "request-123" };
            Controller controller = controllerName == "Home"
                ? new HomeController(new StubMailService())
                : new DetailsController(new StubMailService());
            controller.ControllerContext = new ControllerContext { HttpContext = context };

            var result = controller is HomeController home
                ? home.Error()
                : ((DetailsController)controller).Error();

            var model = Assert.IsType<ErrorViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(activity?.Id ?? "request-123", model.RequestId);
            Assert.True(model.ShowRequestId);
        }
        finally
        {
            Activity.Current = previous;
        }
    }
}
