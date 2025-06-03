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
    public partial class Review : Form
    {
        Home h;
        int id;
        public Review(Home home, int id)
        {
            InitializeComponent();
            h = home;
            this.id = id;
        }

        private void Review_Load(object sender, EventArgs e)
        {
              // Set your ProductId here
            LoadReviews(id);
        }

        private void LoadReviews(int productId)
        {
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = @"
                SELECT r.RID, r.date, r.rating, r.Message, c.FirstName as CustomerName 
                FROM Review r
                JOIN Customer c ON r.CustomerId = c.CustomerId
                WHERE r.ProductId = @ProductId
                ORDER BY r.date DESC;";  // Sorting reviews by most recent

            // Open the connection and execute the query
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Create a SqlCommand to execute the query
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameter to prevent SQL injection
                        command.Parameters.AddWithValue("@ProductId", productId);

                        // Execute the command and read the results using SqlDataReader
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Clear existing panels in FlowLayoutPanel
                            flowLayoutPanel1.Controls.Clear();

                            // Loop through the results and create panels for each review
                            while (reader.Read())
                            {
                                // Retrieve the data
                                string customerName = reader["CustomerName"].ToString();
                                string message = reader["Message"].ToString();
                                int rating = Convert.ToInt32(reader["rating"]);
                                DateTime reviewDate = Convert.ToDateTime(reader["date"]);

                                // Create a new panel for each review
                                Panel reviewPanel = new Panel
                                {
                                    Size = new System.Drawing.Size(700, 150),  // Set a fixed size for the panel
                                    BorderStyle = BorderStyle.FixedSingle,
                                    Margin = new Padding(10)
                                };

                                // Create label for Customer Name
                                Label customerNameLabel = new Label
                                {
                                    Text = $"Customer: {customerName}",
                                    Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold),
                                    Location = new System.Drawing.Point(10, 10),
                                    AutoSize = true
                                };

                                // Create label for Rating (you can show stars or just the number)
                                Label ratingLabel = new Label
                                {
                                    Text = $"Rating: {rating}/5",
                                    Font = new System.Drawing.Font("Arial", 10),
                                    Location = new System.Drawing.Point(10, 40),
                                    AutoSize = true
                                };

                                // Create label for Review Date
                                Label dateLabel = new Label
                                {
                                    Text = $"Date: {reviewDate.ToString("MM/dd/yyyy")}",
                                    Font = new System.Drawing.Font("Arial", 10),
                                    Location = new System.Drawing.Point(10, 70),
                                    AutoSize = true
                                };

                                // Create label for Review Message
                                Label messageLabel = new Label
                                {
                                    Text = $"Comment: {message}",
                                    Font = new System.Drawing.Font("Arial", 10),
                                    Location = new System.Drawing.Point(10, 100),
                                    AutoSize = true,
                                    MaximumSize = new System.Drawing.Size(680, 0), // Limit the width
                                    AutoEllipsis = true
                                };

                                // Add labels to the panel
                                reviewPanel.Controls.Add(customerNameLabel);
                                reviewPanel.Controls.Add(ratingLabel);
                                reviewPanel.Controls.Add(dateLabel);
                                reviewPanel.Controls.Add(messageLabel);

                                // Add the panel to the FlowLayoutPanel
                                flowLayoutPanel1.Controls.Add(reviewPanel);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error retrieving reviews: " + ex.Message);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1)  // Check if rating is not selected
            {
                MessageBox.Show("Please select a rating.");
                return;
            }

            if (string.IsNullOrEmpty(textBox1.Text))  // Check if comment is empty
            {
                MessageBox.Show("Please enter a comment.");
                return;
            }

            // Step 2: Get the rating and comment
            int rating = Convert.ToInt32(comboBox1.SelectedItem);  // Rating value (1-5)
            string comment = textBox1.Text.Trim();  // Comment text

            // Get CustomerId from UserSession
            int customerId = UserSession.CustomerId;

            // Step 3: Prepare the SQL INSERT query
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = "INSERT INTO Review (date, rating, ProductId, CustomerId, Message) VALUES (@date, @rating, @productId, @customerId, @message)";

            // Use the current date/time for the review submission
            DateTime currentDate = DateTime.Now;

            // Step 4: Execute the query using SqlConnection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // Open the connection
                    connection.Open();

                    // Prepare the command
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters to the command
                        command.Parameters.AddWithValue("@date", currentDate);
                        command.Parameters.AddWithValue("@rating", rating);
                        command.Parameters.AddWithValue("@productId", id);  // Example: Replace with the actual ProductId you want to review
                        command.Parameters.AddWithValue("@customerId", customerId);
                        command.Parameters.AddWithValue("@message", comment);

                        // Execute the query to insert the review
                        int result = command.ExecuteNonQuery();

                        // Step 5: Check if the review was added successfully
                        if (result > 0)
                        {
                            MessageBox.Show("Review submitted successfully!");
                            LoadReviews(id);
                        }
                        else
                        {
                            MessageBox.Show("Failed to submit review. Please try again.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any errors
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        // Call this method to load reviews when needed

    }
}
