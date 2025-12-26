using Microsoft.AspNetCore.Identity;

namespace env_analysis_project.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Role { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
