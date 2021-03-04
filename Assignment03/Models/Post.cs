using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Assignment03.Models
{
    /*class is used to create post type object.*/
    public class Post
    {
        [Required(ErrorMessage = "Please enter title of post")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Please enter content of post")]
        public string Content { get; set; }
        public int ID { get; set; }
        public string isNew { get; set; }
        public string Date { get; set; }
        public string AuthorName { get; set; }
        public string AuthorEmail { get; set; }
        public string imagePath { get; set; }
    }
}
