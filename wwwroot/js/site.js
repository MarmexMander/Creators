// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
 // Define styles based on message type using Tailwind CSS classes
 const popupStyles = {
    info: "bg-blue-500 text-white",
    warning: "bg-yellow-500 text-white",
    error: "bg-red-500 text-white",
};

function showPopup(message, type = "info") {
    const container = document.getElementById("popupContainer");

    // Create popup element
    const popup = document.createElement("div");
    popup.className = `p-4 rounded shadow-lg ${popupStyles[type]} flex items-center justify-between`;
    popup.innerHTML = `
        <span>${message}</span>
        <button onclick="this.parentElement.remove()" class="ml-4 text-white">&times;</button>
    `;

    // Append popup and remove it after 5 seconds
    container.appendChild(popup);
    setTimeout(() => popup.remove(), 5000);
}