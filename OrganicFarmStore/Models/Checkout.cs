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
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required]
        [Display(Name = "Credit Card Number")]
        public string CreditCardNumber { get; set; }
        [Required]
        [Display(Name = "Expiration Date")]
        public DateTime ExpirationDate { get; set; }
        [Required]
        [Display(Name = "Billing Address")]
        public string BillingAddress { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Email { get; set; }
    }
}
