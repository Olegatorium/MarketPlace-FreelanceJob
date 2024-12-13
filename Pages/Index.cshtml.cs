using BestShop.Pages.Shared;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace BestShop.Pages
{
    public class IndexModel : PageModel
    {
        private readonly string _connectionString;

        public List<ProductInfo> listNewestProducts = new List<ProductInfo>();
        public List<ProductInfo> listTopSales = new List<ProductInfo>();

        public IndexModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public void OnGet()
        {
            var apiKey = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

            Console.WriteLine(apiKey);
            Console.WriteLine("Test api keys");

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = "SELECT TOP 4 * FROM books ORDER BY created_at DESC";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ProductInfo productInfo = new ProductInfo
                                {
                                    Id = reader.GetInt32(0),
                                    Title = reader.GetString(1),
                                    Price = reader.GetDecimal(2),
                                    Category = reader.GetString(3),
                                    Description = reader.GetString(4),
                                    ImageFileName = reader.GetString(5),
                                    CreatedAt = reader.GetDateTime(6).ToString("MM/dd/yyyy")
                                };

                                listNewestProducts.Add(productInfo);
                            }
                        }
                    }

                    sql = "SELECT TOP 4 books.*, (" +
                       "SELECT SUM(order_items.quantity) FROM order_items WHERE books.id = order_items.book_id" +
                       ") AS total_sales " +
                       "FROM books " +
                       "ORDER BY total_sales DESC";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ProductInfo productInfo = new ProductInfo
                                {
                                    Id = reader.GetInt32(0),
                                    Title = reader.GetString(1),
                                    Price = reader.GetDecimal(2),
                                    Category = reader.GetString(3),
                                    Description = reader.GetString(4),
                                    ImageFileName = reader.GetString(5),
                                    CreatedAt = reader.GetDateTime(6).ToString("MM/dd/yyyy")
                                };

                                listTopSales.Add(productInfo);
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
