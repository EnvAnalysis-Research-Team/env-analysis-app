using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace env_analysis_project.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; } 

        [Required]
        public string PasswordHash { get; set; } 

        [StringLength(100)]
        public string FullName { get; set; }    

        [EmailAddress, StringLength(100)]
        public string Email { get; set; }

        [StringLength(50)]
        public string Role { get; set; }

        public bool IsActive { get; set; }

        public DateTime? LastLogin { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
