// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
import {toast, mountToaster} from "@yusufceng1z/ding";


mountToaster();

toast.configure({
  // position: 'top-center', // top-left, top-center, top-right, bottom-left, bottom-center, bottom-right
  theme: 'light',            // system, light, dark
  sound: true,
  volume: 0.5,
  held: 'queue',
    autoDismiss: true
  })
// once, at startup, (Animation)
toast.configure({
    springs: {
        enter: { stiffness: 520, damping: 32 },
        morph: { stiffness: 560, damping: 34 }, 
    },
});
document.addEventListener("fireNotification", ({detail}) => {
    const {message, type, title, detailsUrl} = detail;
    toast.show(type, title, {
        duration: 5000,
        description: message,
        action: detailsUrl ? {
            label: "Open",
            onClick: () => window.location.assign(detailsUrl)
        } : undefined
    });
});

