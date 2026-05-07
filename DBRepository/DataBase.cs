using System.Data;
using System.Data.SqlClient;

namespace SRRAMOils.DBRepository
{
    /// <summary>
    /// Lightweight database helper using Microsoft.Data.SqlClient and ADO.NET.
    /// Reads connection string from IConfiguration with key "DefaultConnection" by default.
    /// Designed for DI in Razor Pages projects.
    /// </summary>
    public sealed class DataBase
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of <see cref="DataBase"/>.
        /// Expects an IConfiguration containing a connection string under the provided name.
        /// </summary>
        /// <param name="configuration">Application configuration (IConfiguration)</param>
        /// <param name="connectionStringName">Name of the connection string in configuration (default: "DefaultConnection")</param>
        public DataBase(IConfiguration configuration, string connectionStringName = "DevConnection")
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            _connectionString = configuration.GetConnectionString(connectionStringName)
                ?? configuration[connectionStringName];

            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new ArgumentException($"Connection string '{connectionStringName}' was not found in configuration.", nameof(connectionStringName));
        }

        /// <summary>
        /// Creates a new SqlConnection instance (closed). Caller may open or use OpenConnectionAsync helper.
        /// </summary>
        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        /// <summary>
        /// Creates and opens a SqlConnection asynchronously.
        /// The caller is responsible for disposing the returned connection.
        /// </summary>
        public async Task<SqlConnection> OpenConnectionAsync()
        {
            var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync().ConfigureAwait(false);
            return conn;
        }

        /// <summary>
        /// Executes a non-query command (INSERT/UPDATE/DELETE) and returns affected rows.
        /// </summary>
        public async Task<int> ExecuteNonQueryAsync(string sql, params SqlParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));

            await using var conn = await OpenConnectionAsync().ConfigureAwait(false);
            await using var cmd = new SqlCommand(sql, conn)
            {
                CommandType = CommandType.Text
            };

            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            return await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a scalar query and returns the result cast to T (or default if null/DbNull).
        /// </summary>
        public async Task<T?> ExecuteScalarAsync<T>(string sql, params SqlParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));

            await using var conn = await OpenConnectionAsync().ConfigureAwait(false);
            await using var cmd = new SqlCommand(sql, conn)
            {
                CommandType = CommandType.Text
            };

            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
            if (result == null || result == DBNull.Value) return default;
            return (T)Convert.ChangeType(result, typeof(T));
        }

        /// <summary>
        /// Executes a query and returns the result as a DataTable.
        /// </summary>
        public async Task<DataTable> ExecuteQueryAsync(string sql, params SqlParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));

            var table = new DataTable();

            await using var conn = await OpenConnectionAsync().ConfigureAwait(false);
            await using var cmd = new SqlCommand(sql, conn)
            {
                CommandType = CommandType.Text
            };

            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);
            try
            {
                await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                table.Load(reader);
            }
            catch (Exception ex)
            {
                // Log or handle exceptions as needed
                Console.WriteLine($"Error executing query: {ex.Message}");
                throw; // Re-throw after logging
            }
            return table;
        }

        /// <summary>
        /// Helper to quickly create a SqlParameter.
        /// </summary>
        public static SqlParameter Param(string name, object? value, SqlDbType? dbType = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            var p = new SqlParameter(name, value ?? DBNull.Value);
            if (dbType.HasValue) p.SqlDbType = dbType.Value;
            return p;
        }
    }
}
