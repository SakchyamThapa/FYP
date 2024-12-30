document.addEventListener('DOMContentLoaded', function () {

    // Form Switch Logic
    const loginForm = document.getElementById('loginPage');
    const registerForm = document.getElementById('registerPage');
    const toRegister = document.getElementById('toRegister');
    const toLogin = document.getElementById('toLogin');

    // Show login form by default
    loginForm.classList.add('active');

    // Switch to register form
    toRegister.addEventListener('click', function (e) {
        e.preventDefault();
        loginForm.classList.remove('active');
        registerForm.classList.add('active');
    });

    // Switch to login form
    toLogin.addEventListener('click', function (e) {
        e.preventDefault();
        registerForm.classList.remove('active');
        loginForm.classList.add('active');
    });

    // Handle Login Form submission
    document.getElementById('loginForm').addEventListener('submit', async function (event) {
        event.preventDefault();

        // Collect form data
        const email = document.getElementById('loginEmail').value;
        const password = document.getElementById('loginPassword').value;

        // Prepare data to send to the backend
        const data = { Email: email, Password: password };

        try {
            const response = await fetch('/Login/Login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            const result = await response.json();

            if (result.success) {
                document.getElementById('messageBox').innerHTML = `<div class="success">${result.message}</div>`;
                // Redirect to the home page or any other page after successful login
                window.location.href = result.redirectTo; // Redirect to the Index page
            } else {
                document.getElementById('messageBox').innerHTML = `<div class="error">${result.message}</div>`;
            }
        } catch (error) {
            document.getElementById('messageBox').innerHTML = `<div class="error">An error occurred. Please try again later.</div>`;
        }
    });


    // Handle Register Form submission
    document.getElementById('registerForm').addEventListener('submit', async function (event) {
        event.preventDefault();

        const fullName = document.getElementById('fullName').value;
        const email = document.getElementById('email').value;
        const password = document.getElementById('password').value;
        const confirmPassword = document.getElementById('confirmPassword').value;

        // Check for empty fields
        if (!fullName || !email || !password || !confirmPassword) {
            showMessage('Please fill in all fields!', 'error');
            return;
        }

        // Check if passwords match
        if (password !== confirmPassword) {
            showMessage('Passwords do not match!', 'error');
            return;
        }

        const data = { FullName: fullName, Email: email, Password: password, ConfirmPassword: confirmPassword };

        try {
            const response = await fetch('/Register/Register', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            const result = await response.json();

            if (result.success) {
                showMessage(result.message, 'success');
                // Optionally, redirect to login page
                // loginForm.classList.add('active');
                // registerForm.classList.remove('active');
            } else {
                showMessage(result.message, 'error');
            }
        } catch (error) {
            showMessage('An error occurred. Please try again later.', 'error');
        }
    });

    // Function to display messages
    function showMessage(message, type) {
        const messageBox = document.getElementById('messageBox');
        messageBox.innerHTML = message;
        messageBox.className = type === 'success' ? 'success' : 'error';
        messageBox.style.display = 'block';
    }
});
