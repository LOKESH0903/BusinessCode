namespace SRRAMOils.Models
{
    public class Organization
    {
        public int Id { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
