using System;
using Microsoft.EntityFrameworkCore;

namespace env_analysis_project.Models
{
    [Keyless]
    public sealed class LatestParameterMeasurementRecord
    {
        public int ResultID { get; set; }
        public string ParameterCode { get; set; } = string.Empty;
        public DateTime MeasurementDate { get; set; }
        public DateTime EntryDate { get; set; }
        public double? Value { get; set; }
        public string? Unit { get; set; }
        public int EmissionSourceID { get; set; }
        public string ParameterName { get; set; } = string.Empty;
        public double? StandardValue { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}
