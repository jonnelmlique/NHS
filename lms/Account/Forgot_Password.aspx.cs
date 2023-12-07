using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Net.Mail;
using MySql.Data.MySqlClient;
    
namespace lms.Account
{
    public partial class Forgot_Password : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnSent_Click(object sender, EventArgs e)
        {
            try
            { 
            string email = txtemail.Text;
            string token = Guid.NewGuid().ToString();
            DateTime timestamp = DateTime.Now.AddHours(24);

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            string tableName = DetermineUserType(email);

            if (!string.IsNullOrEmpty(tableName))
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();

                    string updateQuery = $"UPDATE users SET ResetToken = @Token, TokenExpiration = @Timestamp WHERE email = @email";

                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Token", token);
                        cmd.Parameters.AddWithValue("@Timestamp", timestamp);
                        cmd.Parameters.AddWithValue("@email", email);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            string resetLink = $"https://localhost:44304/Account/Reset_Password.aspx?table={tableName}&token={token}";
                            SendPasswordResetEmail(email, resetLink);
                                ShowSuccessMessage("Password reset link sent to your email.");
                            //lblMessage.Text = "Password reset link sent to your email.";
                            txtemail.Text = "";
                        }
                        else
                        {
                                ShowErrorMessage("Email Not found");
                                //lblMessage.Text = "Email Not found";
                            }
                        }
                }
            }
            else
            {
                    ShowErrorMessage("Invalid Email Address");
                    //lblMessage.Text = "Invalid Email Address";
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("An error occurred while processing your request. Please try again later.");
                //lblMessage.Text = "An error occurred while processing your request. Please try again later.";
            }
        }
        private string DetermineUserType(string email)
        {

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT usertype FROM users WHERE email = @email";


                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    var userType = cmd.ExecuteScalar();
                    return userType != null ? userType.ToString() : null;

                }
            }
        }
        private void SendPasswordResetEmail(string toEmail, string resetLink)
        {
            using (MailMessage message = new MailMessage("novalichesseniorhighschool@gmail.com", toEmail))
            {
                message.Subject = "Password Reset at Novaliches Senior High School LMS";
                message.Body = "To reset your password, click on the following link: " + resetLink;

                message.IsBodyHtml = true;

                using (SmtpClient client = new SmtpClient("smtp.gmail.com"))
                {
                    client.Port = 587;
                    client.Credentials = new NetworkCredential("novalichesseniorhighschool@gmail.com", "jpscuyqtbmgpkcqw");
                    client.EnableSsl = true;
                    client.Send(message);
                }
            }
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
    }
}