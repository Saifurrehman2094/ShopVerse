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
    public partial class AdminOrders : Form
    {
        AdminCustomer customer;
        public AdminOrders(AdminCustomer customer)
        {
            InitializeComponent();
            this.customer = customer;
        }

        private void AdminOrders_Load(object sender, EventArgs e)
        {
            LoadOrders();
        }

        public void LoadOrders()
        {
            flowLayoutPanel1.Controls.Clear(); // Clear existing panels

            string connectionString = @"Data Source=DESKTOP-6SCBHN6\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = @"
        SELECT 
            o.OrderId,
            c.FirstName AS CustomerName,
            l.CompanyName AS LogisticsName,
            STRING_AGG(p.name, ', ') AS ProductsList,
            o.OrderStatus
        FROM 
            [Orders] o
        INNER JOIN 
            Customer c ON o.CustomerId = c.CustomerId
        LEFT JOIN 
            Logistics l ON o.LID = l.LID
        INNER JOIN 
            OrderDetails od ON o.OrderId = od.OrderId
        INNER JOIN 
            Product p ON od.ProductId = p.ProductId
        WHERE 
            o.OrderStatus IN ('Pending', 'Processing')
        GROUP BY 
            o.OrderId, c.FirstName, l.CompanyName, o.OrderStatus;
    ";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    // Create a panel for each order
                    Panel orderPanel = new Panel
                    {
                        Size = new Size(300, 150),
                        BorderStyle = BorderStyle.FixedSingle,
                        Margin = new Padding(10)
                    };

                    // Create and add CustomerName label
                    Label lblCustomerName = new Label
                    {
                        Text = $"Customer: {reader["CustomerName"]}",
                        Font = new Font("Arial", 10, FontStyle.Bold),
                        Location = new Point(10, 10),
                        AutoSize = true
                    };
                    orderPanel.Controls.Add(lblCustomerName);

                    // Create and add LogisticsName label
                    Label lblLogisticsName = new Label
                    {
                        Text = $"Logistics: {reader["LogisticsName"]}",
                        Location = new Point(10, 40),
                        AutoSize = true
                    };
                    orderPanel.Controls.Add(lblLogisticsName);

                    // Create and add ProductsList label
                    Label lblProductsList = new Label
                    {
                        Text = $"Products: {reader["ProductsList"]}",
                        Location = new Point(10, 70),
                        Size = new Size(280, 40),
                        AutoEllipsis = true // Ensures long text is truncated with "..."
                    };
                    orderPanel.Controls.Add(lblProductsList);

                    // Create and add Reject button
                    Button btnReject = new Button
                    {
                        Text = "Reject",
                        Size = new Size(80, 30),
                        Location = new Point(200, 110),
                        Tag = reader["OrderId"] // Store OrderId for updating status
                    };
                    btnReject.Click += RejectOrder;
                    orderPanel.Controls.Add(btnReject);

                    // Add the panel to the FlowLayoutPanel
                    flowLayoutPanel1.Controls.Add(orderPanel);
                }
            }
        }

        private void RejectOrder(object sender, EventArgs e)
        {
            Button btnReject = sender as Button;
            int orderId = Convert.ToInt32(btnReject.Tag);

            string connectionString = @"Data Source=DESKTOP-6SCBHN6\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string updateQuery = "UPDATE [Orders] SET OrderStatus = 'Rejected' WHERE OrderId = @OrderId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(updateQuery, conn);
                cmd.Parameters.AddWithValue("@OrderId", orderId);
                cmd.ExecuteNonQuery();
            }

            // Remove the panel from the FlowLayoutPanel
            Control panelToRemove = btnReject.Parent;
            flowLayoutPanel1.Controls.Remove(panelToRemove);
            panelToRemove.Dispose();

            MessageBox.Show("Order has been rejected.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            customer.Show();
            this.Close();
        }
    }
}
