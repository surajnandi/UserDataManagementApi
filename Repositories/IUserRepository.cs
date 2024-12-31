using UserdataManagement.Models;

namespace UserdataManagement.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserDataModel>> GetAllUsers();
        Task<IEnumerable<UserDataModel>> GetActiveUsers();
        Task<UserDataModel> GetUserById(int id);
        Task AddUser(UserDataModel user);
        Task UpdateUser(int id, UserDataModel user);
        Task DeleteUser(int id);
        bool IsEmailExists(string email);
        bool IsUsernameExists(string username);
        bool IsPhoneNumberExists(string phone);
        Task DeactivateUsers(IEnumerable<int> userIds);
        Task<UserDataModel> LoginUser(string emailOrUsername, string password);
    }
}
