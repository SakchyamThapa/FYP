// Toggle the create project form
function toggleProjectForm() {
  const form = document.getElementById('create-project-form');
  form.classList.toggle('active');
}

// Handle project creation
function createProject(event) {
  event.preventDefault();

  // Get form values
  const title = document.getElementById('project-title').value;
  const description = document.getElementById('project-description').value;
  const deadline = document.getElementById('project-deadline').value;

  // Create a new project card
  const dashboard = document.querySelector('.dashboard');
  const projectCard = document.createElement('div');
  projectCard.classList.add('project-card');

  projectCard.innerHTML = `
      <h2>${title}</h2>
      <p><strong>Deadline:</strong> ${deadline}</p>
      <div class="progress-bar-container">
        <div class="progress-bar" style="width: 0%" data-progress="0"></div>
      </div>
      <p>0% Completed</p>
      <button class="view-details" onclick="viewProject(${Date.now()})">
        View Details
      </button>
    `;

  // Add the new project card to the dashboard
  const noProjectsMessage = document.querySelector('.no-projects');
  if (noProjectsMessage.style.display !== 'none') {
    noProjectsMessage.style.display = 'none';
  }
  dashboard.insertBefore(projectCard, dashboard.querySelector('.project-card'));

  // Reset and hide the form
  document.getElementById('new-project-form').reset();
  toggleProjectForm();
}

// Placeholder for viewing project details
function viewProject(projectId) {
  // Implement navigation or modal for project details
  console.log(`Viewing project with ID: ${projectId}`);
}

const referenceCard = dashboard.querySelector('.project-card');
if (referenceCard) {
  dashboard.insertBefore(projectCard, referenceCard);
} else {
  dashboard.appendChild(projectCard);
}
