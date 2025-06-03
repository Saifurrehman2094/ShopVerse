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

namespace ShopVerse
{
    public partial class AdminCustomer : Form
    {
        public AdminCustomer()
        {
            InitializeComponent();
        }

        private void OrderAssignment_Load(object sender, EventArgs e)
        {
            PopulateCustomerPanels();
        }

        private void PopulateCustomerPanels()
        {
            // Clear existing controls
            flowLayoutPanel1.Controls.Clear();

            // Database connection string
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Fetch all customers
                    string query = "SELECT CustomerId, CONCAT(FirstName, ' ', LastName) AS FullName, Email, PhoneNO FROM Customer";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int customerId = reader.GetInt32(0);
                                string fullName = reader["FullName"].ToString();
                                string email = reader["Email"].ToString();
                                string phone = reader["PhoneNO"].ToString();

                                // Create a panel for each customer
                                CreateCustomerPanel(customerId, fullName, email, phone);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading customers: " + ex.Message);
                }
            }
        }
        private void CreateCustomerPanel(int customerId, string fullName, string email, string phone)
        {
            // Create a new panel
            Panel customerPanel = new Panel
            {
                Size = new Size(300, 150),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10),
                Tag = customerId // Store the CustomerId in the Tag
            };

            // Add customer name label
            Label nameLabel = new Label
            {
                Text = $"Name: {fullName}",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            // Add email label
            Label emailLabel = new Label
            {
                Text = $"Email: {email}",
                Font = new Font("Arial", 10, FontStyle.Regular),
                Location = new Point(10, 40),
                AutoSize = true
            };

            // Add phone number label
            Label phoneLabel = new Label
            {
                Text = $"Phone: {phone}",
                Font = new Font("Arial", 10, FontStyle.Regular),
                Location = new Point(10, 70),
                AutoSize = true
            };

            // Add delete button
            Button deleteButton = new Button
            {
                Text = "Delete",
                BackColor = Color.Red,
                ForeColor = Color.White,
                Size = new Size(100, 30),
                Location = new Point(10, 100),
                Tag = customerId // Store the CustomerId in the Tag
            };

            deleteButton.Click += DeleteCustomer; // Attach the click event

            // Add controls to the panel
            customerPanel.Controls.Add(nameLabel);
            customerPanel.Controls.Add(emailLabel);
            customerPanel.Controls.Add(phoneLabel);
            customerPanel.Controls.Add(deleteButton);

            // Add the panel to the FlowLayoutPanel
            flowLayoutPanel1.Controls.Add(customerPanel);
        }

        private void DeleteCustomer(object sender, EventArgs e)
        {
            Button deleteButton = sender as Button;
            int customerId = (int)deleteButton.Tag;
            MessageBox.Show($"{customerId.ToString()}");

            // Database connection string
           
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Delete the customer from the database
                    string query = "DELETE FROM Customer WHERE CustomerId = @customerId";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@customerId", customerId);
                        command.ExecuteNonQuery();
                    }

                    // Remove the corresponding panel from the FlowLayoutPanel
                    Panel panelToRemove = null;
                    foreach (Panel panel in flowLayoutPanel1.Controls)
                    {
                        if ((int)panel.Tag == customerId)
                        {
                            panelToRemove = panel;
                            break;
                        }
                    }

                    if (panelToRemove != null)
                    {
                        flowLayoutPanel1.Controls.Remove(panelToRemove);
                    }

                    MessageBox.Show("Customer deleted successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting customer: " + ex.Message);
                }
            }
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AdminSeller seller = new AdminSeller(this);
            seller.Show();
            this.Hide();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AdminProduct prod = new AdminProduct(this);
            prod.Show();
            this.Hide();
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AdminCategory category = new AdminCategory(this);
            category.Show();
            this.Hide();
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AdminOrders orders = new AdminOrders(this);
            orders.Show(); 
            this.Hide();
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AdminReview review = new AdminReview(this);
            review.Show(); this.Hide();
        }
    }
}
