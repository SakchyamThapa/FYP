import { getToken, isAuthenticated, clearToken } from './sessionStorage.js';

document.addEventListener("DOMContentLoaded", () => {
  if (!isAuthenticated()) {
    //window.location.href = "/index.html";
    return;
  }

  const projectId = getProjectId(); // You can replace this with dynamic logic
  fetchLeaderboard(projectId);
  updateUserPoints(0); // You can fetch real points if endpoint exists
  setupLogout();
});

// 🔐 Fetch leaderboard data from backend
async function fetchLeaderboard(projectId, page = 1, size = 20) {
  const token = getToken();
  try {
    const response = await fetch(`https://localhost:7146/api/leaderboard/${projectId}?pageNumber=${page}&pageSize=${size}`, {
      method: "GET",
      headers: {
        "Authorization": `Bearer ${token}`,
        "Content-Type": "application/json"
      }
    });

    if (response.status === 401) {
      console.warn("❌ Unauthorized: Token may be invalid or expired.");
      clearToken();
      //window.location.href = "/index.html";
      return;
    }

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`API Error: ${errorText}`);
    }

    const data = await response.json();
    populateLeaderboard(data);
  } catch (error) {
    console.error("🚨 Failed to fetch leaderboard:", error.message);
    showMessage("Could not load leaderboard. Try again later.", "error");
  }
}

// 🧾 Render the leaderboard table
function populateLeaderboard(data) {
  const leaderboardTable = document.getElementById("leaderboard-data");
  leaderboardTable.innerHTML = "";

  data.forEach((player) => {
    const row = `
      <tr>
        <td>${player.leaderboardRank}</td>
        <td>${player.userName}</td>
        <td>${player.pointsEarned}</td>
      </tr>
    `;
    leaderboardTable.innerHTML += row;
  });
}

// 🟡 Optional: Set your points display (you can fetch real data from another API if needed)
function updateUserPoints(points) {
  const userPointsElement = document.getElementById("user-points");
  userPointsElement.textContent = `Your Points: ${points}`;
}

// 📌 Get project ID dynamically or hardcoded
function getProjectId() {
  return 1; // Replace with logic to read from query param or sessionStorage
}

// 🚪 Logout functionality
function setupLogout() {
  const logoutLink = document.querySelector('.dropdown-item.text-danger');
  if (logoutLink) {
    logoutLink.addEventListener('click', () => {
      clearToken();
      window.location.href = "/index.html";
    });
  }
}

// 💬 Optional alert handler
function showMessage(message, type = "error") {
  alert(message); // Replace with a better UI toast if needed
}
