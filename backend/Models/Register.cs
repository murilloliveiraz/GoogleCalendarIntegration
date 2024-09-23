using System.ComponentModel.DataAnnotations;

namespace IntegrationWithGoogleCalendarAPI.Models
{
    public class Register
    {
        public string UserName { get; set; }
        public string Role { get; set; }
        public string BirthDay { get; set; }
        public string Password { get; set; }
    }
}
