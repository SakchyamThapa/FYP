using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using MVC.Interface;



public class SendMailController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SendMailController> _logger;
    public IEmailLogsRepo _EmailLogRepo;

    public SendMailController(ApplicationDbContext context, ILogger<SendMailController> logger, IEmailLogsRepo EmailLogReoo)
    {
        _context = context;
        _logger = logger;
        _EmailLogRepo = EmailLogReoo;
    }

    public IActionResult WorkManagement()
    {
        return View();  
    }
    public IActionResult TrashEmails()
    {
        return View(); 
    }
        public IActionResult Invoice()
    {
        return View(); 
    }
    public IActionResult Recieved()
    {
        // Retrieve received emails from the database
        var receivedEmails = _context.ReceivedEmails.ToList();

        // Pass the emails to the view
        return View(receivedEmails);
    }

    public IActionResult AdminPanel()
    {
        var viewModel = new AdminPanelViewModel
        {
            // Retrieve users and set them to the view model
            users = _context.UserViewModels.ToList()
        };

        return View(viewModel);
    }

    //get redeem
    [HttpGet("redeem")]
    public IActionResult Redeem(int userId)
    {
        _logger.LogInformation($"Redeem action called with userId: {userId}");

        var user = _context.UserViewModels.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            _logger.LogWarning("User not found.");
            return NotFound(); // User not found
        }

        var model = new RedeemModel
        {
            Username = user.FullName,
            Points = user.KPIPoints,
            UserId = user.Id
        };

        _logger.LogInformation($"Redeem model prepared for userId: {userId}");
        return View(model);
    }

    // GET: Index to display all emails (Sent, Received, and Trash)
    public IActionResult Index()
    {
        //var SentEmails = _context.SentEmails.ToList();
        var SentEmails = _EmailLogRepo.GetList().ToList();
        var ReceivedEmails = _context.ReceivedEmails.ToList();
        var TrashEmails = _context.TrashEmails.ToList();

        ViewBag.SentEmails = SentEmails;
        ViewBag.ReceivedEmails = ReceivedEmails;
        ViewBag.TrashEmails = TrashEmails;

        return View();
    }

    public ActionResult Login()
    {
        return View();
    }
   

    // GET: Compose Email
    public IActionResult Compose()
    {
        return View();
    }

    public IActionResult Dashboard()
    {
        // Retrieve the current user's ID from claims
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized(); // Ensure the user is authenticated
        }

        // Retrieve projects associated with the current user
        var projects = _context.Projects
            .Where(p => p.UserId == userId)
            .ToList();

        return View(projects);
    }

    public IActionResult Home()
    {
        // Sample data, replace with your database call
        var projects = new List<Project>
            {
                new Project { Id = 1, Name = "Project Alpha", Deadline = DateTime.Now.AddDays(10), Progress = 75 },
                new Project { Id = 2, Name = "Project Beta", Deadline = DateTime.Now.AddDays(20), Progress = 50 },
                new Project { Id = 3, Name = "Project Gamma", Deadline = DateTime.Now.AddDays(30), Progress = 30 }
            };

        return View(projects);
    }

    [HttpPost]
    public async Task<IActionResult> SendEmail(Email em, IFormFileCollection attachments)
    {
        if (ModelState.IsValid)
        {
            const long maxAttachmentSize = 12 * 1024 * 1024;


            Console.WriteLine("Starting email send process");
            // Check attachment size
            foreach (var file in attachments)
            {
                if (file.Length > maxAttachmentSize)
                {
                    TempData["Message"] = $"Attachment {file.FileName} is too large to send.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Index");
                }
            }

            Console.WriteLine("Start of SendEmail method");
            // Create SentEmail record
            var sentEmail = new SentEmail
            {
                To = em.To,
                From = "oneshotaura@gmail.com",
                Subject = em.Subject,
                Body = em.Body,
                SentDate = DateTime.Now,
                HasAttachment = attachments?.Count > 0
            };
            _EmailLogRepo.Insert(sentEmail);
            await _EmailLogRepo.SaveAsync();

            //_context.SentEmails.Add(sentEmail);
            //await _context.SaveChangesAsync();

            var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            // Create directory if it doesn't exist
            if (!Directory.Exists(uploadDirectory))
            {
                try
                {
                    Directory.CreateDirectory(uploadDirectory);
                }
                catch (Exception ex)
                {
                    TempData["Message"] = $"Error creating directory: {ex.Message}";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Index");
                }
            }

            try
            {
                // Save attachments
                if (attachments != null && attachments.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("About to save attachments...");
                    foreach (var file in attachments)
                    {
                        var filePath = Path.Combine(uploadDirectory, file.FileName);

                        // Ensure unique filename if file exists
                        if (System.IO.File.Exists(filePath))
                        {
                            filePath = Path.Combine(uploadDirectory, Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));
                        }

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        var attachment = new MVC.Models.Attachment
                        {
                            FileName = file.FileName,
                            FilePath = filePath,
                            SentEmailId = sentEmail.Id
                        };

                        _context.Attachments.Add(attachment);
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error saving attachment: {ex.Message}";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            // Send the email
            try
            {
                await SendEmailUsingSMTP(em.To, em.Subject, em.Body, attachments);
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error sending email: {ex.Message}";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            TempData["Message"] = $"The email to {em.To} was sent successfully!";
            TempData["MessageType"] = "success";
        }
        else
        {
            TempData["Message"] = "Failed to send the email. Please check your input.";
            TempData["MessageType"] = "error";
        }

        return RedirectToAction("Index");
    }





    // SMTP Email sending logic
    [HttpPost]
    private async Task SendEmailUsingSMTP(string to, string subject, string body, IFormFileCollection attachments)
    {
        try
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("oneshotaura@gmail.com", "tkfu tqkx gmfv kjvg"), // Use your app password
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("oneshotaura@gmail.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(to);

            // Attach files if they exist
            if (attachments != null && attachments.Count > 0)
            {
                foreach (var file in attachments)
                {
                    // Use a memory stream to read the file data
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin); // Reset stream position

                        var attachment = new System.Net.Mail.Attachment(memoryStream, file.FileName);
                        mailMessage.Attachments.Add(attachment);

                        Console.WriteLine($"Attachment added: {file.FileName}");
                    }
                }
            }

            await smtpClient.SendMailAsync(mailMessage);
            Console.WriteLine("Email sent successfully with attachments!");
        }
        catch (SmtpException smtpEx)
        {
            Console.WriteLine($"SMTP Error: {smtpEx.Message}");
            TempData["Message"] = $"SMTP Error: {smtpEx.Message}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General Error: {ex.Message}");
            TempData["Message"] = $"Error sending email: {ex.Message}";
        }
    }






    // POST: Move Email to Trash
    [HttpPost]
    public JsonResult DeleteEmail(int id)
    {
        // Check for SentEmail
        var email = _context.SentEmails.FirstOrDefault(e => e.Id == id);
        if (email != null)
        {
            var trashEmail = new TrashEmail
            {
                To = email.To,
                From = email.From,
                Subject = email.Subject,
                Body = email.Body,
                SentDate = email.SentDate,
                OriginalEmailType = "Sent"
            };

            _context.TrashEmails.Add(trashEmail);
            _context.SentEmails.Remove(email);
            _context.SaveChanges();

            return Json(new { success = true, message = "Email moved to Trash." });
        }

        // Check for ReceivedEmail
        var receivedEmail = _context.ReceivedEmails.FirstOrDefault(e => e.Id == id);
        if (receivedEmail != null)
        {
            try
            {
                var trashEmail = new TrashEmail
                {
                    From = receivedEmail.From, // Use From for received emails
                    Subject = receivedEmail.Subject,
                    Body = receivedEmail.Body,
                    SentDate = receivedEmail.ReceivedDate.HasValue ? receivedEmail.ReceivedDate.Value : DateTime.Now,  // Fix null DateTime issue
                    OriginalEmailType = "Received"
                };

                _context.TrashEmails.Add(trashEmail);
                _context.ReceivedEmails.Remove(receivedEmail);
                _context.SaveChanges();

                return Json(new { success = true, message = "Email moved to Trash." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error moving email to Trash: {ex.Message}" });
            }
        }

        return Json(new { success = false, message = "Email not found." });
    }

    // POST: Restore Email from Trash
    [HttpPost]
    public JsonResult RestoreEmail(int id)
    {
        var email = _context.TrashEmails.FirstOrDefault(e => e.Id == id);
        if (email != null)
        {
            // Logic to restore email
            if (email.OriginalEmailType == "Sent")
            {
                var sentEmail = new SentEmail
                {
                    To = email.To,
                    From = email.From,
                    Subject = email.Subject,
                    Body = email.Body,
                    SentDate = email.SentDate
                };

                _context.SentEmails.Add(sentEmail);
            }
            else if (email.OriginalEmailType == "Received")
            {
                var receivedEmail = new ReceivedEmail
                {
                    From = email.From,
                    Subject = email.Subject,
                    Body = email.Body,
                    ReceivedDate = email.SentDate
                };

                _context.ReceivedEmails.Add(receivedEmail);
            }

            _context.TrashEmails.Remove(email);
            _context.SaveChanges();

            return Json(new { success = true, message = "Email restored." });
        }

        return Json(new { success = false, message = "Email not found." });
    }
    [HttpPost]
    public IActionResult DeleteReceivedEmail(int id)
    {
        var email = _context.ReceivedEmails.FirstOrDefault(e => e.Id == id);
        if (email == null)
        {
            return Json(new { success = false, message = "Email not found." });
        }

        // Simulate moving to trash or deleting
        _context.ReceivedEmails.Remove(email);
        _context.SaveChanges();

        return Json(new { success = true });
    }

}
