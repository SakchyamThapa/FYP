// Fetch all users
function getAllUsers() {
    fetch('/Register/GetAll')
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                console.log(data.data);
                alert('Users retrieved successfully. Check the console for details.');
            } else {
                alert('Failed to retrieve users.');
            }
        });
}

// Fetch user by ID
function getUserById() {
    const userId = prompt("Enter the user ID:");
    if (!userId) return;

    fetch(`/Register/GetById/${userId}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                console.log(data.data);
                alert(`User retrieved successfully: ${data.data.fullName}`);
            } else {
                alert('User not found.');
            }
        });
}

// Update user
function updateUser() {
    const userId = prompt("Enter the user ID:");
    const fullName = prompt("Enter the new full name:");
    const email = prompt("Enter the new email:");

    if (!userId || !fullName || !email) {
        alert("All fields are required.");
        return;
    }

    const userData = { id: userId, fullName, email };

    fetch('/Register/Update', {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(userData)
    }).then(response => response.json())
        .then(data => {
            if (data.success) {
                alert('User updated successfully.');
            } else {
                alert('Failed to update user.');
            }
        });
}

// Delete user
function deleteUser() {
    const userId = prompt("Enter the user ID:");
    if (!userId) return;

    fetch(`/Register/Delete/${userId}`, { method: 'DELETE' })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert('User deleted successfully.');
            } else {
                alert('Failed to delete user.');
            }
        });
}
