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
    public partial class Cartitem : Form
    {
        Ord ord;
        Home h;
        public Cartitem(Home home)
        {
            InitializeComponent();
            h = home;
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void Cartitem_Load(object sender, EventArgs e)
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
           
        }

        public bool IsProductAlreadyInCart(int productId)
        {
            foreach (Panel panel in flowLayoutPanel1.Controls)
            {

                if (panel.Tag != null && (int)panel.Tag == productId)
                {
                    return true;
                }
            }
            return false;
        }

        public void RemoveFromCart(Panel cartItemPanel)
        {
            // Remove the panel from the FlowLayoutPanel
            flowLayoutPanel1.Controls.Remove(cartItemPanel);
            cartItemPanel.Dispose();
        }
        public void AddToCart(string name, decimal price, string imagePath,int id)
        {

            if (IsProductAlreadyInCart(id))
            {
                MessageBox.Show($"Product {name} is already in cart"); ;
                return;
            }

            // Create and add the product panel
            Panel cartItemPanel = CreateCartProductPanel(name, price, imagePath, RemoveFromCart,id);
            flowLayoutPanel1.Controls.Add(cartItemPanel);
        }
        private Panel CreateCartProductPanel(string name, decimal price, string imagePath, Action<Panel> onRemove,int id)
        {
            // Create a new panel to represent the cart item
            Panel cartProductPanel = new Panel
            {
                Size = new Size(400, 100),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(30),
                Tag = id,
            };

            // PictureBox for product image
            PictureBox productImage = new PictureBox
            {
                Size = new Size(80, 80),
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            try
            {
                if (System.IO.File.Exists(imagePath))
                {
                    productImage.Image = Image.FromFile(imagePath);
                }
                else
                {
                    productImage.Image = SystemIcons.Warning.ToBitmap(); // Placeholder for missing images
                }
            }
            catch
            {
                productImage.Image = SystemIcons.Warning.ToBitmap(); // Error fallback
            }

            // Label for product name
            Label nameLabel = new Label
            {
                Text = name,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Location = new Point(100, 10),
                AutoSize = true
            };

            // Label for product price
            Label priceLabel = new Label
            {
                Text = $"Price: ${price:F2}",
                Font = new Font("Arial", 9, FontStyle.Regular),
                Location = new Point(100, 40),
                AutoSize = true
            };

            // TextBox for quantity
            TextBox quantityTextBox = new TextBox
            {
                Text = "1",
                Location = new Point(300, 10),
                Size = new Size(40, 20),
                TextAlign = HorizontalAlignment.Center
            };

            // Button to remove the item from the cart
            Button removeButton = new Button
            {
                Text = "Remove",
                BackColor = Color.LightCoral,
                Location = new Point(300, 50),
                Size = new Size(80, 30)
            };
            removeButton.Click += (sender, e) =>
            {
                onRemove?.Invoke(cartProductPanel); // Invoke the callback to handle removal
                if (flowLayoutPanel1.Controls.Count == 0) {
                    MessageBox.Show("Cart is Dropped");
                }
            };

            // Add controls to the panel
            cartProductPanel.Controls.Add(productImage);
            cartProductPanel.Controls.Add(nameLabel);
            cartProductPanel.Controls.Add(priceLabel);
            cartProductPanel.Controls.Add(quantityTextBox);
            cartProductPanel.Controls.Add(removeButton);

            return cartProductPanel;
        }

        public FlowLayoutPanel getPanel() {
            return flowLayoutPanel1 as FlowLayoutPanel;
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private decimal CalculateTotalPrice()
        {
            decimal totalPrice = 0;

            foreach (Panel panel in flowLayoutPanel1.Controls)
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

       

        private void button1_Click_1(object sender, EventArgs e)
        {
            if(flowLayoutPanel1.Controls.Count == 0)
            {
                MessageBox.Show("Cart is Empty cant proceed with empty cart");
                return;
            }

            // Database connection string
            //string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3 ;Integrated Security=True;Encrypt=False";

            //using (SqlConnection connection = new SqlConnection(connectionString))
            //{
            //    connection.Open();

            //    using (SqlTransaction transaction = connection.BeginTransaction())
            //    {
            //        try
            //        {
            //            // Step 1: Insert into the Cart table
            //            string insertCartQuery = @"
            //        INSERT INTO Cart (Date, TotalPrice, CustomerId)
            //        OUTPUT INSERTED.CartId
            //        VALUES (@Date, @TotalPrice, @CustomerId)";

            //            decimal totalPrice = CalculateTotalPrice(); // Calculate total price from panels
            //            int customerId = GetCurrentCustomerId();    // Replace with logic to get the current customer ID
            //            int cartId;

            //            using (SqlCommand cartCommand = new SqlCommand(insertCartQuery, connection, transaction))
            //            {
            //                cartCommand.Parameters.AddWithValue("@Date", DateTime.Now);
            //                cartCommand.Parameters.AddWithValue("@TotalPrice", totalPrice);
            //                cartCommand.Parameters.AddWithValue("@CustomerId", customerId);

            //                // Retrieve the newly inserted CartId
            //                cartId = (int)cartCommand.ExecuteScalar();
            //            }

            //            // Step 2: Insert into the CartItems table
            //            string insertCartItemQuery = @"
            //        INSERT INTO CartItems (CartId, ProductId, Quantity)
            //        VALUES (@CartId, @ProductId, @Quantity)";

            //            using (SqlCommand cartItemCommand = new SqlCommand(insertCartItemQuery, connection, transaction))
            //            {
            //                foreach (Panel panel in flowLayoutPanel1.Controls)
            //                {
            //                    // Assuming ProductId is stored in the Panel.Tag and quantity in a TextBox
            //                    if (panel.Tag is int productId)
            //                    {
            //                        // Find the quantity TextBox inside the panel
            //                        TextBox quantityTextBox = panel.Controls.OfType<TextBox>().FirstOrDefault();
            //                        int quantity = int.TryParse(quantityTextBox?.Text, out var q) ? q : 1;

            //                        cartItemCommand.Parameters.Clear();
            //                        cartItemCommand.Parameters.AddWithValue("@CartId", cartId);
            //                        cartItemCommand.Parameters.AddWithValue("@ProductId", productId);
            //                        cartItemCommand.Parameters.AddWithValue("@Quantity", quantity);

            //                        cartItemCommand.ExecuteNonQuery();
            //                    }
            //                }
            //            }

            //            // Commit the transaction
            //            transaction.Commit();

            //            MessageBox.Show("Cart and cart items added successfully.");
            //            flowLayoutPanel1.Controls.Clear();
            //        }
            //        catch (Exception ex)
            //        {
            //            // Rollback in case of an error
            //            transaction.Rollback();
            //            MessageBox.Show("An error occurred: " + ex.Message);
            //        }
            //    }
            //}
            List<Panel> cartPanels = new List<Panel>();

            // Loop through all controls in the FlowLayoutPanel (cart items)
            foreach (Control control in flowLayoutPanel1.Controls)
            {
                if (control is Panel panel)
                {
                    cartPanels.Add(panel);
                }
            }

            // Open the OrderForm and pass the list of cart panels
            Ord orderForm = new Ord(cartPanels);
            orderForm.ShowDialog();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            this.Hide();
            h.Show();
        }
    }
}
