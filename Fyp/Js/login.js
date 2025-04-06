document.addEventListener('DOMContentLoaded', function () {
  // Get DOM elements
  const loginPage = document.getElementById('loginPage');
  const registerPage = document.getElementById('registerPage');
  const toRegister = document.getElementById('toRegister');
  const toLogin = document.getElementById('toLogin');
  const messageBox = document.getElementById('messageBox');
  
  // Switch to register form
  toRegister.addEventListener('click', function (e) {
    e.preventDefault();
    loginPage.classList.remove('active');
    registerPage.classList.add('active');
  });
  
  // Switch to login form
  toLogin.addEventListener('click', function (e) {
    e.preventDefault();
    registerPage.classList.remove('active');
    loginPage.classList.add('active');
  });
  
  // Function to display messages
  function showMessage(message, type) {
    messageBox.innerHTML = `<div class="${type}">${message}</div>`;
    messageBox.style.display = "block";
    
    // Auto-hide message after 5 seconds
    setTimeout(() => {
      messageBox.style.display = "none";
    }, 5000);
  }
  
  // Handle Login Form submission
  const loginFormElement = document.getElementById('loginForm');
  if (loginFormElement) {
    loginFormElement.addEventListener('submit', async function (event) {
      event.preventDefault();
      
      const loginEmail = document.getElementById('loginEmail');
      const loginPassword = document.getElementById('loginPassword');
      
      if (!loginEmail || !loginPassword) {
        showMessage("Login form elements not found", "error");
        return;
      }
      
      const email = loginEmail.value.trim();
      const password = loginPassword.value;
      
      if (!email || !password) {
        showMessage("Please enter both email and password", "error");
        return;
      }
  
      try {
        const response = await fetch("https://localhost:7146/api/auth/login", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ email, password }),
          credentials: "include" // Include cookies in the request
        });
  
        // Handle non-JSON responses
        const contentType = response.headers.get("content-type");
        if (!contentType || !contentType.includes("application/json")) {
          showMessage("Received invalid response from server", "error");
          return;
        }
  
        const data = await response.json();
  
        if (response.ok && data.success) {
          sessionStorage.setItem("jwt_token", data.token); // Store JWT in sessionStorage 
          showMessage(data.message || "Login successful!", "success");
          
          // Redirect after a short delay to allow the user to see the success message
          setTimeout(() => {
            window.location.href = "/Html/Home.html"; // You can use a JavaScript router here instead if it's an SPA
          }, 1000);
        } else {
          showMessage(data.message || "Login failed. Please check your credentials.", "error");
        }
      } catch (error) {
        console.error("Login error:", error);
        showMessage("An error occurred. Please check your connection and try again.", "error");
      }
    });
  }
  
  // Handle Register Form submission
  const registerFormElement = document.getElementById('registerForm');
  if (registerFormElement) {
    registerFormElement.addEventListener('submit', async function (event) {
      event.preventDefault();
      
      const fullNameInput = document.getElementById('fullName');
      const emailInput = document.getElementById('email');
      const passwordInput = document.getElementById('password');
      const confirmPasswordInput = document.getElementById('confirmPassword');
      
      if (!fullNameInput || !emailInput || !passwordInput || !confirmPasswordInput) {
        showMessage("Registration form elements not found", "error");
        return;
      }
      
      const username = fullNameInput.value.trim(); // Using fullName as username
      const email = emailInput.value.trim();
      const password = passwordInput.value;
      const confirmPassword = confirmPasswordInput.value;
  
      // Validation
      if (!username || !email || !password || !confirmPassword) {
        showMessage("Please fill in all fields!", "error");
        return;
      }
      
      // Email validation
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(email)) {
        showMessage("Please enter a valid email address", "error");
        return;
      }
      
      if (password !== confirmPassword) {
        showMessage("Passwords do not match!", "error");
        return;
      }
      
      // Password validation - at least 8 characters, one uppercase, one lowercase, one number, one special character
      const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;
      if (!passwordRegex.test(password)) {
        showMessage("Password must be at least 8 characters long and include uppercase, lowercase, number, and special character", "error");
        return;
      }
  
      try {
        // Create the exact JSON structure you provided
        const requestBody = {
          username: username, // Using fullName as username
          email: email,
          password: password
        };
  
        const response = await fetch("https://localhost:7146/api/auth/register", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(requestBody),
          credentials: "include" // Include cookies in the request
        });
  
        // Handle non-JSON responses
        const contentType = response.headers.get("content-type");
        if (!contentType || !contentType.includes("application/json")) {
          showMessage("Received invalid response from server", "error");
          return;
        }
  
        const data = await response.json();
  
        if (response.ok && data.success) {
          showMessage(data.message || "Registration successful!", "success");
          
          // Clear form fields
          fullNameInput.value = "";
          emailInput.value = "";
          passwordInput.value = "";
          confirmPasswordInput.value = "";
          
          // Switch to login form after success
          setTimeout(() => {
            loginPage.classList.add("active");
            registerPage.classList.remove("active");
          }, 1500);
        } else {
          showMessage(data.message || "Registration failed. Please try again.", "error");
        }
      } catch (error) {
        console.error("Registration error:", error);
        showMessage("An error occurred. Please check your connection and try again.", "error");
      }
    });
  }
});
