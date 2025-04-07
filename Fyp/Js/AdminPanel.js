
        document.addEventListener("DOMContentLoaded", () => {
            const modal = document.getElementById("confirmModal");
            const confirmActionBtn = document.getElementById("confirmAction");
            const cancelActionBtn = document.getElementById("cancelAction");
            const modalMessage = document.getElementById("modalMessage");
            
            let pendingAction = null;
            let pendingParams = null;
            
            // Close modal on cancel
            cancelActionBtn.addEventListener("click", () => {
                modal.style.display = "none";
                pendingAction = null;
            });
            
            // Execute pending action on confirm
            confirmActionBtn.addEventListener("click", () => {
                if (pendingAction && typeof window[pendingAction] === "function") {
                    window[pendingAction](pendingParams.id, true);
                }
                modal.style.display = "none";
            });
            
            // Close modal if clicked outside
            window.addEventListener("click", (event) => {
                if (event.target === modal) {
                    modal.style.display = "none";
                }
                pendingAction = null;
            });
            
            // Show confirmation dialog
            window.showConfirmation = function(message, action, params) {
                modalMessage.textContent = message;
                pendingAction = action;
                pendingParams = params;
                modal.style.display = "block";
            };

            // Reset KPI Handler
            window.resetKPI = function(userId, confirmed = false) {
                if (!confirmed) {
                    showConfirmation(
                        "Are you sure you want to reset this user's KPI points?", 
                        "resetKPI", 
                        { id: userId }
                    );
                    return;
                }
                
                fetch(`/Admin/ResetKPI/${userId}`, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        alert("KPI points reset successfully.");
                        location.reload();
                    } else {
                        alert("Failed to reset KPI points.");
                    }
                })
                .catch(error => {
                    console.error("Error resetting KPI:", error);
                });
            };
            
            // Remove User Handler
            window.removeUser = function(userId, userName, confirmed = false) {
                if (!confirmed) {
                    showConfirmation(
                        `Are you sure you want to remove ${userName}?`, 
                        "removeUserConfirmed", 
                        { id: userId }
                    );
                    return;
                }
                
                fetch(`/Admin/RemoveUser/${userId}`, {
                    method: "DELETE",
                    headers: { "Content-Type": "application/json" },
                })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        alert("User removed successfully.");
                        location.reload();
                    } else {
                        alert("Failed to remove user.");
                    }
                })
                .catch(error => {
                    console.error("Error removing user:", error);
                });
            };
            
            // Wrapper for confirmed user removal
            window.removeUserConfirmed = function(userId) {
                fetch(`/Admin/RemoveUser/${userId}`, {
                    method: "DELETE",
                    headers: { "Content-Type": "application/json" },
                })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        alert("User removed successfully.");
                        location.reload();
                    } else {
                        alert("Failed to remove user.");
                    }
                })
                .catch(error => {
                    console.error("Error removing user:", error);
                });
            };

            // Add User Handler
            document.getElementById("addUserForm").addEventListener("submit", function(e) {
                e.preventDefault();
                const userInfo = document.getElementById("userName").value;
                
                // Basic validation
                if (!userInfo.includes(',')) {
                    alert("Please use the correct format: Name,Email,Department,Role");
                    return;
                }
                
                fetch("/Admin/AddUser", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ userInfo })
                })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        alert("User added successfully!");
                        location.reload();
                    } else {
                        alert("Failed to add user.");
                    }
                })
                .catch(error => {
                    console.error("Error adding user:", error);
                });
            });

            // Navigation Handler
            document.querySelectorAll('.nav-link').forEach(link => {
    link.addEventListener('click', function() {
        document.querySelectorAll('.nav-link').forEach(l => l.classList.remove('active'));
        this.classList.add('active');
        // Navigation will now proceed normally for anchor tags
    });
});

        });
  