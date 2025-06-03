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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using Microsoft.Data.SqlClient;
using ShopVerse.Helpers;
using System.Drawing.Drawing2D;

namespace ShopVerse
{
    public partial class Ord : Form
    {
        private FlowLayoutPanel productListPanel;
        private ComboBox logisticsComboBox;
        private ComboBox paymentComboBox;
        private Button saveButton;
        private Button cancelButton;
        public Ord(List<Panel> cartPanels)
        {
            InitializeComponent();
            //this.Size = new Size(700, 600);

            // FlowLayoutPanel to list products
            productListPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(10),
                WrapContents = false
            };

            this.Controls.Add(productListPanel);

            // Add products to the FlowLayoutPanel
            foreach (Panel cartPanel in cartPanels)
            {
                AddProductToOrderList(cartPanel);
            }

            // ComboBox for Logistics
            logisticsComboBox = new ComboBox
            {
                Location = new Point(20, productListPanel.Bottom + 20),
                Width = 300
            };
            this.Controls.Add(logisticsComboBox);

            // Populate logistics options from the database
            PopulateLogisticsComboBox();

            // ComboBox for Payment Method
            paymentComboBox = new ComboBox
            {
                Location = new Point(20, logisticsComboBox.Bottom + 20),
                Width = 300,
                Items = { "Cash on Delivery", "Online Payment" }
            };
            this.Controls.Add(paymentComboBox);

            // Save Button
            saveButton = new Button
            {
                Text = "Confirm Order",
                BackColor = Color.LightGreen,
                Location = new Point(20, paymentComboBox.Bottom + 20),
                Width = 150
            };
            saveButton.Click += SaveOrderToDatabase;
            this.Controls.Add(saveButton);

            // Cancel Button
            cancelButton = new Button
            {
                Text = "Cancel Order",
                BackColor = Color.LightCoral,
                Location = new Point(200, paymentComboBox.Bottom + 20),
                Width = 150
            };
            cancelButton.Click += CancelOrder;
            this.Controls.Add(cancelButton);
        }
        private void AddProductToOrderList(Panel cartPanel)
        {
            // Extract information from the cart panel
            string productName = cartPanel.Controls.OfType<Label>().FirstOrDefault(l => l.Font.Bold)?.Text;
            decimal price = decimal.Parse(cartPanel.Controls.OfType<Label>().FirstOrDefault(l => l.Text.Contains("Price"))?.Text.Replace("Price: $", ""));
            int productId = (int)cartPanel.Tag;
            TextBox quantityTextBox = cartPanel.Controls.OfType<TextBox>().FirstOrDefault();
            int quantity = int.TryParse(quantityTextBox?.Text, out var q) ? q : 1;

            // Create a panel to display in the order list
            Panel productPanel = new Panel
            {
                Size = new Size(600, 40),
                Tag = productId
            };

            // Add Product Name Label
            Label nameLabel = new Label
            {
                Text = productName,
                Location = new Point(10, 10),
                AutoSize = true
            };
            productPanel.Controls.Add(nameLabel);

            Label quantityLabel = new Label
            {
                Text = quantity.ToString(),
                Location = new Point(10, 25),
                AutoSize = true
            };
            productPanel.Controls.Add(quantityLabel);

            // Add Price Label
            Label priceLabel = new Label
            {
                Text = $"Price: ${price:F2}",
                Location = new Point(200, 10),
                AutoSize = true
            };
            productPanel.Controls.Add(priceLabel);

            // Add Remove Button
            Button removeButton = new Button
            {
                Text = "Remove",
                Location = new Point(400, 10),
                BackColor = Color.LightCoral
            };
            removeButton.Click += (s, e) =>
            {
                productListPanel.Controls.Remove(productPanel); // Remove from the UI
            };
            productPanel.Controls.Add(removeButton);

            // Add the product panel to the order list
            productListPanel.Controls.Add(productPanel);
        }

        private void PopulateLogisticsComboBox()
        {
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = "SELECT LID, CompanyName FROM Logistics";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        logisticsComboBox.Items.Add(new ComboBoxItem
                        {
                            Text = reader["CompanyName"].ToString(),
                            Value = Convert.ToInt32(reader["LID"])
                        });
                    }
                }
            }

            if (logisticsComboBox.Items.Count > 0)
            {
                logisticsComboBox.SelectedIndex = 0;
            }
        }

        private void SaveOrderToDatabase(object sender, EventArgs e)
        {
            // Check if inputs are valid
            if (productListPanel.Controls.Count == 0)
            {
                MessageBox.Show("No products in the order.");
                return;
            }

            if (logisticsComboBox.SelectedItem == null || paymentComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select logistics and payment method.");
                return;
            }

            int cartId = CreateCartInDatabase(); // Step 1: Create Cart
            AddCartItemsToDatabase(cartId);      // Step 2: Add Cart Items
            SaveOrder(cartId);                   // Step 3: Save Order
            
            MessageBox.Show("Order successfully placed!");
            this.Close();
        }

        private int CreateCartInDatabase()
        {
            decimal total = CalculateTotalPrice();
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = "INSERT INTO Cart (Date, TotalPrice, CustomerId) OUTPUT INSERTED.CartId VALUES (GETDATE(), @total, @CustomerId)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CustomerId", UserSession.CustomerId);
                command.Parameters.AddWithValue("@total", total);
                connection.Open();
                return (int)command.ExecuteScalar(); // Return the newly created CartId
            }
        }

        private void AddCartItemsToDatabase(int cartId)
        {
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = "INSERT INTO CartItems (CartId, ProductId, Quantity) VALUES (@CartId, @ProductId, @Quantity)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (Panel productPanel in productListPanel.Controls)
                {
                    int productId = (int)productPanel.Tag;
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CartId", cartId);
                        command.Parameters.AddWithValue("@ProductId", productId);
                        command.Parameters.AddWithValue("@Quantity", 1); // Default quantity
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private void AddOrderDetailsToDatabase(int orderId)
        {
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = "INSERT INTO OrderDetails (OrderId, ProductId, Quantity, Price) VALUES (@OrderId, @ProductId, @Quantity, @Price)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (Panel productPanel in productListPanel.Controls)
                {
                    int productId = (int)productPanel.Tag; // Assuming product ID is stored in the Tag property
                    decimal productPrice = GetProductPrice(productId); // Method to fetch product price
                    TextBox quantityTextBox = productPanel.Controls.OfType<TextBox>().FirstOrDefault();
                    int quantity = int.TryParse(quantityTextBox?.Text, out var q) ? q : 1;

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OrderId", orderId);
                        command.Parameters.AddWithValue("@ProductId", productId);
                        command.Parameters.AddWithValue("@Quantity", quantity);
                        command.Parameters.AddWithValue("@Price", productPrice);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        // Example helper method to retrieve the product price
        private decimal GetProductPrice(int productId)
        {
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = "SELECT Price FROM Product WHERE ProductId = @ProductId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);
                    object result = command.ExecuteScalar();
                    return result != null ? Convert.ToDecimal(result) : 0;
                }
            }
        }


        private void SaveOrder(int cartId)
        {
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = @"
    INSERT INTO Orders (OrderDate, DelieveryDate, Payment, CustomerId, LID, OrderStatus)
    VALUES (GETDATE(), DATEADD(DAY, 7, GETDATE()), @Payment, @CustomerId, @LID, @OrderStatus);
    SELECT SCOPE_IDENTITY();"; // Retrieve the newly inserted OrderId

            int orderId;

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                ComboBoxItem selectedLogistics = (ComboBoxItem)logisticsComboBox.SelectedItem;

                command.Parameters.AddWithValue("@Payment", paymentComboBox.SelectedItem.ToString());
                command.Parameters.AddWithValue("@CustomerId", UserSession.CustomerId);
                command.Parameters.AddWithValue("@LID", selectedLogistics.Value);
                command.Parameters.AddWithValue("@OrderStatus", "Pending");

                connection.Open();

                // Execute the query and retrieve the OrderId
                orderId = Convert.ToInt32(command.ExecuteScalar());
            }

            // Call AddOrderDetailsToDatabase with the retrieved OrderId
            AddOrderDetailsToDatabase(orderId);
        }


        private decimal CalculateTotalPrice()
        {
            decimal totalPrice = 0;

            foreach (Panel panel in productListPanel.Controls)
            {
                Label priceLabel = panel.Controls.OfType<Label>().FirstOrDefault(label => label.Text.StartsWith("Price:"));
                if (priceLabel != null)
                {
                    string priceText = priceLabel.Text.Replace("Price: $", "").Trim();
                    if (decimal.TryParse(priceText, out decimal price))
                    {
                        // Find the quantity TextBox
                        TextBox quantityTextBox = panel.Controls.OfType<TextBox>().FirstOrDefault();
                        int quantity = int.TryParse(quantityTextBox?.Text, out var q) ? q : 1;

                        totalPrice += price * quantity;
                    }
                }
            }

            return totalPrice;
        }
        private void CancelOrder(object sender, EventArgs e)
        {
            MessageBox.Show("Order canceled.");
            this.Close();
        }
        public class ComboBoxItem
        {
            public string Text { get; set; }
            public int Value { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }
        private void panel3_Paint(object sender, PaintEventArgs e)
        {
        }

        private void Ord_Load(object sender, EventArgs e)
        {

        }
    }
}
