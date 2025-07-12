using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models
{
    public class Company
    {
        [Key]
       public int companyId {  get; set; }
        [Required]
        public string companyName { get; set; }
        public string companyStreetAddress { get; set; }
        public string companyCity { get; set; }
        public string companyState { get; set; }
        public string companyPostalCode { get; set; }
        public string companyphoneNumber { get; set; }
    }
}
