// Store the JWT token in sessionStorage
function setToken(token) {
  sessionStorage.setItem("jwt_token", token);
}

// Retrieve the JWT token from sessionStorage
function getToken() {
  return sessionStorage.getItem("jwt_token");
}

// Remove the JWT token from sessionStorage (e.g., on logout)
function removeToken() {
  sessionStorage.removeItem("jwt_token");
}

// Check if the user is authenticated (token exists and not expired)
function isAuthenticated() {
  const token = getToken();
  return token && !isTokenExpired(token);  // Check if token exists and is not expired
}

// Check if the token is expired
function isTokenExpired(token) {
  try {
    const payload = token.split('.')[1];  // JWT token is in 3 parts (header, payload, signature)
    const decoded = JSON.parse(atob(payload));  // Decode the payload
    const expiration = decoded.exp;  // Expiration timestamp (in seconds)

    if (!expiration) {
      return false; // No expiration time in token, so it's valid indefinitely
    }

    // Compare the expiration time with the current time (in milliseconds)
    return expiration * 1000 < Date.now();
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
    const payload = token.split('.')[1];  // JWT token is in 3 parts (header, payload, signature)
    const decodedPayload = atob(payload);  // Decode the payload (Base64)
    const userData = JSON.parse(decodedPayload);  // Parse the payload into JSON

    return userData.role || userData.roles || null;  // Return user role(s)
  } catch (error) {
    console.error('Error decoding JWT token:', error);
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

// Refresh token expiration (optional): This can be useful to renew a token without logging out the user
function refreshToken() {
  const token = getToken();
  if (!token) return null;

  // You can add logic here to refresh the token (e.g., call an API endpoint that issues a new token).
  // For example, using an API call to refresh the token:
  // return fetch('/api/refresh-token', { method: 'POST', headers: { Authorization: `Bearer ${token}` } })
  //   .then(res => res.json())
  //   .then(data => setToken(data.token));  // Assume the API returns a new token

  return token; // Placeholder, no actual refreshing logic in this code
}

export { setToken, getToken, removeToken, isAuthenticated, getUserRole, hasRole, refreshToken };
