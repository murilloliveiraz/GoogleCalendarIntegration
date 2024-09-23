namespace IntegrationWithGoogleCalendarAPI.Models
{
    public class CreateAppointmentRequest
    {
        public Consulta Consulta { get; set; }
        public string AccessToken { get; set; }
    }
}
