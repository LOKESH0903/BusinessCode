using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SRRAMOils.Models;
using SRRAMOils.Service;

namespace SRRAMOils.Pages
{
    public class MakeVendorPaymentModel : PageModel
    {

        [BindProperty]
        public int VendorPurchaseId { get; set; }

        [BindProperty]
        public decimal PurchaseAmount { get; set; }

        [BindProperty]
        public decimal DueAmount { get; set; }

        [BindProperty]
        public string InvoiceNumber { get; set; } = string.Empty;

        [BindProperty]
        public decimal PayAmount { get; set; }

        [BindProperty]
        public int PayType { get; set; }

        [BindProperty]
        public string PaymentReference { get; set; } = string.Empty;

        [BindProperty]
        public DateTime PaymentDate { get; set; }

        [BindProperty]
        public string VendorName { get; set; } = string.Empty;

        [BindProperty]
        public string CityName { get; set; } = string.Empty;

        [BindProperty]
        public string OrderDate { get; set; } = string.Empty;
        public MakeVendorPaymentModel()
        {

        }
        public void OnGet()
        {
            if (Request.Query["vendorPurchaseId"].ToString() != "")
            {
                VendorService vs = new VendorService();
                VendorPurchaseId = int.Parse(Request.Query["vendorPurchaseId"].ToString());
                var _vendorPaymentHistory = vs.GetInvoiceDetailsByInvoiceId(VendorPurchaseId);
                PurchaseAmount = _vendorPaymentHistory.PurchaseAmount;
                InvoiceNumber = _vendorPaymentHistory.InvoiceId;
                VendorName = _vendorPaymentHistory.VendorName;
                CityName = _vendorPaymentHistory.CityName;
                OrderDate = _vendorPaymentHistory.OrderDate;
                if (_vendorPaymentHistory.Payments != null && _vendorPaymentHistory.Payments.Count > 0)
                {
                    var paidAmount = _vendorPaymentHistory.Payments.Sum(x => x.Amount);
                    DueAmount = (decimal)(PurchaseAmount - paidAmount);
                }
                else
                {
                    DueAmount = PurchaseAmount;
                }
            }
        }

        public IActionResult OnPost()
        {
            var _payAmount = PayAmount;
            var _payType = PayType;
            var _paymentReference = PaymentReference;   
            var _paymentDate = PaymentDate;
            var _dueAmount = DueAmount;

            var _iSPaymentDone = false;

            if(_dueAmount <= _payAmount)
            {
                _iSPaymentDone = true;
            }

            VendorService vs = new VendorService();
            var result = vs.VendorPayment(VendorPurchaseId, _payAmount, _payType, _paymentReference, _paymentDate, _iSPaymentDone);
            if(result)
            {
                return new JsonResult(new { success = true, name = "VendorName" });
            }

            return new JsonResult(new { success = false, name = "VendorName" });
        }
    }
}
