using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OrganicFarmStore.Models
{
    public class SignInViewModel
    {

        [Required]
        public string Email { get; set; }
        [Required]
        [MinLength(5, ErrorMessage = "Your password must be atlest 5 characters")]
        public string Password { get; set; }
       
    }
}
