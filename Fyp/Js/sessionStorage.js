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
  
  try {
    // Basic token structure validation
    const parts = token.split('.');
    if (parts.length !== 3) {
      console.error("Invalid JWT token format: Expected 3 parts");
      sessionStorage.removeItem("jwt_token"); // Remove invalid token
      return false;
    }
    
    // Check token payload
    try {
      const payload = JSON.parse(atob(parts[1]));
      console.log("Token payload successfully decoded");
      
      // Check token expiration
      const expiry = payload.exp;
      if (expiry) {
        const currentTime = Math.floor(Date.now() / 1000);
        if (currentTime >= expiry) {
          console.error(`JWT token expired at ${new Date(expiry * 1000).toLocaleString()}`);
          sessionStorage.removeItem("jwt_token"); // Remove expired token
          return false;
        }
        
        // Log time until expiration for debugging
        const timeRemaining = expiry - currentTime;
        console.log(`Token valid for ${Math.floor(timeRemaining / 60)} minutes and ${timeRemaining % 60} seconds`);
      } else {
        console.warn("Token has no expiration (exp) claim");
      }
      
      // Check for required claims (customize based on your JWT structure)
      if (!payload.sub) {
        console.warn("Token missing subject (sub) claim");
      }
      
      return true;
    } catch (decodeError) {
      console.error("Failed to decode JWT payload:", decodeError);
      return false;
    }
  } catch (err) {
    console.error("Error validating JWT token:", err);
    return false;
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