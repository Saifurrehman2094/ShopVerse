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
using ShopVerse.Helpers;

namespace ShopVerse
{
    public partial class ShipmentAssignmentForm : Form
    {
        LogisticsOrder l;
        private ComboBox shipmentComboBox;
        private Button assignButton;
        private int _orderId;

        public ShipmentAssignmentForm(int orderId, LogisticsOrder l)
        {
            _orderId = orderId;

            // Initialize form components
            InitializeComponen();
            this.l = l;
        }

        private void InitializeComponen()
        {
            // Create a ComboBox for selecting shipment
            shipmentComboBox = new ComboBox
            {
                Location = new Point(20, 20),
                Size = new Size(250, 30)
            };

            // Create a Button for assigning the shipment
            assignButton = new Button
            {
                Text = "Assign Shipment",
                Location = new Point(20, 70),
                Size = new Size(150, 30)
            };
            assignButton.Click += new EventHandler(assignButton_Click);

            // Add controls to the form
            this.Controls.Add(shipmentComboBox);
            this.Controls.Add(assignButton);

            // Set form properties
            this.Text = "Assign Shipment to Order";
            this.Size = new Size(300, 150);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Load shipments on form load
            this.Load += new EventHandler(ShipmentAssignmentForm_Load);
        }

        private void ShipmentAssignmentForm_Load(object sender, EventArgs e)
        {
            // Load delivered shipments into the ComboBox
            LoadDeliveredShipments();
        }

        private void LoadDeliveredShipments()
        {
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";
            string query = @"
        SELECT ShipType, Shipid 
        FROM shipment 
        WHERE shipStatus = 'Delivered'";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                // Clear existing items before adding new ones
                shipmentComboBox.Items.Clear();

                while (reader.Read())
                {
                    string shipmentName = reader["ShipType"].ToString();
                    int shipmentId = Convert.ToInt32(reader["Shipid"]);

                    // Add shipment options to ComboBox
                    shipmentComboBox.Items.Add(new ComboBoxItem1(shipmentName, shipmentId));
                }

                if (shipmentComboBox.Items.Count > 0)
                {
                    shipmentComboBox.SelectedIndex = 0; // Select the first item by default
                }
            }
        }

        private void assignButton_Click(object sender, EventArgs e)
        {
            if (shipmentComboBox.SelectedItem is ComboBoxItem1 selectedItem)
            {
                // Assign the selected shipment to the order
                AssignShipmentToOrder(selectedItem.Value,_orderId);
                MessageBox.Show("Shipment assigned successfully!");
                this.Close(); // Close the form after assignment
            }
            else
            {
                MessageBox.Show("Please select a shipment.");
            }
        }

        private void AssignShipmentToOrder(int shipmentId, int id)
        {
            string connectionString = "Data Source=DESKTOP-6SCBHN6\\SQLEXPRESS;Initial Catalog=shopverse3;Integrated Security=True;Encrypt=False";

            // Step 1: Update the Shipment status to 'Shipped' and set OID
            string updateShipmentQuery = @"
    UPDATE Shipment
    SET ShipStatus = 'Shipped', OID = @id
    WHERE ShipId = @ShipmentId";

            // Step 2: Update the Order status to 'Delivered' once the shipment is assigned
            string updateOrderQuery = @"
    UPDATE Orders
    SET OrderStatus = 'Delivered'
    WHERE OrderId = (SELECT OID FROM Shipment WHERE ShipId = @ShipmentId)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Begin a transaction to ensure both updates are executed together
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Step 1: Update the ShipStatus in Shipment table
                        using (SqlCommand updateShipmentCommand = new SqlCommand(updateShipmentQuery, connection, transaction))
                        {
                            updateShipmentCommand.Parameters.AddWithValue("@id", id);
                            updateShipmentCommand.Parameters.AddWithValue("@ShipmentId", shipmentId);
                            updateShipmentCommand.ExecuteNonQuery();
                        }

                        // Step 2: Update the OrderStatus in Orders table
                        using (SqlCommand updateOrderCommand = new SqlCommand(updateOrderQuery, connection, transaction))
                        {
                            updateOrderCommand.Parameters.AddWithValue("@ShipmentId", shipmentId);
                            int rowsAffected = updateOrderCommand.ExecuteNonQuery();

                            // Check if the update affected any rows
                            if (rowsAffected == 0)
                            {
                                throw new Exception("Failed to update OrderStatus. Ensure the Shipment ID is linked to an order.");
                            }
                        }

                        // Commit the transaction if both updates are successful
                        transaction.Commit();
                        MessageBox.Show("Shipment assigned and order status updated to 'Delivered'.");

                        // Clear and reload pending orders
                        l.GetPanel().Controls.Clear();
                        l.GetPanel2().Controls.Clear();
                        l.LoadShippedOrders();
                        l.LoadPendingOrders();
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction if there is an error
                        transaction.Rollback();
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }


    }

}
