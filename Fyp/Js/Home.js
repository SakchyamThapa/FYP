// Home.js - Main front-end code for home page interaction

import { isAuthenticated, getToken } from './sessionStorage.js';


window.toggleProjectForm = toggleProjectForm;
window.createProject = createProject;
window.viewProject = viewProject;
window.logout = logout;
window.refreshProjects = fetchProjects; // Expose fetch function globally

document.addEventListener("DOMContentLoaded", function () {
  if (!isAuthenticated()) {
    window.location.href = "/login";
    return;
  }

  setUsername();
  fetchProjects();
});

// Add history state change listener to detect navigation events
window.addEventListener('popstate', function() {
  if (window.location.pathname === '/' || window.location.pathname === '/home') {
    fetchProjects();
  }
});

// Extract and display username from JWT
function setUsername() {
  try {
    const token = getToken();
    const payload = token.split('.')[1];
    const decoded = JSON.parse(atob(payload));
    const username = decoded.name || decoded.username || decoded.email || "User";
    document.getElementById("username").textContent = username;
  } catch (err) {
    console.error("Failed to decode JWT:", err);
  }
}

// Logout user
function logout() {
  sessionStorage.removeItem("jwt_token");
  window.location.href = "/login";
}

// Fetch projects from the API
async function fetchProjects() {
  clearProjects();
  
  // Show loading indicator
  const loadingIndicator = document.createElement("div");
  loadingIndicator.id = "projects-loading";
  loadingIndicator.className = "loading-indicator";
  loadingIndicator.innerHTML = "Loading projects...";
  document.querySelector(".dashboard").appendChild(loadingIndicator);

  try {
    const token = getToken();
    
    // Log authentication status
    console.log("Authentication status:", isAuthenticated());
    console.log("Token prefix:", token ? token.substring(0, 15) + "..." : "No token");
    
    const response = await fetch("https://localhost:7146/api/projects", {
      method: "GET",
      headers: {
        "Authorization": `Bearer ${token}`,
        "Content-Type": "application/json"
      },
      credentials: "include"
    });

    // Remove loading indicator
    const loadingElem = document.getElementById("projects-loading");
    if (loadingElem) loadingElem.remove();

    console.log("Response status:", response.status);
    
    if (response.status === 401) {
      console.error("Unauthorized: Token rejected by server");
      showMessage("Your session has expired. Please log in again.", "error");
      sessionStorage.removeItem("jwt_token");
      setTimeout(() => {
        window.location.href = "/login";
      }, 2000);
      return;
    }

    if (!response.ok) {
      const errText = await response.text();
      console.error("API Error:", response.status, errText);
      throw new Error(`Failed to fetch projects: ${response.status}`);
    }

    const projects = await response.json();
    console.log("Projects received:", projects);
    console.log("Number of projects:", projects.length);

    if (projects.length === 0) {
      document.querySelector(".no-projects").style.display = "block";
    } else {
      document.querySelector(".no-projects").style.display = "none";
      projects.forEach(addProjectToUI);
    }
  } catch (err) {
    console.error("Fetch error:", err);
    console.error("Error details:", err.message);
    showMessage("Error fetching projects. Please try again later.", "error");
    
    // Remove loading indicator on error too
    const loadingElem = document.getElementById("projects-loading");
    if (loadingElem) loadingElem.remove();
  }
}

// Submit new project
async function createProject(event) {
  event.preventDefault();

  if (!isAuthenticated()) {
    window.location.href = "/login";
    return;
  }

  const name = document.getElementById("project-title").value.trim();
  const description = document.getElementById("project-description").value.trim();
  const dueDate = document.getElementById("project-deadline").value;
  const formattedDueDate = new Date(dueDate).toISOString();

  if (!name || !dueDate) {
    showMessage("Please enter a project title and deadline.", "error");
    return;
  }

  const newProject = { name, description, dueDate: formattedDueDate };

  try {
    const token = getToken();
    const response = await fetch("https://localhost:7146/api/projects", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify(newProject),
    });

    if (!response.ok) {
      const errText = await response.text();
      console.error("API Error:", errText);
      throw new Error("Failed to create project");
    }

    const project = await response.json();
    addProjectToUI(project);
    toggleProjectForm();
    document.getElementById("new-project-form").reset();
    showMessage("Project created successfully!", "success");
  } catch (err) {
    console.error("Create error:", err);
    showMessage("Error creating project. Please try again.", "error");
  }
}

// Show/hide project form
function toggleProjectForm() {
  const form = document.getElementById("create-project-form");
  form.style.display = form.style.display === "block" ? "none" : "block";
}

// Remove all current project cards
function clearProjects() {
  const projectList = document.getElementById("project-list");
  projectList.innerHTML = ''; // Clear only the project list container
}

// Render single project card
function addProjectToUI(project) {
  console.log("Adding project to UI:", project);
  
  // Get project list container
  const projectList = document.getElementById("project-list");
  
  // Hide "no projects" message
  document.querySelector(".no-projects").style.display = "none";

  const deadline = new Date(project.dueDate).toLocaleDateString('en-US');
  const progress = project.progress ?? 0;

  const card = document.createElement("div");
  card.className = "project-card";
  card.innerHTML = `
    <h2>${project.name}</h2>
    <p><strong>Deadline:</strong> ${deadline}</p>
    <div class="progress-bar-container">
      <div class="progress-bar" style="width: ${progress}%;" data-progress="${progress}"></div>
    </div>
    <p>${progress}% Completed</p>
    <button class="view-details" onclick="viewProject(${project.id})">View Details</button>
  `;

  projectList.appendChild(card);
}

// Navigate to individual project page
function viewProject(projectId) {
  window.location.href = `/project/${projectId}`;
}

// Show success or error message
function showMessage(message, type) {
  const box = document.getElementById("messageBox");
  box.textContent = message;
  box.className = "message-box " + (type === "error" ? "error-message" : "success-message");
  box.style.display = "block";

  setTimeout(() => {
    box.style.display = "none";
  }, 3000);
}