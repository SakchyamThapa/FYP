using MVC.Models;
using RepoBaseModelCore;

namespace MVC.Interface
{
    public interface IAdminRepo : IGeneralRepositories<AdminRepo, int>
    {
    }
    public class AdminRepo : _AbsGeneralRepositories<ApplicationDbContext, AdminRepo, int>, IAdminRepo
    {
        public AdminRepo(ApplicationDbContext context) : base(context)
        {

        }

    }
}
