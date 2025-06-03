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

    public partial class LogisticsOrder : Form
    {
        
        public LogisticsOrder()
        {
            InitializeComponent();
        }

        private void LogisticsOrder_Load(object sender, EventArgs e)
        {
            UpdateShipStatusAutomatically();
            LoadShippedOrders();

            // Load pending orders (after updating statuses)
            LoadPendingOrders();
        }

        public FlowLayoutPanel GetPanel()
        {
            return flowLayoutPanel1;
        }
        public FlowLayoutPanel GetPanel2()
        {
            return flowLayoutPanel2;
        }
        private void UpdateShipStatusAutomatically()
        {
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";

            // Step 1: Update the Order status
            string updateOrderQuery = @"
    DECLARE @OutputTable TABLE (OrderId INT, CustomerId INT);

-- Capture the output into a temporary table
UPDATE Orders
SET Orderstatus = 'Delivered'
OUTPUT INSERTED.OrderId, INSERTED.CustomerId INTO @OutputTable
WHERE DelieveryDate <= GETDATE() AND Orderstatus = 'Processing';

";

            // Step 2: Insert notifications for the updated orders
            string insertNotificationQuery = @"
    INSERT INTO Notifications (CustomerId, Message, IsRead, CreatedAt)
    VALUES (@CustomerId, @Message, 0, GETDATE())";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Step 1: Update Orders and get the affected OrderIds and CustomerIds
                        List<(int OrderId, int CustomerId)> updatedOrders = new List<(int, int)>();

                        using (SqlCommand updateCommand = new SqlCommand(updateOrderQuery, connection, transaction))
                        using (SqlDataReader reader = updateCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int orderId = reader.GetInt32(0);
                                int customerId = reader.GetInt32(1);
                                updatedOrders.Add((orderId, customerId));
                            }
                        }

                        // Step 2: Send notifications for each updated order
                        foreach (var (orderId, customerId) in updatedOrders)
                        {
                            string message = $"Your order with OrderId {orderId} is delivered. Kindly receive it as soon as possible.";

                            using (SqlCommand notificationCommand = new SqlCommand(insertNotificationQuery, connection, transaction))
                            {
                                notificationCommand.Parameters.AddWithValue("@CustomerId", customerId);
                                notificationCommand.Parameters.AddWithValue("@Message", message);
                                notificationCommand.ExecuteNonQuery();
                            }
                        }

                        // Commit the transaction
                        transaction.Commit();

                        //if (updatedOrders.Count > 0)
                        //{
                        //    MessageBox.Show($"Successfully updated {updatedOrders.Count} orders and sent notifications.");
                        //}
                        //else
                        //{
                        //    MessageBox.Show("No orders were updated.");
                        //}
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



        public void LoadPendingOrders()
        {
            int LID = UserSession.LogisticsId;
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = @"
    SELECT Orders.OrderId, Orders.CustomerId, Orders.OrderDate, Orders.DelieveryDate, Orders.Orderstatus, 
           OrderDetails.ProductId, OrderDetails.Quantity, Product.name
    FROM Orders
    INNER JOIN OrderDetails ON Orders.OrderId = OrderDetails.OrderId
    INNER JOIN Product ON OrderDetails.ProductId = Product.ProductId
    WHERE Orders.Orderstatus = 'Processing' and LID =@LID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                connection.Open();
                command.Parameters.AddWithValue("@LID", LID);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int orderId = Convert.ToInt32(reader["OrderId"]);
                    int customerId = Convert.ToInt32(reader["CustomerId"]);
                    string customerName = GetCustomerName(customerId);
                    string productName = reader["name"].ToString();
                    DateTime deliveryDate = Convert.ToDateTime(reader["DelieveryDate"]);

                    // Create the order panel
                    Panel orderPanel = new Panel
                    {
                        Size = new Size(400, 150),
                        BorderStyle = BorderStyle.FixedSingle,
                        Padding = new Padding(10),
                        Tag = orderId // Store the OrderId in the Tag property
                    };

                    Label customerLabel = new Label
                    {
                        Text = "Customer: " + customerName,
                        Location = new Point(10, 10),
                        AutoSize = true
                    };

                    Label productLabel = new Label
                    {
                        Text = "Product: " + productName,
                        Location = new Point(10, 40),
                        AutoSize = true
                    };

                    Label deliveryDateLabel = new Label
                    {
                        Text = "Delivery Date: " + deliveryDate.ToString("yyyy-MM-dd"),
                        Location = new Point(10, 70),
                        AutoSize = true
                    };

                    Button assignButton = new Button
                    {
                        Text = "Assign Shipment",
                        Location = new Point(10, 100),
                        Size = new Size(150, 30)
                    };
                    assignButton.Click += (sender, e) => AssignToShipment(orderId);

                    orderPanel.Controls.Add(customerLabel);
                    orderPanel.Controls.Add(productLabel);
                    orderPanel.Controls.Add(deliveryDateLabel);
                    orderPanel.Controls.Add(assignButton);

                    // Add the panel to the FlowLayoutPanel
                    flowLayoutPanel1.Controls.Add(orderPanel);
                }
            }
        }

        private void AssignToShipment(int orderId)
        {
            // Create and show the ShipmentAssignmentForm
            ShipmentAssignmentForm shipmentForm = new ShipmentAssignmentForm(orderId,this);
            shipmentForm.ShowDialog(); // Show the form modally

        }

        private string GetCustomerName(int customerId)
        {
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = "SELECT FirstName FROM Customer WHERE CustomerId = @CustomerId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CustomerId", customerId);
                connection.Open();
                return command.ExecuteScalar()?.ToString();
            }
        }

        public void LoadShippedOrders()
        {
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = @"
    SELECT o.OrderId, s.ShipType, c.FirstName
    FROM Orders o
    JOIN Shipment s ON o.OrderId = s.OID
    JOIN Customer c ON o.CustomerId = c.CustomerId
    WHERE o.OrderStatus = 'Delivered' and shipStatus ='Shipped'";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    flowLayoutPanel2.Controls.Clear(); // Clear existing panels

                    while (reader.Read())
                    {
                        int orderId = reader.GetInt32(0);
                        string shipType = reader.GetString(1);
                        string customerName = reader.GetString(2);

                        // Create a panel for each shipped order
                        Panel orderPanel = new Panel
                        {
                            Size = new Size(500, 100),
                            BorderStyle = BorderStyle.FixedSingle,
                            Margin = new Padding(10)
                        };

                        // Add labels for OrderId, ShipType, and CustomerName
                        Label lblOrderId = new Label
                        {
                            Text = "Order ID: " + orderId,
                            Location = new Point(10, 10),
                            AutoSize = true
                        };
                        Label lblShipType = new Label
                        {
                            Text = "Ship Type: " + shipType,
                            Location = new Point(10, 40),
                            AutoSize = true
                        };
                        Label lblCustomerName = new Label
                        {
                            Text = "Customer Name: " + customerName,
                            Location = new Point(10, 70),
                            AutoSize = true
                        };

                        // Add a Remove button
                        Button btnRemove = new Button
                        {
                            Text = "Remove",
                            Size = new Size(80, 30),
                            Location = new Point(400, 35),
                            Tag = orderId // Store OrderId in the button's Tag property for reference
                        };
                        btnRemove.Click += (s, e) => CancelOrder(orderId);

                        // Add controls to the panel
                        orderPanel.Controls.Add(lblOrderId);
                        orderPanel.Controls.Add(lblShipType);
                        orderPanel.Controls.Add(lblCustomerName);
                        orderPanel.Controls.Add(btnRemove);

                        // Add the panel to the FlowLayoutPanel
                        flowLayoutPanel2.Controls.Add(orderPanel);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void CancelOrder(int orderId)
        {
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = @"
    UPDATE Orders
    SET OrderStatus = 'Rejected'
    WHERE OrderId = @OrderId;

    UPDATE Shipment
    SET ShipStatus = 'Delivered'
    WHERE OID = @OrderId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@OrderId", orderId);

                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Order cancelled successfully.");
                        LoadShippedOrders(); // Reload the shipped orders to reflect changes
                    }
                    else
                    {
                        MessageBox.Show("Failed to cancel the order.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Notification notify = new Notification(this);
            notify.Show();
            //this.Hide();
        }
    }
}
