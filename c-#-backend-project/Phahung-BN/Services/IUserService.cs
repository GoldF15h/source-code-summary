using Phahung_BN.Models;
using System.Threading.Tasks;

namespace Phahung_BN.Services
{
    public interface IUserService
    {
        Task<PrivateUserModel> GetUserInfo(string uid);
    }

}
