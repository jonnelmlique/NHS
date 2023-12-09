using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit.HtmlEditor.ToolbarButtons;
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
            string un = txtemail.Text;
            string pass = txtpassword.Text;

            try
            {
                using (MySqlConnection conn = new MySqlConnection("server=localhost;username=root;database=learninghub;password=''"))
                {
                    conn.Open();

                    // Check in the manageuser table
                    MySqlCommand manageuserCmd = new MySqlCommand("SELECT * FROM manageuser WHERE Email=@username AND Password=@password", conn);
                    manageuserCmd.Parameters.AddWithValue("@username", un);
                    manageuserCmd.Parameters.AddWithValue("@password", pass);

                    // Check in the users table
                    MySqlCommand usersCmd = new MySqlCommand("SELECT * FROM lmsusers WHERE email=@username AND password=@password", conn);
                    usersCmd.Parameters.AddWithValue("@username", un);
                    usersCmd.Parameters.AddWithValue("@password", pass);

                    using (MySqlDataReader manageuserReader = manageuserCmd.ExecuteReader())
                    {
                        if (manageuserReader.Read())
                        {
                            // User found in the manageuser table
                            //HandleUserFound("Student", "/Student/DashBoard.aspx", manageuserReader);
                                        {
                                string roles = manageuserReader.GetString("Role");
                                string role = manageuserReader.GetString("Role");
                            Session["Role"] = role;
                            //Session["Name"] = manageuserReader.GetString("username");
                            Session["ID"] = manageuserReader.GetString("UserID");
                            Session["LoggedInUserEmail"] = un;

                                if (roles == "Student")
                            {
                                Session["LoggedInUser"] = un;
                                    //HandleUserFound("Teacher", "/Professor/DashBoard.aspx", manageuserReader);

                                    HandleUserFoun1("Student",  "/Student/DashBoard.aspx", manageuserReader);
                            }
                            else if (roles == "Admin")
                            {
                                Session["LoggedInUser"] = un;
                                    //Response.Redirect("/sis/SISPAGES/Admin.aspx");
                                    HandleUserFoun1("Admin", "/sis/SISPAGES/Admin.aspx", manageuserReader);

                                }
                            }
                    }

                    }

                    // If user is not found in the manageuser table, check in the users table
                    using (MySqlDataReader usersReader = usersCmd.ExecuteReader())
                    {
                        if (usersReader.Read())
                        {
                            // User found in the users table
                            string userType = usersReader.GetString("usertype");
                            Session["Name"] = usersReader.GetString("username");
                            Session["ID"] = usersReader.GetInt32("userid").ToString();
                            Session["LoggedInUserEmail"] = un;
                            Session["LoggedInUserType"] = userType;

                            if (userType == "teacher")
                            {
                                // User is a teacher
                                HandleUserFound("Teacher", "/Professor/DashBoard.aspx", usersReader);
                            }
                            else if (userType == "admin")
                            {
                                // User is an admin
                                HandleUserFound("Admin", "/Admin/DashBoard.aspx", usersReader);
                            }
                        }
                    }

                    // If no matching user is found in either table, throw an exception
                    throw new    Exception("User not found or invalid credentials");
                }
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", $"swal('Error', '{ex.Message}', 'error');", true);
            }
        }

        private void HandleUserFound(string role, string redirectUrl, MySqlDataReader reader)
        {
            string connectionString = "server=localhost;username=root;database=learninghub;password=''";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                Session["Role"] = role;
                Session["LoggedInUser"] = reader.GetString("Email"); // Assuming the email is the username
                Session["Name"] = reader.GetString("username");
                Session["ID"] = reader.GetInt32("UserID").ToString();
                Session["LoggedInUserEmail"] = reader.GetString("Email");

                if (role == "Teacher")
                {
                    MySqlCommand teacherInfoCmd = new MySqlCommand("SELECT * FROM teacher_info WHERE email = @email", conn);
                    teacherInfoCmd.Parameters.AddWithValue("@email", Session["LoggedInUserEmail"]);

                    using (MySqlDataReader teacherInfoReader = teacherInfoCmd.ExecuteReader())
                    {
                        if (teacherInfoReader.Read())
                        {
                            Session["TeacherFirstName"] = teacherInfoReader.GetString("firstname");
                            Session["TeacherLastName"] = teacherInfoReader.GetString("lastname");
                            Session["TeacherEmail"] = teacherInfoReader.GetString("email");
                            Session["LoggedInUserID"] = teacherInfoReader.GetInt32("teacherid").ToString();
                        }
                    }
                }

                Response.Redirect(redirectUrl);
            }
        }


        //    string un = txtemail.Text;
        //    string pass = txtpassword.Text;

        //    try
        //    {

        //        using (MySqlConnection conn = new MySqlConnection("server=localhost;username=root;database=learninghub;password=''"))
        //        {
        //            conn.Open();



        //            // Check in the manageuser table
        //            MySqlCommand manageuserCmd = new MySqlCommand("SELECT * FROM manageuser WHERE Email=@username AND Password=@password", conn);
        //            manageuserCmd.Parameters.AddWithValue("@username", un);
        //            manageuserCmd.Parameters.AddWithValue("@password", pass);

        //            // Check in the users table
        //            MySqlCommand usersCmd = new MySqlCommand("SELECT * FROM lmsusers WHERE email=@username AND password=@password", conn);
        //            usersCmd.Parameters.AddWithValue("@username", un);
        //            usersCmd.Parameters.AddWithValue("@password", pass);

        //            using (MySqlDataReader manageuserReader = manageuserCmd.ExecuteReader())
        //            {
        //                if (manageuserReader.Read())
        //                {
        //                    string role = manageuserReader.GetString("Role");
        //                    Session["Role"] = role;
        //                    Session["Name"] = manageuserReader.GetString("Name");
        //                    Session["ID"] = manageuserReader.GetString("UserID");
        //                    Session["LoggedInUserEmail"] = un;

        //                    if (role == "Student")
        //                    {
        //                        Session["LoggedInUser"] = un;
        //                        Response.Redirect("/Student/DashBoard.aspx");
        //                    }
        //                    else if (role == "Admin")
        //                    {
        //                        Session["LoggedInUser"] = un;
        //                        Response.Redirect("/sis/SISPAGES/Admin.aspx");
        //                    }
        //                }
        //            }

        //            // If user is not found in the manageuser table, check in the users table
        //            using (MySqlDataReader usersReader = usersCmd.ExecuteReader())
        //            {
        //                if (usersReader.Read())

        //                {
        //                    string userType = usersReader.GetString("usertype");
        //                    Session["Name"] = usersReader.GetString("username");
        //                    Session["ID"] = usersReader.GetInt32("userid").ToString();
        //                    Session["LoggedInUserEmail"] = un;
        //                    Session["LoggedInUserType"] = userType;

        //                    if (userType == "teacher")
        //                    {
        //                        Session["Role"] = "Teacher";
        //                        Session["LoggedInUser"] = un;
        //                        Response.Redirect("/Professor/DashBoard.aspx");
        //                    }
        //                    else if (userType == "admin")
        //                    {
        //                        Session["Role"] = "Admin";
        //                        Session["LoggedInUser"] = un;
        //                        Response.Redirect("/Admin/DashBoard.aspx");
        //                    }
        //                }
        //            }

        //            // If no matching user is found in either table, throw an exception
        //            throw new Exception("User not found or invalid credentials");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ClientScript.RegisterStartupScript(this.GetType(), "alert", $"swal('Error', '{ex.Message}', 'error');", true);
        //    }
        //}

        private void HandleUserFoun1(string role, string redirectUrl, MySqlDataReader reader)
        {
            string connectionString = "server=localhost;username=root;database=learninghub;password=''";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                Session["Role"] = role;
                Session["LoggedInUser"] = reader.GetString("Email"); // Assuming the email is the username
                //Session["Name"] = reader.GetString("username");
                Session["ID"] = reader.GetInt32("UserID").ToString();
                Session["LoggedInUserEmail"] = reader.GetString("Email");

                if (role == "Student")
                {
                    MySqlCommand teacherInfoCmd = new MySqlCommand("SELECT * FROM student_info WHERE email = @email", conn);
                    teacherInfoCmd.Parameters.AddWithValue("@email", Session["LoggedInUserEmail"]);

                    using (MySqlDataReader studentInfoReader = teacherInfoCmd.ExecuteReader())
                    {
                        if (studentInfoReader.Read())
                        {
                            Session["studentname"] = studentInfoReader.GetString("name");
                            Session["studentemail"] = studentInfoReader.GetString("email");
                            Session["LoggedInUserID"] = studentInfoReader.GetInt32("studentid").ToString();
                        }
                    }
                }

                Response.Redirect(redirectUrl);
            }
        }

        private void ShowErrorMessage(string message)
            {
                string script = $"Swal.fire({{ icon: 'error', text: '{message}' }})";
                ClientScript.RegisterClientScriptBlock(this.GetType(), "alert", script, true);
            }
        }
    } 

    