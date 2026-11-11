// Documentation on https://tom-select.js.org/docs/
function initializeRecipientAutocomplete(selector) {
  return new TomSelect(selector, {
    valueField: "id",
    labelField: "name",
    searchField: ["name", "email"],
    plugins: ["remove_button"],
    maxItems: null,
    clearOnSelect: true,
    closeAfterSelect: true,
    openOnFocus: false,
    hideSelected: true,
    selectOnTab: false,
    hidePlaceholder: true,

    // Disable text highlighting
    highlight: false,
    load(query, callback) {
      if (!query.trim()) {
        return callback();
      }

      fetch(`/api/recipients/search?query=${encodeURIComponent(query)}`)
        .then((response) => {
          if (!response.ok) throw new Error("Search failed");
          return response.json();
        })
        .then((data) => callback(data))
        .catch(() => callback());
    },

    render: {
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
  initializeRecipientAutocomplete("#composeToInput");
  initializeRecipientAutocomplete("#composeCCInput");
});
Console.log("*".repeat(12));
