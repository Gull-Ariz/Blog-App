using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Assignment03.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Assignment03.Controllers
{
    public class SignUpController : Controller
    {
        /*property is used get information about the webhosting environment*/
        private readonly IHostingEnvironment hostingEnvironment;
        public SignUpController(IHostingEnvironment environment)
        {
            hostingEnvironment = environment;
        }
        /*method show user sign up page*/
        [HttpGet]
        public ViewResult Index()
        {
            return View("Signup", new User());
        }
        /*method called when user user click SignUp butthon from SignUp page, it validats the User object
         properties and show error messages if entered values are not correct, if entered values are correct
        it creates user account and redirect it to the home page.*/
        [HttpPost]
        public IActionResult Index(User user )
        {
            if(ModelState.IsValid)
            {
                if(user.Password.Equals(user.C_Password))
                {
                    if (user.ProfilePic != null)
                    {
                        var uniqueFileName = GetUniqueFileName(user.ProfilePic.FileName);
                        var uploads = Path.Combine(hostingEnvironment.WebRootPath, "Images");
                        var filePath = Path.Combine(uploads, uniqueFileName);
                        user.ProfilePic.CopyTo(new FileStream(filePath, FileMode.Create));
                        user.imagePath = (string)uniqueFileName;
                    }
                    AdminServices adminServices = new AdminServices();
                    (bool result, bool p_key_vol) = adminServices.AddUser(user);
                    if (result == true && p_key_vol != true)
                    {
                        return RedirectToAction("HomePage", "Home");
                    }
                    else if (p_key_vol == true)
                    {
                        ModelState.AddModelError(String.Empty, "Account with this Email already exists.");
                        return View("Signup");
                    }
                    else
                    {
                        return View("Signup");
                    }
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "Password and confirm password are not same.");
                    return View("Signup");
                }
            }
            else
            {
                return View("Signup");
            }
        }
        /*helper method use to get unique name of image file so if more then one user upload same file name
         * image then it store on server with different and unique names.*/
        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                      + "_"
                      + Guid.NewGuid().ToString().Substring(0, 4)
                      + Path.GetExtension(fileName);
        }
        /*method show the login page to user.*/
        [HttpGet]
        public ViewResult Login()
        {
            HttpContext.Session.Clear();
            return View("Login");
        }
        /*method called when user click login button from login page, it validates email and password fields
         and calls adminservices class loginUser method which validates the record of user from database 
        if account exists it return true and user redirects to the home page.*/
        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                bool isEmail = Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
                if (isEmail)
                {
                    User user = new User { Email = email, Password = password };
                    user.ToString();
                    AdminServices adminServices = new AdminServices();
                    (bool res , User user1) = adminServices.loginUser(user);
                    if(res == true)
                    {
                        HttpContext.Session.SetString("user",user1.Email);
                        return RedirectToAction("HomePage", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError(String.Empty, "Account with this Email not exists.");
                        return View("Login");
                    }
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "Email and password is invalid");
                    return View("Login");
                }
            }
            else
            {
                ModelState.AddModelError(String.Empty, "Please enter email and password.");
                return View("Login");
            }
        }
        /*method show admin login page.*/
        [HttpGet]
        public ViewResult LoginAdmin()
        {
            HttpContext.Session.Clear();
            return View("AdminLogin");
        }
        /*method called when user click login button from admin login page, it validates user name and password fields
         and calls adminservices class loginAdmin method which validates the record of admin from database 
        if account exists it return true and admin redirects to the admin panel.*/
        [HttpPost]
        public ActionResult LoginAdmin(Admin admin)
        {
            if (ModelState.IsValid)
            {
                AdminServices adminServices = new AdminServices();
                bool res = adminServices.loginAdmin(admin);
                if (res == true)
                {
                    HttpContext.Session.SetString("admin", admin.UserName+"?"+admin.Password);
                    return RedirectToAction("AdminPanel", "Home");
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "Invalid User Name or Password.");
                    return View("AdminLogin");
                }
            }
            else
            {
                ModelState.AddModelError(String.Empty, "Email and password is invalid");
                return View("AdminLogin");
            }
        }
    }
}
