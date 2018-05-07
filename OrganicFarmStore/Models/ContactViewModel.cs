using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OrganicFarmStore.Models
{
    public class ContactViewModel
    {
        [Required]
        public string Name { get; set; }
        [EmailAddress][Required]
        public string Email { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Message { get; set; }
    }
}
