import { isAuthenticated, getToken, clearToken } from './sessionStorage.js';

console.log("Token at script start:", getToken());

// Bind functions to window for use in HTML
window.toggleProjectForm = toggleProjectForm;
window.createProject = createProject;
window.viewProject = viewProject;
window.logout = logout;
window.refreshProjects = fetchProjects;

// ✅ Wait for token before loading protected content
function waitForTokenThenInit(retries = 5) {
  const token = getToken();

  if (!token) {
    console.warn("🔁 Token not available yet. Retrying...");
    if (retries > 0) {
      setTimeout(() => waitForTokenThenInit(retries - 1), 150);
    } else {
      console.error("❌ Token never became available.");
      showMessage("Session expired. Please log in again.", "error");
      clearToken();
      window.location.href = "/index.html";
    }
    return;
  }

  console.log("✅ Token available:", token.substring(0, 10));
  setUsername();
  fetchProjects();
}

function isTokenExpired(token) {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload.exp < Date.now() / 1000;
  } catch {
    return true;
  }
}

document.addEventListener("DOMContentLoaded", () => {
  if (!isAuthenticated()) {
    window.location.href = "/index.html";
    return;
  }
  waitForTokenThenInit();
});

window.addEventListener('popstate', function () {
  if (window.location.pathname === '/' || window.location.pathname === '/home') {
    fetchProjects();
  }
});

// ✅ Decode and set username
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

// ✅ Logout and clear token
function logout() {
  clearToken();
  window.location.href = "/index.html";
}

// ✅ Fetch projects
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

// ✅ Create new project
async function createProject(event) {
  event.preventDefault();

  if (!isAuthenticated()) {
    window.location.href = "/index.html";
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
        "Authorization": `Bearer ${token}`,
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

// ✅ Toggle project form visibility
function toggleProjectForm() {
  const form = document.getElementById("create-project-form");
  form.style.display = form.style.display === "block" ? "none" : "block";
}

// ✅ Clear existing project cards
function clearProjects() {
  const projectList = document.getElementById("project-list");
  projectList.innerHTML = '';
}

// ✅ Render project in UI
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

// ✅ Navigate to project page
function viewProject(projectId) {
  window.location.href = `/project/${projectId}`;
}

// ✅ Show alert box
function showMessage(message, type) {
  const box = document.getElementById("messageBox");
  box.textContent = message;
  box.className = "message-box " + (type === "error" ? "error-message" : "success-message");
  box.style.display = "block";

  setTimeout(() => {
    box.style.display = "none";
  }, 3000);
  
}
