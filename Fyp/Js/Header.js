document.addEventListener('DOMContentLoaded', function () {
  fetch('header.html') // Load the navbar file
    .then((response) => response.text())
    .then((data) => {
      document.getElementById('header-placeholder').innerHTML = data; // Insert navbar
    })
    .catch((error) => console.error('Error loading header:', error));
});
