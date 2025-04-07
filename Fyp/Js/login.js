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
    setTimeout(() => {
      registerPage.classList.add('active');
    }, 50);
  });
  
  // Switch to login form
  toLogin.addEventListener('click', function (e) {
    e.preventDefault();
    registerPage.classList.remove('active');
    setTimeout(() => {
      loginPage.classList.add('active');
    }, 50);
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
  
  // Email validation function
  function validateEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }
  
  // Password strength checker
  function checkPasswordStrength(password) {
    const passwordStrength = document.getElementById('passwordStrength');
    const strengthText = document.getElementById('strengthText');
    
    // Calculate strength
    let strength = 0;
    
    // Length check
    if (password.length >= 8) strength += 25;
    
    // Contains lowercase
    if (/[a-z]/.test(password)) strength += 25;
    
    // Contains uppercase
    if (/[A-Z]/.test(password)) strength += 25;
    
    // Contains number
    if (/\d/.test(password)) strength += 12.5;
    
    // Contains special character
    if (/[@$!%*?&]/.test(password)) strength += 12.5;
    
    // Update UI
    passwordStrength.style.width = `${strength}%`;
    
    if (strength < 50) {
      passwordStrength.style.background = '#e74c3c'; // Red
      strengthText.textContent = 'Weak';
      strengthText.style.color = '#e74c3c';
      return false;
    } else if (strength < 75) {
      passwordStrength.style.background = '#f39c12'; // Orange
      strengthText.textContent = 'Moderate';
      strengthText.style.color = '#f39c12';
      return false;
    } else {
      passwordStrength.style.background = '#27ae60'; // Green
      strengthText.textContent = 'Strong';
      strengthText.style.color = '#27ae60';
      return true;
    }
  }
  
  // Reset form errors
  function resetErrors(formName) {
    const errors = document.querySelectorAll(`#${formName} .input-error`);
    const inputs = document.querySelectorAll(`#${formName} input`);
    
    errors.forEach(error => {
      error.style.display = 'none';
      error.textContent = '';
    });
    
    inputs.forEach(input => {
      input.classList.remove('error');
    });
  }
  
  // Show validation error
  function showError(inputId, errorId, message) {
    const input = document.getElementById(inputId);
    const error = document.getElementById(errorId);
    
    input.classList.add('error');
    error.textContent = message;
    error.style.display = 'block';
    
    // Add shake animation
    input.classList.add('shake');
    setTimeout(() => {
      input.classList.remove('shake');
    }, 800);
    
    return false;
  }
  
  // Password field validation
  const passwordInput = document.getElementById('password');
  if (passwordInput) {
    passwordInput.addEventListener('input', function() {
      checkPasswordStrength(this.value);
    });
  }
  
  // Handle Login Form submission
  const loginFormElement = document.getElementById('loginForm');
  if (loginFormElement) {
    loginFormElement.addEventListener('submit', async function (event) {
      event.preventDefault();
      resetErrors('loginForm');
      
      const loginEmail = document.getElementById('loginEmail');
      const loginPassword = document.getElementById('loginPassword');
      
      // Validate email
      if (!loginEmail.value.trim()) {
        return showError('loginEmail', 'loginEmailError', 'Email is required');
      }
      
      if (!validateEmail(loginEmail.value.trim())) {
        return showError('loginEmail', 'loginEmailError', 'Please enter a valid email');
      }
      
      // Validate password
      if (!loginPassword.value) {
        return showError('loginPassword', 'loginPasswordError', 'Password is required');
      }
      
      const email = loginEmail.value.trim();
      const password = loginPassword.value;
      
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
          sessionStorage.setItem("jwt_token", receivedToken); // Store JWT in sessionStorage 
          showMessage(data.message || "Login successful!", "success");
          
          // Redirect after a short delay to allow the user to see the success message
          setTimeout(() => {
            window.location.href = "/Html/Home.html";
          }, 1000);
        } else {
          // Check for specific error messages
          if (data.message && data.message.includes("Invalid Email")) {
            showError('loginEmail', 'loginEmailError', 'Email not found');
          } else if (data.message && data.message.includes("Invalid Password")) {
            showError('loginPassword', 'loginPasswordError', 'Incorrect password');
          } else {
            showMessage(data.message || "Login failed. Please check your credentials.", "error");
          }
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
      resetErrors('registerForm');
      
      const fullNameInput = document.getElementById('fullName');
      const emailInput = document.getElementById('email');
      const passwordInput = document.getElementById('password');
      const confirmPasswordInput = document.getElementById('confirmPassword');
      
      let isValid = true;
      
      // Validate full name
      if (!fullNameInput.value.trim()) {
        isValid = false;
        showError('fullName', 'fullNameError', 'Full name is required');
      } else if (fullNameInput.value.trim().length < 3) {
        isValid = false;
        showError('fullName', 'fullNameError', 'Name must be at least 3 characters');
      }
      
      // Validate email
      if (!emailInput.value.trim()) {
        isValid = false;
        showError('email', 'emailError', 'Email is required');
      } else if (!validateEmail(emailInput.value.trim())) {
        isValid = false;
        showError('email', 'emailError', 'Please enter a valid email');
      }
      
      // Validate password
      const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;
      if (!passwordInput.value) {
        isValid = false;
        showError('password', 'passwordError', 'Password is required');
      } else if (!passwordRegex.test(passwordInput.value)) {
        isValid = false;
        showError('password', 'passwordError', 'Password must have at least 8 characters, including uppercase, lowercase, number, and special character');
      }
      
      // Validate confirm password
      if (!confirmPasswordInput.value) {
        isValid = false;
        showError('confirmPassword', 'confirmPasswordError', 'Please confirm your password');
      } else if (passwordInput.value !== confirmPasswordInput.value) {
        isValid = false;
        showError('confirmPassword', 'confirmPasswordError', 'Passwords do not match');
      }
      
      if (!isValid) return;
      
      const username = fullNameInput.value.trim();
      const email = emailInput.value.trim();
      const password = passwordInput.value;
      
      try {
        // Create the exact JSON structure you provided
        const requestBody = {
          username: username,
          email: email,
          password: password
        };
        
        const response = await fetch("https://localhost:7146/api/auth/register", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(requestBody),
          credentials: "include"
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
          document.getElementById('passwordStrength').style.width = '0%';
          document.getElementById('strengthText').textContent = '';
          
          // Switch to login form after success
          setTimeout(() => {
            registerPage.classList.remove("active");
            setTimeout(() => {
              loginPage.classList.add("active");
            }, 50);
          }, 1500);
        } else {
          if (data.message && data.message.includes("email already exists")) {
            showError('email', 'emailError', 'This email is already registered');
          } else {
            showMessage(data.message || "Registration failed. Please try again.", "error");
          }
        }
      } catch (error) {
        console.error("Registration error:", error);
        showMessage("An error occurred. Please check your connection and try again.", "error");
      }
    });
  }
  
  // Real-time validation for email fields
  const emailFields = document.querySelectorAll('input[type="email"]');
  emailFields.forEach(field => {
    field.addEventListener('blur', function() {
      if (this.value.trim() && !validateEmail(this.value.trim())) {
        const errorId = this.id === 'loginEmail' ? 'loginEmailError' : 'emailError';
        showError(this.id, errorId, 'Please enter a valid email');
      }
    });
  });
  
  // Real-time validation for password confirmation
  const confirmPasswordField = document.getElementById('confirmPassword');
  if (confirmPasswordField) {
    confirmPasswordField.addEventListener('input', function() {
      const passwordField = document.getElementById('password');
      if (this.value && passwordField.value !== this.value) {
        showError('confirmPassword', 'confirmPasswordError', 'Passwords do not match');
      } else {
        document.getElementById('confirmPasswordError').style.display = 'none';
        this.classList.remove('error');
      }
    });
  }
});