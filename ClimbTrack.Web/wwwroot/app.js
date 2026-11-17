/* Show/hide password. Event delegation works on static SSR pages (no Blazor interactivity needed). */
document.addEventListener('click', function (event) {
    var button = event.target.closest('[data-toggle-password]');
    if (!button) return;

    var input = document.getElementById(button.getAttribute('data-toggle-password'));
    if (!input) return;

    var show = input.type === 'password';
    input.type = show ? 'text' : 'password';
    button.textContent = show ? 'Hide' : 'Show';
    button.setAttribute('aria-pressed', String(show));
    button.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
});