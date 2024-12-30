document.addEventListener("DOMContentLoaded", () => {
    // Reset KPI handler
    window.resetKPI = function (userId) {
        if (confirm("Are you sure you want to reset this user's KPI points?")) {
            fetch(`/Admin/ResetKPI/${userId}`, {
                method: "POST",
                headers: { "Content-Type": "application/json" }
            })
                .then((response) => response.json())
                .then((data) => {
                    if (data.success) {
                        alert("KPI points reset successfully.");
                        location.reload();
                    } else {
                        alert("Failed to reset KPI points.");
                    }
                });
        }
    };
});
