// Example items
const items = [
    { name: 'Mythical Car', points: 10000, image: '/src/car.png', mythical: true },
    { name: 'Coffee', points: 100, image: '/src/coffee.avif' },
    { name: 'Cake', points: 200, image: '/src/cake.avif' },
    { name: 'Burger', points: 300, image: '/src/burger.jpg' },
    { name: 'Pizza', points: 400, image: '/src/pizza.jpg' },
    { name: 'Ice Cream', points: 150, image: '/src/icecream.jpg' },
    { name: 'Hot Dog', points: 250, image: '/src/hotdog.jpg' },
];

let currentPage = 1;
const itemsPerPage = 4;

// Function to render items
function renderItems(page) {
    const start = (page - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    const itemsToRender = items.slice(start, end);

    const container = document.getElementById('redeem-items');
    container.innerHTML = '';

    itemsToRender.forEach(item => {
        const cardClass = item.mythical ? 'card mythical-item' : 'card';
        const card = `
            <div class="col">
                <div class="${cardClass}">
                    <img src="${item.image}" alt="${item.name}">
                    <div class="card-body">
                        <h5 class="card-title">${item.name}</h5>
                        <p class="card-text">Redeem for ${item.points} Sonic Points</p>
                        <button class="redeem-btn">Redeem</button>
                    </div>
                </div>
            </div>
        `;
        container.innerHTML += card;
    });

    // Update pagination buttons
    document.getElementById('prev-page').parentElement.classList.toggle('disabled', page === 1);
    document.getElementById('next-page').parentElement.classList.toggle('disabled', end >= items.length);
}

// Event Listeners for Pagination
document.getElementById('prev-page').addEventListener('click', () => {
    if (currentPage > 1) {
        currentPage--;
        renderItems(currentPage);
    }
});

document.getElementById('next-page').addEventListener('click', () => {
    if (currentPage * itemsPerPage < items.length) {
        currentPage++;
        renderItems(currentPage);
    }
});

// Initial Render
renderItems(currentPage);
