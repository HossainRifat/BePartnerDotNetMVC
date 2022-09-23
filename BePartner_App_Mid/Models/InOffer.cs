using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BePartner_App_Mid.Models
{
    public class InOffer
    {
        [Required]
        public string En_Email { get; set; }
        public string In_Email { get; set; }
        [Required]
        public string Share { get; set; }
        [Required]
        public string Value { get; set; }
        public string Details { get; set; }
    }
}