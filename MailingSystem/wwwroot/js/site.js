// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", () => {
  const toastElement = document.getElementById("newMessageToast");

  if (!toastElement) return;

  const toast = bootstrap.Toast.getOrCreateInstance(toastElement, {
    delay: 3000,
    autohide: true,
  });

  // Show notification when the page loads
  toast.show();

  // Keep Action1 for testing
  document
    .getElementById("showNewMessageNotification")
    ?.addEventListener("click", () => toast.show());
});
