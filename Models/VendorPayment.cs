namespace SRRAMOils.Models
{
    public class VendorPayment
    {
        public VendorPayment()
        {
            
        }

        public string PaymentDate { get; set; } = string.Empty;
        public string PaymentModeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentReferenceNumber { get; set; } = string.Empty;
    }


    public class VendorPaymentHistory
    {
        public VendorPaymentHistory()
        {
            Payments = new List<VendorPayment>();
        }
        public decimal PurchaseAmount { get; set; } 
        
        public decimal PaidAmount { get; set; }

        public decimal BalanceAmount { get; set; }

        public string PurchaseDate { get; set; } = string.Empty;

        public string InvoiceId { get; set; } = string.Empty;

        public string VendorName { get; set; } = string.Empty;

        public string CityName { get; set; } = string.Empty;

        public string OrderDate { get; set; } = string.Empty;

        public List<VendorPayment> Payments { get; set; } 
    }
}
