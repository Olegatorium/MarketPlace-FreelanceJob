using System.Data.SqlClient;

namespace BestShop.Pages.Admin.Orders
{
    public class OrderInfo
    {
        public int id;
        public int clientId;
        public string orderDate;
        public decimal shippingFee;
        public string deliveryAddress;
        public string paymentMethod;
        public string paymentStatus;
        public string orderStatus;


        public List<OrderItemInfo> items = new List<OrderItemInfo>();

        public static List<OrderItemInfo> getOrderItems(int orderId, string _connectionString)
        {
            List<OrderItemInfo> items = new List<OrderItemInfo>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = "SELECT order_items.*, books.* FROM order_items, books " +
                        "WHERE order_items.order_id=@order_id AND order_items.book_id = books.id";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@order_id", orderId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                OrderItemInfo item = new OrderItemInfo();

                                item.id = reader.GetInt32(0);
                                item.orderId = reader.GetInt32(1);
                                item.bookId = reader.GetInt32(2);
                                item.quantity = reader.GetInt32(3);
                                item.unitPrice = reader.GetDecimal(4);

                                item.productInfo.Id = reader.GetInt32(5);
                                item.productInfo.Title = reader.GetString(6);
                                item.productInfo.Price = reader.GetDecimal(7);
                                item.productInfo.Category = reader.GetString(8);
                                item.productInfo.Description = reader.GetString(9);
                                item.productInfo.ImageFileName = reader.GetString(10);
                                item.productInfo.CreatedAt = reader.GetDateTime(11).ToString("MM/dd/yyyy");

                                items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return items;
        }
    }
}
        
    

    

