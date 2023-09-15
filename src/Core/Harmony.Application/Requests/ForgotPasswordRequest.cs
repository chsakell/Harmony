using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Requests
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}