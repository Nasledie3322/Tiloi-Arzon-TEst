using System.ComponentModel.DataAnnotations;

namespace TiloiArzon.Client.Models
{
    public class UpdateProfileDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}