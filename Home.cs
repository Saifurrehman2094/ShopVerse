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
    public partial class Home : Form
    {
        Cartitem cart;
        WishList wishList;
        Review reviewItem;
        public Home()
        {
            InitializeComponent();
            LoadCategoriesOrBrands();
            //flowLayoutPanel1.Controls.Add(CreateProductPanel("Product",2000, "C:\\Users\\saifs\\OneDrive\\Pictures\\Saved Pictures\\watch.png"));
            LoadProducts(flowLayoutPanel1);
            cart = new Cartitem(this);
            wishList = new WishList(this,cart);
            SearchBox.TextChanged += SearchBox_TextChanged;
        }

        private void LoadCategoriesOrBrands()
        {
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = "SELECT name FROM Category"; // Fetch category names

            comboBox1.Items.Clear();
            comboBox1.Items.Add("None"); // Add "None" option to clear the filter

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    comboBox1.Items.Add(reader["name"].ToString()); // Add categories to ComboBox
                }
            }

            comboBox1.SelectedIndex = 0; // Set the default selection to "None"
        }


        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Prod1Namepanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel10_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel14_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel26_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel23_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel18_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel8_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Home_Load(object sender, EventArgs e)
        {

        }



        private void LoadProducts(FlowLayoutPanel parentPanel, string searchQuery = "", string filter = "")
        {
            // Replace with your actual database connection string
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";

            // Base query to retrieve product details, now joining with Category table
            string query = @"
        SELECT p.ProductId, p.name, p.price, p.imagepath, c.name AS CategoryName 
        FROM Product p
        INNER JOIN Category c ON p.CategoryId = c.CategoryId"; // Join with Category table

            // Add filtering conditions for search and selected filter
            bool hasSearchQuery = !string.IsNullOrEmpty(searchQuery);
            bool hasFilter = !string.IsNullOrEmpty(filter) && filter != "None"; // Check that filter is not "None"

            if (hasSearchQuery || hasFilter)
            {
                query += " WHERE";
            }

            // Apply search query filter if it's not empty
            if (hasSearchQuery)
            {
                query += " p.name LIKE @searchQuery";
            }

            // Apply the filter from comboBox1 (Category) if it's selected
            if (hasFilter)
            {
                if (hasSearchQuery) query += " AND";
                query += " c.name = @filter"; // Filter based on category name
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                // Add parameters to prevent SQL injection
                if (hasSearchQuery)
                {
                    command.Parameters.AddWithValue("@searchQuery", "%" + searchQuery + "%");
                }

                if (hasFilter && filter != "None")
                {
                    command.Parameters.AddWithValue("@filter", filter);
                }

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    // Clear previous results from FlowLayoutPanel
                    parentPanel.Controls.Clear();

                    while (reader.Read())
                    {
                        // Retrieve data from the database
                        string name = reader["name"].ToString();
                        decimal price = Convert.ToDecimal(reader["price"]);
                        string imagePath = reader["imagepath"].ToString();
                        int id = Convert.ToInt32(reader["ProductId"]);

                        // Create a product panel using the CreateProductPanel function
                        Panel productPanel = CreateProductPanel(name, price, imagePath, id);

                        // Add the panel to the parent container
                        parentPanel.Controls.Add(productPanel);
                    }
                }
            }
        }



        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            string searchQuery = SearchBox.Text; // Get the search query text
            string selectedFilter = comboBox1.SelectedItem.ToString(); // Get selected category filter

            // Load products based on the current search query and selected filter
            LoadProducts(flowLayoutPanel1, searchQuery, selectedFilter);
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedFilter = comboBox1.SelectedItem.ToString();
            string searchQuery = SearchBox.Text; // Assuming you have a searchBox TextBox

            // Load products based on the current search query and selected filter
            LoadProducts(flowLayoutPanel1, searchQuery, selectedFilter);
        }

        private Panel CreateProductPanel(string name, decimal price, string imagePath,int Productid)
        {
            
            // Create a new panel to serve as the container
            Panel productPanel = new Panel
            {
                Size = new Size(250, 400),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10)
            };

            // Add PictureBox for the product image
            PictureBox productImage = new PictureBox
            {
                Size = new Size(230, 150),
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
                    productImage.Image = SystemIcons.Error.ToBitmap(); // Placeholder for missing image
                }
            }
            catch
            {
                productImage.Image = SystemIcons.Error.ToBitmap(); // Handle any errors loading the image
            }

            // Add Label for the product name
            Label nameLabel = new Label
            {
                Text = name,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(10, 170),
                AutoSize = true
            };

            // Add Label for the product price
            Label priceLabel = new Label
            {
                Text = $"Price: ${price:F2}",
                Font = new Font("Arial", 10, FontStyle.Regular),
                Location = new Point(10, 200),
                AutoSize = true
            };

            // Add Button for adding to cart
            Button addToCartButton = new Button
            {
                Text = "Add to Cart",
                Location = new Point(10, 300),
                Size = new Size(230, 40),
                BackColor = Color.LightGreen
            };
            addToCartButton.Click += (sender, e) =>
            {
                if (!(cart.IsProductAlreadyInCart((int)productPanel.Tag)))
                {
                    MessageBox.Show($"Added {name} to cart!");
                }
                cart.AddToCart(name, price, imagePath, (int)productPanel.Tag);

            };

            Button addToWishlist = new Button
            {
                Text = "Add to WishList",
                Location = new Point(10, 350),
                Size = new Size(230, 40),
                BackColor = Color.Red
            };
            addToWishlist.Click += (sender, e) =>
            {
                if (!(wishList.IsProductAlreadyInWishlist((int)productPanel.Tag)))
                {
                    MessageBox.Show($"Added {name} to Wishlist!");
                }
                wishList.AddProductToWishlistPanel(name,price, imagePath,(int)productPanel.Tag);
            };

           

            Button Details = new Button
            {
                Text = "Details",
                Location = new Point(160, 250),
                Size = new Size(80, 50),
                BackColor = Color.Black,
                ForeColor = Color.White
            };
            Details.Click += (sender, e) =>
            {
                MessageBox.Show($"Details Clicked");
                reviewItem = new Review(this,Productid);
                //reviewItem.pictureBox1.Image = Image.FromFile(imagePath);
                try
                {
                    if (System.IO.File.Exists(imagePath))
                    {
                        reviewItem.pictureBox1.Image = Image.FromFile(imagePath);
                    }
                    else
                    {
                        reviewItem.pictureBox1.Image = SystemIcons.Error.ToBitmap(); // Placeholder for missing image
                    }
                }
                catch
                {
                    reviewItem.pictureBox1.Image = SystemIcons.Error.ToBitmap(); // Handle any errors loading the image
                }
                reviewItem.ProductNameLabel.Text = name;
                reviewItem.Price.Text = price.ToString();
                string brand = "";
                string Desc = "";
                string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
                int p = Productid;  // Assuming Productid is already defined

                // SQL query to extract Brand and Description
                string query = "SELECT brand, description FROM Product WHERE ProductId = @p";

                // Open the connection and execute the query
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        // Open the connection to the database
                        connection.Open();

                        // Create a SqlCommand to execute the query
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            // Add the parameter for ProductId
                            command.Parameters.AddWithValue("@p", p);

                            // Execute the command and read the results using SqlDataReader
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                // Check if there are any rows returned
                                if (reader.Read())  // There's only one record for the specific ProductId
                                {
                                    // Retrieve values from the database and store in variables
                                    brand = reader["brand"].ToString();
                                    Desc = reader["description"].ToString();
                                }
                                else
                                {
                                    MessageBox.Show("No product found with the provided ProductId.");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Show any errors that occur during the database operation
                        MessageBox.Show("Error retrieving product details: " + ex.Message);
                    }
                }

                // Output the brand and description values (just to verify or for testing)
                

                reviewItem.Brand.Text = brand;
                reviewItem.label6.Text = Desc;
                reviewItem.Show();

            };

            // Add all controls to the panel
            productPanel.Controls.Add(productImage);
            productPanel.Controls.Add(nameLabel);
            productPanel.Controls.Add(priceLabel);
            productPanel.Controls.Add(addToCartButton);
            productPanel.Controls.Add(addToWishlist);
            productPanel.Controls.Add(Details);
            productPanel.Tag = Productid;

            return productPanel;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            
            cart.Show();
            this.Hide();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Hide();
            ProfileEdit p = new ProfileEdit(this);
            p.Show();
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //int customerId = UserSession.CustomerId; // Retrieve CustomerId from UserSession
            //int wishlistId = ; // Variable to store the most recent Wishlist ID

            //// Database connection string
            //string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";

            //using (SqlConnection connection = new SqlConnection(connectionString))
            //{
            //    try
            //    {
            //        connection.Open();

            //        // Step 1: Retrieve the most recent Wishlist ID for the customer
            //        string queryWishlist = "SELECT TOP(1) WID FROM Wishlist WHERE CustomerId = @customerId ORDER BY WID DESC";

            //        using (SqlCommand command = new SqlCommand(queryWishlist, connection))
            //        {
            //            command.Parameters.AddWithValue("@customerId", customerId);

            //            object result = command.ExecuteScalar();
            //            if (result != null)
            //            {
            //                wishlistId = Convert.ToInt32(result);
            //            }
            //            else
            //            {
            //                MessageBox.Show("No wishlist found for the customer.");
            //                return;
            //            }
            //        }

            //        // Step 2: Retrieve Product IDs from WishlistItems for the retrieved Wishlist ID
            //        List<int> productIds = new List<int>();
            //        string queryWishlistItems = "SELECT ProductId FROM WishlistItems WHERE WID = @wishlistId";

            //        using (SqlCommand command = new SqlCommand(queryWishlistItems, connection))
            //        {
            //            command.Parameters.AddWithValue("@wishlistId", wishlistId);

            //            using (SqlDataReader reader = command.ExecuteReader())
            //            {
            //                while (reader.Read())
            //                {
            //                    productIds.Add(reader.GetInt32(0)); // Add ProductId to the list
            //                }
            //            }
            //        }

            //        // Step 3: Fetch product details and populate the FlowLayoutPanel
            //        FetchAndDisplayProducts(productIds, connection);
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show("Error loading wishlist: " + ex.Message);
            //    }
            //}
            this.Hide();
            wishList.Show();
        }
        private void FetchAndDisplayProducts(List<int> productIds, SqlConnection connection)
        {
            //flowLayoutPanel1.Controls.Clear(); // Clear existing controls

            foreach (int productId in productIds)
            {
                // Fetch product details for each ProductId
                string queryProductDetails = "SELECT Name, Price, ImagePath FROM Product WHERE ProductId = @productId";
                using (SqlCommand command = new SqlCommand(queryProductDetails, connection))
                {
                    command.Parameters.AddWithValue("@productId", productId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string name = reader["Name"].ToString();
                            decimal price = Convert.ToDecimal(reader["Price"]);
                            string imagePath = reader["ImagePath"].ToString();

                            // Add the product to the wishlist panel using the existing method
                            wishList.AddProductToWishlistPanel(name, price, imagePath, productId);
                        }
                    }
                }
            }
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            CustomerNotify customerNotify = new CustomerNotify();
            customerNotify.Show();
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            History history = new History();
            history.Show();
        }
    }

}
