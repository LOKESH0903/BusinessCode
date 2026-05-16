using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SRRAMOils.Models;
using SRRAMOils.Service;
using System.CodeDom;

namespace SRRAMOils.Pages
{
    public class VendorPurchaseReportModel : PageModel
    {
        public List<SelectListItem> VendorOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> InvoiceNumbers { get; set; } = new List<SelectListItem>();


        public List<DropDownModel> VendorNames { get; set; } = new();

        public List<DropDownModel> InvoiceNumberList { get; set; } = new();

        [BindProperty]
        public string VendorName { get; set; } = string.Empty;

        [BindProperty]
        public int VendorId { get; set; }

        [BindProperty]
        public string InvoiceNumber { get; set; } = string.Empty;

        public VendorPurchaseReportModel()
        {
            
        }

        public async Task OnGetAsync()
        {
            VendorService vs = new VendorService();

            // Await the async call and assign the result directly to VendorNames
            VendorNames = await vs.GetVendorNames();

            // Start with a placeholder "select" option
            var options = new List<SelectListItem>
            {
                new SelectListItem { Value = string.Empty, Text = "-- Select Vendor --", Selected = true }
            };

            // Map VendorNames to SelectListItems using reflection to support multiple DropDownModel shapes
            var mapped = VendorNames
                .Select(v =>
                {
                    var type = v?.GetType();
                    string value = type?.GetProperty("Value")?.GetValue(v)?.ToString()
                                   ?? type?.GetProperty("Id")?.GetValue(v)?.ToString()
                                   ?? string.Empty;
                    string text = type?.GetProperty("Text")?.GetValue(v)?.ToString()
                                  ?? type?.GetProperty("Name")?.GetValue(v)?.ToString()
                                  ?? value;
                    return new SelectListItem { Value = value, Text = text };
                })
                .ToList();

            options.AddRange(mapped);

            VendorOptions = options;
        }

        public async Task OnGetInvoiceAsync()
        {
            VendorService vs = new VendorService();
            // Fetch invoice numbers based on the selected vendor
            InvoiceNumberList = await vs.GetInvoiceNumbersByVendor(VendorId);
            // Start with a placeholder "select" option
            var options = new List<SelectListItem>
                {
                    new SelectListItem { Value = string.Empty, Text = "-- Select Invoice Number --", Selected = true }
                };

            // Map InvoiceNumberList to SelectListItems
            var mapped = InvoiceNumberList
                .Select(i =>
                {
                    var type = i?.GetType();
                    string value = type?.GetProperty("Value")?.GetValue(i)?.ToString()
                                ?? type?.GetProperty("Id")?.GetValue(i)?.ToString()
                                ?? string.Empty;
                    string text = type?.GetProperty("Text")?.GetValue(i)?.ToString()
                                ?? type?.GetProperty("Name")?.GetValue(i)?.ToString()
                                ?? value;
                    return new SelectListItem { Value = value, Text = text };
                })
                .ToList();

            options.AddRange(mapped);
            InvoiceNumbers = options;

        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Handle form submission logic here, such as saving the selected vendor and invoice number
            // You can access VendorName and InvoiceNumber properties which are bound to the form inputs
            // For example, you might want to save this information to a database or perform some processing
            // After processing, you can redirect to another page or return a result
            return RedirectToPage("/VendorPurchaseReport");
        }
    }
}
