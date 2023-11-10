using System.Security.Cryptography;
using System.Text;
using WebApi.Helpers;
using WebApi.Models;

namespace WebApi.Repositories
{
    public class UserRepository : IRepository<User>
    {
        public async Task<bool> CreateAsync(User entity)
        {
            var dbService = new DbService();

            entity.PasswordSalt = GenerateSalt();

            entity.Password = GenerateHash(entity.Password, entity.PasswordSalt);

            var query = "INSERT INTO Users VALUES (@Name, @Username, @Email, @PasswordHash, @PasswordSalt, @RoleID, @BossID, NULL, @Active);";
            var parameters = new Dictionary<string, object>
            {
                { "@Name", $"{entity.Name}" },
                { "@Username", $"{entity.Username}" },
                { "@Email", $"{entity.Email}"},
                { "@PasswordHash", $"{entity.Password}"},
                { "@PasswordSalt", $"{entity.PasswordSalt}"},
                { "@RoleID", entity.RoleId },
                { "@BossID", entity.BossId },
                { "@Active", $"{entity.Active}"}
            };

            // Ya que cuando los usuarios se crean no necesariamente son accountants, este puede ser null
            if (entity.AccountantType is not null)
            {
                query = "INSERT INTO Users VALUES (@Name, @Username, @Email, @PasswordHash, @PasswordSalt, @RoleID, @BossID, @AccountantType, @Active);";

                parameters = new Dictionary<string, object>
                {
                    { "@Name", $"{entity.Name}" },
                    { "@Username", $"{entity.Username}" },
                    { "@Email", $"{entity.Email}"},
                    { "@PasswordHash", $"{entity.Password}"},
                    { "@PasswordSalt", $"{entity.PasswordSalt}"},
                    { "@RoleID", entity.RoleId },
                    { "@BossID", entity.BossId },
                    { "@AccountantType", entity.AccountantType},
                    { "@Active", $"{entity.Active}"}
                };
            }

            try
            {
                await dbService.SendData(query, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }

        public async Task<User?> ReadAsync(User entity)
        {
            var dbService = new DbService();

            // Never returns password hash or salt.
            const string query = "SELECT UserId, Name, Username, Email, BossID, RoleID, AccountantType, Active FROM Users WHERE UserId = @UserId";

            var parameters = new Dictionary<string, object>
            {
                { "@UserId", $"{entity.UserId}" }
            };

            // This collection actually only has 1 element because ID is unique, but it comes as a collection by default.
            var users = await dbService.ReadData<User>(query, parameters);

            var user = users.FirstOrDefault();

            return user;
        }

        public async Task<IEnumerable<User>?> ReadAllAsync()
        {
            var dbService = new DbService();

            // Never returns password hash or salt.
            const string query = "SELECT UserId, Name, Username, Email, BossID, RoleID, AccountantType, Active FROM Users";

            var users = await dbService.ReadData<User>(query);

            return users;
        }

        public async Task<User?> ReadByUsernameAsync(string username)
        {
            var dbService = new DbService();

            const string query = "SELECT UserId, Name, Username, Email, BossID, RoleID, AccountantType, Active FROM Users WHERE Username = @Username";

            var parameters = new Dictionary<string, object>
            {
                { "@Username", $"{username}" }
            };

            var users = await dbService.ReadData<User>(query, parameters);

            var user = users.FirstOrDefault();

            return user;
        }

        public async Task<bool> UpdateAsync(User entity)
        {
            var dbService = new DbService();

            // Update also never touches password hash or salt. 
            // Must use the UpdatePasswordAsync method below.
            var query = "UPDATE Users SET Username = @Username, Name = @Name, Email = @Email, " +
                                 " BossID = @BossID, RoleID = @RoleID, AccountantType = NULL, Active = @Active " +
                                 "WHERE UserId = @UserId";

            var parameters = new Dictionary<string, object>
            {
                { "@Username", $"{entity.Username}" },
                { "@Name", $"{entity.Name}" },
                { "@Email", $"{entity.Email}" },
                { "@BossID", entity.BossId },
                { "@RoleID", entity.RoleId },
                { "@Active", entity.Active },
                { "@UserId", $"{entity.UserId}" }
            };

            if (entity.AccountantType is not null)
            {
                query = "UPDATE Users SET Username = @Username, Email = @Email, BossID = @BossID," +
                                 " RoleID = @RoleID, AccountantType = @AccountantType, Active = @Active " +
                                 "WHERE UserId = @UserId";

                parameters = new Dictionary<string, object>
                {
                    { "@Username", $"{entity.Username}" },
                    { "@Name" , $"{entity.Name}"},
                    { "@Email", $"{entity.Email}" },
                    { "@BossID", entity.BossId },
                    { "@RoleID", entity.RoleId },
                    { "@AccountantType", $"{entity.AccountantType}" },
                    { "@Active", entity.Active },
                    { "@UserId", $"{entity.UserId}" }
                };
            }

            try
            {
                await dbService.SendData(query, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
        {
            // Same as above but using the Encrypt method in this same class to encrypt the password.

            var dbService = new DbService();

            const string query = "UPDATE Users SET Password = @PasswordHash, PasswordSalt = @PasswordSalt WHERE UserId = @UserId";
            
            var passwordSalt = GenerateSalt();

            var passwordHash = GenerateHash(newPassword, passwordSalt);

            var parameters = new Dictionary<string, object>
            {
                { "@PasswordHash", $"{passwordHash}" },
                { "@PasswordSalt", $"{passwordSalt}" },
                { "@UserId", $"{userId}" }
            };

            try
            {
                await dbService.SendData(query, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteAsync(User entity)
        {
            // Ideally this method should not be used, this is to complete the interface implementation and for testing purposes.
            // Users should only be deactivated

            var dbService = new DbService();

            const string query = "DELETE FROM Users WHERE UserId = @UserId";

            var parameters = new Dictionary<string, object>
            {
                { "@UserId", $"{entity.UserId}" }
            };

            try
            {
                await dbService.SendData(query, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }

        public async Task<bool> ActivateAsync(int userId)
        {
            var dbService = new DbService();

            const string query = "UPDATE Users SET Active = 1 WHERE UserId = @UserId";

            var parameters = new Dictionary<string, object>
            {
                { "@UserId", $"{userId}" }
            };

            try
            {
                await dbService.SendData(query, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }

        public async Task<bool> DeactivateAsync(int userId)
        {
            var dbService = new DbService();

            const string query = "UPDATE Users SET Active = 0 WHERE UserId = @UserId";

            var parameters = new Dictionary<string, object>
            {
                { "@UserId", $"{userId}" }
            };

            try
            {
                await dbService.SendData(query, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            var dbService = new DbService();

            const string query = "SELECT Password, PasswordSalt FROM Users WHERE Username = @Username";

            var parameters = new Dictionary<string, object>
            {
                { "@Username", $"{username}" }
            };

            var users = await dbService.ReadData<User>(query, parameters);

            var user = users.FirstOrDefault();

            if (user is null)
                return false;

            var computedHash = GenerateHash(password, user.PasswordSalt);

            return computedHash == user.Password;
        }

        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            var dbService = new DbService();

            const string query = "SELECT Username FROM Users WHERE Username = @Username";

            var parameters = new Dictionary<string, object>
            {
                { "@Username", $"{username}" }
            };

            var users = await dbService.ReadData<User>(query, parameters);

            var user = users.FirstOrDefault();

            return user is null;
        }   

        public string GenerateSalt()
        {
            var secretKey = RandomNumberGenerator.GetBytes(16);

            return Convert.ToBase64String(secretKey);
        }

        public string GenerateHash(string password, string salt)
        {
            using var hmac = new HMACSHA256(Convert.FromBase64String(salt));
            var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));

            return computedHash;
        }
    }
}
