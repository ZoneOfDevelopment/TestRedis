using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Linq;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;

namespace TestRedis.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly IDistributedCache _redisCache;
        private readonly IConfiguration _config;

        public UserRepository(IDistributedCache redisCache, IConfiguration config)
        {
            _redisCache = redisCache;
            _config = config;
        }

        public async Task AddUser(User newUser)
        {
            await SaveIntoCache(newUser);
        }


        public async Task<List<User>> GetUsers()
        {
            var listUsers = new List<User>();
            List<string> redisKeys = GetAllkeys();

            if(redisKeys.Count>0)
            {
                foreach(string key in redisKeys)
                {
                    var userFromCache = await _redisCache.GetAsync(key);
                    if(userFromCache!=null)
                    {
                        // we take User from cache
                        var serializedUser = Encoding.UTF8.GetString(userFromCache);
                        var userOut = JsonConvert.DeserializeObject<User>(serializedUser);
                        listUsers.Add(userOut);
                    }
                }
            }
            else
            {
                // we create a list of User
                listUsers.Add(new User { Id = 1, Email = "email1_fromDB", UserName = "user1_fromDB", Password = "password1_fromDB" });
                listUsers.Add(new User { Id = 2, Email = "email2_fromDB", UserName = "user2_fromDB", Password = "password2_fromDB" });
                listUsers.Add(new User { Id = 3, Email = "email3_fromDB", UserName = "user3_fromDB", Password = "password3_fromDB" });

                // we save a list of user into Redis
                await SaveIntoCache(new User { Id = 1, Email = "email1_fromCache", UserName = "user1_fromCache", Password = "password1_fromCache" });
                await SaveIntoCache(new User { Id = 2, Email = "email2_fromCache", UserName = "user2_fromCache", Password = "password2_fromCache" });
                await SaveIntoCache(new User { Id = 3, Email = "email3_fromCache", UserName = "user3_fromCache", Password = "password3_fromCache" });
            }

            return listUsers;
        }

        public async Task<User> GetUserByKey(int key)
        {
            User userOut = null;
            var userFromCache = await _redisCache.GetAsync(key.ToString());
            if (userFromCache != null)
            {
                var serializedUser = Encoding.UTF8.GetString(userFromCache);
                userOut = JsonConvert.DeserializeObject<User>(serializedUser);
            }

            return userOut;
        }

        public async Task DeleteUser(int key)
        {
            await _redisCache.RemoveAsync(key.ToString());
        }


        private async Task SaveIntoCache(User user)
        {
            var serializedUser = JsonConvert.SerializeObject(user);
            var redisUsers = Encoding.UTF8.GetBytes(serializedUser);
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.Now.AddMinutes(10))
                .SetSlidingExpiration(TimeSpan.FromMinutes(8));

            await _redisCache.SetAsync(user.Id.ToString(), redisUsers, options);
        }

        private List<string> GetAllkeys()
        {
            List<string> listKeys = new List<string>();
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_config.GetValue<string>("RedisCache:ServerUrl")))
            {
                string host = _config.GetValue<string>("RedisCache:ServerUrl").Split(':')[0];
                int port = Convert.ToInt32(_config.GetValue<string>("RedisCache:ServerUrl").Split(':')[1]);
                var keys = redis.GetServer(host, port).Keys();
                listKeys.AddRange(keys.Select(key => (string)key).ToList());
            }

            return listKeys;
        }
    }
}
