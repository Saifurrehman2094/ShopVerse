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
    public partial class AdminCategory : Form
    {
        AdminCustomer customer;
        public AdminCategory(AdminCustomer customer)
        {
            InitializeComponent();
            this.customer = customer;
        }

        private void AdminCategory_Load(object sender, EventArgs e)
        {
            LoadCategories();   
        }

        public void LoadCategories()
        {
            flowLayoutPanel1.Controls.Clear(); // Clear existing panels

            string connectionString = @"Data Source=DESKTOP-6SCBHN6\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = @"
        SELECT 
            c.CategoryId,
            c.name AS CategoryName,
            COUNT(p.ProductId) AS ProductCount
        FROM 
            Category c
        LEFT JOIN 
            Product p ON c.CategoryId = p.CategoryId
        GROUP BY 
            c.CategoryId, c.name;
    ";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    // Create a panel for each category
                    Panel categoryPanel = new Panel
                    {
                        Size = new Size(300, 120),
                        BorderStyle = BorderStyle.FixedSingle,
                        Margin = new Padding(10)
                    };

                    // Create and add CategoryName Label
                    Label lblCategoryName = new Label
                    {
                        Text = $"Category: {reader["CategoryName"]}",
                        Font = new Font("Arial", 10, FontStyle.Bold),
                        Location = new Point(10, 10),
                        AutoSize = true
                    };
                    categoryPanel.Controls.Add(lblCategoryName);

                    // Create and add ProductCount Label
                    Label lblProductCount = new Label
                    {
                        Text = $"Products: {reader["ProductCount"]}",
                        Location = new Point(10, 40),
                        AutoSize = true
                    };
                    categoryPanel.Controls.Add(lblProductCount);

                    // Create and add Remove Button
                    Button btnRemove = new Button
                    {
                        Text = "Remove",
                        Size = new Size(80, 30),
                        Location = new Point(200, 70),
                        Tag = reader["CategoryId"] // Store CategoryId for deletion
                    };
                    btnRemove.Click += RemoveCategory;
                    categoryPanel.Controls.Add(btnRemove);

                    // Add the panel to the FlowLayoutPanel
                    flowLayoutPanel1.Controls.Add(categoryPanel);
                }
            }
        }

        private void RemoveCategory(object sender, EventArgs e)
        {
            Button btnRemove = sender as Button;
            int categoryId = Convert.ToInt32(btnRemove.Tag);

            string connectionString = @"Data Source=DESKTOP-6SCBHN6\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string deleteQuery = "DELETE FROM Category WHERE CategoryId = @CategoryId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Ensure there are no products before deletion due to foreign key constraints
                string checkProductsQuery = "SELECT COUNT(*) FROM Product WHERE CategoryId = @CategoryId";
                SqlCommand checkCmd = new SqlCommand(checkProductsQuery, conn);
                checkCmd.Parameters.AddWithValue("@CategoryId", categoryId);
                int productCount = (int)checkCmd.ExecuteScalar();

                if (productCount > 0)
                {
                    MessageBox.Show("Cannot delete this category as it contains products.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Delete the category
                SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn);
                deleteCmd.Parameters.AddWithValue("@CategoryId", categoryId);
                deleteCmd.ExecuteNonQuery();
            }

            // Remove the panel from the FlowLayoutPanel
            Control panelToRemove = btnRemove.Parent;
            flowLayoutPanel1.Controls.Remove(panelToRemove);
            panelToRemove.Dispose();

            MessageBox.Show("Category removed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            customer.Show();
            this.Close();
        }
    }
}
