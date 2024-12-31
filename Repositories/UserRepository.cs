using Microsoft.EntityFrameworkCore;
using UserdataManagement.Data;
using UserdataManagement.Models;

namespace UserdataManagement.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDataDbContext _context;

        public UserRepository(UserDataDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserDataModel>> GetAllUsers()
        {
            return await _context.UserData.ToListAsync();
        }

        public async Task<IEnumerable<UserDataModel>> GetActiveUsers()
        {
            return await _context.UserData.Where(user => user.IsActive).ToListAsync();
        }


        public async Task<UserDataModel> GetUserById(int id)
        {
            return await _context.UserData.FindAsync(id);
        }

        public async Task AddUser(UserDataModel user)
        {
            user.IsActive = true;
            user.CreatedAt = DateTime.Now;
            await _context.UserData.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUser(int id, UserDataModel user)
        {
            var existingUser = await _context.UserData.FindAsync(id);

            if (existingUser == null)
            {
                throw new Exception("User not found.");
            }

            existingUser.Name = user.Name;
            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.Password = user.Password;
            existingUser.Age = user.Age;
            existingUser.Phone = user.Phone;
            existingUser.Address = user.Address;
            existingUser.UpdatedAt = DateTime.Now;

            // Save changes to the database
            _context.UserData.Update(existingUser);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUser(int id)
        {
            var user = await _context.UserData.FindAsync(id);
            if (user != null)
            {
                _context.UserData.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public bool IsEmailExists(string email)
        {
            return _context.UserData.Any(u => u.Email == email);
        }

        public bool IsUsernameExists(string username)
        {
            return _context.UserData.Any(u => u.Username == username);
        }

        public bool IsPhoneNumberExists(string phone)
        {
            return _context.UserData.Any(u => u.Phone == phone);
        }

        public async Task DeactivateUsers(IEnumerable<int> userIds)
        {
            var users = await _context.UserData.Where(user => userIds.Contains(user.Id)).ToListAsync();

            foreach (var user in users)
            {
                user.IsActive = false;
                user.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<UserDataModel> LoginUser(string emailOrUsername, string password)
        {
            return await _context.UserData
                .Where(u => (u.Username == emailOrUsername || u.Email == emailOrUsername) && u.Password == password)
                .FirstOrDefaultAsync();
        }


    }
}
