using MVC.Models;
using RepoBaseModelCore;

namespace MVC.Interface
{
    public interface IEmailLogsRepo: IGeneralRepositories<SentEmail, int>
    {
    }
    public class EmailLogRepo : _AbsGeneralRepositories<ApplicationDbContext, SentEmail, int>, IEmailLogsRepo
    {
        public EmailLogRepo(ApplicationDbContext context) : base(context)
        {
        }
    }
}
