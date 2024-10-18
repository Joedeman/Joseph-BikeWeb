using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace u23642425_HW02.Models
{
    public class Customer
    {
       
        public int customer_id { get; set; }
        [Required]
        [Display(Name = "First Name")]
        public string first_name { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string last_name { get; set; }

        [Display(Name = "Phone Number")]
        public int phone { get; set; }

        [EmailAddress]
        [Required]
        [Display(Name = "Email Address")]
        public string email { get; set; }

        [Display(Name = "Street Address")]
        public string street { get; set; }
        [Display(Name = "City")]
        public string city { get; set; }
        [Display(Name = "State")]
        public string state { get; set; }

        [Display(Name = "ZIP Code")]
        public int zipcode { get; set; }
        public int customertype_id { get; set; }
    }
}