using System;
using System.Collections.Generic;

namespace Barbora.Core.Models
{
    public class DeliveriesResponse
    {
        public IList<Delivery> deliveries { get; set; }
        public int reservationValidForSeconds { get; set; }
        public Message messages { get; set; }
    }

    public class Delivery
    {
        public string title { get; set; }

        public DeliveryParams @params { get; set; }
    }

    public class DeliveryParams
    {
        public IList<DeliveryMatrix> matrix { get; set; }
    }

    public class DeliveryMatrix
    {
        public string id { get; set; }
        public bool isExpressDelivery { get; set; }
        public string day { get; set; }
        public string dayShort { get; set; }
        public IList<DeliveryHour> hours { get; set; }
    }

    public class DeliveryHour
    {
        public string id { get; set; }
        public DateTime deliveryTime { get; set; }
        public string hour { get; set; }
        public decimal price { get; set; }
        public bool available { get; set; }
        public bool isUnavailableAlcSellingTime { get; set; }
        public bool isUnavailableAlcOrEnergySelling { get; set; }
        public bool isLockerOrPup { get; set; }
        public decimal salesCoefficient { get; set; }
        public string deliveryWave { get; set; }
        public int pickingHour { get; set; }
        public object changeTimeslotShop { get; set; }
    }
}