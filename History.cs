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
    public partial class History : Form
    {
        public History()
        {
            InitializeComponent();
        }

        private void History_Load(object sender, EventArgs e)
        {
            LoadCustomerOrders();
            LoadPendingAndProcessingOrders();
        }

        private DataTable GetCustomerPendingAndProcessingOrders()
        {
            int customerId = UserSession.CustomerId; // Retrieve CustomerId from UserSession

            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = @"
        SELECT 
            o.OrderId, 
            o.OrderStatus, 
            o.OrderDate, 
            STRING_AGG(p.Name, ', ') AS ProductNames
        FROM Orders o
        INNER JOIN OrderDetails od ON o.OrderId = od.OrderId
        INNER JOIN Product p ON od.ProductId = p.ProductId
        WHERE o.OrderStatus IN ('Pending', 'Processing') 
            AND od.Status = 'Accepted'
            AND o.CustomerId = @CustomerId
        GROUP BY o.OrderId, o.OrderStatus, o.OrderDate";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CustomerId", customerId); // Use CustomerId in query
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                DataTable ordersTable = new DataTable();
                ordersTable.Load(reader);
                return ordersTable;
            }
        }


        private void LoadPendingAndProcessingOrders()
        {
            DataTable ordersTable = GetCustomerPendingAndProcessingOrders();
            flowLayoutPanel2.Controls.Clear();

            foreach (DataRow row in ordersTable.Rows)
            {
                int orderId = Convert.ToInt32(row["OrderId"]);
                string productNames = row["ProductNames"].ToString();
                string orderStatus = row["OrderStatus"].ToString();
                DateTime orderDate = Convert.ToDateTime(row["OrderDate"]);

                // Create a panel for each order
                Panel panel = CreateOrderPanel(orderId, productNames, orderStatus, orderDate);
                flowLayoutPanel2.Controls.Add(panel);
            }
        }

        private DataTable GetCustomerDeliveredOrRejectedOrders()
        {
            int customerId = UserSession.CustomerId; // Retrieve CustomerId from UserSession

            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = @"
SELECT 
    o.OrderId, 
    o.OrderStatus, 
    o.OrderDate, 
    STRING_AGG(p.Name, ', ') AS ProductNames
FROM Orders o
INNER JOIN OrderDetails od ON o.OrderId = od.OrderId
INNER JOIN Product p ON od.ProductId = p.ProductId
WHERE o.OrderStatus IN ('Delivered', 'Rejected') 
    AND od.Status = 'Accepted'
    AND o.CustomerId = @CustomerId
GROUP BY o.OrderId, o.OrderStatus, o.OrderDate";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CustomerId", customerId); // Use CustomerId in query
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                DataTable ordersTable = new DataTable();
                ordersTable.Load(reader);
                return ordersTable;
            }
        }




        private Panel CreateOrderPanel(int orderId, string productNames, string orderStatus, DateTime orderDate)
        {
            // Create a new panel
            Panel panel = new Panel
            {
                Size = new Size(450, 120),
                BorderStyle = BorderStyle.FixedSingle,
                Tag = orderId // Store OrderId for reference
            };

            // Add Order ID Label
            Label orderIdLabel = new Label
            {
                Text = $"Order ID: {orderId}",
                Location = new Point(10, 10),
                AutoSize = true
            };
            panel.Controls.Add(orderIdLabel);

            // Add Product Names Label
            Label productsLabel = new Label
            {
                Text = $"Products: {productNames}",
                Location = new Point(10, 35),
                AutoSize = true
            };
            panel.Controls.Add(productsLabel);

            // Add Order Status Label
            Label statusLabel = new Label
            {
                Text = $"Status: {orderStatus}",
                Location = new Point(10, 60),
                AutoSize = true
            };
            panel.Controls.Add(statusLabel);

            // Add Order Date Label
            Label dateLabel = new Label
            {
                Text = $"Date: {orderDate.ToShortDateString()}",
                Location = new Point(10, 85),
                AutoSize = true
            };
            panel.Controls.Add(dateLabel);

            return panel;
        }

        private void LoadCustomerOrders()
        {
            DataTable ordersTable = GetCustomerDeliveredOrRejectedOrders();
            flowLayoutPanel1.Controls.Clear();

            foreach (DataRow row in ordersTable.Rows)
            {
                int orderId = Convert.ToInt32(row["OrderId"]);
                string productNames = row["ProductNames"].ToString();
                string orderStatus = row["OrderStatus"].ToString();
                DateTime orderDate = Convert.ToDateTime(row["OrderDate"]);

                // Create a panel for each order
                Panel panel = CreateOrderPanel(orderId, productNames, orderStatus, orderDate);
                flowLayoutPanel1.Controls.Add(panel);
            }
        }


    }
}
