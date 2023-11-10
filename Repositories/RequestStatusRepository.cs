using WebApi.Helpers;
using WebApi.Models;

namespace WebApi.Repositories
{
    public class RequestStatusRepository : IRepository<RequestStatus>
    {
        public async Task<bool> CreateAsync(RequestStatus entity)
        {
            var dbService = new DbService();
            var query = "INSERT INTO RequestStatus VALUES (@RequestId, @UserId, @Date, @Time, @Description);";

            var parameters = new Dictionary<string, object>
            {
                { "@RequestId", entity.RequestId },
                { "@UserId", entity.UserId },
                { "@Date", entity.Date },
                { "@Time", entity.Time },
                { "@Description", $"{entity.Description}" }
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
        public async Task<List<RequestStatus>?> ReadAllRequestStatus(int requestId)
        {
            var dbService = new DbService();
            var query = "SELECT * FROM RequestStatus WHERE RequestId = @RequestId;";

            var parameters = new Dictionary<string, object>
            {
                { "@RequestId", requestId }
            };

            var data = await dbService.ReadData<RequestStatus>(query, parameters);

            return data.ToList();
        }

        public async Task<RequestStatus?> ReadAsync(RequestStatus entity)
        {
            var dbService = new DbService();
            var query = "SELECT * FROM RequestStatus WHERE RequestId = @RequestId;";

            var parameters = new Dictionary<string, object>
            {
                { "@StatusId", entity.StatusId }
            };

            var data = await dbService.ReadData<RequestStatus>(query, parameters);

            var status = data.FirstOrDefault();

            return status;
        }

        public async Task<RequestStatus?> ReadByRequestIdAsync(int requestId)
        {
            var dbService = new DbService();
            var query = "SELECT * FROM RequestStatus WHERE RequestId = @RequestId;";

            var parameters = new Dictionary<string, object>
    {
        { "@RequestId", requestId }
    };

            var data = await dbService.ReadData<RequestStatus>(query, parameters);

            var status = data.FirstOrDefault();

            return status;
        }

        public async Task<IEnumerable<RequestStatus>?> ReadByRequestAsync(int requestId)
        {
            var dbService = new DbService();
            var query = "SELECT * FROM RequestStatus WHERE RequestId = @RequestId;";

            var parameters = new Dictionary<string, object>
            {
                { "@RequestId", requestId }
            };

            var data = await dbService.ReadData<RequestStatus>(query, parameters);

            return data;
        }

        public async Task<RequestStatus?> ReadLatestAsync(int requestId)
        {
            var dbService = new DbService();
            var query = "SELECT TOP 1 * FROM RequestStatus WHERE RequestId = @RequestId ORDER BY Date DESC, Time DESC;";

            var parameters = new Dictionary<string, object>
            {
                { "@RequestId", requestId }
            };

            var data = await dbService.ReadData<RequestStatus>(query, parameters);

            var status = data.FirstOrDefault();

            return status;
        }

        public async Task<bool> IsApprovedByBoss(int requestId)
        {
            var dbService = new DbService();

            const string query = @"
                SELECT TOP 1 *
                FROM RequestStatus
                WHERE RequestId = @RequestId
                AND Description = 'Aprobada por el jefe'
                ORDER BY Date DESC, Time DESC;";

            var parameters = new Dictionary<string, object>
            {
                { "@RequestId", requestId }
            };

            var data = await dbService.ReadData<RequestStatus>(query, parameters);

            var status = data.FirstOrDefault();

            return status != null;
        }

        public async Task<bool> IsApprovedByAccountant(int requestId)
        {
            var dbService = new DbService();

            const string query = @"
                SELECT TOP 1 *
                FROM RequestStatus
                WHERE RequestId = @RequestId
                AND Description = 'Aprobada por el contador'
                ORDER BY Date DESC, Time DESC;";

            var parameters = new Dictionary<string, object>
            {
                { "@RequestId", requestId }
            };

            var data = await dbService.ReadData<RequestStatus>(query, parameters);

            var status = data.FirstOrDefault();

            return status != null;
        }

        public async Task<bool> UpdateAsync(RequestStatus entity)
        {
            var dbService = new DbService();
            var query = "UPDATE RequestStatus SET RequestId = @RequestId, UserId = @UserId, Date = @Date, Time = @Time, Description = @Description WHERE StatusId = @StatusId;";

            var parameters = new Dictionary<string, object>
            {
                { "@StatusId", entity.StatusId },
                { "@RequestId", entity.RequestId },
                { "@UserId", entity.UserId },
                { "@Date", entity.Date },
                { "@Time", entity.Time },
                { "@Description", $"{entity.Description}" }
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

        public async Task<bool> DeleteAsync(RequestStatus entity)
        {
            var dbService = new DbService();
            var query = "DELETE FROM RequestStatus WHERE StatusId = @StatusId;";

            var parameters = new Dictionary<string, object>
            {
                { "@StatusId", entity.StatusId }
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
