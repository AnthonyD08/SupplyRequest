using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.Data.SqlClient;

namespace WebApi.Helpers
{
    internal class DbService
    {
        // Standard class to make SQL query to the localhost server

        private readonly SqlConnection _connection;

        public DbService()
        {
            const string connectionString = "Server=localhost;Database=SupplyRequest;Trusted_Connection=True;TrustServerCertificate=true;";

            // For every instance of this class, there will be a new DB connection.
            _connection = new SqlConnection(connectionString);
        }

        public async Task SendData(string query, Dictionary<string, object> parameters)
        {
            await _connection.OpenAsync();

            // Utilizes the query parameter and executes it to the DB
            using var dataAdapter = new SqlDataAdapter
            {
                InsertCommand = new SqlCommand(query, _connection)
            };

            foreach (var parameter in parameters)
            {
                dataAdapter.InsertCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
            }

            _ = await dataAdapter.InsertCommand.ExecuteNonQueryAsync();

            await _connection.CloseAsync();
        }

        public async Task SendData(string query)
        {
            // WARNING: This method is vulnerable to SQL injection attacks.
            // Only use this method if you're sure that the query is safe and NEVER coming from user input.

            await _connection.OpenAsync();

            var dataAdapter = new SqlDataAdapter
            {
                InsertCommand = new SqlCommand(query, _connection)
            };

            _ = await dataAdapter.InsertCommand.ExecuteNonQueryAsync();

            await _connection.CloseAsync();
            dataAdapter.Dispose();
        }

        public async Task<IEnumerable<T>> ReadData<T>(string query, Dictionary<string, object> parameters)
        {
            await _connection.OpenAsync();

            // Utilizes the query parameter and executes it to the DB
            var result = await _connection.QueryAsync<T>(query, parameters);

            if (result is null) return Enumerable.Empty<T>();

            await _connection.CloseAsync();

            return result;
        }

        public async Task<IEnumerable<T>> ReadData<T>(string query)
        {
            // WARNING: This method is vulnerable to SQL injection attacks.
            // Only use this method if you're sure that the query is safe and NEVER coming from user input.

            await _connection.OpenAsync();

            var result = await _connection.QueryAsync<T>(query);

            await _connection.CloseAsync();
            return result;
        }

        /* Legacy DataSet method
        
        public async Task<DataSet> ReadData(string query, Dictionary<string, object> parameters)
        {
            await _connection.OpenAsync();

            // Utilizes the query parameter and executes it to the DB
            using var dataAdapter = new SqlDataAdapter(query, _connection);

            foreach (var parameter in parameters)
            {
                dataAdapter.SelectCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
            }

            await dataAdapter.SelectCommand.ExecuteNonQueryAsync();

            var dataSet = new DataSet();
            dataAdapter.Fill(dataSet);

            // Leaving open connections to the DB is bad practice
            await _connection.OpenAsync();

            return dataSet;
        }*/

        public async Task<DataTable> ReadDataTable(string query, Dictionary<string, object> parameters)
        {
            await _connection.OpenAsync();

            // Utilizes the query parameter and executes it to the DB
            using var dataAdapter = new SqlDataAdapter(query, _connection);

            foreach (var parameter in parameters)
            {
                dataAdapter.SelectCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
            }

            var dataTable = new DataTable();
            dataAdapter.Fill(dataTable);

            // Leaving open connections to the DB is bad practice
            await _connection.OpenAsync();

            return dataTable;
        }
    }
}
