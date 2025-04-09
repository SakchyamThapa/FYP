import { isAuthenticated, getToken, clearToken } from './sessionStorage.js';

window.toggleProjectForm = toggleProjectForm;
window.createProject = createProject;
window.viewProject = viewProject;
window.logout = logout;
window.refreshProjects = fetchProjects;

document.addEventListener("DOMContentLoaded", function () {
  // Check if user is authenticated
  if (!isAuthenticated()) {
    window.location.href = "/index.html"; // Redirect to login page if not authenticated
    return;
  }

  setUsername();
  fetchProjects();
});

window.addEventListener('popstate', function () {
  // Refresh the projects if we navigate back to the home page
  if (window.location.pathname === '/' || window.location.pathname === '/home') {
    fetchProjects();
  }
});

// Set the logged-in user's username
function setUsername() {
  try {
    const token = getToken();
    const payload = token.split('.')[1];
    const decoded = JSON.parse(atob(payload));
    const username = decoded.name || decoded.username || decoded.email || "User"; // Fallback to email if no name
    document.getElementById("username").textContent = username;
  } catch (err) {
    console.error("Failed to decode JWT:", err);
  }
}

// Log the user out by clearing the token and redirecting to login
function logout() {
  clearToken();
  window.location.href = "/index.html";
}

// Fetch the projects and display them on the home page
async function fetchProjects() {
  clearProjects();

  const loadingIndicator = document.createElement("div");
  loadingIndicator.id = "projects-loading";
  loadingIndicator.className = "loading-indicator";
  loadingIndicator.innerHTML = "Loading projects...";
  document.querySelector(".dashboard").appendChild(loadingIndicator);

  try {
    const token = getToken();
    const response = await fetch("https://localhost:7146/api/projects", {
      method: "GET",
      headers: {
        "Authorization": `Bearer ${token}`,
        "Content-Type": "application/json"
      },
    });

    document.getElementById("projects-loading")?.remove();

    if (response.status === 401) {
      console.error("Unauthorized: Token rejected by server");
      showMessage("Your session has expired. Please log in again.", "error");
      clearToken();
      setTimeout(() => {
        window.location.href = "/index.html";
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

    if (projects.length === 0) {
      document.querySelector(".no-projects").style.display = "block";
    } else {
      document.querySelector(".no-projects").style.display = "none";
      projects.forEach(addProjectToUI);
    }
  } catch (err) {
    console.error("Fetch error:", err);
    showMessage("Error fetching projects. Please try again later.", "error");
    document.getElementById("projects-loading")?.remove();
  }
}

// Create a new project
async function createProject(event) {
  event.preventDefault();

  if (!isAuthenticated()) {
    window.location.href = "/index.html"; // Redirect if not authenticated
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

// Toggle the display of the project creation form
function toggleProjectForm() {
  const form = document.getElementById("create-project-form");
  form.style.display = form.style.display === "block" ? "none" : "block";
}

// Clear the project list on the home page
function clearProjects() {
  const projectList = document.getElementById("project-list");
  projectList.innerHTML = '';
}

// Add a project to the UI
function addProjectToUI(project) {
  const projectList = document.getElementById("project-list");
  document.querySelector(".no-projects").style.display = "none";

  const deadline = new Date(project.dueDate).toLocaleDateString('en-US');
  const progress = project.progress ?? 0;

  const card = document.createElement("div");
  card.className = "project-card";
  card.innerHTML = `
    <h2>${project.name}</h2>
    <p><strong>Deadline:</strong> ${deadline}</p>
    <div class="progress-bar-container">
      <div class="progress-bar" style="width: ${progress}%" data-progress="${progress}"></div>
    </div>
    <p>${progress}% Completed</p>
    <button class="view-details" onclick="viewProject(${project.id})">View Details</button>
  `;

  projectList.appendChild(card);
}

// View a specific project by ID
function viewProject(projectId) {
  window.location.href = `/project/${projectId}`;
}

// Show messages to the user (error or success)
function showMessage(message, type) {
  const box = document.getElementById("messageBox");
  box.textContent = message;
  box.className = "message-box " + (type === "error" ? "error-message" : "success-message");
  box.style.display = "block";

  setTimeout(() => {
    box.style.display = "none";
  }, 3000);
}
