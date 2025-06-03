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
    public partial class AdminProduct : Form
    {
        AdminCustomer customer;
        public AdminProduct(AdminCustomer customer)
        {
            InitializeComponent();
            this.customer = customer;
        }

        private void AdminProduct_Load(object sender, EventArgs e)
        {
            LoadProducts();
        }

        public void LoadProducts()
        {
            flowLayoutPanel1.Controls.Clear(); // Clear existing panels

            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = @"
        SELECT 
            p.ProductId,
            p.name AS ProductName,
            p.price AS Price,
            p.brand AS Brand,
            s.name AS SellerName,
            c.name AS CategoryName
        FROM 
            Product p
        JOIN 
            Seller s ON p.SellerId = s.SId
        JOIN 
            Category c ON p.CategoryId = c.CategoryId;
    ";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    // Create a panel for each product
                    Panel productPanel = new Panel
                    {
                        Size = new Size(300, 150),
                        BorderStyle = BorderStyle.FixedSingle,
                        Margin = new Padding(10)
                    };

                    // Create and add ProductName Label
                    Label lblProductName = new Label
                    {
                        Text = $"Name: {reader["ProductName"]}",
                        Font = new Font("Arial", 10, FontStyle.Bold),
                        Location = new Point(10, 10),
                        AutoSize = true
                    };
                    productPanel.Controls.Add(lblProductName);

                    // Create and add Price Label
                    Label lblPrice = new Label
                    {
                        Text = $"Price: {reader["Price"]}",
                        Location = new Point(10, 35),
                        AutoSize = true
                    };
                    productPanel.Controls.Add(lblPrice);

                    // Create and add Brand Label
                    Label lblBrand = new Label
                    {
                        Text = $"Brand: {reader["Brand"]}",
                        Location = new Point(10, 60),
                        AutoSize = true
                    };
                    productPanel.Controls.Add(lblBrand);

                    // Create and add SellerName Label
                    Label lblSellerName = new Label
                    {
                        Text = $"Seller: {reader["SellerName"]}",
                        Location = new Point(10, 85),
                        AutoSize = true
                    };
                    productPanel.Controls.Add(lblSellerName);

                    // Create and add Category Label
                    Label lblCategory = new Label
                    {
                        Text = $"Category: {reader["CategoryName"]}",
                        Location = new Point(10, 110),
                        AutoSize = true
                    };
                    productPanel.Controls.Add(lblCategory);

                    // Create and add Remove Button
                    Button btnRemove = new Button
                    {
                        Text = "Remove",
                        Size = new Size(80, 30),
                        Location = new Point(200, 110),
                        Tag = reader["ProductId"] // Store ProductId for deletion
                    };
                    btnRemove.Click += RemoveProduct;
                    productPanel.Controls.Add(btnRemove);

                    // Add the panel to the FlowLayoutPanel
                    flowLayoutPanel1.Controls.Add(productPanel);
                }
            }
        }

        private void RemoveProduct(object sender, EventArgs e)
        {
            Button btnRemove = sender as Button;
            int productId = Convert.ToInt32(btnRemove.Tag);

            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string deleteQuery = "DELETE FROM Product WHERE ProductId = @ProductId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(deleteQuery, conn);
                cmd.Parameters.AddWithValue("@ProductId", productId);
                cmd.ExecuteNonQuery();
            }

            // Remove the panel from the FlowLayoutPanel
            Control panelToRemove = btnRemove.Parent;
            flowLayoutPanel1.Controls.Remove(panelToRemove);
            panelToRemove.Dispose();

            MessageBox.Show("Product removed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
            customer.Show();
        }
    }
}
