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
                string Email = txtemail.Text;
                string token = Guid.NewGuid().ToString();
                DateTime timestamp = DateTime.Now.AddHours(24);

                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
                string tableName = DetermineUserType(Email);

                if (!string.IsNullOrEmpty(tableName))
                {
                    using (MySqlConnection con = new MySqlConnection(connectionString))
                    {
                        con.Open();

                        string updateQuery = $"UPDATE manageuser SET ResetToken = @Token, TokenExpiration = @Timestamp WHERE Email = @Email";

                        using (MySqlCommand cmd = new MySqlCommand(updateQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@Token", token);
                            cmd.Parameters.AddWithValue("@Timestamp", timestamp);
                            cmd.Parameters.AddWithValue("@Email", Email);

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                string resetLink = $"https://localhost:44304/Account/Reset_Password.aspx?table={tableName}&token={token}";
                                SendPasswordResetEmail(Email, resetLink);
                                ShowSuccessMessage("Password reset link sent to your email.");
                                txtemail.Text = "";
                            }
                            //else
                            //{
                            //    ShowErrorMessage("Email not found");
                            //}
                        }
                    }
                }
                else
                {
                    ShowErrorMessage("Email not found");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("An error occurred while processing your request. Please try again later.");
            }
        }

        private string DetermineUserType(string Email)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT Role FROM manageuser WHERE Email = @Email";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Email", Email);
                    var Role = cmd.ExecuteScalar();
                    return Role != null ? Role.ToString() : null;
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
                    client.Credentials = new NetworkCredential("novalichesseniorhighschoolnhs@gmail.com", "kymucdmewcovazly");
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