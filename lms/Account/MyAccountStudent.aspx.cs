using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace lms.Account
{
    public partial class MyAccountStudent : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["LoggedInUserEmail"] == null)
                {
                    Response.Redirect("~/Account/Login.aspx");
                }
                else
                {
                    string userType = Session["LoggedInUserType"] as string;

                    if (userType == "teacher")
                    {
                        Response.Redirect("~/Professor/DashBoard.aspx");
                    }
                    else if (userType == "admin")
                    {
                        Response.Redirect("~/Admin/DashBoard.aspx");
                    }
                    else
                    {
                        string userEmail = Session["LoggedInUserEmail"] as string;

                        if (!string.IsNullOrEmpty(userEmail))
                        {
                            lblUserEmail.Text = userEmail;


                            byte[] profileImageBytes = GetUserProfileImage(userEmail);
                            if (profileImageBytes != null)
                            {
                                string base64Image = Convert.ToBase64String(profileImageBytes);
                                string imageSrc = "data:image/jpeg;base64," + base64Image;
                                Image1.ImageUrl = imageSrc;
                            }

                            DisplayTeacherProfileInfo(userEmail);
                        }
                    }
                }
            }
        }
        private byte[] GetUserProfileImage(string userEmail)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = "SELECT profileimage FROM users WHERE email = @userEmail";

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userEmail", userEmail);
                    connection.Open();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (!(reader["profileimage"] is DBNull))
                            {
                                return (byte[])reader["profileimage"];
                            }
                        }
                    }
                }
            }

            return null;
        }

        private void DisplayTeacherProfileInfo(string userEmail)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = "SELECT studentid, firstname, lastname, age, birthday, email, profileimage, username FROM student_info WHERE email = @userEmail";

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userEmail", userEmail);
                    connection.Open();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            TextBox1.Text = reader["firstname"].ToString();
                            TextBox2.Text = reader["lastname"].ToString();
                            TextBox3.Text = reader["studentid"].ToString();
                            TextBox4.Text = reader["age"].ToString();
                            TextBox8.Text = reader["birthday"].ToString();
                            TextBox9.Text = reader["email"].ToString();
                            txtusername.Text = reader["username"].ToString();

                            byte[] imageBytes = reader["profileimage"] as byte[];
                            if (imageBytes != null && imageBytes.Length > 0)
                            {
                                string base64String = Convert.ToBase64String(imageBytes);
                                Image2.ImageUrl = "data:image/jpeg;base64," + base64String;
                            }
                        }
                    }
                }
            }
        }

        protected void btnchangeimage_Click(object sender, EventArgs e)
        {

            string username = txtusername.Text;
            try
            {
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    byte[] fileBytes = null;

                    if (FileUpload1.HasFile)
                    {
                        fileBytes = new byte[FileUpload1.PostedFile.InputStream.Length];
                        FileUpload1.PostedFile.InputStream.Read(fileBytes, 0, fileBytes.Length);
                    }


                    string userQuery = "UPDATE users SET profileimage = @ProfileImage WHERE username = @Username";
                    using (MySqlCommand userCmd = new MySqlCommand(userQuery, con))
                    {
                        userCmd.Parameters.AddWithValue("@Username", username);
                        userCmd.Parameters.AddWithValue("@ProfileImage", fileBytes ?? GetExistingProfileImage(username, con));

                        int userRowsAffected = userCmd.ExecuteNonQuery();

                        string teacherQuery = "UPDATE student_info SET  profileimage = @ProfileImage WHERE username = @Username";
                        using (MySqlCommand teacherCmd = new MySqlCommand(teacherQuery, con))
                        {
                            teacherCmd.Parameters.AddWithValue("@Username", username);
                            teacherCmd.Parameters.AddWithValue("@ProfileImage", fileBytes ?? GetExistingProfileImage(username, con));

                            int teacherRowsAffected = teacherCmd.ExecuteNonQuery();

                            if (userRowsAffected > 0 && teacherRowsAffected > 0)
                            {
                                ShowSuccessMessage("The Teacher Profile Picture has been updated successfully.");
                                ClearInputFields();
                                ClientScript.RegisterStartupScript(this.GetType(), "successMessage", "showSuccessMessage();", true);

                            }
                            else
                            {
                                ShowErrorMessage("Error updating Teacher Profile Picture");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error processing: " + ex.Message);
            }
        }
        private void ClearInputFields()
        {
            Image2.ImageUrl = "";

        }
        private byte[] GetExistingProfileImage(string username, MySqlConnection con)
        {
            string query = "SELECT profileimage FROM users WHERE username = @Username";
            using (MySqlCommand cmd = new MySqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                var result = cmd.ExecuteScalar();
                return result as byte[];
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {

            string userEmail = Session["LoggedInUserEmail"] as string;
            string currentPassword = TextBox5.Text;
            string newPassword = TextBox6.Text;
            string confirmPassword = TextBox7.Text;

            if (ValidateCurrentPassword(userEmail, currentPassword) && newPassword == confirmPassword)
            {
                ChangePassword(userEmail, newPassword);
                ShowSuccessMessage("Password updated successfully.");
                TextBox5.Text = "";
                TextBox6.Text = "";
                TextBox7.Text = "";

            }
            else
            {
                ShowErrorMessage("Failed to update password.");

            }

        }
        private void ChangePassword(string userEmail, string newPassword)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = "UPDATE users SET password = @newPassword WHERE email = @userEmail";

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@newPassword", newPassword);
                    cmd.Parameters.AddWithValue("@userEmail", userEmail);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private bool ValidateCurrentPassword(string userEmail, string currentPassword)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = "SELECT password FROM users WHERE email = @userEmail";

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userEmail", userEmail);
                    connection.Open();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedPassword = reader["password"].ToString();

                            if (currentPassword == storedPassword)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
        private void ShowErrorMessage(string message)
        {
            string script = $"Swal.fire({{ icon: 'error', text: '{message}' }})";
            ClientScript.RegisterClientScriptBlock(this.GetType(), "alert", script, true);
        }
        private void ShowSuccessMessage(string message)
        {
            string script = $"Swal.fire({{ icon: 'success', text: '{message}' }})";
            ClientScript.RegisterClientScriptBlock(this.GetType(), "alert", script, true);
        }



        protected void Button2_Click(object sender, EventArgs e)
        {

        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            try
            {
                string teacherid = TextBox3.Text;
                string email = TextBox9.Text;
                string smtppassword = TextBox10.Text;

                if (!string.IsNullOrEmpty(teacherid) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(smtppassword))
                {
                    string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        if (!IsEmailExists(email, connection))
                        {
                            string query = "INSERT INTO smtp_credentials (teacherid, smtp_email, smtp_password) VALUES (@teacherid, @smtp_email, @smtp_password)";

                            using (MySqlCommand cmd = new MySqlCommand(query, connection))
                            {
                                cmd.Parameters.AddWithValue("@teacherid", teacherid);
                                cmd.Parameters.AddWithValue("@smtp_email", email);
                                cmd.Parameters.AddWithValue("@smtp_password", smtppassword);

                                connection.Open();
                                cmd.ExecuteNonQuery();
                                ShowSuccessMessage("SMTP Password has been Inserted");
                                TextBox10.Text = "";
                            }
                        }
                        else
                        {
                            ShowErrorMessage("SMTP already exists.");
                        }
                    }
                }
                else
                {
                    ShowErrorMessage("Please provide SMTP password.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("An error occurred: " + ex.Message);
            }
        }

        private bool IsEmailExists(string email, MySqlConnection connection)
        {
            using (MySqlConnection checkConnection = new MySqlConnection(connection.ConnectionString))
            {
                string query = "SELECT COUNT(*) FROM smtp_credentials WHERE smtp_email = @smtp_email";

                using (MySqlCommand cmd = new MySqlCommand(query, checkConnection))
                {
                    cmd.Parameters.AddWithValue("@smtp_email", email);

                    checkConnection.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    return count > 0;
                }
            }
        }
        protected void Button4_Click(object sender, EventArgs e)
        {
            try
            {
                string email = TextBox9.Text;
                string newSmtppassword = TextBox10.Text;
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    string getCurrentPasswordQuery = "SELECT smtp_password FROM smtp_credentials WHERE smtp_email = @smtp_email";
                    using (MySqlCommand getCurrentPasswordCmd = new MySqlCommand(getCurrentPasswordQuery, connection))
                    {
                        getCurrentPasswordCmd.Parameters.AddWithValue("@smtp_email", email);

                        connection.Open();
                        object currentPasswordObj = getCurrentPasswordCmd.ExecuteScalar();

                        if (currentPasswordObj != null)
                        {
                            string currentPassword = currentPasswordObj.ToString();

                            if (!string.Equals(currentPassword, newSmtppassword))
                            {
                                if (!string.IsNullOrEmpty(newSmtppassword))
                                {
                                    string updateQuery = "UPDATE smtp_credentials SET smtp_password = @smtp_password WHERE smtp_email = @smtp_email";

                                    using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection))
                                    {
                                        updateCmd.Parameters.AddWithValue("@smtp_email", email);
                                        updateCmd.Parameters.AddWithValue("@smtp_password", newSmtppassword);

                                        int rowsAffected = updateCmd.ExecuteNonQuery();

                                        if (rowsAffected > 0)
                                        {
                                            ShowSuccessMessage("SMTP Password has been updated.");
                                        }
                                        else
                                        {
                                            ShowErrorMessage("Failed to update SMTP Password.");
                                        }
                                    }
                                }
                                else
                                {
                                    ShowErrorMessage("New SMTP Password cannot be null or empty. No update performed.");
                                }
                            }
                            else
                            {
                                ShowErrorMessage("The new password is the same as the current password. No update performed.");
                            }
                        }
                        else
                        {
                            ShowErrorMessage("Email not found in the database.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("An error occurred: " + ex.Message);
            }
        }
    }
}