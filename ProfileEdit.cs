using ShopVerse.Helpers;
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
    public partial class ProfileEdit : Form
    {
        Home home;
        public ProfileEdit(Home home)
        {
            InitializeComponent();
            this.home = home;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Validation for email format in textBox3
            if (!textBox3.Text.EndsWith("@gmail.com"))
            {
                MessageBox.Show("Email must end with '@gmail.com'.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Collect data from text boxes
            string firstName = textBox1.Text.Trim();
            string lastName = textBox2.Text.Trim();
            string email = textBox3.Text.Trim();
            string password = textBox4.Text.Trim();
            string address = textBox5.Text.Trim();
            string phoneNo = textBox6.Text.Trim();

            // Assume that CustomerId is stored in a variable or fetched through a session
            int customerId = UserSession.CustomerId; // Replace this with actual CustomerId fetching logic

            // Build the update query dynamically based on non-empty fields
            List<string> updateClauses = new List<string>();
            if (!string.IsNullOrEmpty(firstName))
                updateClauses.Add("FirstName = @FirstName");
            if (!string.IsNullOrEmpty(lastName))
                updateClauses.Add("LastName = @LastName");
            if (!string.IsNullOrEmpty(email))
                updateClauses.Add("Email = @Email");
            if (!string.IsNullOrEmpty(password))
                updateClauses.Add("Password = @Password");
            if (!string.IsNullOrEmpty(address))
                updateClauses.Add("Address = @Address");
            if (!string.IsNullOrEmpty(phoneNo))
                updateClauses.Add("PhoneNO = @PhoneNO");

            if (updateClauses.Count == 0)
            {
                MessageBox.Show("No changes to update.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string query = $"UPDATE Customer SET {string.Join(", ", updateClauses)} WHERE CustomerId = @CustomerId";

            // Database update logic
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False"; // Replace with your actual connection string
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                // Add parameters dynamically based on non-empty fields
                if (!string.IsNullOrEmpty(firstName))
                    command.Parameters.AddWithValue("@FirstName", firstName);
                if (!string.IsNullOrEmpty(lastName))
                    command.Parameters.AddWithValue("@LastName", lastName);
                if (!string.IsNullOrEmpty(email))
                    command.Parameters.AddWithValue("@Email", email);
                if (!string.IsNullOrEmpty(password))
                    command.Parameters.AddWithValue("@Password", password);
                if (!string.IsNullOrEmpty(address))
                    command.Parameters.AddWithValue("@Address", address);
                if (!string.IsNullOrEmpty(phoneNo))
                    command.Parameters.AddWithValue("@PhoneNO", phoneNo);

                command.Parameters.AddWithValue("@CustomerId", customerId);

                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Customer details updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("No customer found with the given ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
            home.Show();
        }
    }
}
