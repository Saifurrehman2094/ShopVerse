using ShopVerse.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShopVerse.Helpers;

namespace ShopVerse
{
    public partial class login : Form
    {
        Form Signup;
        public login()
        {
            InitializeComponent();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox2.UseSystemPasswordChar = false;
            }
            else
            {
                textBox2.UseSystemPasswordChar= true;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Signup = new Signup();
            Signup.Show();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();

            // Validate that a user type is selected
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Please select a user type.");
                return;
            }

            string selectedUserType = comboBox1.SelectedItem.ToString();
            string query = "";

            // Query selection based on user type
            switch (selectedUserType)
            {
                case "Customer":
                    query = "SELECT CustomerId FROM Customer WHERE Email = @Email AND Password = @Password";
                    break;
                case "seller":
                    query = "SELECT SId FROM seller WHERE Email = @Email AND Password = @Password";
                    break;
                case "admin":
                    query = "SELECT AId FROM admin WHERE Email = @Email AND Password = @Password";
                    break;
                case "Logistics":
                    query = "SELECT LID FROM Logistics WHERE Email = @Email AND Password = @Password";
                    break;
                default:
                    MessageBox.Show("Invalid user type selected.");
                    return;
            }

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Email", textBox1.Text.Trim());
            cmd.Parameters.AddWithValue("@Password", textBox2.Text.Trim());
            object result = cmd.ExecuteScalar();

            if (result != null)
            {
                int userId = (int)result;

                // Handle form navigation based on user type
                switch (selectedUserType)
                {
                    case "Customer":
                        UserSession.LogisticsId = -1;
                        UserSession.SellerId = -1;
                        UserSession.AdminId = -1;
                        UserSession.CustomerId = userId; // Set globally for Customer
                        Home homeForm = new Home();
                        homeForm.Show();
                        break;
                    case "seller":
                        UserSession.CustomerId = -1;
                        UserSession.LogisticsId = -1;
                        UserSession.AdminId = -1;
                        UserSession.SellerId = userId; // Add Seller session tracking if needed
                        sellerHome sellerForm = new sellerHome(); // Replace with actual Seller form
                        sellerForm.Show();
                        break;
                    case "admin":
                        UserSession.CustomerId = -1;
                        UserSession.LogisticsId = -1;
                        UserSession.SellerId = -1;
                        UserSession.AdminId = userId; // Add Admin session tracking if needed
                        AdminCustomer adminForm = new AdminCustomer(); // Replace with actual Admin form
                        adminForm.Show();
                        break;
                    case "Logistics":
                        UserSession.CustomerId = -1;
                        UserSession.SellerId = -1;
                        UserSession.AdminId = -1;
                        UserSession.LogisticsId = userId; // Add Logistics session tracking if needed
                        LogisticsOrder logisticsForm = new LogisticsOrder(); // Replace with actual Logistics form
                        logisticsForm.Show();
                        break;
                }

                this.Hide(); // Hide the login form
            }
            else
            {
                MessageBox.Show("Invalid email or password. Please try again.");
            }
        }


        private void login_Load(object sender, EventArgs e)
        {

        }
    }
}
