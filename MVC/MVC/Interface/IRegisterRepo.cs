using MVC.Models;
using RepoBaseModelCore;

namespace MVC.Interface
{
    public interface IRegisterRepo : IGeneralRepositories<User, int>
    {
    }
    public class RegisterRepo : _AbsGeneralRepositories<ApplicationDbContext, User, int>, IRegisterRepo
    {
        public RegisterRepo(ApplicationDbContext context) : base(context)
        {

        }
    }
}
