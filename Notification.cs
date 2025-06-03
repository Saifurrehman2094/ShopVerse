using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ShopVerse.Ord;
using Microsoft.Data.SqlClient;

namespace ShopVerse
{
    public partial class Notification : Form
    {
        LogisticsOrder log;
        public Notification(LogisticsOrder log)
        {
            InitializeComponent();
            this.log = log;
        }

        private void Notification_Load(object sender, EventArgs e)
        {
            LoadCustomersWithOrders();
        }

        private void LoadCustomersWithOrders()
        {
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = @"
    SELECT DISTINCT C.CustomerId, C.FirstName
    FROM Customer C
    INNER JOIN Orders O ON C.CustomerId = O.CustomerId
    WHERE O.LID = 1";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    comboBox1.Items.Clear(); // Clear any existing items

                    while (reader.Read())
                    {
                        int customerId = reader.GetInt32(0);
                        string firstName = reader.GetString(1);

                        // Add to ComboBox
                        comboBox1.Items.Add(new ComboBoxItem { Value = customerId, Text = firstName });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading customers: " + ex.Message);
                }
            }
        }
    //    private void button1_Click(object sender, EventArgs e)
    //    {
    //        if (comboBox1.SelectedItem == null || string.IsNullOrWhiteSpace(textBox1.Text))
    //        {
    //            MessageBox.Show("Please select a customer and enter a message.");
    //            return;
    //        }

    //        ComboBoxItem selectedCustomer = (ComboBoxItem)comboBox1.SelectedItem;
    //        int customerId = selectedCustomer.Value;
    //        string message = textBox1.Text.Trim();

    //        string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
    //        string query = @"
    //INSERT INTO Notifications (CustomerId, Message, IsRead, CreatedAt)
    //VALUES (@CustomerId, @Message, 0, GETDATE())";

    //        using (SqlConnection connection = new SqlConnection(connectionString))
    //        using (SqlCommand command = new SqlCommand(query, connection))
    //        {
    //            command.Parameters.AddWithValue("@CustomerId", customerId);
    //            command.Parameters.AddWithValue("@Message", message);

    //            try
    //            {
    //                connection.Open();
    //                command.ExecuteNonQuery();
    //                MessageBox.Show("Notification sent successfully.");

    //                // Clear the form after submission
    //                comboBox1.SelectedIndex = -1;
    //                textBox1.Clear();
    //            }
    //            catch (Exception ex)
    //            {
    //                MessageBox.Show("Error sending notification: " + ex.Message);
    //            }
    //        }
    //    }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null || string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Please select a customer and enter a message.");
                return;
            }

            ComboBoxItem selectedCustomer = (ComboBoxItem)comboBox1.SelectedItem;
            int customerId = selectedCustomer.Value;
            string message = textBox1.Text.Trim();

            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = @"
    INSERT INTO Notifications (CustomerId, Message, IsRead, CreatedAt)
    VALUES (@CustomerId, @Message, 0, GETDATE())";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CustomerId", customerId);
                command.Parameters.AddWithValue("@Message", message);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    MessageBox.Show("Notification sent successfully.");

                    // Clear the form after submission
                    comboBox1.SelectedIndex = -1;
                    textBox1.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error sending notification: " + ex.Message);
                }
            }
            }
    }
}
