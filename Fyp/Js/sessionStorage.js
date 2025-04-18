// sessionStorage.js - Enhanced token handling and validation

/**
 * Get JWT token from session storage with improved error handling
 * @returns {string|null} The JWT token or null if not found
 */
export function getToken() {
  const token = sessionStorage.getItem("jwt_token");
  if (!token) {
    console.error("No JWT token found in session storage");
    return null;
  }

  // Log first 10 chars of token for debugging
  console.log(`Token retrieved: ${token.substring(0, 10)}...`);
  return token;
}

/**
 * Check if user is authenticated with enhanced token validation
 * @returns {boolean} True if authenticated with valid token
 */
export function isAuthenticated() {
  const token = getToken();
  if (!token) {
    console.warn("Authentication check failed: No token found");
    return false;
  }

  if (isTokenExpired(token)) {
    clearToken(); // Clear expired token
    return false; // Token is expired
  }

  return true; // Token is valid
}

/**
 * Check if the token is expired
 * @param {string} token The JWT token
 * @returns {boolean} True if token is expired
 */
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

/**
 * Store JWT token in session storage
 * @param {string} token The JWT token to store
 * @returns {boolean} True if token was stored successfully
 */
export function storeToken(token) {
  if (!token) {
    console.error("Cannot store empty token");
    return false;
  }

  try {
    sessionStorage.setItem("jwt_token", token);
    const storedToken = sessionStorage.getItem("jwt_token");

    if (storedToken === token) {
      console.log("Token successfully stored in session storage");
      return true;
    } else {
      console.error("Token verification failed after storage");
      return false;
    }
  } catch (err) {
    console.error("Failed to store token:", err);
    return false;
  }
}

/**
 * Clear authentication token from session storage
 */
export function clearToken() {
  sessionStorage.removeItem("jwt_token");
  console.log("Authentication token cleared");
}

/**
 * Get user role from the JWT token
 * @returns {string|null} The user's role or null if not found
 */
export function getUserRole() {
  const token = getToken();
  if (!token) return null;

  try {
    const payload = token.split('.')[1];
    const decodedPayload = atob(payload);
    const userData = JSON.parse(decodedPayload);

    // Look for standard or custom role claim names
    const roles =
      userData.role ||
      userData.roles ||
      userData["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

    if (!roles) return null;

    // Always return as array
    return Array.isArray(roles) ? roles : [roles];
  } catch (error) {
    console.error("Error decoding token:", error);
    return null;
  }
}

/**
 * Check if user has required role
 * @param {string|string[]} requiredRoles Role(s) to check against
 * @returns {boolean} True if user has any of the required roles
 */
export function hasRole(requiredRoles) {
  const userRole = getUserRole();
  if (!userRole) return false;

  // Ensure requiredRoles is always an array
  if (!Array.isArray(requiredRoles)) {
    requiredRoles = [requiredRoles];
  }

  // Check if the user has any of the required roles
  return requiredRoles.some(role => userRole.includes(role) || userRole === role);
}

/**
 * Get information from token payload
 * @param {string} claimName Name of the claim to retrieve
 * @returns {any} The claim value or null if not found
 */
export function getTokenClaim(claimName) {
  const token = getToken();
  if (!token) return null;

  try {
    const parts = token.split('.');
    if (parts.length !== 3) return null;

    const payload = JSON.parse(atob(parts[1]));
    return payload[claimName] || null;
  } catch (err) {
    console.error(`Error retrieving claim '${claimName}':`, err);
    return null;
  }
}
