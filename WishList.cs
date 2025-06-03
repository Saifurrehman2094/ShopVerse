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
    public partial class WishList : Form
    {
        Home h;
        Cartitem cart;
        public WishList(Home home, Cartitem cart)
        {
            InitializeComponent();
            h = home;
            this.cart = cart;
        }

        public void AddProductToWishlistPanel(string name, decimal price, string imagePath, int productId)
        {
            if (IsProductAlreadyInWishlist(productId))
            {
                MessageBox.Show("This product is already in the cart.");
                return;
            }
            // Create a new panel for the wishlist item
            Panel wishlistItemPanel = new Panel
            {
                Size = new Size(250, 350),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10),
                Tag = productId // Tag to store product ID
            };

            // Add the product image
            PictureBox wishlistImage = new PictureBox
            {
                Size = new Size(230, 150),
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            try
            {
                if (System.IO.File.Exists(imagePath))
                {
                    wishlistImage.Image = Image.FromFile(imagePath);
                    wishlistImage.ImageLocation = imagePath;
                }
                else
                {
                    wishlistImage.Image = SystemIcons.Error.ToBitmap(); // Placeholder for missing image
                }
            }
            catch
            {
                wishlistImage.Image = SystemIcons.Error.ToBitmap(); // Handle any errors loading the image
            }

            // Add product name label
            Label wishlistNameLabel = new Label
            {
                Text = name,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(10, 170),
                AutoSize = true,
                Name = "wishlistNameLabel"
            };

            // Add product price label
            Label wishlistPriceLabel = new Label
            {
                Text = $"Price: ${price:F2}",
                Font = new Font("Arial", 10, FontStyle.Regular),
                Location = new Point(10, 200),
                AutoSize = true,
                Name = "wishlistPriceLabel"
            };
            Button addToCartButton = new Button
            {
                Text = "Add to Cart",
                Location = new Point(10, 300),
                Size = new Size(230, 40),
                BackColor = Color.LightGreen
            };
            addToCartButton.Click += (a, b) =>
            {
                cart.AddToCart(name, price, imagePath, productId);
                flowLayoutPanel1.Controls.Remove(wishlistItemPanel);
            };

            // Add the "Remove" button
            Button removeButton = new Button
            {
                Text = "Remove",
                Location = new Point(10, 240),
                Size = new Size(230, 40),
                BackColor = Color.Red
            };

            removeButton.Click += (removeSender, removeE) =>
            {
                // Remove the product panel from the wishlist
                flowLayoutPanel1.Controls.Remove(wishlistItemPanel);
            };

            // Add all the controls to the wishlist item panel
            wishlistItemPanel.Controls.Add(wishlistImage);
            wishlistItemPanel.Controls.Add(wishlistNameLabel);
            wishlistItemPanel.Controls.Add(wishlistPriceLabel);
            wishlistItemPanel.Controls.Add(addToCartButton);
            wishlistItemPanel.Controls.Add(removeButton);

            // Add the panel to the FlowLayoutPanel in the WishlistForm
            flowLayoutPanel1.Controls.Add(wishlistItemPanel);
        }

        public bool IsProductAlreadyInWishlist(int productId)
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

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            h.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //if (flowLayoutPanel1.Controls.Count == 0)
            //{
            //    MessageBox.Show("Wishlist is Empty cant proceed with empty cart");
            //    return;
            //}

            //string name = "";
            //decimal price = 0;
            //string imagepath = "";
            //// Loop through all controls in the FlowLayoutPanel (cart items)
            //foreach (Control control in flowLayoutPanel1.Controls)
            //{
            //    if (control is Panel panel)
            //    {
            //        foreach (Control cont in panel.Controls)
            //        {
            //            if (cont is Label label && label.Name == "wishlistNameLabel")
            //            {
            //                name = cont.Text;
            //            }
            //            else if (cont is Label label1 && label1.Name == "wishlistPriceLabel")
            //            {
            //                string text = label1.Text; // "Price: $123.45"

            //                // Remove "Price: $" and parse the decimal value
            //                string priceText = text.Replace("Price: $", "");

            //                if (decimal.TryParse(priceText, out decimal extractedPrice))
            //                {
            //                    price = extractedPrice; // Successfully extracted the price
            //                }
            //                else
            //                {
            //                    // Handle invalid price format if needed
            //                    MessageBox.Show("Invalid price format.");
            //                }
            //            }
            //            else if (cont is PictureBox p)
            //            {
            //                imagepath = p.ImageLocation;
            //                //MessageBox.Show(imagepath);
            //            }
            //        }
            //        //cart.AddToCart(name, price, imagepath,(int)panel.Tag);

            //        flowLayoutPanel1.Controls.Remove(panel);
            //    }
            //}

            List<int> selectedProductIds = new List<int>();
            foreach (Control panel in flowLayoutPanel1.Controls)
            {
                if (panel is Panel && panel.Tag is int productId)
                {
                    selectedProductIds.Add(productId);
                }
            }

            if (selectedProductIds.Count == 0)
            {
                MessageBox.Show("No products selected for the wishlist.");
                return;
            }

            // Step 2: Retrieve CustomerId and current date
            int customerId = UserSession.CustomerId; // Assuming UserSession class is set
            DateTime currentDate = DateTime.Now;

            // Step 3: Insert into Wishlist and WishlistItems
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Begin a transaction
                    SqlTransaction transaction = connection.BeginTransaction();

                    try
                    {
                        // Insert into Wishlist
                        string insertWishlistQuery = "INSERT INTO Wishlist (Date, CustomerId) OUTPUT INSERTED.WID VALUES (@date, @customerId)";
                        int wishlistId;

                        using (SqlCommand cmd = new SqlCommand(insertWishlistQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@date", currentDate);
                            cmd.Parameters.AddWithValue("@customerId", customerId);

                            wishlistId = (int)cmd.ExecuteScalar(); // Retrieve the newly generated WID
                        }

                        // Insert into WishlistItems
                        string insertWishlistItemsQuery = "INSERT INTO WishlistItems (WID, ProductId) VALUES (@wishlistId, @productId)";

                        using (SqlCommand cmd = new SqlCommand(insertWishlistItemsQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@wishlistId", wishlistId);

                            foreach (int productId in selectedProductIds)
                            {
                                cmd.Parameters.AddWithValue("@productId", productId);
                                cmd.ExecuteNonQuery();
                                cmd.Parameters.RemoveAt("@productId");
                            }
                        }

                        // Commit the transaction
                        transaction.Commit();

                        MessageBox.Show("Wishlist successfully saved!");
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction if an error occurs
                        transaction.Rollback();
                        MessageBox.Show("Error saving wishlist: " + ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error connecting to the database: " + ex.Message);
                }
            }
            flowLayoutPanel1.Controls.Clear();
            this.Hide();
            h.Show() ;
        }

        private void WishList_Load(object sender, EventArgs e)
        {

        }
    }
}
