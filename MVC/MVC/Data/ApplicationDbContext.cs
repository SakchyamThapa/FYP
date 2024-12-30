using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<LoginHistory> LoginHistories { get; set; }
    public DbSet<SentEmail> SentEmails { get; set; }
    public DbSet<TrashEmail> TrashEmails { get; set; }
    public DbSet<ReceivedEmail> ReceivedEmails { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Email> Emails { get; set; }

    public DbSet<UserViewModel> UserViewModels { get; set; }

    public DbSet<RedeemModel>Redeems { get; set; }
   

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SentEmail>().ToTable("SentEmails");
        modelBuilder.Entity<TrashEmail>().ToTable("TrashEmails");
        modelBuilder.Entity<ReceivedEmail>().ToTable("ReceivedEmails");

        // Ignore MVC-related types
        modelBuilder.Ignore<Endpoint>();
        modelBuilder.Ignore<FilterDescriptor>();
        modelBuilder.Ignore<BindingSource>();
        modelBuilder.Ignore<ModelExplorer>();
        modelBuilder.Ignore<ViewDataDictionary>();
        modelBuilder.Ignore<Func<ActionContext, bool>>();
        modelBuilder.Ignore<Func<IRazorPage>>(); 
    }
}
