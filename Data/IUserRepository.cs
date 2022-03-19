using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestRedis.Data
{
    public interface IUserRepository
    {
        Task<List<User>> GetUsers();
        Task<User> GetUserByKey(int key);
        Task AddUser(User newUser);
        Task DeleteUser(int key);
    }
}
