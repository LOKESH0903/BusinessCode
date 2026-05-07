using SRRAMOils.DBRepository;
using System.Data;

namespace SRRAMOils.Service
{
    public class OrganizationService
    {
        public OrganizationService()
        {

        }

        public async Task<DataTable> GetOrgianizations()
        {
            DataTable dataTable = new DataTable();
            DataBase db = new DataBase(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());
           await db.ExecuteQueryAsync("SELECT * FROM Organization").ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                     dataTable = task.Result;
                    // Process the dataTable as needed

                    foreach (DataRow row in dataTable.Rows)
                    {
                        var dict = new Dictionary<string, object>();
                        foreach (DataColumn col in dataTable.Columns)
                        {
                            dict[col.ColumnName] = row[col] ?? string.Empty;
                        }
                    }
                }
                else
                {
                    // Handle exceptions
                    Console.WriteLine($"Error fetching organizations: {task.Exception?.Message}");
                }
            });

            return dataTable;
        }
    }
}
