using ShopVerse;
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
using System.Net;

namespace ShopVerse
{
    public partial class Signup : Form
    {
        public Signup()
        {
            InitializeComponent();
        }

        private void ValidateFields()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                errorProvider1.SetError(textBox1, "This field is required.");
                isValid = false;
            }
            else
            {
                errorProvider1.SetError(textBox1, "");
            }

            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                errorProvider1.SetError(textBox2, "This field is required.");
                isValid = false;
            }
            else
            {
                errorProvider1.SetError(textBox2, "");
            }

            if (string.IsNullOrWhiteSpace(textBox4.Text))
            {
                errorProvider1.SetError(textBox4, "This field is required.");
                isValid = false;
            }
            else
            {
                errorProvider1.SetError(textBox4, "");
            }
            if (string.IsNullOrWhiteSpace(textBox5.Text))
            {
                errorProvider1.SetError(textBox5, "This field is required.");
                isValid = false;
            }
            else
            {
                errorProvider1.SetError(textBox5, "");
            }

            button1.Enabled = isValid;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            ValidateFields();
            string email = textBox3.Text.Trim();
            if (string.IsNullOrWhiteSpace(email) || !email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                errorProvider1.SetError(textBox3, "Email must end with @gmail.com.");
                button1.Enabled = false;
            }
            else
            {
                errorProvider1.Clear(); // Clear the error if validation passes
            }
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // SQL query to insert data
                    string query = "INSERT INTO Customer (FirstName,LastName,Email, PhoneNO, Address,Password) VALUES (@FirstName,@LastName, @Email, @PhoneNO, @Address,@Password)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Add parameters to prevent SQL injection
                        cmd.Parameters.AddWithValue("@FirstName", textBox2.Text.Trim());
                        cmd.Parameters.AddWithValue("@LastName", textBox1.Text.Trim());
                        cmd.Parameters.AddWithValue("@Email", textBox3.Text.Trim());
                        cmd.Parameters.AddWithValue("@PhoneNO", textBox6.Text.Trim());
                        cmd.Parameters.AddWithValue("@Address", textBox5.Text.Trim());
                        cmd.Parameters.AddWithValue("@Password", textBox4.Text.Trim());

                        // Execute the query
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Data saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                        }
                        else
                        {
                            MessageBox.Show("Data save failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Clear text fields after successful save
        private void ClearFields()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Close(); 
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void Signup_Load(object sender, EventArgs e)
        {

        }
    }
}
