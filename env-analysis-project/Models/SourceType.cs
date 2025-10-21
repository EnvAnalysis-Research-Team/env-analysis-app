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
        // Make nullable and initialize to avoid model-binding validation error
        public ICollection<EmissionSource>? EmissionSources { get; set; } = new List<EmissionSource>();
    }
}
