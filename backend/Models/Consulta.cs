using Google.Apis.Calendar.v3.Data;

namespace IntegrationWithGoogleCalendarAPI.Models
{
    public class Consulta
    {
        public string Summary { get; set; }
        public string Description { get; set; }
        public EventDateTime Start { get; set; }
        public EventDateTime End { get; set; }
        public ConferenceData? ConferenceData { get; set; }

        public Consulta()
        {
            this.Start = new EventDateTime()
            {
                TimeZone = "America/Sao_Paulo"
            };
            this.End = new EventDateTime()
            {
                TimeZone = "America/Sao_Paulo"
            };
        }
    }

    public class EventDateTime
    {
        public string DateTime { get; set; }
        public string TimeZone { get; set; }
    }
}