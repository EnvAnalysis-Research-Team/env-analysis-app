using System.ComponentModel.DataAnnotations;

namespace env_analysis_project.Options
{
    public sealed class JwtOptions
    {
        [Required]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        [Range(5, 1440)]
        public int ExpirationMinutes { get; set; } = 120;
    }
}
