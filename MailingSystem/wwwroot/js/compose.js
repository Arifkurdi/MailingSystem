document.addEventListener("DOMContentLoaded", () => {
  const compose = document.getElementById("composeWindow");
  const closeButton = document.getElementById("closeCompose");
  const minimizeButton = document.getElementById("minimizeCompose");
  const maximizeButton = document.getElementById("maximizeCompose");
  const title = document.getElementById("composeTitle");

  if (!compose) return;

  function setMinimized(minimized) {
    compose.classList.toggle("minimized", minimized);
    maximizeButton?.classList.toggle("d-none", !minimized);
    minimizeButton?.classList.toggle("d-none", minimized);
  }

  closeButton?.addEventListener("click", () => {
    compose.hidden = true;
  });

  document.addEventListener("click", (event) => {
    const trigger = event.target.closest("#openCompose, [data-compose-open]");
    if (trigger) {
      event.preventDefault();
      const replyToId = trigger.dataset.replyToId || "";
      const replyToInput = compose.querySelector('[name="ReplyToId"]');
      

      if (replyToInput) replyToInput.value = replyToId;
      if (title) {
        title.textContent = replyToId ? title.dataset.replyLabel : title.dataset.newMessageLabel;
      }
      setMinimized(false);
      compose.hidden = false;
      compose.querySelector('[name="Title"]')?.focus();
      return;
    }

    if (!compose.hidden && !compose.contains(event.target)) {
      compose.hidden = true;
    }
  });

  minimizeButton?.addEventListener("click", () => {
    setMinimized(true);
  });

  maximizeButton?.addEventListener("click", () => {
    setMinimized(false);
  });
});
