using BestShop.MyHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace BestShop.Pages.Admin.Orders
{
    [RequireAuth(RequiredRole = "admin")]
    public class IndexModel : PageModel
    {
        private readonly string _connectionString;

        public List<OrderInfo> listOrders = new List<OrderInfo>();

        public int page = 1; // the current html page
        public int totalPages = 0;
        private readonly int pageSize = 3; // orders per page

        public IndexModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");

        }
        public void OnGet()
        {
            try
            {
                string requestPage = Request.Query["page"];
                page = int.Parse(requestPage);
            }
            catch (Exception ex)
            {
                page = 1;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sqlCount = "SELECT COUNT(*) FROM orders";
                    using (SqlCommand command = new SqlCommand(sqlCount, connection))
                    {
                        decimal count = (int)command.ExecuteScalar();
                        totalPages = (int)Math.Ceiling(count / pageSize);
                    }

                    string sql = "SELECT * FROM orders ORDER BY id DESC";
                    sql += " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@skip", (page - 1) * pageSize);
                        command.Parameters.AddWithValue("@pageSize", pageSize);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                OrderInfo orderInfo = new OrderInfo();
                                orderInfo.id = reader.GetInt32(0);
                                orderInfo.clientId = reader.GetInt32(1);
                                orderInfo.orderDate = reader.GetDateTime(2).ToString("MM/dd/yyyy");
                                orderInfo.shippingFee = reader.GetDecimal(3);
                                orderInfo.deliveryAddress = reader.GetString(4);
                                orderInfo.paymentMethod = reader.GetString(5);
                                orderInfo.paymentStatus = reader.GetString(6);
                                orderInfo.orderStatus = reader.GetString(7);

                                orderInfo.items = OrderInfo.getOrderItems(orderInfo.id, _connectionString);

                                listOrders.Add(orderInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}
