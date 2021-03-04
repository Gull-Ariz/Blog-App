using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Assignment03.Models
{
    /*this class is used to create the object of admin with properties of user name and password.*/
    public class Admin
    {
        [Required (ErrorMessage = "Please Enter User Name.")]
        [StringLength(50,ErrorMessage ="Max user name length is 50.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please enter Password")]
        [StringLength(20, ErrorMessage = "max password length is 20")]
        public string Password { get; set; }
    }
}
