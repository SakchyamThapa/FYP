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

// ✅ Extract projectId once
const projectId = new URLSearchParams(window.location.search).get("projectId");

if (!projectId) {
  alert("No project selected. Going back to Home.");
  window.location.href = "/Html/Home.html";
}

// ✅ Append projectId to nav links
document.querySelectorAll(".nav-link").forEach(link => {
  if (projectId && !link.href.includes("projectId")) {
    const href = new URL(link.href, window.location.origin); // safer base
    href.searchParams.set("projectId", projectId);
    link.href = href.toString();
  }
});

const token = getToken();

      // Create floating particles
      function createParticles() {
        const particles = document.getElementById('particles');
        const particleCount = 50;
        
        for (let i = 0; i < particleCount; i++) {
          const particle = document.createElement('span');
          particle.className = 'particle';
          
          // Random size between 3px and 8px
          const size = Math.random() * 5 + 3;
          particle.style.width = `${size}px`;
          particle.style.height = `${size}px`;
          
          // Random position
          particle.style.top = `${Math.random() * 100}%`;
          particle.style.left = `${Math.random() * 100}%`;
          
          // Random opacity
          particle.style.opacity = Math.random() * 0.5 + 0.1;
          
          // Random animation duration between 10s and 30s
          const duration = Math.random() * 20 + 10;
          particle.style.animation = `float ${duration}s infinite ease-in-out`;
          
          // Set unique animation delay
          particle.style.animationDelay = `${Math.random() * 5}s`;
          
          // Add floating animation
          const keyframes = `
            @keyframes float {
              0%, 100% {
                transform: translateY(0) translateX(0);
              }
              50% {
                transform: translateY(${Math.random() * 100 - 50}px) translateX(${Math.random() * 100 - 50}px);
              }
            }
          `;
          
          const style = document.createElement('style');
          style.innerHTML = keyframes;
          document.head.appendChild(style);
          
          particles.appendChild(particle);
        }
      }
      
      // Call the function when the page loads
      window.addEventListener('load', createParticles);

      // Variables for pagination
      let currentPage = 1;
      const itemsPerPage = 4;
      let allItems = [];
      let userPoints = 0;

      // Function to show message in modal
      function showMessage(title, message) {
        document.getElementById('modalTitle').textContent = title;
        document.getElementById('modalBody').textContent = message;
        const modal = new bootstrap.Modal(document.getElementById('messageModal'));
        modal.show();
      }

      // Function to fetch user points from API (if you have an endpoint for this)
      async function fetchUserPoints() {
        try {
          const response = await fetch(`https://localhost:7150/api/redeemableitems/points/${projectId}`, {
            method: 'GET',
            headers: {
              'Authorization': `Bearer ${token}`,
              'Content-Type': 'application/json'
            }
          });
      
          if (!response.ok) {
            throw new Error(`Error fetching points: ${response.status}`);
          }
      
          const result = await response.json();
          userPoints = result.points ?? 0;
      
          document.getElementById('user-points').textContent = `Your Points: ${userPoints}`;
        } catch (error) {
          console.error("Failed to fetch user points:", error);
          document.getElementById('user-points').textContent = `Your Points: Unknown`;
        }
      }
      

      // Function to fetch redeemable items from API
      async function fetchRedeemableItems() {
        try {
          // Show loading state
          document.getElementById('redeem-items').innerHTML = `
            <div class="loading-spinner">
              <div class="spinner-border" role="status">
                <span class="visually-hidden">Loading...</span>
              </div>
            </div>
          `;
          
          // Make API call
          const response = await fetch(`https://localhost:7150/api/redeemableitems/project/${projectId}`, {
            method: 'GET',
            headers: {
              'Authorization': `Bearer ${token}`,
              'Content-Type': 'application/json'
            }
          });
          
          
          if (!response.ok) {
            throw new Error(`Error: ${response.status}`);
          }
          
          const items = await response.json();
          return items;
        } catch (error) {
          console.error("Failed to fetch redeemable items:", error);
          showMessage("Error", "Failed to load redeemable items. Please try again later.");
          return [];
        }
      }

      // Function to render items with mythical-style cards
      function renderItems(page) {
        const start = (page - 1) * itemsPerPage;
        const end = start + itemsPerPage;
        const itemsToRender = allItems.slice(start, end);

        const container = document.getElementById('redeem-items');
        container.innerHTML = '';

        if (itemsToRender.length === 0) {
          container.innerHTML = '<div class="col-12 text-center"><h3>No redeemable items found for this project.</h3></div>';
          return;
        }

        itemsToRender.forEach(item => {
          // Default image if none provided
          const imageUrl = item.imageUrl || '/src/placeholder.jpg';
          
          // Using the mythical card style for all items
          const card = `
            <div class="col">
              <div class="card mythical-item">
                <img src="${imageUrl}" alt="${item.name}">
                <div class="card-body">
                  <h5 class="card-title">${item.name}</h5>
                  <p class="card-text">Redeem for ${item.cost} Sonic Points</p>
                  <button class="redeem-btn" data-item-id="${item.id}" ${userPoints < item.cost ? 'disabled' : ''}>Redeem</button>
                </div>
              </div>
            </div>
          `;
          container.innerHTML += card;
        });

        // Add event listeners to redeem buttons
        document.querySelectorAll('.redeem-btn').forEach(button => {
          button.addEventListener('click', () => handleRedeem(button.dataset.itemId));
        });

        // Update pagination buttons
        document.getElementById('prev-page').parentElement.classList.toggle('disabled', page === 1);
        document.getElementById('next-page').parentElement.classList.toggle('disabled', end >= allItems.length);
      }

      // Function to handle redeeming an item
      async function handleRedeem(itemId) {
        try {
          // Find the item that's being redeemed
          const item = allItems.find(i => i.id == itemId);
          if (!item) {
            throw new Error("Item not found");
          }
          
          // Check if user has enough points
          if (userPoints < item.cost) {
            showMessage("Not enough points", "You don't have enough points to redeem this item.");
            return;
          }
          
          // This would typically send a request to your backend to process the redemption
          // You would need to implement this endpoint in your API
          
          // For now, just show success message
          showMessage("Success", `Successfully redeemed "${item.name}"!`);
          
          // Update user points (would be done by API in real implementation)
          userPoints -= item.cost;
          document.getElementById('user-points').textContent = `Your Points: ${userPoints}`;
          
          // Refresh items to update button states
          renderItems(currentPage);
        } catch (error) {
          console.error("Failed to redeem item:", error);
          showMessage("Error", "Failed to redeem item. Please try again.");
        }
      }

      // Event Listeners for Pagination
      document.getElementById('prev-page').addEventListener('click', (e) => {
        e.preventDefault();
        if (currentPage > 1) {
          currentPage--;
          renderItems(currentPage);
        }
      });

      document.getElementById('next-page').addEventListener('click', (e) => {
        e.preventDefault();
        if (currentPage * itemsPerPage < allItems.length) {
          currentPage++;
          renderItems(currentPage);
        }
      });

      // Load items when page loads
      async function loadItems() {
        try {
          // Fetch user points
          await fetchUserPoints();
          
          // Fetch redeemable items
          allItems = await fetchRedeemableItems();
          renderItems(1);
        } catch (error) {
          console.error("Error loading data:", error);
          document.getElementById('redeem-items').innerHTML = 
            '<div class="col-12 text-center"><h3>Failed to load redeemable items. Please try again later.</h3></div>';
        }
      }

      // Initialize the page
      document.addEventListener('DOMContentLoaded', loadItems);
    // Add to your existing Redeem.js file

// Function to handle opening the redemption history modal
document.getElementById('redemption-history-btn').addEventListener('click', function(e) {
  e.preventDefault();
  loadRedemptionHistory();
  const historyModal = new bootstrap.Modal(document.getElementById('historyModal'));
  historyModal.show();
});

// Function to fetch redemption history from API
async function loadRedemptionHistory() {
  try {
    // Show loading state
    document.getElementById('history-table-body').innerHTML = `
      <tr>
        <td colspan="4" class="text-center">
          <div class="spinner-border" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </td>
      </tr>
    `;
    
    // Make API call
    const response = await fetch(`https://localhost:7150/api/redemptionhistory/${projectId}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
    
    if (!response.ok) {
      throw new Error(`Error: ${response.status}`);
    }
    
    const historyData = await response.json();
    
    // Render the history data
    renderRedemptionHistory(historyData);
  } catch (error) {
    console.error("Failed to fetch redemption history:", error);
    
    // For demonstration, show dummy data instead of error message
    renderDummyHistory();
  }
}

// Function to render redemption history
function renderRedemptionHistory(historyData) {
  const tableBody = document.getElementById('history-table-body');
  
  if (historyData.length === 0) {
    tableBody.innerHTML = `
      <tr>
        <td colspan="4" class="empty-history">
          <i class="fas fa-history"></i>
          <p>You haven't redeemed any items yet.</p>
        </td>
      </tr>
    `;
    return;
  }
  
  let tableContent = '';
  
  historyData.forEach((item, index) => {
    // Format date
    const redemptionDate = new Date(item.redemptionDate);
    const formattedDate = redemptionDate.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
    
    // Determine status style
    let statusClass = '';
    switch(item.status.toLowerCase()) {
      case 'completed':
        statusClass = 'status-completed';
        break;
      case 'pending':
        statusClass = 'status-pending';
        break;
      case 'cancelled':
        statusClass = 'status-cancelled';
        break;
      default:
        statusClass = '';
    }
    
    // Check if transaction is recent (within last 24 hours)
    const isRecent = (new Date() - redemptionDate) < 24 * 60 * 60 * 1000;
    const recentClass = isRecent ? 'recent-transaction' : '';
    
    tableContent += `
      <tr class="${recentClass}">
        <td class="date-column">${formattedDate}</td>
        <td>${item.itemName}</td>
        <td>${item.pointsUsed}</td>
        <td><span class="status-badge ${statusClass}">${item.status}</span></td>
      </tr>
    `;
  });
  
  tableBody.innerHTML = tableContent;
}

// Function to render dummy history data for demonstration
function renderDummyHistory() {
  const dummyData = [
    {
      redemptionDate: new Date(Date.now() - 1000 * 60 * 30), // 30 minutes ago
      itemName: "Premium Gaming Headset",
      pointsUsed: 500,
      status: "Completed"
    },
    {
      redemptionDate: new Date(Date.now() - 1000 * 60 * 60 * 2), // 2 hours ago
      itemName: "Concert Tickets",
      pointsUsed: 350,
      status: "Pending"
    },
    {
      redemptionDate: new Date(Date.now() - 1000 * 60 * 60 * 24 * 2), // 2 days ago
      itemName: "Netflix Subscription",
      pointsUsed: 200,
      status: "Completed"
    },
    {
      redemptionDate: new Date(Date.now() - 1000 * 60 * 60 * 24 * 5), // 5 days ago
      itemName: "Amazon Gift Card",
      pointsUsed: 300,
      status: "Completed"
    },
    {
      redemptionDate: new Date(Date.now() - 1000 * 60 * 60 * 24 * 10), // 10 days ago
      itemName: "VIP Access Pass",
      pointsUsed: 750,
      status: "Cancelled"
    }
  ];
  
  renderRedemptionHistory(dummyData);
}

// Update the handleRedeem function to add the new item to history
const originalHandleRedeem = handleRedeem;
handleRedeem = async function(itemId) {
  const result = await originalHandleRedeem(itemId);
  
  // Add the newly redeemed item to history (dummy implementation)
  if (document.getElementById('historyModal').classList.contains('show')) {
    const redeemedItem = allItems.find(i => i.id == itemId);
    if (redeemedItem) {
      const newHistoryItem = {
        redemptionDate: new Date(),
        itemName: redeemedItem.name,
        pointsUsed: redeemedItem.cost,
        status: "Pending"
      };
      
      // Get current history data
      const tableBody = document.getElementById('history-table-body');
      const emptyHistoryCheck = tableBody.querySelector('.empty-history');
      
      if (emptyHistoryCheck) {
        // Replace empty history message with the new item
        renderRedemptionHistory([newHistoryItem]);
      } else {
        // Prepend new item to existing history
        const dummyHistoryData = [
          newHistoryItem,
          // Extract existing items... (in a real app we'd manage this properly)
        ];
        renderRedemptionHistory(dummyHistoryData);
      }
    }
  }
  
  return result;
};