namespace Barbora.Core.Models
{
    public class AvailableTimeInfo
    {
        public string DayId { get; set; }
        public string Day { get; set; }

        public string HourId { get; set; }
        public string Hour { get; set; }

        public decimal Price { get; set; }

        public bool IsExpressDelivery { get; set; }
    }
}