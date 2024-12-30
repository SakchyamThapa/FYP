using Microsoft.AspNetCore.Mvc;
using MVC.Interface;
using MVC.Models;
using System.Linq;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    public IRegisterRepo _registerRepo;

    public AdminController(ApplicationDbContext context, IRegisterRepo registerRepo)
    {
        _context = context;
        _registerRepo= registerRepo;

    }

    // GET: Admin/Index
    public IActionResult Index()
    {
        // Selecting UserViewModel, not User
        var users = _context.UserViewModels.Select(u => new UserViewModel
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            KPIPoints = u.KPIPoints
        }).ToList();  // This is a List<UserViewModel>

        // Selecting ProjectViewModel, not Project
        var projects = _context.Projects.Select(p => new Project
        {
            Id = p.Id,
            Name = p.Name,
            Deadline = p.Deadline,
            Progress = p.Progress,
            Description = p.Description
        }).ToList();  // This is a List<ProjectViewModel>

        // Creating the AdminPanelViewModel with both users and projects
        var viewModel = new AdminPanelViewModel
        {
            users = users,  // Pass UserViewModel to AdminPanelViewModel
            Projects = projects  // Pass ProjectViewModel to AdminPanelViewModel
        };

        return View(viewModel); // Pass viewModel to the view
    }

    // POST: Admin/AddProject
    [HttpPost]
    public IActionResult AddProject(Project model)
    {
        if (ModelState.IsValid)
        {
            var project = new Project
            {
                Name = model.Name,
                Deadline = model.Deadline,
                Description = model.Description
            };

            _context.Projects.Add(project);
            _context.SaveChanges();
            return RedirectToAction("Index"); // Redirect back to the Index view after successful project creation
        }

        // If the model is invalid, retrieve necessary data for the Index view
        var users = _context.UserViewModels.Select(u => new UserViewModel
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            KPIPoints = u.KPIPoints
        }).ToList();

        var projects = _context.Projects.Select(p => new Project
        {
            Id = p.Id,
            Name = p.Name,
            Deadline = p.Deadline,
            Progress = p.Progress,
            Description = p.Description
        }).ToList();

        var viewModel = new AdminPanelViewModel
        {
            users = users,
            Projects = projects
        };

        // Use the full path to render the view in the SendMail folder, passing the model if necessary
        return View("~/Views/SendMail/Home.cshtml", viewModel); // Use the full path to the view
    }

    // POST: Admin/ResetKPI
    [HttpPost]
    public IActionResult ResetKPI(int id)
    {
        var user = _context.UserViewModels.FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            user.KPIPoints = 0;
            _context.SaveChanges();
            return Json(new { success = true });
        }

        return Json(new { success = false });
    }
}
