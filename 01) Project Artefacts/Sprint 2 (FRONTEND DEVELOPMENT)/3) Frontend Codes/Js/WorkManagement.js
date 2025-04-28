import {
  getToken,
  storeToken,
  isAuthenticated,
  clearToken,
  getUserRole,
  hasRole,
  getTokenClaim
} from './sessionStorage.js';

// ✅ Redirect to login if not authenticated
if (!isAuthenticated()) {
  clearToken();
  window.location.href = "/index.html";
}

const token = getToken(); // 

const urlParams = new URLSearchParams(window.location.search);
const projectId = urlParams.get("projectId");

if (!projectId) {
  alert("No project selected. Going back to Home.");
  window.location.href = "/Html/Home.html";
}



const API_BASE_URL = "https://localhost:7150/api";

// ✅ DOM Elements
const taskTitleInput = document.getElementById("task-title");
const taskPrioritySelect = document.getElementById("task-priority");
const taskDescriptionInput = document.getElementById("task-description");
const taskPointsInput = document.getElementById("task-points");
const taskDueDateInput = document.getElementById("task-due-date");
const addTaskBtn = document.getElementById("add-task-btn");
const taskContainers = document.querySelectorAll(".task-container");

let tasks = {
  'backlog': [],
  'in-progress': [],
  'review': [],
  'completed': []
};

document.addEventListener("DOMContentLoaded", async () => {
  setDefaultDueDate();
  addTaskBtn.addEventListener("click", createNewTask);
  setupDragAndDrop();
  await loadProjectTasks();
  const currentProjectId = new URLSearchParams(window.location.search).get("projectId");

document.querySelectorAll(".nav-link").forEach(link => {
  if (currentProjectId && !link.href.includes("projectId")) {
    const href = new URL(link.href);
    href.searchParams.set("projectId", currentProjectId);
    link.href = href.toString();
  }
});

});

// ✅ Set default due date to tomorrow
function setDefaultDueDate() {
  const tomorrow = new Date();
  tomorrow.setDate(tomorrow.getDate() + 1);
  taskDueDateInput.valueAsDate = tomorrow;
}

// ✅ Load tasks for current project
async function loadProjectTasks() {
  try {
    const res = await fetch(`${API_BASE_URL}/tasks/project/${projectId}`, {
      headers: { Authorization: `Bearer ${token}` }
    });

    if (!res.ok) throw new Error("Failed to fetch tasks");

    const data = await res.json();
    tasks = { 'backlog': [], 'in-progress': [], 'review': [], 'completed': [] };

    data.forEach(t => {
      const statusKey = t.status.toLowerCase().replace(/([a-z])([A-Z])/g, '$1-$2');
      if (!tasks[statusKey]) tasks[statusKey] = [];
      tasks[statusKey].push(t);
    });

    renderAllTasks();
    updateColumnCounts();
  } catch (err) {
    console.error("Failed to load tasks:", err);
    alert("Error loading tasks.");
  }
}


// ✅ Create new task
async function createNewTask() {
  const title = taskTitleInput.value.trim();
  const description = taskDescriptionInput.value.trim();
  const priorityMap = { "Low": 0, "Medium": 1, "High": 2 };
  const priority = priorityMap[taskPrioritySelect.value];
  const points = parseInt(taskPointsInput.value) || 0;
  const dueDateRaw = taskDueDateInput.value;

  if (!title) return alert("Task title is required.");
  if (!dueDateRaw) return alert("Please select a due date.");

  const dueDate = new Date(dueDateRaw).toISOString();
  const payload = {
    title,
    description,
    priority,
    projectId: parseInt(projectId),
    rewardPoints: points,
    dueDate
  };

  try {
    const res = await fetch(`${API_BASE_URL}/tasks`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json"
      },
      body: JSON.stringify(payload)
    });

    if (!res.ok) {
      const errorText = await res.text();
      throw new Error("Failed to create task: " + errorText);
    }

    const created = await res.json();
    tasks.backlog.push(created);
    renderTask(created, document.getElementById("backlog"));
    updateColumnCounts();
    resetForm();
  } catch (err) {
    console.error("Error creating task:", err);
    alert("Failed to create task.");
  }
}




// ✅ Reset task form
function resetForm() {
  taskTitleInput.value = "";
  taskDescriptionInput.value = "";
  taskPointsInput.value = "10";
  setDefaultDueDate();
  taskTitleInput.focus();
}

// ✅ Render all tasks
function renderAllTasks() {
  taskContainers.forEach(c => c.innerHTML = "");
  for (const [status, list] of Object.entries(tasks)) {
    const container = document.getElementById(status);
    list.forEach(t => renderTask(t, container));
  }
}

// ✅ Render individual task
function renderTask(task, container) {
  const el = document.createElement("div");
  el.className = "task glass-card";
  el.setAttribute("draggable", "true");
  el.setAttribute("data-id", task.id);

  const points = `<div class="task-points"><i class="fas fa-star"></i> ${task.rewardPoints} points</div>`;
  const due = new Date(task.dueDate).toLocaleDateString();
  const dueHtml = `<div class="task-due-date"><i class="fas fa-calendar-alt"></i> ${due}</div>`;

  el.innerHTML = `
    <div class="task-header">
      <h4 class="task-title">${task.title}</h4>
      <div class="task-actions">
        <button class="btn-edit"><i class="fas fa-edit"></i></button>
        <button class="btn-delete"><i class="fas fa-trash"></i></button>
      </div>
    </div>
    <div class="task-info">
      <div class="task-priority priority-${task.priority.toLowerCase()}">${task.priority} Priority</div>
      ${points}
    </div>
    ${dueHtml}
    <div class="task-description">${task.description || ""}</div>
  `;

  el.querySelector(".btn-delete").addEventListener("click", () => deleteTask(task.id, el));
  el.addEventListener("dragstart", handleDragStart);
  el.addEventListener("dragend", handleDragEnd);

  container.appendChild(el);
}

// ✅ Delete task
async function deleteTask(id, el) {
  if (!confirm("Delete this task?")) return;
  try {
    await fetch(`${API_BASE_URL}/tasks/${id}`, {
      method: "DELETE",
      headers: { Authorization: `Bearer ${token}` }
    });
    el.remove();
    await loadProjectTasks();
  } catch (err) {
    console.error("Error deleting task:", err);
  }
}

// ✅ Update column counters
function updateColumnCounts() {
  for (const status in tasks) {
    document.getElementById(`${status}-count`).textContent = tasks[status].length;
  }
}

// ✅ Drag and Drop Setup
function setupDragAndDrop() {
  taskContainers.forEach(c => {
    c.addEventListener("dragover", e => e.preventDefault());
    c.addEventListener("drop", handleDrop);
  });
}

let draggedTask = null;

function handleDragStart(e) {
  draggedTask = this;
  this.classList.add("dragging");
  e.dataTransfer.setData("text/plain", this.getAttribute("data-id"));
}

function handleDragEnd() {
  this.classList.remove("dragging");
}

// ✅ Handle drop and update status
async function handleDrop(e) {
  e.preventDefault();
  this.classList.remove("drag-over");

  const taskId = e.dataTransfer.getData("text/plain");
  const targetColumnId = this.id;

  const statusMap = {
    'backlog': 0,
    'in-progress': 1,
    'review': 2,
    'completed': 3
  };

  try {
    await fetch(`${API_BASE_URL}/tasks/reorder`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify([{ taskId: parseInt(taskId), newStatus: statusMap[targetColumnId] }])
    });
    await loadProjectTasks();
  } catch (err) {
    console.error("Error updating task status:", err);
  }
}
