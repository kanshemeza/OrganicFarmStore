using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OrganicFarmStore.Models
{
    public class Checkout
    {
        [Required]
        [Display(Name = "Email")]
        public string ContactEmail { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        public string ContactPhoneNumber { get; set; }

        public Cart Cart { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string ShippingAddressLine1 { get; set; }

        [Required]
        [Display(Name = "Apartment/Unit/Suite")]
        public string ShippingAddressLine2 { get; set; }

        [Required]
        [Display(Name = "City")]
        public string ShippingLocale { get; set; }

        [Required]

        [Display(Name = "State")]
        public string ShippingRegion { get; set; }

        [Required]
        [Display(Name = "Country")]
        public string ShippingCountry { get; set; }

        [Required]
        [Display(Name = "Zip Code")]
        public string ShippingPostalCode { get; set; }

        [Required]
        [Display(Name = "Name on Card")]
        public string BillingNameOnCard { get; set; }


        //[CreditCard]  -- A little buggy!
        [Required]
        [Display(Name = "Credit Card Number")]
        [MaxLength(16)]
        public string BillingCardNumber { get; set; }

        [Required]
        [Display(Name = "Expiration Date")]
        [Range(1, 12)]
        public int BillingCardExpirationMonth { get; set; }

        [Required]
        public int BillingCardExpirationYear { get; set; }

        [Required]
        [Display(Name = "CVV/CVV2")]
        public string BillingCardVerificationValue { get; set; }

    }
}
