// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', function () {

    // Selector for all sidebar buttons that are used for navigation
    const navButtons = document.querySelectorAll('.sidebar-wrapper button[data-nav-url]');

    // Get the current page path, typically /Controller/Action
    // We remove the domain part (e.g., https://mysite.com) and keep only the path.
    let currentPath = window.location.pathname.toLowerCase();

    // If the path is just '/', set it to the default home page path (e.g., /Home/Index)
    // NOTE: You may need to adjust '/Home/Index' based on your default route configuration.
    if (currentPath === '/') {
        currentPath = '/home/index';
    }

    navButtons.forEach(button => {
        const navUrl = button.getAttribute('data-nav-url').toLowerCase();

        // --- 1. Navigation Handler (Click Functionality) ---

        button.addEventListener('click', function () {
            // Navigate the user to the URL stored in the data attribute
            window.location.href = navUrl;
        });

        // --- 2. Active Class Management (Highlighting) ---

        // Check if the button's navigation URL matches the current page URL
        if (currentPath === navUrl) {
            // Add the 'active' class to the current button
            button.classList.add('active');

            // You might also need to add the background color class if it's tied to active state
            // Example:
            // button.classList.add('bg-color-E9EFFB'); 
        } else {
            // Ensure any stale 'active' class is removed
            button.classList.remove('active');
            // button.classList.remove('bg-color-E9EFFB'); 
        }
    });
});