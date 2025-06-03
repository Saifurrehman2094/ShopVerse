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
    public partial class AdminReview : Form
    {
        AdminCustomer customer;
        public AdminReview(AdminCustomer customer)
        {
            InitializeComponent();
            this.customer = customer;
        }

        private string connectionString = @"Data Source=DESKTOP-6SCBHN6\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";

        private void AdminReview_Load(object sender, EventArgs e)
        {
            LoadProductReviews();
        }

        private void LoadProductReviews()
        {
            // Query to fetch product reviews, average ratings, and seller names
            string query = @"
                SELECT 
                    p.name AS ProductName,
                    ISNULL(AVG(r.rating), 0) AS AverageRating,
                    s.name AS SellerName,
                    r.RId
                FROM 
                    Product p
                LEFT JOIN 
                    Review r ON p.ProductId = r.ProductId
                INNER JOIN 
                    seller s ON p.SellerId = s.SId
                GROUP BY 
                    p.ProductId, p.name, s.name, r.RId
                HAVING 
                    AVG(r.rating) BETWEEN 1 AND 5;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                flowLayoutPanel1.Controls.Clear();

                while (reader.Read())
                {
                    // Extract data
                    string productName = reader["ProductName"].ToString();
                    decimal averageRating = Convert.ToDecimal(reader["AverageRating"]);
                    string sellerName = reader["SellerName"].ToString();
                    int reviewId = Convert.ToInt32(reader["RId"]);

                    // Create panel
                    Panel reviewPanel = new Panel
                    {
                        Size = new Size(250, 120),
                        Margin = new Padding(10),
                        BackColor = GetBackColorForRating(averageRating)
                    };

                    // Add product name label
                    Label lblProductName = new Label
                    {
                        Text = $"Product: {productName}",
                        AutoSize = true,
                        Location = new Point(10, 10)
                    };
                    reviewPanel.Controls.Add(lblProductName);

                    // Add average rating label
                    Label lblAverageRating = new Label
                    {
                        Text = $"Average Rating: {averageRating:F1}",
                        AutoSize = true,
                        Location = new Point(10, 40)
                    };
                    reviewPanel.Controls.Add(lblAverageRating);

                    // Add seller name label
                    Label lblSellerName = new Label
                    {
                        Text = $"Seller: {sellerName}",
                        AutoSize = true,
                        Location = new Point(10, 70)
                    };
                    reviewPanel.Controls.Add(lblSellerName);

                    // Add remove button
                    Button btnRemove = new Button
                    {
                        Text = "Remove",
                        Size = new Size(75, 30),
                        Location = new Point(10, 90),
                        BackColor = Color.Red,
                        ForeColor = Color.White
                    };
                    btnRemove.Click += (sender, e) => RemoveReview(reviewId);
                    reviewPanel.Controls.Add(btnRemove);

                    // Add panel to FlowLayoutPanel
                    flowLayoutPanel1.Controls.Add(reviewPanel);
                }

                connection.Close();
            }
        }

        private Color GetBackColorForRating(decimal rating)
        {
            if (rating >= 1 && rating < 2)
                return Color.LightCoral; // Light Red
            if (rating >= 2 && rating < 3)
                return Color.LightSalmon; // Light Orange
            if (rating >= 3 && rating < 4)
                return Color.Yellow; // Yellow
            if (rating >= 4 && rating <= 5)
                return Color.LightGreen; // Light Green

            return Color.White; // Default color
        }

        private void RemoveReview(int reviewId)
        {
            // SQL to delete review by ID
            string deleteQuery = "DELETE FROM Review WHERE RId = @ReviewId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(deleteQuery, connection);
                command.Parameters.AddWithValue("@ReviewId", reviewId);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }

            // Reload the reviews after deletion
            LoadProductReviews();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            customer.Show();
            this.Close();
        }
    }
}
