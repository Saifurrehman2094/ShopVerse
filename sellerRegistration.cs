using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using ShopVerse.Helpers;

namespace ShopVerse
{
    public partial class sellerRegistration : Form
    {
        public sellerRegistration()
        {
            InitializeComponent();
        }

        private void sellerRegistration_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Get values from textboxes
            string name = textBox1.Text;
            string email = textBox2.Text;
            string password = textBox3.Text;
            string address = textBox4.Text;
            string phoneNo = textBox5.Text;  // Assuming TextBox5 is for PhoneNo

            // Validate email format
            if (!email.EndsWith("@gmail.com"))
            {
                MessageBox.Show("Please enter a valid Gmail address.");
                return;
            }

            // Connection string (replace with your actual connection string)
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";

            // SQL query to insert data into seller table
            string query = "INSERT INTO seller (name, email, password, address, PhoneNo) " +
                           "VALUES (@Name, @Email, @Password, @Address, @PhoneNo)";

            // Use SqlConnection and SqlCommand to execute the query
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // Open connection
                    connection.Open();

                    // Create SQL command
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters to avoid SQL injection
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", password);
                        command.Parameters.AddWithValue("@Address", address);
                        command.Parameters.AddWithValue("@PhoneNo", phoneNo);

                        // Execute the command to insert data
                        int rowsAffected = command.ExecuteNonQuery();

                        // Check if insertion was successful
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Seller added successfully.");
                        }
                        else
                        {
                            MessageBox.Show("Failed to add seller.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any errors that occur during the database operation
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }
    }
}
