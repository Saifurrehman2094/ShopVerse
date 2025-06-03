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
    public partial class AdminSeller : Form
    {
        AdminCustomer cust;
        public AdminSeller(AdminCustomer cust)
        {
            InitializeComponent();
            this.cust = cust;
        }

        private void AdminSeller_Load(object sender, EventArgs e)
        {
            LoadSellerData();
        }
        string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
        private void LoadSellerData()
        {
            string query = @"
            SELECT s.SId, s.name, s.email, s.address, COUNT(p.ProductId) AS ProductCount
            FROM seller s
            LEFT JOIN Product p ON s.SId = p.SellerId
            GROUP BY s.SId, s.name, s.email, s.address";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Loop through each seller record
                            while (reader.Read())
                            {
                                int sellerId = reader.GetInt32(0);
                                string name = reader.GetString(1);
                                string email = reader.GetString(2);
                                string address = reader.IsDBNull(3) ? "N/A" : reader.GetString(3);
                                int productCount = reader.GetInt32(4);

                                // Create a new panel for each seller
                                Panel sellerPanel = new Panel
                                {
                                    Size = new System.Drawing.Size(1074, 50), // Adjust size as needed
                                    BorderStyle = BorderStyle.FixedSingle
                                };

                                // Create labels for seller information
                                Label lblName = new Label
                                {
                                    Text = "Name: " + name,
                                    Location = new System.Drawing.Point(10, 10),
                                    Width = 200
                                };

                                Label lblEmail = new Label
                                {
                                    Text = "Email: " + email,
                                    Location = new System.Drawing.Point(220, 10),
                                    Width = 250
                                };

                                Label lblAddress = new Label
                                {
                                    Text = "Address: " + address,
                                    Location = new System.Drawing.Point(480, 10),
                                    Width = 300
                                };

                                Label lblProductCount = new Label
                                {
                                    Text = "No of Products: " + productCount,
                                    Location = new System.Drawing.Point(790, 10),
                                    Width = 120
                                };

                                // Create the remove button
                                Button btnRemove = new Button
                                {
                                    Text = "Remove",
                                    Location = new System.Drawing.Point(920, 10),
                                    Width = 100,
                                    Tag = sellerId // Store SellerId in Tag property for later use
                                };

                                btnRemove.Click += (s, e) => RemoveSeller((int)((Button)s).Tag);

                                // Add controls to the panel
                                sellerPanel.Controls.Add(lblName);
                                sellerPanel.Controls.Add(lblEmail);
                                sellerPanel.Controls.Add(lblAddress);
                                sellerPanel.Controls.Add(lblProductCount);
                                sellerPanel.Controls.Add(btnRemove);

                                // Add panel to the FlowLayoutPanel
                                flowLayoutPanel1.Controls.Add(sellerPanel);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        // Method to remove a seller from the database
        private void RemoveSeller(int sellerId)
        {
            string query = "DELETE FROM seller WHERE SId = @SellerId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameter to the query
                        command.Parameters.AddWithValue("@SellerId", sellerId);

                        // Execute the delete command
                        int rowsAffected = command.ExecuteNonQuery();

                        // If deletion is successful, remove the panel from FlowLayoutPanel
                        if (rowsAffected > 0)
                        {
                            foreach (Control control in flowLayoutPanel1.Controls)
                            {
                                // Find the panel by seller ID (stored in the button's Tag)
                                if (control is Panel panel && panel.Controls.OfType<Button>().FirstOrDefault()?.Tag.Equals(sellerId) == true)
                                {
                                    flowLayoutPanel1.Controls.Remove(control);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete the seller.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cust.Show();
            this.Close();
        }
    }
}
