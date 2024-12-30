using MVC.Models;
using RepoBaseModelCore;

namespace MVC.Interface
{
    public interface ILoginRepo : IGeneralRepositories<LoginHistory,int >
    {
    }
    public class LoginRepo : _AbsGeneralRepositories<ApplicationDbContext, LoginHistory, int>, ILoginRepo
    {
        public LoginRepo(ApplicationDbContext context) : base(context)
        {
           
        } 

    }
}
