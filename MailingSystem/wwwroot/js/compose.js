document.addEventListener("DOMContentLoaded", () => {
  const compose = document.getElementById("composeWindow");
  const openButton = document.getElementById("openCompose");
  const closeButton = document.getElementById("closeCompose");
  const minimizeButton = document.getElementById("minimizeCompose");
  const maximizeButton = document.getElementById("maximizeCompose");

  if (!compose || !openButton) return;

  openButton.addEventListener("click", () => {
    compose.hidden = false;
    document.getElementById("composeTitleInput")?.focus();
  });

  closeButton?.addEventListener("click", () => {
    compose.hidden = true;
  });

  document.addEventListener("click", (event) => {
    if (
      !compose.hidden &&
      !compose.contains(event.target) &&
      !openButton.contains(event.target)
    ) {
      compose.hidden = true;
    }
  });

  minimizeButton?.addEventListener("click", () => {
    compose.classList.toggle("minimized");
    maximizeButton.classList.toggle("d-none");
    minimizeButton.classList.toggle("d-none");
  });

  maximizeButton?.addEventListener("click", () => {
    compose.classList.toggle("minimized");
    maximizeButton.classList.toggle("d-none");
    minimizeButton.classList.toggle("d-none");
  });
});
