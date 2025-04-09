document.addEventListener('DOMContentLoaded', function() {
  // DOM Elements
  const taskTitleInput = document.getElementById('task-title');
  const taskPrioritySelect = document.getElementById('task-priority');
  const taskDescriptionInput = document.getElementById('task-description');
  const taskPointsInput = document.getElementById('task-points');
  const addTaskBtn = document.getElementById('add-task-btn');
  const taskContainers = document.querySelectorAll('.task-container');
  
  const workManagementLink = document.getElementById('work-management-link');
  
  
  
  // State
  let tasks = JSON.parse(localStorage.getItem('tasks')) || {
    'backlog': [],
    'in-progress': [],
    'review': [],
    'completed': []
  };
  
  // Initialize
  renderAllTasks();
  updateColumnCounts();
  
  // Event Listeners
  addTaskBtn.addEventListener('click', createNewTask);
  
  // Setup drag and drop
  setupDragAndDrop();
  
  // Functions
  function createNewTask() {
    const title = taskTitleInput.value.trim();
    const priority = taskPrioritySelect.value;
    const description = taskDescriptionInput.value.trim();
    const points = parseInt(taskPointsInput.value) || 0;
    
    if (!title) {
      alert('Please enter a task title');
      return;
    }
    
    const newTask = {
      id: Date.now().toString(),
      title: title,
      priority: priority,
      description: description,
      points: points,
      createdAt: new Date().toISOString()
    };
    
    // Add to backlog by default
    tasks.backlog.push(newTask);
    saveTasksToLocalStorage();
    
    // Render the new task
    renderTask(newTask, document.getElementById('backlog'));
    updateColumnCounts();
    
    // Reset form
    taskTitleInput.value = '';
    taskDescriptionInput.value = '';
    taskPointsInput.value = '10'; // Reset to default value
    taskTitleInput.focus();
  }
  
  function renderTask(task, container) {
    const taskElement = document.createElement('div');
    taskElement.className = 'task glass-card';
    taskElement.setAttribute('draggable', 'true');
    taskElement.setAttribute('data-id', task.id);
    
    const descriptionHtml = task.description ? 
      `<div class="task-description">${task.description}</div>` : '';
    
    // Include points in the task display
    const pointsHtml = `<div class="task-points"><i class="fas fa-star"></i> ${task.points || 0} points</div>`;
    
    taskElement.innerHTML = `
      <div class="task-header">
        <h4 class="task-title">${task.title}</h4>
        <div class="task-actions">
          <button class="btn-edit" title="Edit Task"><i class="fas fa-edit"></i></button>
          <button class="btn-delete" title="Delete Task"><i class="fas fa-trash"></i></button>
        </div>
      </div>
      <div class="task-info">
        <div class="task-priority priority-${task.priority}">
          ${task.priority.charAt(0).toUpperCase() + task.priority.slice(1)} Priority
        </div>
        ${pointsHtml}
      </div>
      ${descriptionHtml}
    `;
    
    // Add event listeners
    taskElement.querySelector('.btn-edit').addEventListener('click', () => editTask(task, taskElement));
    taskElement.querySelector('.btn-delete').addEventListener('click', () => deleteTask(task, taskElement));
    
    // Setup drag events
    taskElement.addEventListener('dragstart', handleDragStart);
    taskElement.addEventListener('dragend', handleDragEnd);
    
    // Add to container
    container.appendChild(taskElement);
  }
  
  function renderAllTasks() {
    // Clear all containers
    taskContainers.forEach(container => {
      container.innerHTML = '';
    });
    
    // Render tasks in each column
    for (const [status, taskList] of Object.entries(tasks)) {
      const container = document.getElementById(status);
      taskList.forEach(task => renderTask(task, container));
    }
  }
  
  function editTask(task, taskElement) {
    // Find which column the task is in
    const columnId = taskElement.closest('.task-container').id;
    const taskIndex = tasks[columnId].findIndex(t => t.id === task.id);
    
    if (taskIndex === -1) return;
    
    // Populate form with task data
    taskTitleInput.value = task.title;
    taskPrioritySelect.value = task.priority;
    taskDescriptionInput.value = task.description || '';
    taskPointsInput.value = task.points || 0;
    
    // Change add button to update
    addTaskBtn.innerHTML = '<i class="fas fa-save"></i> Update Task';
    addTaskBtn.classList.add('btn-update');
    
    // Remove old event listener and add new one
    addTaskBtn.removeEventListener('click', createNewTask);
    
    const updateTaskHandler = function() {
      // Update task data
      task.title = taskTitleInput.value.trim();
      task.priority = taskPrioritySelect.value;
      task.description = taskDescriptionInput.value.trim();
      task.points = parseInt(taskPointsInput.value) || 0;
      
      // Update in tasks array
      tasks[columnId][taskIndex] = task;
      saveTasksToLocalStorage();
      
      // Re-render the task
      taskElement.remove();
      renderTask(task, document.getElementById(columnId));
      
      // Reset form
      taskTitleInput.value = '';
      taskDescriptionInput.value = '';
      taskPointsInput.value = '10'; // Reset to default
      
      // Restore add button
      addTaskBtn.innerHTML = '<i class="fas fa-plus-circle"></i> Add Task';
      addTaskBtn.classList.remove('btn-update');
      addTaskBtn.removeEventListener('click', updateTaskHandler);
      addTaskBtn.addEventListener('click', createNewTask);
    };
    
    addTaskBtn.addEventListener('click', updateTaskHandler);
  }
  
  function deleteTask(task, taskElement) {
    if (!confirm('Are you sure you want to delete this task?')) return;
    
    // Find which column the task is in
    const columnId = taskElement.closest('.task-container').id;
    
    // Remove from tasks array
    tasks[columnId] = tasks[columnId].filter(t => t.id !== task.id);
    saveTasksToLocalStorage();
    
    // Remove from DOM
    taskElement.remove();
    updateColumnCounts();
  }
  
  function saveTasksToLocalStorage() {
    localStorage.setItem('tasks', JSON.stringify(tasks));
  }
  
  function updateColumnCounts() {
    for (const status in tasks) {
      const count = tasks[status].length;
      document.getElementById(`${status}-count`).textContent = count;
    }
  }
  
  // Drag and Drop Functions
  function setupDragAndDrop() {
    // Add event listeners to task containers
    taskContainers.forEach(container => {
      container.addEventListener('dragover', handleDragOver);
      container.addEventListener('dragenter', handleDragEnter);
      container.addEventListener('dragleave', handleDragLeave);
      container.addEventListener('drop', handleDrop);
    });
  }
  
  let draggedTask = null;
  
  function handleDragStart(e) {
    draggedTask = this;
    this.classList.add('dragging');
    e.dataTransfer.effectAllowed = 'move';
    e.dataTransfer.setData('text/plain', this.getAttribute('data-id'));
    
    // Add a slight delay to make the drag visual more noticeable
    setTimeout(() => {
      this.style.opacity = '0.4';
    }, 0);
  }
  
  function handleDragEnd(e) {
    this.classList.remove('dragging');
    this.style.opacity = '1';
    document.querySelectorAll('.task-container').forEach(container => {
      container.classList.remove('drag-over');
    });
  }
  
  function handleDragOver(e) {
    e.preventDefault();
    e.dataTransfer.dropEffect = 'move';
    return false;
  }
  
  function handleDragEnter(e) {
    this.classList.add('drag-over');
  }
  
  function handleDragLeave(e) {
    this.classList.remove('drag-over');
  }
  
  function handleDrop(e) {
    e.preventDefault();
    this.classList.remove('drag-over');
    
    const taskId = e.dataTransfer.getData('text/plain');
    const targetColumnId = this.id;
    
    // Find the task in the original column
    let sourceColumnId = null;
    let taskToMove = null;
    let taskIndex = -1;
    
    for (const [columnId, taskList] of Object.entries(tasks)) {
      taskIndex = taskList.findIndex(task => task.id === taskId);
      if (taskIndex !== -1) {
        sourceColumnId = columnId;
        taskToMove = taskList[taskIndex];
        break;
      }
    }
    
    if (!taskToMove || sourceColumnId === targetColumnId) return;
    
    // Remove from source column
    tasks[sourceColumnId].splice(taskIndex, 1);
    
    // Add to target column
    tasks[targetColumnId].push(taskToMove);
    
    // Save and re-render
    saveTasksToLocalStorage();
    renderAllTasks();
    updateColumnCounts();
    
    return false;
  }
  
  // Add some sample tasks if none exist
  function addSampleTasks() {
    if (Object.values(tasks).every(column => column.length === 0)) {
      tasks = {
        'backlog': [
          {
            id: '1',
            title: 'Research market trends',
            priority: 'medium',
            description: 'Analyze competitor products and identify target audience preferences to guide our product development.',
            points: 15,
            createdAt: new Date().toISOString()
          },
          {
            id: '2',
            title: 'Create wireframes',
            priority: 'high',
            description: 'Design wireframes for homepage, user dashboard, and ensure mobile responsive layout is implemented.',
            points: 25,
            createdAt: new Date().toISOString()
          }
        ],
        'in-progress': [
          {
            id: '3',
            title: 'Develop landing page',
            priority: 'high',
            description: 'Create header section, features section, testimonials, and contact form for the new landing page.',
            points: 30,
            createdAt: new Date().toISOString()
          }
        ],
        'review': [
          {
            id: '4',
            title: 'Review code changes',
            priority: 'medium',
            description: 'Check for bugs, verify functionality, and test on different browsers to ensure cross-browser compatibility.',
            points: 20,
            createdAt: new Date().toISOString()
          }
        ],
        'completed': [
          {
            id: '5',
            title: 'Setup project repository',
            priority: 'low',
            description: 'Initialize Git repository, create README with project information, and setup development branches.',
            points: 10,
            createdAt: new Date().toISOString()
          }
        ]
      };
      
      saveTasksToLocalStorage();
      renderAllTasks();
      updateColumnCounts();
    }
  }
  
  // Add sample tasks if none exist
  addSampleTasks();
});