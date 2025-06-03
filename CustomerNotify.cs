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
    public partial class CustomerNotify : Form
    {
        public CustomerNotify()
        {
            InitializeComponent();
        }

        private void CustomerNotify_Load(object sender, EventArgs e)
        {
            LoadUnreadNotifications();
            LoadUnreadLogisticsNotifications();
        }

        private DataTable GetUnreadNotifications()
        {
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = @"
SELECT sn.NotificationId, sn.Message, sn.CreatedAt, s.Name AS SellerName
FROM sellerNotifications sn
INNER JOIN Orders o ON sn.CustomerId = o.CustomerId
INNER JOIN OrderDetails od ON o.OrderId = od.OrderId
INNER JOIN Product p ON od.ProductId = p.ProductId
INNER JOIN Seller s ON p.SellerId = s.SId
WHERE sn.IsRead = 0";


            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                DataTable notificationsTable = new DataTable();
                notificationsTable.Load(reader);
                return notificationsTable;
            }
        }

        private Panel CreateNotificationPanel(int notificationId, string message, string sellerName, DateTime createdAt)
        {
            Panel panel = new Panel
            {
                Size = new Size(650, 100),
                BorderStyle = BorderStyle.FixedSingle,
                Tag = notificationId // Store the NotificationId for reference
            };

            // Message Label
            Label messageLabel = new Label
            {
                Text = $"Message: {message}",
                Location = new Point(10, 10),
                AutoSize = true
            };
            panel.Controls.Add(messageLabel);

            // Seller Name Label
            Label sellerLabel = new Label
            {
                Text = $"Seller: {sellerName}",
                Location = new Point(10, 35),
                AutoSize = true
            };
            panel.Controls.Add(sellerLabel);

            // Created Date Label
            Label dateLabel = new Label
            {
                Text = $"Date: {createdAt}",
                Location = new Point(10, 60),
                AutoSize = true
            };
            panel.Controls.Add(dateLabel);

            // Clear Button
            Button clearButton = new Button
            {
                Text = "Clear",
                Location = new Point(550, 35),
                Tag = notificationId // Store the NotificationId in the button's Tag
            };
            clearButton.Click += ClearButton_Click;
            panel.Controls.Add(clearButton);

            return panel;
        }

        private void LoadUnreadNotifications()
        {
            flowLayoutPanel1.Controls.Clear();
            DataTable notifications = GetUnreadNotifications();

            foreach (DataRow row in notifications.Rows)
            {
                int notificationId = (int)row["NotificationId"];
                string message = row["Message"].ToString();
                string sellerName = row["SellerName"].ToString();
                DateTime createdAt = (DateTime)row["CreatedAt"];

                Panel panel = CreateNotificationPanel(notificationId, message, sellerName, createdAt);
                flowLayoutPanel1.Controls.Add(panel);
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int notificationId = (int)button.Tag;

            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = "UPDATE sellerNotifications SET IsRead = 1 WHERE NotificationId = @NotificationId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                connection.Open();
                command.Parameters.AddWithValue("@NotificationId", notificationId);
                command.ExecuteNonQuery();
            }

            // Remove the panel from FlowLayoutPanel
            Panel panel = button.Parent as Panel;
            flowLayoutPanel1.Controls.Remove(panel);

            MessageBox.Show("Notification cleared.");
        }

        private DataTable GetUnreadLogisticsNotifications()
        {
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = "SELECT NotificationId, Message, CreatedAt FROM Notifications WHERE IsRead = 0";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                DataTable notificationsTable = new DataTable();
                notificationsTable.Load(reader);
                return notificationsTable;
            }
        }

        private Panel CreateNotificationPanel(int notificationId, string message, DateTime createdAt)
        {
            // Create a new panel
            Panel panel = new Panel
            {
                Size = new Size(450, 80),
                BorderStyle = BorderStyle.FixedSingle,
                Tag = notificationId // Store NotificationId for reference
            };

            // Add Message Label
            Label messageLabel = new Label
            {
                Text = $"Message: {message}",
                Location = new Point(10, 10),
                AutoSize = true
            };
            panel.Controls.Add(messageLabel);

            // Add Created Date Label
            Label dateLabel = new Label
            {
                Text = $"Date: {createdAt.ToString("g")}",
                Location = new Point(10, 35),
                AutoSize = true
            };
            panel.Controls.Add(dateLabel);

            // Add Clear Button
            Button clearButton = new Button
            {
                Text = "Clear",
                Location = new Point(350, 25),
                Tag = notificationId // Store NotificationId in the button's Tag
            };
            clearButton.Click += ClearNotificationButton_Click;
            panel.Controls.Add(clearButton);

            return panel;
        }

        private void LoadUnreadLogisticsNotifications()
        {
            DataTable notificationsTable = GetUnreadLogisticsNotifications();
            flowLayoutPanel2.Controls.Clear();

            foreach (DataRow row in notificationsTable.Rows)
            {
                int notificationId = Convert.ToInt32(row["NotificationId"]);
                string message = row["Message"].ToString();
                DateTime createdAt = Convert.ToDateTime(row["CreatedAt"]);

                // Create a panel for each notification
                Panel panel = CreateNotificationPanel(notificationId, message, createdAt);
                flowLayoutPanel2.Controls.Add(panel);
            }
        }

        private void ClearNotificationButton_Click(object sender, EventArgs e)
        {
            Button clearButton = sender as Button;
            int notificationId = (int)clearButton.Tag;

            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = "UPDATE Notifications SET IsRead = 1 WHERE NotificationId = @NotificationId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                connection.Open();
                command.Parameters.AddWithValue("@NotificationId", notificationId);
                command.ExecuteNonQuery();
            }

            // Remove the panel after marking the notification as read
            Panel parentPanel = clearButton.Parent as Panel;
            flowLayoutPanel2.Controls.Remove(parentPanel);

            MessageBox.Show("Notification marked as read.");
        }

    }
}
