using System.Collections.Generic;

namespace Barbora.Core.Models
{
    public class ChangeDeliveryAddressResponse
    {
        public Cart cart { get; set; }
    }

    public class Cart
    {
        public string id { get; set; }
        public string name { get; set; }
        public string couponCode { get; set; }
        public string deliveryComment { get; set; }
        public string address_id { get; set; }
        public int addressDeliveryMethodType { get; set; }
        public string address { get; set; }
        public bool declined_to_add_address_details { get; set; }
        public int packagingType { get; set; }
        public string house_entrance_no { get; set; }
        public string floor { get; set; }
        public string door_code { get; set; }
        public string shopCodeForDelivery { get; set; }
        public object GiftsForBasket { get; set; }
        public bool is_allowed { get; set; }
        public bool is_adult { get; set; }
        public int reservationValidForSeconds { get; set; }
        public object lastTimeProductAddedToCart { get; set; }
        public object show_delivery_destination_selection_window { get; set; }
        public bool hasOrderInUpdateMode { get; set; }
        public object reservationDateTime { get; set; }
        public object showTermsAndConditionsCheckbox { get; set; }
        public CompanyAddress companyAddress { get; set; }
        public bool unattendedDelivery { get; set; }
        public bool isAllProductsInBasketForDonations { get; set; }

        public Message messages { get; set; }

        public IList<object> ordersForAddition { get; set; }
        public IList<Slice> slices { get; set; }
    }

    public class Slice
    {
        public string id { get; set; }
        public string vendor_title { get; set; }
        public decimal discount { get; set; }
        public decimal min_price { get; set; }
        public decimal min_free_delivery { get; set; }
        public decimal usable_points { get; set; }
        public decimal points_granted { get; set; }
        public string page_policy { get; set; }
        public string page_delivery { get; set; }

        public IList<object> products { get; set; }
        public IList<object> prices { get; set; }
    }

    public class CompanyAddress
    {
        public bool active { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string VATCode { get; set; }
        public string address { get; set; }
        public bool requestFromClient { get; set; }
    }
}