// Sample leaderboard data
const leaderboardData = [
    { rank: 1, name: "SonicFan99", points: 1500 },
    { rank: 2, name: "Speedster", points: 1400 },
    { rank: 3, name: "RingCollector", points: 1350 },
    { rank: 4, name: "BlazeRunner", points: 1300 },
    { rank: 5, name: "TailsBuddy", points: 1250 },
    { rank: 6, name: "KnucklesPunch", points: 1200 },
    { rank: 7, name: "ShadowX", points: 1150 },
];

// Display leaderboard data dynamically
function populateLeaderboard() {
    const leaderboardTable = document.getElementById("leaderboard-data");
    leaderboardTable.innerHTML = ""; // Clear any existing rows

    leaderboardData.forEach((player) => {
        const row = `
            <tr>
                <td>${player.rank}</td>
                <td>${player.name}</td>
                <td>${player.points}</td>
            </tr>
        `;
        leaderboardTable.innerHTML += row;
    });
}

// Update user's Sonic Points (example logic)
function updateUserPoints(points) {
    const userPointsElement = document.getElementById("user-points");
    userPointsElement.textContent = `Your Points: ${points}`;
}

// Initialize leaderboard
document.addEventListener("DOMContentLoaded", () => {
    populateLeaderboard();
    updateUserPoints(1200); // Example: Set the user's Sonic Points
});
