// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


// Keep Action1 for testing
document
    .getElementById("showNewMessageNotification")
    ?.addEventListener("click", ({currentTarget}) => {
        const {detailsUrl, notificationTitle, notificationMessage} = currentTarget.dataset;
        const event = new CustomEvent("fireNotification", {
            detail: {
                title: notificationTitle,
                message: notificationMessage,
                type: "success",
                detailsUrl
            }
        });

        document.dispatchEvent(event);
    });
