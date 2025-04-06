// Home.js - Main front-end code for home page interaction

import { isAuthenticated, getToken } from './sessionStorage.js';

window.toggleProjectForm = toggleProjectForm;
window.createProject = createProject;
window.viewProject = viewProject;
window.logout = logout;

document.addEventListener("DOMContentLoaded", function () {
  if (!isAuthenticated()) {
    window.location.href = "/login";
    return;
  }

  setUsername();
  fetchProjects();
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

  try {
    const token = getToken();
    const response = await fetch("https://localhost:7146/api/projects", {
      method: "GET",
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json"
      },
    });

    if (!response.ok) {
      const errText = await response.text();
      console.error("Error:", errText);
      throw new Error("Failed to fetch projects");
    }

    const projects = await response.json();
    console.log("Projects:", projects);

    if (projects.length === 0) {
      document.querySelector(".no-projects").style.display = "block";
    } else {
      document.querySelector(".no-projects").style.display = "none";
      projects.forEach(addProjectToUI);
    }
  } catch (err) {
    console.error("Fetch error:", err);
    showMessage("Error fetching projects. Please try again.", "error");
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
  const dashboard = document.querySelector(".dashboard");
  const cards = dashboard.querySelectorAll(".project-card");
  cards.forEach((card) => card.remove());
}

// Render single project card
function addProjectToUI(project) {
  const dashboard = document.querySelector(".dashboard");
  const noProjects = document.querySelector(".no-projects");
  noProjects.style.display = "none";

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

  const insertAfter = document.querySelector(".create-project-btn").parentNode;
  dashboard.insertBefore(card, insertAfter.nextSibling);
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
