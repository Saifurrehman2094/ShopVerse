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
    public partial class sellerHome : Form
    {
        public sellerHome()
        {
            InitializeComponent();
            int sellerId = UserSession.SellerId;
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = "SELECT name FROM seller WHERE SId = @SellerId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@SellerId", sellerId);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        // Set the seller's name to the label
                        label1.Text = "Seller Name: " + result.ToString();
                    }
                    else
                    {
                        label1.Text = "Seller not found.";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error fetching seller name: " + ex.Message);
                }
            }
        }

        private void sellerHome_Load(object sender, EventArgs e)
        {
            LoadSellerProducts();
            LoadPendingOrdersForSeller();
        }

        public void LoadSellerProducts()
        {
            // Connection string
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";

            // Query to fetch products for the logged-in seller
            string query = @"
        SELECT p.ProductId, p.Name, p.Price, s.Quantity 
        FROM Product p
        INNER JOIN Stock s ON p.ProductId = s.ProductId
        WHERE p.SellerId = @SellerId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@SellerId", UserSession.SellerId); // Assuming UserSession holds the SellerId
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    flowLayoutPanel1.Controls.Clear(); // Clear previous panels

                    while (reader.Read())
                    {
                        int productId = reader.GetInt32(0);
                        string productName = reader.GetString(1);
                        decimal price = reader.GetDecimal(2);
                        int stockQuantity = reader.GetInt32(3);

                        // Create a panel for each product
                        Panel productPanel = CreateProductPanel(productId, productName, price, stockQuantity);
                        flowLayoutPanel1.Controls.Add(productPanel);
                    }
                }
            }
        }
        private Panel CreateProductPanel(int productId, string productName, decimal price, int stockQuantity)
        {
            // Create a new panel
            Panel panel = new Panel
            {
                Size = new Size(500, 120),
                BorderStyle = BorderStyle.FixedSingle,
                Tag = productId // Store the ProductId in the panel's Tag for reference
            };

            // Product Name Label
            Label nameLabel = new Label
            {
                Text = $"Name: {productName}",
                Location = new Point(10, 10),
                AutoSize = true
            };
            panel.Controls.Add(nameLabel);

            // Price Label
            Label priceLabel = new Label
            {
                Text = $"Price: ${price:F2}",
                Location = new Point(10, 35),
                AutoSize = true
            };
            panel.Controls.Add(priceLabel);

            // Stock Quantity Label
            Label stockLabel = new Label
            {
                Text = $"Stock: {stockQuantity}",
                Location = new Point(10, 60),
                AutoSize = true
            };
            panel.Controls.Add(stockLabel);

            // Update Stock Button
            Button updateStockButton = new Button
            {
                Text = "Update_Stock",
                Location = new Point(300, 10),
                Size = new Size(120,20), 
                Tag = productId // Store the ProductId in the button's Tag
            };
            updateStockButton.Click += UpdateStockButton_Click;
            panel.Controls.Add(updateStockButton);

            // Update Price Button
            Button updatePriceButton = new Button
            {
                Text = "Update_Price",
                Location = new Point(300, 40),
                Size = new Size(120, 20),
                Tag = productId // Store the ProductId in the button's Tag
            };
            updatePriceButton.Click += UpdatePriceButton_Click;
            panel.Controls.Add(updatePriceButton);

            // Remove Product Button
            Button removeButton = new Button
            {
                Text = "Remove",
                Location = new Point(300, 70),
                Tag = productId // Store the ProductId in the button's Tag
            };
            removeButton.Click += RemoveButton_Click;
            panel.Controls.Add(removeButton);

            return panel;
        }

        private void UpdatePriceButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int productId = (int)button.Tag;

            // Prompt user to enter a new price
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter new price:",
                "Update Price",
                "0.00");

            if (decimal.TryParse(input, out decimal newPrice) && newPrice > 0)
            {
                UpdatePriceInDatabase(productId, newPrice);

                // Refresh the UI
                LoadSellerProducts();
            }
            else
            {
                MessageBox.Show("Invalid price entered. Please try again.");
            }
        }

        // Method to update price in the database
        private void UpdatePriceInDatabase(int productId, decimal newPrice)
        {
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = "UPDATE Product SET Price = @Price WHERE ProductId = @ProductId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Price", newPrice);
                command.Parameters.AddWithValue("@ProductId", productId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }


        private void UpdateStockButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int productId = (int)button.Tag;

            // Prompt the seller to enter a new stock quantity
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter new stock quantity:",
                "Update Stock",
                "0");

            if (int.TryParse(input, out int newStockQuantity) && newStockQuantity >= 0)
            {
                string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
                string query = "UPDATE Stock SET Quantity = @Quantity WHERE ProductId = @ProductId";

                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Quantity", newStockQuantity);
                    command.Parameters.AddWithValue("@ProductId", productId);

                    connection.Open();
                    command.ExecuteNonQuery();
                    MessageBox.Show("Stock updated successfully!");

                    // Reload the products to refresh the panel
                    LoadSellerProducts();
                }
            }
            else
            {
                MessageBox.Show("Invalid stock quantity. Please enter a valid number.");
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int productId = (int)button.Tag;

            var confirmResult = MessageBox.Show(
                "Are you sure you want to remove this product?",
                "Confirm Remove",
                MessageBoxButtons.YesNo);

            if (confirmResult == DialogResult.Yes)
            {
                string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
                string query = "DELETE FROM Product WHERE ProductId = @ProductId";

                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);

                    connection.Open();
                    command.ExecuteNonQuery();
                    MessageBox.Show("Product removed successfully!");

                    // Reload the products to refresh the panel
                    LoadSellerProducts();
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        private void LoadPendingOrdersForSeller()
        {
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = @"
    SELECT 
        od.OrderId, 
        p.ProductId, 
        p.Name AS ProductName, 
        p.Price, 
        p.Brand 
    FROM 
        Orders o
        INNER JOIN OrderDetails od ON o.OrderId = od.OrderId
        INNER JOIN Product p ON od.ProductId = p.ProductId
    WHERE 
        o.OrderStatus = 'Pending' 
        AND p.SellerId = @SellerId and od.Status = 'Pending'";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@SellerId", UserSession.SellerId); // Use the logged-in seller's ID
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                flowLayoutPanel2.Controls.Clear(); // Clear existing panels

                while (reader.Read())
                {
                    int orderId = reader.GetInt32(0);
                    int productId = reader.GetInt32(1);
                    string productName = reader.GetString(2);
                    decimal price = reader.GetDecimal(3);
                    string brand = reader.GetString(4);

                    Panel productPanel = CreateProductPanelForOrder(orderId, productId, productName, price, brand);
                    flowLayoutPanel2.Controls.Add(productPanel);
                }
            }
        }

        private Panel CreateProductPanelForOrder(int orderId, int productId, string productName, decimal price, string brand)
        {
            Panel panel = new Panel
            {
                Size = new Size(300, 100),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Product Name Label
            Label nameLabel = new Label
            {
                Text = $"Name: {productName}",
                Location = new Point(10, 10),
                AutoSize = true
            };
            panel.Controls.Add(nameLabel);

            // Price Label
            Label priceLabel = new Label
            {
                Text = $"Price: ${price:F2}",
                Location = new Point(10, 35),
                AutoSize = true
            };
            panel.Controls.Add(priceLabel);

            // Brand Label
            Label brandLabel = new Label
            {
                Text = $"Brand: {brand}",
                Location = new Point(10, 60),
                AutoSize = true
            };
            panel.Controls.Add(brandLabel);

            // Accept Button
            Button acceptButton = new Button
            {
                Text = "Accept",
                Location = new Point(200, 10),
                Tag = new { OrderId = orderId, ProductId = productId } // Tag to hold relevant IDs
            };
            acceptButton.Click += AcceptButton_Click;
            panel.Controls.Add(acceptButton);

            // Reject Button
            Button rejectButton = new Button
            {
                Text = "Reject",
                Location = new Point(200, 50),
                Tag = new { OrderId = orderId, ProductId = productId } // Tag to hold relevant IDs
            };
            rejectButton.Click += RejectButton_Click;
            panel.Controls.Add(rejectButton);

            return panel;
        }

        private void RejectButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            dynamic data = button.Tag;
            int orderId = data.OrderId;
            int productId = data.ProductId;

            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string deleteQuery = "DELETE FROM OrderDetails WHERE OrderId = @OrderId AND ProductId = @ProductId";
            string checkOrderQuery = "SELECT COUNT(*) FROM OrderDetails WHERE OrderId = @OrderId";
            string updateOrderQuery = "UPDATE Orders SET OrderStatus = 'Rejected' WHERE OrderId = @OrderId";
            string notificationQuery = @"
        INSERT INTO sellerNotifications (CustomerId, Message) 
        VALUES (@CustomerId, @Message)";
            string productDetailsQuery = @"
        SELECT 
            o.CustomerId, 
            p.Name AS ProductName, 
            s.Name AS SellerName
        FROM 
            Orders o
        INNER JOIN 
            OrderDetails od ON o.OrderId = od.OrderId
        INNER JOIN 
            Product p ON od.ProductId = p.ProductId
        INNER JOIN 
            Seller s ON p.SellerId = s.SId
        WHERE 
            o.OrderId = @OrderId AND p.ProductId = @ProductId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Retrieve product and customer details for notification
                        int customerId;
                        string productName, sellerName;
                        using (SqlCommand productDetailsCommand = new SqlCommand(productDetailsQuery, connection, transaction))
                        {
                            productDetailsCommand.Parameters.AddWithValue("@OrderId", orderId);
                            productDetailsCommand.Parameters.AddWithValue("@ProductId", productId);

                            using (SqlDataReader reader = productDetailsCommand.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    customerId = reader.GetInt32(0); // CustomerId
                                    productName = reader.GetString(1); // ProductName
                                    sellerName = reader.GetString(2); // SellerName
                                }
                                else
                                {
                                    throw new Exception("Failed to retrieve product or customer details.");
                                }
                            }
                        }

                        // Delete the product from OrderDetails
                        using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection, transaction))
                        {
                            deleteCommand.Parameters.AddWithValue("@OrderId", orderId);
                            deleteCommand.Parameters.AddWithValue("@ProductId", productId);
                            deleteCommand.ExecuteNonQuery();
                        }

                        // Check if the order has any remaining products
                        using (SqlCommand checkCommand = new SqlCommand(checkOrderQuery, connection, transaction))
                        {
                            checkCommand.Parameters.AddWithValue("@OrderId", orderId);
                            int remainingProducts = (int)checkCommand.ExecuteScalar();

                            if (remainingProducts == 0)
                            {
                                // If no products remain, update the order status to 'Rejected'
                                using (SqlCommand updateCommand = new SqlCommand(updateOrderQuery, connection, transaction))
                                {
                                    updateCommand.Parameters.AddWithValue("@OrderId", orderId);
                                    updateCommand.ExecuteNonQuery();
                                }
                            }
                        }

                        // Insert notification for the customer
                        string message = $"Your order for '{productName}' was rejected by seller '{sellerName}'.";
                        using (SqlCommand notificationCommand = new SqlCommand(notificationQuery, connection, transaction))
                        {
                            notificationCommand.Parameters.AddWithValue("@CustomerId", customerId);
                            notificationCommand.Parameters.AddWithValue("@Message", message);
                            notificationCommand.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        MessageBox.Show("Product rejected successfully, and notification sent to the customer.");
                        LoadPendingOrdersForSeller(); // Reload pending orders
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }


        private void AcceptButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            dynamic data = button.Tag;
            int orderId = data.OrderId;
            int productId = data.ProductId;

            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string updateProductStatusQuery = "UPDATE OrderDetails SET Status = 'Accepted' WHERE OrderId = @OrderId AND ProductId = @ProductId";
            string checkAcceptedQuery = "SELECT COUNT(*) FROM OrderDetails WHERE OrderId = @OrderId AND Status = 'Accepted'";
            string checkRejectedQuery = "SELECT COUNT(*) FROM OrderDetails WHERE OrderId = @OrderId AND Status = 'Rejected'";
            string checkPendingQuery = "SELECT COUNT(*) FROM OrderDetails WHERE OrderId = @OrderId AND Status = 'Pending'";
            string updateOrderQuery = "UPDATE Orders SET OrderStatus = 'Processing' WHERE OrderId = @OrderId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Update the product status to 'Accepted' in OrderDetails
                        using (SqlCommand updateStatusCommand = new SqlCommand(updateProductStatusQuery, connection, transaction))
                        {
                            updateStatusCommand.Parameters.AddWithValue("@OrderId", orderId);
                            updateStatusCommand.Parameters.AddWithValue("@ProductId", productId);
                            updateStatusCommand.ExecuteNonQuery();
                        }

                        // Check the status of all products in the order
                        int acceptedCount = 0, rejectedCount = 0, pendingCount = 0;

                        // Count accepted products
                        using (SqlCommand checkAcceptedCommand = new SqlCommand(checkAcceptedQuery, connection, transaction))
                        {
                            checkAcceptedCommand.Parameters.AddWithValue("@OrderId", orderId);
                            acceptedCount = (int)checkAcceptedCommand.ExecuteScalar();
                        }

                        // Count rejected products
                        using (SqlCommand checkRejectedCommand = new SqlCommand(checkRejectedQuery, connection, transaction))
                        {
                            checkRejectedCommand.Parameters.AddWithValue("@OrderId", orderId);
                            rejectedCount = (int)checkRejectedCommand.ExecuteScalar();
                        }

                        // Count pending products
                        using (SqlCommand checkPendingCommand = new SqlCommand(checkPendingQuery, connection, transaction))
                        {
                            checkPendingCommand.Parameters.AddWithValue("@OrderId", orderId);
                            pendingCount = (int)checkPendingCommand.ExecuteScalar();
                        }

                        // Update the order status to 'Processing' if conditions are met
                        if (pendingCount == 0 && acceptedCount > 0 && rejectedCount > 0)
                        {
                            using (SqlCommand updateOrderCommand = new SqlCommand(updateOrderQuery, connection, transaction))
                            {
                                updateOrderCommand.Parameters.AddWithValue("@OrderId", orderId);
                                updateOrderCommand.ExecuteNonQuery();
                            }
                        }

                        // Commit the transaction if all operations succeed
                        transaction.Commit();
                        MessageBox.Show($"Product with ID {productId} from Order {orderId} accepted.");
                        LoadPendingOrdersForSeller(); // Reload the pending orders for the seller
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction if an error occurs
                        transaction.Rollback();
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }



        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            addProduct prod = new addProduct(this);
            prod.Show();
            this.Hide();
        }
    }
}
