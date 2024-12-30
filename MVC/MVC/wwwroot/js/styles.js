$(document).ready(function () {
    // Handle form submission for sending email
    $('#composeForm').on('submit', function (e) {
        e.preventDefault(); // Prevent default form submission

        var formData = new FormData(this); // Create FormData object to send form data (including attachments)

        // Show loading spinner and disable button
        $('#loading').show();
        $('button[type="submit"]').prop('disabled', true);

        // Send the AJAX request
        $.ajax({
            url: '/SendMail/SendEmail', // URL for the controller action
            type: 'POST',
            data: formData,
            contentType: false, // Tells jQuery not to set contentType for FormData
            processData: false, // Tells jQuery not to process data (FormData handles it)
            success: function (response) {
                // Handle the response from the server
                if (response.success) {
                    $('#responseMessage').text('Email sent successfully!').css('color', 'green').show();
                } else {
                    $('#responseMessage').text('Failed to send email: ' + response.message).css('color', 'red').show();
                }

                // Hide loading spinner and enable submit button
                $('#loading').hide();
                $('button[type="submit"]').prop('disabled', false);
            },
            error: function (xhr, status, error) {
                // Handle any error during the request
                $('#responseMessage').text('Error sending email. Please try again later.').css('color', 'red').show();
                console.error('Error details:', error, status, xhr); // Log error for debugging

                // Hide loading spinner and enable submit button
                $('#loading').hide();
                $('button[type="submit"]').prop('disabled', false);
            }
        });
    });

    //move to trash
    function moveToTrash(emailId) {
        if (confirm("Are you sure you want to move this email to Trash?")) { // Added confirmation
            $.post('@Url.Action("MoveToTrash", "SendMail")', { id: emailId }, function (response) { // Corrected endpoint URL
                if (response.success) {
                    alert('Email moved to Trash.');
                    location.reload();
                } else {
                    alert('Failed to move email to Trash.');
                }
            });
        }
    }

    }


    // Handle email deletion from Sent or Received
    $('.delete-email').on('click', function () {
        var emailId = $(this).data('id');
        var isReceived = $(this).data('type') === 'received';
        var url = isReceived ? '/SendMail/DeleteReceivedEmail' : '/SendMail/DeleteEmail';

        $.ajax({
            url: url,
            type: 'POST',
            data: { id: emailId },
            success: function (response) {
                if (response.success) {
                    alert(response.message);
                    location.reload(); // Refresh the page
                } else {
                    alert(response.message);
                }
            },
            error: function () {
                alert('An error occurred.');
            }
        });
    });
});
document.addEventListener("DOMContentLoaded", () => {
    const toRegister = document.getElementById("toRegister");
    const toLogin = document.getElementById("toLogin");
    const loginPage = document.getElementById("loginPage");
    const registerPage = document.getElementById("registerPage");

    toRegister.addEventListener("click", (e) => {
        e.preventDefault();
        loginPage.classList.remove("active");
        registerPage.classList.add("active");
    });

    toLogin.addEventListener("click", (e) => {
        e.preventDefault();
        registerPage.classList.remove("active");
        loginPage.classList.add("active");
    });
});
