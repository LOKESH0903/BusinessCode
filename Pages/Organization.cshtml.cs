using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SRRAMOils.Service;
using System.Data;

namespace SRRAMOils.Pages
{
    public class OrganizationModel : PageModel
    {


        [BindProperty]
        public string OrganizationName { get; set; } = string.Empty;

        // Expose data to the Razor view: a list of rows, each row is a dictionary
        public List<Dictionary<string, object>> Organizations { get; set; } = new();

        public async Task OnGetAsync()
        {
            DataTable result = new DataTable();
            OrganizationService Org = new OrganizationService();
            result = await Org.GetOrgianizations();

            Organizations.Clear();

            if (result != null && result.Rows.Count > 0)
            {
                foreach (DataRow row in result.Rows)
                {
                    var dict = new Dictionary<string, object>();

                    foreach (DataColumn col in result.Columns)
                    {
                        object cell = row[col];
                        // Convert DBNull to empty string to avoid issues in the view
                        dict[col.ColumnName] = cell is DBNull ? string.Empty : cell!;
                    }

                    Organizations.Add(dict);
                }
            }

            Results.Ok(Organizations); // Return an empty list or modify as needed
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var orgName = OrganizationName;
            OrganizationService Org = new OrganizationService();
            if (string.IsNullOrWhiteSpace(orgName))
            {
                return BadRequest(new { success = false, error = "OrganizationName is required" });

            }

            if (!await Org.CheckOrgianization(orgName))
            {
                if(await Org.AddOrganization(orgName))
                {
                    return new JsonResult(new { success = true, name = OrganizationName });
                }
            }
            else
            {
                return BadRequest(new { success = false, error = "Organization Name already available." });
            }

           

            // TODO: save OrganizationName to database here (await db.SaveChangesAsync();)

            // return JSON that your JS expects
            return new JsonResult(new { success = true, name = OrganizationName });
        }
    } 

}
   
