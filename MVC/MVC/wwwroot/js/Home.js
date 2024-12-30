document.addEventListener("DOMContentLoaded", () => {
    // Dynamic resizing for progress bars
    const progressBars = document.querySelectorAll(".progress-bar");
    progressBars.forEach((bar) => {
        const progress = bar.style.width;
        if (parseInt(progress) < 50) {
            bar.style.backgroundColor = "#ff9800"; // Warning color for low progress
        }
    });
});
