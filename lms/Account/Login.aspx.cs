using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;

namespace lms.LOGIN
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            txtemail.Focus();
        }
        protected void btnlogin_Click(object sender, EventArgs e)
        {
            try
            {
                string email = txtemail.Text.Trim();
                string password = txtpassword.Text.Trim();

                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
                bool emailValid = false;
                bool passwordValid = false;
                string userStatus = "";
                int userID = 0;

                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    emailValid = CheckEmail(con, email);

                    if (emailValid)
                    {
                        passwordValid = CheckPassword(con, email, password);
                        userStatus = GetUserStatus(con, email);
                        userID = GetUserID(con, email);
                    }

                    if (emailValid && passwordValid && userStatus == "Activated")
                    {
                        string usertype = DetermineUserUsertype(con, email);
                        if (!string.IsNullOrEmpty(usertype))
                        {
                            Session["LoggedInUserEmail"] = email;

                            Session["LoggedInUserType"] = usertype;
                            Session["LoggedInUserID"] = userID;

                            if (usertype == "admin")
                            {
                                Response.Redirect("/Admin/DashBoard.aspx");
                            }
                            else if (usertype == "student")
                            {
                                Response.Redirect("/Student/DashBoard.aspx");
                            }
                            else if (usertype == "teacher")
                            {
                                Response.Redirect("/Professor/DashBoard.aspx");
                            }
                        }
                        else
                        {
                            ShowErrorMessage("Invalid User Type");
                        }
                    }
                    else
                    {
                        if (userStatus == "Deactivated")
                        {
                            ShowErrorMessage("Your account is deactivated. Email the admin for more info.");
                        }
                        else if (!emailValid)
                        {
                            ShowErrorMessage("Incorrect Email");
                        }
                        else
                        {
                            ShowErrorMessage("Incorrect Password");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("An error occurred while processing your request. Please try again later.");
            }
        
    }
        private int GetUserID(MySqlConnection con, string email)
        {
            int userID = 0;

            string teacherQuery = "SELECT teacherid FROM teacher_info WHERE email = @Email";
            using (MySqlCommand teacherCmd = new MySqlCommand(teacherQuery, con))
            {
                teacherCmd.Parameters.AddWithValue("@Email", email);
                object teacherResult = teacherCmd.ExecuteScalar();

                if (teacherResult != null)
                {
                    userID = Convert.ToInt32(teacherResult);
                    return userID;
                }
            }

            string studentQuery = "SELECT studentid FROM student_Info WHERE email = @Email";
            using (MySqlCommand studentCmd = new MySqlCommand(studentQuery, con))
            {
                studentCmd.Parameters.AddWithValue("@Email", email);
                object studentResult = studentCmd.ExecuteScalar();

                if (studentResult != null)
                {
                    userID = Convert.ToInt32(studentResult);
                    return userID;
                }
            }

            return userID;
        }

        private string GetUserStatus(MySqlConnection con, string email)
        {
            string query = "SELECT Status FROM users WHERE Email = @Email";
            using (MySqlCommand cmd = new MySqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                object status = cmd.ExecuteScalar();
                if (status != null)
                {
                    return status.ToString();
                }
                return null;
            }
        }

        private bool CheckEmail(MySqlConnection con, string email)
        {
            string query = "SELECT Email FROM users WHERE BINARY Email = @Email";
            using (MySqlCommand cmd = new MySqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                return cmd.ExecuteScalar() != null;
            }
        }
        private bool CheckPassword(MySqlConnection con, string email, string password)
        {
            string query = "SELECT Password FROM users WHERE Email = @Email";
            using (MySqlCommand cmd = new MySqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Email", email);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string storedPassword = reader["Password"].ToString();
                        return VerifyPassword(password, storedPassword);
                    }
                }
                return false;
            }
        }

        private string DetermineUserUsertype(MySqlConnection con, string email)
        {
            string query = "SELECT usertype FROM users WHERE Email = @Email";
            using (MySqlCommand cmd = new MySqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                return (string)cmd.ExecuteScalar();
            }
        }

        private bool VerifyPassword(string inputPassword, string hashedPassword)
        {
            return inputPassword.Equals(hashedPassword);
        }

        private void ShowErrorMessage(string message)
        {
            string script = $"Swal.fire({{ icon: 'error', text: '{message}' }})";
            ClientScript.RegisterClientScriptBlock(this.GetType(), "alert", script, true);
        }
    }
}
    