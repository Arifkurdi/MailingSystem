// Documentation on https://tom-select.js.org/docs/
function initializeRecipientAutocomplete(element) {
  if (element.tomselect) return element.tomselect;

  return new TomSelect(element, {
    placeholder: element.dataset.placeholder,
    valueField: "id",
    labelField: "name",
    searchField: ["name", "email"],
    plugins: {
      remove_button: { title: element.dataset.removeTitle },
    },
    maxItems: null,
    closeAfterSelect: true,
    openOnFocus: false,
    hideSelected: true,
    selectOnTab: false,
    hidePlaceholder: true,

    // Disable text highlighting
    highlight: false,
    onInitialize() {
      this.control_input.setAttribute(
        "aria-label",
        element.getAttribute("aria-label"),
      );
    },
    load(query, callback) {
      if (!query.trim()) {
        return callback();
      }

      fetch(
        `${element.dataset.recipientSearchUrl}?query=${encodeURIComponent(query.trim())}`,
      )
        .then((response) => {
          if (!response.ok) throw new Error("Search failed");
          return response.json();
        })
        .then((data) => callback(data))
        .catch(() => callback());
    },

    render: {
      no_results(data, escape) {
        return `<div class="no-results">${escape(element.dataset.noResults)}</div>`;
      },
      loading(data, escape) {
        return `
        <div class="d-flex justify-content-center">
          <div class="spinner-border spinner-border-sm p-1" role="status">
            <span class="sr-only"></span>
          </div>
        </div>
        `;
      },
      option(item, escape) {
        const initial = (item.name || item.email).charAt(0).toUpperCase();

        return `
            <div class="recipient-option">
                <div class="recipient-avatar">
                    ${escape(initial)}
                </div>
                <div class="recipient-info">
                    <span class="recipient-name">
                        ${escape(item.name)}
                    </span>
                    <span class="recipient-email">
                        ${escape(item.email)}
                    </span>
                </div>
            </div>
        `;
      },
      item(item, escape) {
        return `
            <div class="recipient-chip">
                ${escape(item.name)}
            </div>
        `;
      },
    },
  });
}

document.addEventListener("DOMContentLoaded", () => {
  document
    .querySelectorAll(".compose-form .recipient-select")
    .forEach(initializeRecipientAutocomplete);
});
