using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Assignment03.Models
{
    /*class is used to create user object.*/
    public class User
    {
        [Required(ErrorMessage = "Please enter name")]
        [StringLength(50,ErrorMessage ="max user name length is 50.")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Please enter email")]
        [EmailAddress]
        public string Email{ get; set; }
        [Required(ErrorMessage = "Please enter Password")]
        [StringLength(20, ErrorMessage = "max password length is 20")]
        public string Password { get; set; }
        [Required(ErrorMessage ="Please enter confirm password")]
        [Compare("Password", ErrorMessage = "Password and confirm password are not same")]
        public string C_Password { get; set; }   
        public IFormFile ProfilePic { get; set; }
        public string imagePath { get; set; }
    }
}
