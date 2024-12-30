document.addEventListener("DOMContentLoaded", () => {
    const cards = document.querySelectorAll(".card");
    const columns = document.querySelectorAll(".column");
    const addTaskButton = document.getElementById("add-task-btn");
    const taskInput = document.getElementById("new-task-input");

    let draggedCard = null;

    // Drag-and-Drop Logic
    cards.forEach((card) => {
        card.addEventListener("dragstart", () => {
            draggedCard = card;
            card.style.opacity = "0.5";
        });

        card.addEventListener("dragend", () => {
            draggedCard = null;
            card.style.opacity = "1";
        });
    });

    columns.forEach((column) => {
        column.addEventListener("dragover", (e) => {
            e.preventDefault();
            column.classList.add("drag-over");
        });

        column.addEventListener("dragleave", () => {
            column.classList.remove("drag-over");
        });

        column.addEventListener("drop", () => {
            column.classList.remove("drag-over");
            if (draggedCard) {
                column.appendChild(draggedCard);
            }
        });
    });

    // Add Task
    addTaskButton.addEventListener("click", () => {
        const taskText = taskInput.value.trim();
        if (taskText) {
            const newCard = document.createElement("div");
            newCard.className = "card";
            newCard.draggable = true;
            newCard.textContent = taskText;

            // Add delete button
            const deleteBtn = document.createElement("button");
            deleteBtn.innerHTML = "<i class='fas fa-times'></i>";
            deleteBtn.className = "delete-btn";
            deleteBtn.onclick = () => newCard.remove();

            newCard.appendChild(deleteBtn);

            document.querySelector('[data-status="backlog"]').appendChild(newCard);
            taskInput.value = "";

            // Add drag events to the new card
            newCard.addEventListener("dragstart", () => {
                draggedCard = newCard;
                newCard.style.opacity = "0.5";
            });

            newCard.addEventListener("dragend", () => {
                draggedCard = null;
                newCard.style.opacity = "1";
            });
        }
    });
});
