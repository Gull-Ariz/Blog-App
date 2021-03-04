using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment03.Models
{
    public class AdminServices
    {
        private string conString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Users;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        List<Post> postsList = new List<Post>();
        /*method is called from signup controller class it get user object as parameter and verify from database either
         the account is exists or not if account exists then it return true and user object tuple to the
        signup controllerclass
        */
        public (bool,User) loginUser(User user)
        {
            SqlConnection connection = new SqlConnection(conString);
            User user1 = new User();
            try
            {
                bool result = false;
                string query = $"select * from users where email = @u and password = @p";
                SqlParameter p1 = new SqlParameter("u", user.Email);
                SqlParameter p2 = new SqlParameter("p", user.Password);
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                sqlCommand.Parameters.Add(p1);
                sqlCommand.Parameters.Add(p2);
                connection.Open();
                SqlDataReader dataReader = sqlCommand.ExecuteReader();
                if (dataReader.Read())
                {
                    user1.UserName = (string) dataReader.GetValue(1);
                    user1.Email = (string)dataReader.GetValue(2);
                    user1.Password = (string)dataReader.GetValue(3);
                    user1.imagePath = (string)dataReader.GetValue(4);
                    result = true;
                }
                return (result,user1);
            }
            catch (Exception)
            {
                return (false,user1);
            }
            finally
            {
                connection.Close();
            }
        }
        /*method is called from signup controller class it get admin object as parameter and verify from database
         * either the admin user name and password are correct, if correct it return true to signup controller class
        */
        public bool loginAdmin(Admin admin)
        {
            SqlConnection connection = new SqlConnection(conString);
            try
            {
                bool result = false;
                string query = $"select * from admin where username = @u and password = @p";
                SqlParameter p1 = new SqlParameter("u", admin.UserName);
                SqlParameter p2 = new SqlParameter("p", admin.Password);
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                sqlCommand.Parameters.Add(p1);
                sqlCommand.Parameters.Add(p2);
                connection.Open();
                SqlDataReader dataReader = sqlCommand.ExecuteReader();
                if (dataReader.Read())
                {
                    result = true;
                }
                return result;
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                connection.Close();
            }
        }
        /*this method called from home controller class it get user object as parameter and add record of
         user into database return true if record is successfully added otherwise return tuple which false
        means record is not added successfully and primary key violation is true means account with this 
        email already exists.*/
        public (bool,bool) AddUser(User user)
        {
            SqlConnection connection = new SqlConnection(conString);
            bool p_key_vol = false;
            try
            {
                string query = $"insert into users (username,email,password,imagePath)" +
                $"  values(@un,@em,@pwd,@impath)";
                SqlParameter p2 = new SqlParameter("un", user.UserName);
                SqlParameter p3 = new SqlParameter("em", user.Email);
                SqlParameter p4 = new SqlParameter("pwd", user.Password);
                SqlParameter p5 = new SqlParameter("impath", System.Data.SqlDbType.VarChar);
                p5.Value = (object)user.imagePath ?? DBNull.Value;
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                sqlCommand.Parameters.Add(p2);
                sqlCommand.Parameters.Add(p3);
                sqlCommand.Parameters.Add(p4);
                sqlCommand.Parameters.Add(p5);
                connection.Open();
                int rowsInserter = sqlCommand.ExecuteNonQuery();
                if (rowsInserter > 0)
                {
                    return (true, p_key_vol);
                }
                else
                {
                    return (false, p_key_vol);
                }
            }
            catch (SqlException e) when (e.Number == 2627)
            {
                p_key_vol = true;
                return (false, p_key_vol);
            }
            catch (Exception e)
            {
                return (false, p_key_vol);
            }
            finally
            {
                connection.Close();
            }
        }
        /*method called from home controller to add post into data base it save the post into database and
         * return true if post is successfully added.*/
        public bool AddPost(Post post)
        {
            SqlConnection connection = new SqlConnection(conString);
            try
            {
                post.isNew = "f";
                string query = $"insert into Posts (title,content,date,author,author_email,imagePath,isNew)" +
                $"  values('{post.Title}','{post.Content}','{post.Date}','{post.AuthorName}','{post.AuthorEmail}','{post.imagePath}','{post.isNew}')";
                SqlParameter p1 = new SqlParameter("ti", post.Title);
                SqlParameter p2 = new SqlParameter("cn", post.Content);
                SqlParameter p3 = new SqlParameter("da", post.Date);
                SqlParameter p4 = new SqlParameter("au", post.AuthorName);
                SqlParameter p5 = new SqlParameter("au_em", post.AuthorEmail);
                SqlParameter p7 = new SqlParameter("isn", post.isNew);
                SqlParameter p6 = new SqlParameter("impath", System.Data.SqlDbType.VarChar);
                p5.Value = (object)post.imagePath ?? DBNull.Value;
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                sqlCommand.Parameters.Add(p2);
                sqlCommand.Parameters.Add(p3);
                sqlCommand.Parameters.Add(p4);
                sqlCommand.Parameters.Add(p5);
                connection.Open();
                int rowsInserter = sqlCommand.ExecuteNonQuery();
                if (rowsInserter > 0)
                {
                    postsList.Add(post);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                connection.Close();
            }
        }
        /*method is called from home controller it update the user record associated with email passed in parameter
         of method it also update the posts table column which are associated with respective user like post
        author name author email author image path.*/
        public bool updateUser(User user, string email)
        {
            SqlConnection connection = new SqlConnection(conString);
            connection.Open();
            SqlTransaction sqlTransaction = connection.BeginTransaction();
            try
            {
                bool result = false;
                string query = $"update users set " +
                    $"username = @un, email = @em, password = @pwd, imagePath = @impath" +
                    $" where email = @email";
                SqlParameter p1 = new SqlParameter("email", email);
                SqlParameter p2 = new SqlParameter("un", user.UserName);
                SqlParameter p3 = new SqlParameter("em", user.Email);
                SqlParameter p4 = new SqlParameter("pwd", user.Password);
                SqlParameter p5 = new SqlParameter("impath", user.imagePath);
                SqlCommand sqlCommand = new SqlCommand(query, connection, sqlTransaction);
                sqlCommand.Parameters.Add(p1);
                sqlCommand.Parameters.Add(p2);
                sqlCommand.Parameters.Add(p3);
                sqlCommand.Parameters.Add(p4);
                sqlCommand.Parameters.Add(p5);
                int insertedRows = sqlCommand.ExecuteNonQuery();
                if (insertedRows >= 1)
                {
                    string query2 = $"update Posts set author = @authr, author_email = @a_email, imagePath = @imgPath where author_email = @Email";
                    sqlCommand = new SqlCommand(query2, connection, sqlTransaction);
                    SqlParameter p6 = new SqlParameter("Email", email);
                    SqlParameter p7 = new SqlParameter("imgPath", user.imagePath);
                    SqlParameter p8 = new SqlParameter("a_email", user.Email);
                    SqlParameter p9 = new SqlParameter("authr", user.UserName);
                    sqlCommand.Parameters.Add(p6);
                    sqlCommand.Parameters.Add(p7);
                    sqlCommand.Parameters.Add(p8);
                    sqlCommand.Parameters.Add(p9);
                    int updatedRows = sqlCommand.ExecuteNonQuery();
                    if(updatedRows >= 1)
                    {
                        sqlTransaction.Commit();
                        result = true;
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                sqlTransaction.Rollback();
                return false;
            }
            finally
            {
                connection.Close();
            }
        }
        /*this method is called from home controller and update the post content in table and return true if
         record is successfully updated.*/
        public bool updatePost(Post post, int id)
        {
            SqlConnection connection = new SqlConnection(conString);
            try
            {
                bool result = false;
                string query = $"update Posts set " +
                    $"title = @ti, content = @cn where ID = @id";
                SqlParameter p1 = new SqlParameter("ti", post.Title);
                SqlParameter p2 = new SqlParameter("cn", post.Content);
                SqlParameter p3 = new SqlParameter("id", id);
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                sqlCommand.Parameters.Add(p1);
                sqlCommand.Parameters.Add(p2);
                sqlCommand.Parameters.Add(p3);
                connection.Open();
                int insertedRows = sqlCommand.ExecuteNonQuery();
                if (insertedRows >= 1)
                {
                    result = true;
                }
                return result;
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                connection.Close();
            }
        }
        /*this method delete the post from the database that has a specific id which method recives in parameter.*/
        public bool deletePost(int id)
        {
            SqlConnection connection = new SqlConnection(conString);
            try
            {
                bool result = false;
                string query = $"delete from Posts where ID = @id";
                SqlParameter p3 = new SqlParameter("id", id);
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                sqlCommand.Parameters.Add(p3);
                connection.Open();
                int insertedRows = sqlCommand.ExecuteNonQuery();
                if (insertedRows >= 1)
                {
                    result = true;
                }
                return result;
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                connection.Close();
            }
        }
        /*this method delete the user account from the record and return true if record is successfully deleted.*/
        public bool deleteUser(string email)
        {
            SqlConnection connection = new SqlConnection(conString);
            try
            {
                bool result = false;
                string query = $"delete from users where Email = @email";
                SqlParameter p1 = new SqlParameter("email", email);
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                sqlCommand.Parameters.Add(p1);
                connection.Open();
                int insertedRows = sqlCommand.ExecuteNonQuery();
                if (insertedRows >= 1)
                {
                    result = true;
                }
                return result;
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                connection.Close();
            }
        }
        /*this method get all the posts from database and return the list of all the posts.*/
        public List<Post> getAllPosts()
        {
            SqlConnection connection = new SqlConnection(conString);
            try
            {
                
                string query = $"select * from Posts";
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader dataReader = sqlCommand.ExecuteReader();
                while (dataReader.Read())
                {
                    Post post = new Post();
                    post.isNew = (string)dataReader.GetValue(7);
                    post.ID = (int)dataReader.GetValue(0);
                    post.Title = (string)dataReader.GetValue(1);
                    post.Content = (string)dataReader.GetValue(2);
                    post.Date = (string)dataReader.GetValue(3);
                    post.AuthorName = (string)dataReader.GetValue(4);
                    post.AuthorEmail = (string)dataReader.GetValue(5);
                    post.imagePath = (string)dataReader.GetValue(6);
                    postsList.Add(post);
                }
            }
            catch (Exception e)
            {
                return postsList;
            }
            finally
            {
                connection.Close();
            }
            return postsList;
        }
        /*method is used to get all the users list from the database.*/
        public List<User> getAllUsers()
        {
            List<User> usersList = new List<User>();
            SqlConnection connection = new SqlConnection(conString);
            try
            {
                string query = $"select * from users";
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader dataReader = sqlCommand.ExecuteReader();
                while (dataReader.Read())
                {
                    User user = new User();
                    user.UserName = (string)dataReader.GetValue(1);
                    user.Email = (string)dataReader.GetValue(2);
                    user.Password = (string)dataReader.GetValue(3);
                    user.imagePath = (string)dataReader.GetValue(4);
                    usersList.Add(user);
                }
            }
            catch (Exception e)
            {
                return usersList;
            }
            finally
            {
                connection.Close();
            }
            return usersList;
        }
    }
}
