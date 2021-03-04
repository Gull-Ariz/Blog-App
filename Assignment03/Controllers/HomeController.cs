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

using System.Web;

namespace Assignment03.Controllers
{
    public class HomeController : Controller
    {
        /*property is used get information about the webhosting environment*/
        private readonly IHostingEnvironment hostingEnvironment;
        public HomeController(IHostingEnvironment environment)
        {
            hostingEnvironment = environment;
        }
        /*method check the user is login or not if user logged in the get all the posts from database
         and redirect user to the Home page where user can show posts and update or delete his own
        posts. */
        [HttpGet]
        public ActionResult HomePage()
        {
            if(HttpContext.Session.Keys.Contains("user"))
            {
                User user = getLogedInUser();
                List<Post> postsList = new List<Post>();
                ViewBag.userEmail = user.Email;
                AdminServices adminServices = new AdminServices();
                postsList = adminServices.getAllPosts();
                if(postsList.Count > 1)
                {
                    /*these lines of codes helps to show last added post by user at top.*/
                    Post post = postsList.Last();
                    postsList.Remove(post);
                    postsList.Insert(0, post);
                }
                foreach(Post p in postsList)
                {
                    if(p.Title.Length > 50)
                    {
                        p.Title = p.Title.Substring(0, 49);
                    }
                    if(p.Content.Length > 500)
                    {
                        p.Content = p.Content.Substring(0, 499);
                    }
                }
                return View("HomePage", postsList);
            }
            else
            {
                return RedirectToAction("Login", "SignUp");
            }
        }
        /*this method is called when logged in user press update button from any of his post and
         it return update post page to user.*/
        [HttpPost]
        public ViewResult HomePage(string id)
        {
            AdminServices adminServices = new AdminServices();
            List<Post> postsList = adminServices.getAllPosts();
            Post post = postsList.Last();
            return View("UpdatePost",post);
        }
        /*this method is called when user click read more button of any post and it shows the full post to the user.*/
        public ActionResult ViewFullPost(string id)
        {
            AdminServices adminServices = new AdminServices();
            List<Post> postsList = adminServices.getAllPosts();
            int ID = System.Convert.ToInt32(id);
            Post post = postsList.Find(post => post.ID == ID);
            return View("ViewFullPost", post);
        }
        /*this method is called when admin press Add User button and it return AddUser View to admin.*/
        [HttpGet]
        public ActionResult AddUser()
        {
            if(HttpContext.Session.Keys.Contains("admin"))
            {
                return View("AddUser");
            }
            else
            {
                return RedirectToAction("Login", "SignUp");
            }
        }
        /*this method is called when admin press Add User button from AddUser view,
         it validates the User object properties and check the whether the account with this email is already
        exists or not if the account is created then it redirect the admin to Admin Panel. otherwise it shows 
        error messages in Add User page.*/
        [HttpPost]
        public ActionResult AddUser(User user)
        {
            if(HttpContext.Session.Keys.Contains("admin"))
            {
                if (ModelState.IsValid)
                {
                    if (user.Password.Equals(user.C_Password))
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
                            List<User> usersList = adminServices.getAllUsers();
                            return RedirectToAction("AdminPanel", "Home", usersList);
                        }
                        else if (p_key_vol == true)
                        {
                            ModelState.AddModelError(String.Empty, "Account with this Email already exists.");
                            return View("AddUser");
                        }
                        else
                        {
                            return View("AddUser");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(String.Empty, "Password and confirm password are not same.");
                        return View("AddUser");
                    }
                }
                else
                {
                    return View("AddUser");
                }
            }
            else
            {
                return RedirectToAction("Login", "SignUp");
            }
        }
        /*this method is called when admin press Delete User button from Admin panel, it delete the selected
         user and return admin to the admin panel.*/
        public ActionResult DeleteUser(string email)
        {
            if(HttpContext.Session.Keys.Contains("admin"))
            {
                AdminServices adminServices = new AdminServices();
                adminServices.deleteUser(email);
                List<User> usersList = adminServices.getAllUsers();
                return RedirectToAction("AdminPanel", "Home", usersList);
            }
            else
            {
                return RedirectToAction("Login", "SignUp");
            }
        }
        /*this method is called when admin click on update user button from admin panel, it get the selected 
         user email which is unique for every user and find the complete record of this user and send this data
        to the Updateuser view using ViewBag.*/
        [HttpGet]
        public ActionResult UpdateUser(string email)
        {
            if(HttpContext.Session.Keys.Contains("admin"))
            {
                AdminServices adminServices = new AdminServices();
                List<User> usersList = adminServices.getAllUsers();
                User user = usersList.Find(user => user.Email == email);
                ViewBag.userName = user.UserName;
                ViewBag.email = user.Email;
                ViewBag.password = user.Password;
                ViewBag.imagePath = user.imagePath;
                return View("UpdateUser", user);
            }
            else
            {
                return RedirectToAction("Login", "SignUp");
            }
        }
        /*method is called when Update User button from UpdateUser page it validates the User object
         properties, update the User record and return admin to Admin Panel.*/
        [HttpPost]
        public ActionResult UpdateUser(User user,string pwd,string email)
        {
            if(HttpContext.Session.Keys.Contains("admin"))
            {
                user.Password = pwd;
                AdminServices adminServices = new AdminServices();
                List<User> usersList = adminServices.getAllUsers();
                User u = usersList.Find(u => u.Email == user.Email);
                if (user.UserName != u.UserName || user.Email != u.UserName ||
                    user.Password != u.Password || user.ProfilePic != null)
                {
                    if (user.ProfilePic != null)
                    {
                        var uniqueFileName = GetUniqueFileName(user.ProfilePic.FileName);
                        var uploads = Path.Combine(hostingEnvironment.WebRootPath, "Images");
                        var filePath = Path.Combine(uploads, uniqueFileName);
                        user.ProfilePic.CopyTo(new FileStream(filePath, FileMode.Create));
                        user.imagePath = (string)uniqueFileName;
                    }
                    else
                    {
                        user.imagePath = u.imagePath;
                    }
                    adminServices.updateUser(user, u.Email);
                    usersList = adminServices.getAllUsers();
                    return RedirectToAction("AdminPanel", "Home", usersList);
                }
                else
                {
                    return RedirectToAction("AdminPanel", "Home", usersList);
                }
            }
            else
            {
                return RedirectToAction("Login", "SignUp");
            }
            
        }
        /*this method show the Admin Panel to admin where admin can show all users records and 
         add, update or delete any user record.*/
        public ViewResult AdminPanel()
        {
            AdminServices adminServices = new AdminServices();
            List<User> usersList = adminServices.getAllUsers();
            return View("AdminPanel",usersList);
        }
        /*this method clear the created session of logged in user and redirect user to the 
         login page.*/
        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login","SignUp");
        }
        /*method called when admin press logout button from admin panel.*/
        public ActionResult logoutAdmin()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "SignUp");
        }
        /*this method is called when user press Update Post button from any of his post he already posted.
         it redirect the user to update post page i user is logged in.*/
        [HttpGet]
        public ActionResult UpdatePost(string id)
        {
            if(HttpContext.Session.Keys.Contains("user"))
            {
                AdminServices adminServices = new AdminServices();
                List<Post> postsList = adminServices.getAllPosts();
                Post post = postsList.Find(post => post.ID == System.Convert.ToInt32(id));
                return View("UpdatePost",post);
            }
            else
            {
                return RedirectToAction("Login", "SignUp");
            }
        }
        /*method called when user click update button from update post page. it get post id and 
         post other properties and update in database and redirect the user to the home page.*/
        [HttpPost]
        public ActionResult UpdatePost(Post post, string id)
        {
            if (HttpContext.Session.Keys.Contains("user"))
            {
                int i = System.Convert.ToInt32(id);
                AdminServices adminServices = new AdminServices();
                adminServices.updatePost(post, i);
                List<Post> postsList = adminServices.getAllPosts();
                return RedirectToAction("HomePage", "Home", postsList);
            }
            else
            {
                return RedirectToAction("Login", "SignUp");
            }
        }
        /*method called when user click delete post button from any of the his post from home page
         it delete this specific post from the user view and record as well.*/
        [HttpPost]
        public ActionResult DeletePost()
        {
            if(HttpContext.Session.Keys.Contains("user"))
            {
                AdminServices adminServices = new AdminServices();
                List<Post> postsList = adminServices.getAllPosts();
                adminServices.deletePost(System.Convert.ToInt32(postsList.Last().ID.ToString()));
                postsList.Remove(postsList.Last());
                return RedirectToAction("HomePage", "Home", postsList);
            }
            else
            {
                return RedirectToAction("Login", "SignUp");
            }
        }
        /*method called when user click create post button from home page it show the create post page
         to user where user can write post title and content.*/
        [HttpGet]
        public ActionResult CreatePost()
        {
            if(HttpContext.Session.Keys.Contains("user"))
            {
                return View("~/Views/Home/CreatePost.cshtml");
            }
            else
            {
                return RedirectToAction("Login", "SignUp");
            }
        }
        /*method called when user click post button from create post page, method get current date and 
         set post created date and post author name, author email and author image path values are
        set with the logged in user username email and imagepath and redirect the user to the home page 
        where user can see his post with other posts.*/
        [HttpPost]
        public ActionResult CreatePost(string title, string content)
        {
            if (HttpContext.Session.Keys.Contains("user"))
            {
                if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(content))
                {
                    Post post = new Post();
                    post.Title = title;
                    post.Content = content;
                    string date = DateTime.Now.ToString("dd") + ",";
                    date += DateTime.Now.ToString("MMMM") + ",";
                    date += DateTime.Now.ToString("yyyy") + ",";
                    post.Date = date;
                    User user = getLogedInUser();
                    post.AuthorName = user.UserName;
                    post.AuthorEmail = user.Email;
                    post.imagePath = user.imagePath;
                    AdminServices adminServices = new AdminServices();
                    bool res = adminServices.AddPost(post);
                    List<Post> postsList = adminServices.getAllPosts();
                    if (res == true)
                    {
                        return RedirectToAction("HomePage", "Home", postsList);
                    }
                    else
                    {
                        return View("CreatePost");
                    }
                }
                else
                {
                    return View("CreatePost");
                }
            }
            else
            {
                return RedirectToAction("Login", "SignUp");
            }
        }
        /*method called when user click profile item from nav bar items it show the profile page with the
         info of current looged in user, method send data from controller to view using viewbag*/
        [HttpGet]
        public ActionResult Profile()
        {
            if(HttpContext.Session.Keys.Contains("user"))
            {
                User user = getLogedInUser();
                ViewBag.userName = user.UserName;
                ViewBag.email = user.Email;
                ViewBag.password = user.Password;
                ViewBag.imagePath = user.imagePath;
                return View();
            }
            else
            {
                return RedirectToAction("Login", "SignUp");
            }
        }
        /*method called when user click update button from Profile page it validate the User object properties
         and update the user record in user table and also user info in posts table as well like image path, 
        author name and author email.*/
        [HttpPost]
        public ActionResult Profile(User user, string newpwd, string oldpwd)
        {
            if(HttpContext.Session.Keys.Contains("user"))
            {
                bool isEmail = Regex.IsMatch(user.Email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
                User logInUser = getLogedInUser();
                AdminServices adminServices = new AdminServices();
                List<Post> postsList = adminServices.getAllPosts();
                string email = logInUser.Email;
                if (!string.IsNullOrEmpty(newpwd) || !user.UserName.Equals(logInUser.UserName) ||
                    !user.Email.Equals(logInUser.Email) || user.ProfilePic != null)
                {
                    if (!string.IsNullOrEmpty(newpwd) && logInUser.Password.Equals(oldpwd))
                    {
                        logInUser.Password = newpwd;
                    }
                    if (!user.UserName.Equals(logInUser.UserName))
                    {
                        logInUser.UserName = user.UserName;
                    }
                    if (!user.Email.Equals(logInUser.Email) && isEmail)
                    {
                        logInUser.Email = user.Email;
                    }
                    if (user.ProfilePic != null)
                    {
                        var uniqueFileName = GetUniqueFileName(user.ProfilePic.FileName);
                        var uploads = Path.Combine(hostingEnvironment.WebRootPath, "Images");
                        var filePath = Path.Combine(uploads, uniqueFileName);
                        user.ProfilePic.CopyTo(new FileStream(filePath, FileMode.Create));
                        logInUser.imagePath = (string)uniqueFileName;
                    }
                    if (isEmail)
                    {
                        adminServices.updateUser(logInUser, email);
                    }
                    postsList = adminServices.getAllPosts();
                    return RedirectToAction("HomePage", "Home", postsList);
                }
                else
                {
                    postsList = adminServices.getAllPosts();
                    return RedirectToAction("HomePage", "Home", postsList);
                }
            }
            else
            {
                return RedirectToAction("Login", "SignUp");
            }
        }
        /*method called when user select About item from nav bar items it shows user to some dummy data.*/
        public ActionResult About()
        {
            if(HttpContext.Session.Keys.Contains("user"))
            {
                return View("~/Views/Home/About.cshtml");
            }
            else
            {
                return RedirectToAction("Login", "SignUp");
            }
        }
        /*helper method use to get unique name of image file so if more then one user upload same file name image
         then it store on server with different and unique names.*/
        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                      + "_"
                      + Guid.NewGuid().ToString().Substring(0, 4)
                      + Path.GetExtension(fileName);
        }
        /*helper method use to get logged in user data we set session with email id of user which is unique
         with this method we can get complete user object.*/
        private User getLogedInUser()
        {
            string userEmail =  HttpContext.Session.GetString("user");
            AdminServices adminServices = new AdminServices();
            List<User> usersList = adminServices.getAllUsers();
            User user = usersList.Find(user => user.Email == userEmail);
            return user;
        }
    }
}
