// Send Email using AJAX
document.getElementById('composeForm').addEventListener('submit', async function (e) {
    e.preventDefault();  // Prevent form submission

    const to = document.getElementById('to').value;
    const subject = document.getElementById('subject').value;
    const body = document.getElementById('body').value;

    const response = await fetch('/SendMail/SendEmail', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ To: to, Subject: subject, Body: body })
    });

    const result = await response.text();

    // Display success/error message and close modal
    alert(result); // or update your message in a designated area
    $('#composeModal').modal('hide');  // Close modal using jQuery
    location.reload();  // Reload page to reflect sent email
});
