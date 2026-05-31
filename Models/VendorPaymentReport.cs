namespace SRRAMOils.Models
{
    public class VendorPaymentReport
    {
        public VendorPaymentReport()
        {
                
        }

        public string VendorName { get; set; } = string.Empty;

        public string InvoiceNumber { get; set; } = string.Empty;

        public string OrderDate { get; set; } = string.Empty;

        public decimal PurchaseAmount { get; set; }

        public bool ISPaymentDone { get; set; }

        public decimal PaidAmount { get; set; }

        public string PaymentDate { get; set; } = string.Empty;

        public string PaymentReferenceNumber { get; set; }  = string.Empty;
        public string BillPaymentMode { get; set; }= string.Empty;

        public int TotalPurchaseDays { get; set; }
    }
}
