using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SRRAMOils.Models;
using SRRAMOils.Service;

namespace SRRAMOils.Pages
{
    public class MakeVendorPaymentModel : PageModel
    {



        [BindProperty]
        public decimal PurchaseAmount { get; set; }

        [BindProperty]
        public decimal DueAmount { get; set; }

        [BindProperty]
        public string InvoiceNumber { get; set; } = string.Empty;
        public MakeVendorPaymentModel()
        {
            
        }
        public void OnGet()
        {
            if (Request.Query["vendorPurchaseId"].ToString() != "")
            {
                VendorService vs = new VendorService();
                var _vendorPaymentHistory  = vs.GetInvoiceDetailsByInvoiceId(int.Parse(Request.Query["vendorPurchaseId"].ToString()));
                PurchaseAmount = _vendorPaymentHistory.PurchaseAmount;
                InvoiceNumber = _vendorPaymentHistory.InvoiceId;
                if (_vendorPaymentHistory.Payments != null && _vendorPaymentHistory.Payments.Count > 0)
                {
                    var paidAmount = _vendorPaymentHistory.Payments.Sum(x => x.Amount);
                    DueAmount = (decimal)(PurchaseAmount - paidAmount);
                }
            }
        }
    }
}
