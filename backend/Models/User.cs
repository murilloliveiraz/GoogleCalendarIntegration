using System.ComponentModel.DataAnnotations;

namespace IntegrationWithGoogleCalendarAPI.Models
{
    public class User
    {
        [Required]
        public string UserName { get; set; }
        public string Role { get; set; }
        public string BirthDay { get; set; }
        public byte[] PasswordHash { get; set; }
    }
}
