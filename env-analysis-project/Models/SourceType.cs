using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace env_analysis_project.Models
{
    public class SourceType
    {
        [Key]
        public int SourceTypeID { get; set; }

        [Required, StringLength(100)]
        public string SourceTypeName { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Quan hệ 1-n với EmissionSource
        public ICollection<EmissionSource> EmissionSources { get; set; }
    }
}
