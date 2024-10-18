using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace u23642425_HW02.Models
{
    public class Login
    {
        [Required]
        [EmailAddress]
        public string Email { get; set;}
        [Required]
        public string ZipCode { get; set;}
    }
}