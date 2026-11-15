// Run npm build after changes to see the reflects
import { Editor } from "@tiptap/core";
import StarterKit from "@tiptap/starter-kit";
import TextAlign from "@tiptap/extension-text-align";
import Underline from "@tiptap/extension-underline";

function initializeMailEditor(container) {
  const editorElement = container.querySelector(".mail-editor-content");

  const editor = new Editor({
    element: editorElement,
    extensions: [
      StarterKit.configure({ underline: false }),
      Underline,
      TextAlign.configure({
        types: ["heading", "paragraph"],
      }),
    ],
    content: "<p></p>",
    editorProps: {
      attributes: {
        role: "textbox",
        "aria-label": container.dataset.messageLabel,
        "aria-multiline": "true",
        "data-placeholder": container.dataset.placeholder,
      },
    },
  });

  const commandStates = {
    bold: () => editor.isActive("bold"),
    italic: () => editor.isActive("italic"),
    underline: () => editor.isActive("underline"),
    bullet: () => editor.isActive("bulletList"),
    ordered: () => editor.isActive("orderedList"),
    left: () => editor.isActive({ textAlign: "left" }),
    center: () => editor.isActive({ textAlign: "center" }),
    right: () => editor.isActive({ textAlign: "right" }),
  };

  function updateEditorState() {
    container.querySelector(".mail-editor-output").value = editor.getHTML();
    editor.view.dom.classList.toggle("is-empty", editor.isEmpty);
    container.querySelectorAll("[data-editor-command]").forEach((button) => {
      button.setAttribute(
        "aria-pressed",
        String(commandStates[button.dataset.editorCommand]?.() ?? false),
      );
    });
  }

  editor.on("transaction", updateEditorState);
  updateEditorState();

  container.querySelectorAll("[data-editor-command]").forEach((button) => {
    button.addEventListener("click", () => {
      const command = button.dataset.editorCommand;

      switch (command) {
        case "bold":
          editor.chain().focus().toggleBold().run();
          break;

        case "italic":
          editor.chain().focus().toggleItalic().run();
          break;

        case "underline":
          editor.chain().focus().toggleUnderline().run();
          break;

        case "bullet":
          editor.chain().focus().toggleBulletList().run();
          break;

        case "ordered":
          editor.chain().focus().toggleOrderedList().run();
          break;

        case "left":
        case "center":
        case "right":
          editor.chain().focus().setTextAlign(command).run();
          break;
      }
    });
  });

  return editor;
}

document.addEventListener("DOMContentLoaded", () => {
  document.querySelectorAll(".mail-editor").forEach(initializeMailEditor);
});
