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
    public partial class addProduct : Form
    {
        sellerHome s;
        public addProduct(sellerHome s)
        {
            InitializeComponent();
            this.s = s;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";

            // SQL queries
            string insertProductQuery = @"
        INSERT INTO Product (Name, Price, Brand, Description, ImagePath, CategoryId, SellerId)
        VALUES (@Name, @Price, @Brand, @Description, @ImagePath, @CategoryId, @SellerId);
        SELECT SCOPE_IDENTITY();";

            string insertStockQuery = @"
        INSERT INTO Stock (ProductId, Quantity, LastUpdated)
        VALUES (@ProductId, @Quantity, GETDATE());";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction(); // Begin transaction

                try
                {
                    // Insert into Product table
                    int productId;
                    using (SqlCommand insertProductCommand = new SqlCommand(insertProductQuery, connection, transaction))
                    {
                        // Parameters for Product
                        insertProductCommand.Parameters.AddWithValue("@Name", textBox1.Text.Trim());
                        insertProductCommand.Parameters.AddWithValue("@Price", decimal.Parse(textBox2.Text.Trim()));
                        insertProductCommand.Parameters.AddWithValue("@Brand", textBox3.Text.Trim());
                        insertProductCommand.Parameters.AddWithValue("@Description", textBox4.Text.Trim());
                        insertProductCommand.Parameters.AddWithValue("@ImagePath", textBox5.Text.Trim());
                        insertProductCommand.Parameters.AddWithValue("@CategoryId", (int)comboBox1.SelectedValue); // Selected CategoryId
                        insertProductCommand.Parameters.AddWithValue("@SellerId",UserSession.SellerId); // Assuming SellerId is stored in UserSession

                        // Execute and get the new ProductId
                        productId = Convert.ToInt32(insertProductCommand.ExecuteScalar());
                    }

                    // Insert into Stock table
                    using (SqlCommand insertStockCommand = new SqlCommand(insertStockQuery, connection, transaction))
                    {
                        // Parameters for Stock
                        insertStockCommand.Parameters.AddWithValue("@ProductId", productId);
                        insertStockCommand.Parameters.AddWithValue("@Quantity", int.Parse(textBox6.Text.Trim()));

                        insertStockCommand.ExecuteNonQuery();
                    }

                    // Commit transaction if both inserts succeed
                    transaction.Commit();
                    MessageBox.Show("Product added successfully!");
                    this.Close();
                    s.LoadSellerProducts(); 
                    s.Show();
                }
                catch (Exception ex)
                {
                    // Rollback transaction in case of error
                    transaction.Rollback();
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void addProduct_Load(object sender, EventArgs e)
        {
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = "SELECT CategoryId, name FROM Category";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                DataTable categoryTable = new DataTable();
                categoryTable.Load(reader);

                comboBox1.DataSource = categoryTable;
                comboBox1.DisplayMember = "name";
                comboBox1.ValueMember = "CategoryId";
            }
        }
    }
}
