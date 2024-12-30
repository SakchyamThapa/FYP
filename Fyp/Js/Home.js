document.addEventListener("DOMContentLoaded", () => {
    // Adjust progress bar colors based on progress percentage
    const progressBars = document.querySelectorAll(".progress-bar");

    progressBars.forEach((bar) => {
        const progress = parseInt(bar.dataset.progress, 10);
        if (progress < 50) {
            bar.style.backgroundColor = "#ff9800"; // Warning color for low progress
        } else if (progress >= 90) {
            bar.style.backgroundColor = "#4caf50"; // Success color for high progress
        }
    });

    // Check if there are no projects and display the message
    const projectCards = document.querySelectorAll(".project-card");
    const noProjectsMessage = document.querySelector(".no-projects");

    if (projectCards.length === 0) {
        noProjectsMessage.style.display = "block";
    }
});

// Placeholder for handling "View Details" button click
function viewProject(projectId) {
    alert(`Viewing details for Project ID: ${projectId}`);
    // Add navigation or modal logic here
}
