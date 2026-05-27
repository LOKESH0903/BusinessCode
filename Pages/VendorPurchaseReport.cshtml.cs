using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SRRAMOils.Models;
using SRRAMOils.Service;
using System.CodeDom;
using Newtonsoft.Json;
using ClosedXML.Excel;
using System.IO;
using System.Globalization;
using PdfSharpCore.Pdf;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

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

        [BindProperty]
        public List<VendorPaymentReport> VendorPaymentReports { get; set; } = new List<VendorPaymentReport>();

        public VendorPurchaseReportModel()
        {
        }

        /*
         PSEUDOCODE / DETAILED PLAN
         1. Add two new GET handlers to export invoice data for a given vendor:
            - OnGetDownloadExcel(int vendorid) => returns an .xlsx file
            - OnGetDownloadPdf(int vendorid) => returns a .pdf file
         2. Each handler will:
            - Instantiate VendorService and retrieve invoice list via GetInvoiceNumbersByVendor(vendorid)
            - Build a filename that includes vendor id/name and timestamp
         3. Excel export (ClosedXML)
            - Create a new XLWorkbook and worksheet
            - Add header row (Id/Value/Text) - use reflection to read common properties from DropDownModel
            - Iterate invoice list and populate rows with the chosen properties
            - Auto-fit columns and save to MemoryStream
            - Return File(streamBytes, contentType, filename)
         4. PDF export (MigraDoc + PdfSharpCore)
            - Create MigraDoc Document, add a section and heading
            - Create a table and add columns matching the Excel columns
            - Add a formatted header row and populate rows from invoice list using same reflection
            - Render document to PDF in a MemoryStream via PdfDocumentRenderer
            - Return File(streamBytes, "application/pdf", filename)
         5. Keep code defensive: handle empty lists gracefully; always dispose streams/workbooks/renderers.
         6. These handlers are normal Razor Page GET handlers and will be invoked via query ?handler=DownloadExcel&vendorid=123 etc.
        */

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

            ViewData["SelectedVendorName"] = string.Empty; // Clear any previously selected vendor name
        }

        public void OnPost()
        {
            var _vendorId = VendorId;
            if (_vendorId == 0)
            {
                ModelState.AddModelError(string.Empty, "Please select a vendor.");
            }

            VendorService vs = new VendorService();
            VendorPaymentReports = vs.GetVendorPaymentReportByVendorId(_vendorId);
            
            ViewData["SelectedVendorName"] = VendorPaymentReports.Select(x => x.VendorName).FirstOrDefault(); // Store selected vendor ID for use in the Razor view
            //return RedirectToPage("/VendorPurchaseReport");
        }

        public JsonResult OnGetInvoiceByVendorId(int vendorid)
        {
            VendorService vs = new VendorService();
            InvoiceNumberList = vs.GetInvoiceNumbersByVendor(vendorid);
            string json = JsonConvert.SerializeObject(InvoiceNumberList);
            var data = new { data = json };
            return new JsonResult(data);
        }

        public JsonResult OnGetClosedInvoiceByVendorId(int vendorid)
        {
            VendorService vs = new VendorService();
            InvoiceNumberList = vs.GetClosedInvoiceNumbersByVendor(vendorid);
            string json = JsonConvert.SerializeObject(InvoiceNumberList);
            var data = new { data = json };
            return new JsonResult(data);
        }

        // GET handler: /VendorPurchaseReport?handler=DownloadExcel&vendorid=123
        public IActionResult OnGetDownloadExcel(int vendorid)
        {
            VendorService vs = new VendorService();
            InvoiceNumberList = vs.GetInvoiceNumbersByVendor(vendorid) ?? new List<DropDownModel>();

            // Create filename
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            var filename = $"Vendor_{vendorid}_Invoices_{timestamp}.xlsx";

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Invoices");

            // Define headers (use reflection to adapt to DropDownModel shape)
            var headers = new[] { "Id", "Value", "Text" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
            }

            // Populate rows
            int row = 2;
            foreach (var item in InvoiceNumberList)
            {
                var type = item?.GetType();
                var idVal = type?.GetProperty("Id")?.GetValue(item)?.ToString() ?? string.Empty;
                var valueVal = type?.GetProperty("Value")?.GetValue(item)?.ToString() ?? string.Empty;
                var textVal = type?.GetProperty("Text")?.GetValue(item)?.ToString()
                              ?? type?.GetProperty("Name")?.GetValue(item)?.ToString()
                              ?? string.Empty;

                ws.Cell(row, 1).Value = idVal;
                ws.Cell(row, 2).Value = valueVal;
                ws.Cell(row, 3).Value = textVal;
                row++;
            }

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            var bytes = ms.ToArray();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
        }

        // GET handler: /VendorPurchaseReport?handler=DownloadPdf&vendorid=123
        public IActionResult OnGetDownloadPdf(int vendorid)
        {
            VendorService vs = new VendorService();
            InvoiceNumberList = vs.GetInvoiceNumbersByVendor(vendorid) ?? new List<DropDownModel>();

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            var filename = $"Vendor_{vendorid}_Invoices_{timestamp}.pdf";

            // Build MigraDoc document
            var document = new Document();
            document.Info.Title = $"Vendor {vendorid} Invoices";
            var section = document.AddSection();

            var heading = section.AddParagraph($"Vendor {vendorid} - Invoices");
            heading.Format.Font.Size = 14;
            heading.Format.Font.Bold = true;
            heading.Format.SpaceAfter = "0.25cm";

            // Create table with 3 columns
            var table = section.AddTable();
            table.Style = "Table";
            table.Borders.Width = 0.75;

            var col1 = table.AddColumn(Unit.FromCentimeter(3));
            var col2 = table.AddColumn(Unit.FromCentimeter(6));
            var col3 = table.AddColumn(Unit.FromCentimeter(8));

            // Header row
            var headerRow = table.AddRow();
            headerRow.Shading.Color = Colors.LightGray;
            headerRow.Format.Font.Bold = true;
            headerRow.Cells[0].AddParagraph("Id");
            headerRow.Cells[1].AddParagraph("Value");
            headerRow.Cells[2].AddParagraph("Text");

            // Data rows
            foreach (var item in InvoiceNumberList)
            {
                var type = item?.GetType();
                var idVal = type?.GetProperty("Id")?.GetValue(item)?.ToString() ?? string.Empty;
                var valueVal = type?.GetProperty("Value")?.GetValue(item)?.ToString() ?? string.Empty;
                var textVal = type?.GetProperty("Text")?.GetValue(item)?.ToString()
                              ?? type?.GetProperty("Name")?.GetValue(item)?.ToString()
                              ?? string.Empty;

                var row = table.AddRow();
                row.Cells[0].AddParagraph(idVal);
                row.Cells[1].AddParagraph(valueVal);
                row.Cells[2].AddParagraph(textVal);
            }

            // Render to PDF
            var renderer = new PdfDocumentRenderer(unicode: true)
            {
                Document = document
            };
            renderer.RenderDocument();

            using var ms = new MemoryStream();
            renderer.PdfDocument.Save(ms);
            var bytes = ms.ToArray();

            return File(bytes, "application/pdf", filename);
        }
    }
}
