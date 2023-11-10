using WebApi.Helpers;
using WebApi.Models;

namespace WebApi.Repositories
{
    public class RequestRepository : IRepository<Request>
    {
        public async Task<bool> CreateAsync(Request entity)
        {
            var dbService = new DbService();
            var query = "INSERT INTO Request VALUES (@Employee, NULL, @ProductName, @Price, @Quantity, @Active);";
            var parameters = new Dictionary<string, object>
            {
                { "@Employee", entity.Employee },
                { "@ProductName", $"{entity.ProductName}" },
                { "@Price", entity.Price },
                { "@Quantity", entity.Quantity },
                { "@Active", true }
            };

            // Ya que cuando los requests se crean el accountant no necesariamente es conocido, este puede ser null
            if (entity.Accountant is not null)
            {
                query = "INSERT INTO Request VALUES (@Employee, @Accountant, @ProductName, @Price, @Quantity, @Active);";

                parameters = new Dictionary<string, object>
                {
                    { "@Employee", entity.Employee },
                    { "@Accountant", entity.Accountant},
                    { "@ProductName", $"{entity.ProductName}"},
                    { "@Price", entity.Price},
                    { "@Quantity", entity.Quantity},
                    { "@Active", true }
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

        public async Task<Request?> ReadAsync(Request entity)
        {
            var dbService = new DbService();

            // Never returns password hash or salt.
            const string query = "SELECT * FROM Request WHERE RequestID = @RequestId";

            var parameters = new Dictionary<string, object>
            {
                { "@RequestId", entity.RequestId }
            };

            // This collection actually only has 1 element because ID is unique, but it comes as a collection by default.
            var requests = await dbService.ReadData<Request>(query, parameters);

            var request = requests.FirstOrDefault();

            return request;
        }

        public async Task<IEnumerable<Request>?> ReadAllAsync()
        {
            // Un read all requests para el Sysadmin ya que es el único que puede ver todos los requests
            var dbService = new DbService();

            const string query = "SELECT * FROM Request";

            var requests = await dbService.ReadData<Request>(query);

            return requests;
        }

        public async Task<IEnumerable<Request>?> ReadByUsernameAsync(string username)
        {
            var dbService = new DbService();

            const string query = @"
                SELECT r.*
                FROM Request r
                INNER JOIN Users u ON r.Employee = u.UserID
                WHERE u.Username = @Username";

            var parameters = new Dictionary<string, object>
            {
                { "@Username", username }
            };

            var requests = await dbService.ReadData<Request>(query, parameters);

            return requests;
        }

        public async Task<IEnumerable<Request>?> ReadByAccountantIdAsync(int accountantId)
        {
            var dbService = new DbService();

            const string query = @"
                SELECT * FROM Request
                WHERE Accountant = @AccountantID;
";

            var parameters = new Dictionary<string, object>
            {
                { "@AccountantID", accountantId }
            };

            var requests = await dbService.ReadData<Request>(query, parameters);

            return requests;
        }

        public async Task<IEnumerable<Request>?> ReadByBossIdAsync(int bossId)
        {
            var dbService = new DbService();

            const string query = @"
                SELECT r.*
                FROM Request r
                INNER JOIN Users u ON r.Employee = u.UserID
                WHERE u.BossID = @BossID";

            var parameters = new Dictionary<string, object>
            {
                { "@BossID", bossId }
            };

            var requests = await dbService.ReadData<Request>(query, parameters);

            return requests;
        }

        public async Task<bool> AssignToAccountantByAmount(int requestId)
        {
            var dbService = new DbService();

            var request = await ReadAsync(new Request { RequestId = requestId });

            if (request is null)
                return false;

            var accountantType = await GetAccountantTypeByAmountAsync(request.Price * request.Quantity);
            
            // Now we get a random accountant of the type we need.
            const string query = @"
                UPDATE Request
                SET Accountant = (
                    SELECT TOP 1 UserID
                    FROM Users
                    WHERE AccountantType = @AccountantType
                    ORDER BY NEWID()
                )
                WHERE RequestID = @RequestID";

            var parameters = new Dictionary<string, object>
            {
                { "@RequestID", requestId },
                { "@AccountantType", accountantType}
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

        public async Task<int> GetAccountantTypeByAmountAsync(double amount)
        {
            var dbService = new DbService();

            // No usé BETWEEN porque es inclusivo y no queremos que el mínimo y máximo de accountants adyacentes sean iguales.
            const string query = @"
                SELECT AccountantTypeID
                FROM AccountantType 
                WHERE MinPrice < @Amount 
                AND MaxPrice >= @Amount";

            var parameters = new Dictionary<string, object>
            {
                { "@Amount", amount }
            };

            var accountantType = await dbService.ReadData<int>(query, parameters);

            return accountantType.FirstOrDefault();
        }

        public async Task<bool> UpdateAsync(Request entity)
        {
            var dbService = new DbService();

            var query = "UPDATE Request SET Employee = @Employee, Accountant = NULL, " +
                              "ProductName = @ProductName, Price = @Price, Quantity = @Quantity, Active = @Active " +
                              "WHERE RequestID = @RequestID";

            var parameters = new Dictionary<string, object>
            {
                { "@RequestID", entity.RequestId},
                { "@Employee", entity.Employee },
                { "@ProductName", $"{entity.ProductName}" },
                { "@Price", entity.Price },
                { "@Quantity", entity.Quantity },
                { "@Active", entity.Active }
            };

            // Ya que cuando los requests se crean el accountant no necesariamente es conocido, este puede ser null
            if (entity.Accountant is not null)
            {
                query = "UPDATE Request SET Employee = @Employee, Accountant = @Accountant, " +
                        "ProductName = @ProductName, Price = @Price, Quantity = @Quantity, Active = @Active " +
                        "WHERE RequestID = @RequestID";

                parameters = new Dictionary<string, object>
                {
                    { "@RequestID", entity.RequestId},
                    { "@Employee", entity.Employee },
                    { "@Accountant", entity.Accountant},
                    { "@ProductName", $"{entity.ProductName}"},
                    { "@Price", entity.Price},
                    { "@Quantity", entity.Quantity},
                    { "@Active", entity.Active }
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

        public async Task<bool> CancelAsync(int requestId)
        {
            var dbService = new DbService();

            const string query = "UPDATE Request SET Active = @Active WHERE RequestID = @RequestID";

            var parameters = new Dictionary<string, object>
            {
                { "@Active", false },
                { "@RequestID", requestId }
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

        public async Task<bool> DeleteAsync(Request entity)
        {
            // Ideally this method should not be used, this is to complete the interface implementation and for testing purposes.
            // Requests should only be rejected

            var dbService = new DbService();

            const string query = "DELETE FROM Request WHERE Request = @RequestId";

            var parameters = new Dictionary<string, object>
            {
                { "@RequestId", entity.RequestId }
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
    }
}
