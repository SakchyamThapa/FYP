using MVC.Models;
using RepoBaseModelCore;

namespace MVC.Interface
{
    public interface IRedeemRepo : IGeneralRepositories<RedeemModel,int >
    {
    }
    public class RedeemRepo : _AbsGeneralRepositories<ApplicationDbContext, RedeemModel, int>, IRedeemRepo
    {
        public RedeemRepo(ApplicationDbContext context) : base(context)
        {
           
        } 

    }
}
