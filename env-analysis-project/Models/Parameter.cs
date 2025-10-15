using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace env_analysis_project.Models
{
    public class Parameter
    {
        [Key]
        [StringLength(50)]
        public string ParameterCode { get; set; }

        [Required, StringLength(100)]
        public string ParameterName { get; set; }

        [StringLength(50)]
        public string Unit { get; set; }

        public double? StandardValue { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Quan hệ 1-n với MeasurementResult
        public ICollection<MeasurementResult> MeasurementResults { get; set; }
    }
}
