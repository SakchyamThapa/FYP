// protect-page.js - Protect pages based on JWT token and role

// Extract and decode JWT token
function getToken() {
  return sessionStorage.getItem("jwt_token");
}

// Check if the user is authenticated (token exists and not expired)
function isAuthenticated() {
  const token = getToken();
  return token && !isTokenExpired(token);
}

// Check if the token is expired
function isTokenExpired(token) {
  try {
    const payload = token.split('.')[1];
    const decoded = JSON.parse(atob(payload));
    const expiration = decoded.exp;  // Expiration timestamp (usually in seconds)

    if (expiration && expiration < Date.now() / 1000) {
      console.error(`JWT token expired at ${new Date(expiration * 1000).toLocaleString()}`);
      return true; // Token has expired
    }
    return false; // Token is still valid
  } catch (error) {
    console.error('Error checking token expiration:', error);
    return true; // If decoding fails, assume expired or invalid token
  }
}


// Get user role from the JWT token
function getUserRole() {
  const token = getToken();
  if (!token) return null;

  try {
    // Get the payload part of the JWT (second part)
    const payload = token.split('.')[1];

    // Decode the base64 string
    const decodedPayload = atob(payload);

    // Parse the JSON
    const userData = JSON.parse(decodedPayload);

    // Return the role (adjust the property name if yours is different)
    return userData.role || userData.roles || userData.http_role;
  } catch (error) {
    console.error("Error decoding token:", error);
    return null;
  }
}

// Check if user has required role
function hasRole(requiredRoles) {
  const userRole = getUserRole();
  if (!userRole) return false;

  // Ensure requiredRoles is always an array
  if (!Array.isArray(requiredRoles)) {
    requiredRoles = [requiredRoles];
  }

  // Check if the user has any of the required roles
  return requiredRoles.some(role => userRole.includes(role) || userRole === role);
}

// Protect page based on roles
function protectPage(requiredRoles) {
  // Check if user is authenticated
  if (!isAuthenticated()) {
    // Redirect to login page if not authenticated
    window.location.href = '/login';
    return;
  }

  // Check if user has required role
  const hasRequiredRole = Array.isArray(requiredRoles)
    ? requiredRoles.some(role => hasRole(role))
    : hasRole(requiredRoles);

  if (!hasRequiredRole) {
    // Redirect to unauthorized page if role doesn't match
    window.location.href = '/unauthorized';
    return;
  }

  // User has access, continue loading the page
  console.log('Access granted');
}

export { protectPage, getUserRole, hasRole, isAuthenticated };
