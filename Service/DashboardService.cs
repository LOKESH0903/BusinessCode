using SRRAMOils.Models;
using System.Data;
using System.Data.SqlClient;

namespace SRRAMOils.Service
{
    public class DashboardService
    {
        public DashboardService()
        {

        }

        public List<VendorPaymentReport> GetVendorPurchasePaymentPendingDetails()
        {
            var paymentReports = new List<VendorPaymentReport>();
            try
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .Build();
                var connectionString = configuration.GetConnectionString("DevConnection")
                                       ?? configuration["ConnectionStrings:DefaultConnection"]
                                       ?? configuration["ConnectionString"]
                                       ?? configuration["ConnectionStrings:Connection"];
                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new InvalidOperationException("Database connection string not found in configuration.");
                using var connection = new SqlConnection(connectionString);
                connection.Open();
                using var command = connection.CreateCommand();

                command.CommandText = @"SELECT  V.VendorName, VP.InvoiceNumber, VP.OrderDate, VP.Amount AS PurchaseAmount, 
                                                SUM(ISNULL(VPT.Amount,0)) AS PaidAmount, DATEDIFF ( DAY , VP.OrderDate , GETDATE() ) AS TotalDays
                                        FROM Vendor V INNER JOIN VendorPurchase VP ON V.Id = VP.VendorId	
                                                      LEFT JOIN VendorPayment VPT ON VP.Id = VPT.VendorPurchaseId
                                        WHERE ISNULL(VP.ISPaymentDone, 0) = 0
                                        GROUP BY V.VendorName,VP.InvoiceNumber, VP.OrderDate, VP.Amount  order by V.VendorName";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    paymentReports.Add(new VendorPaymentReport
                    {
                        VendorName = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                        InvoiceNumber = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        OrderDate = reader.IsDBNull(2) ? string.Empty : reader.GetDateTime(2).ToString("dd MMMM yyyy"),
                        PurchaseAmount = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
                        PaidAmount = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4),
                        TotalPurchaseDays = reader.IsDBNull(5) ? 0 : reader.GetInt32(5)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving vendor payment report: {ex.Message}");
            }

            return paymentReports;
        }
    }
}
