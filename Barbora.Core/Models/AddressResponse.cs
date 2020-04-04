using System.Collections.Generic;

namespace Barbora.Core.Models
{
    public class AddressResponse
    {
        public IList<Address> address { get; set; }
        public IList<County> counties { get; set; }
        public IList<object> company { get; set; }
    }

    public class Address
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string post_code { get; set; }
        public string city_name { get; set; }
        public bool isCnC { get; set; }
        public bool isPickupPoint { get; set; }
        public string id { get; set; }
        public string phone { get; set; }
        public int city_id { get; set; }
        public string address { get; set; }
        public string fullAddress { get; set; }
        public string address_name { get; set; }
        public string flat { get; set; }
        public string floor { get; set; }
        public string doorCode { get; set; }
        public string houseEntranceNo { get; set; }
        public int addressType { get; set; }
        public bool declinedToAddAddressDetails { get; set; }
        public string comment { get; set; }
        public int deliveryMethodType { get; set; }
    }

    public class County
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public int SequenceNo { get; set; }
        public bool IsEnabled { get; set; }
        public IList<CountyOption> CountyOptions { get; set; }
    }

    public class CountyOption
    {
        public long Id { get; set; }
        public long CountyId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }
}