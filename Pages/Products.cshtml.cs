using BestShop.Pages.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace BestShop.Pages
{
    [BindProperties(SupportsGet = true)]
    public class ProductsModel : PageModel
    {
        private readonly string _connectionString;

        public string? Search { get; set; }
        public string PageRange { get; set; } = "any";
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public string Category { get; set; } = "any";

        public List<ProductInfo> productsList = new List<ProductInfo>();

        public int page = 1; // the current html page
        public int totalPages = 0;
        private readonly int pageSize = 5; // books per page

        public ProductsModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public void OnGet()
        {
            page = 1;
            string requestPage = Request.Query["page"];
            if (requestPage != null)
            {
                try
                {
                    page = int.Parse(requestPage);
                }
                catch (Exception)
                {
                    page = 1;
                }
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Запрос для подсчета общего числа записей
                    string sqlCount = "SELECT COUNT(*) FROM books WHERE (title LIKE @search)";

                    if (MinPrice.HasValue)
                    {
                        sqlCount += " AND price >= @minPrice";
                    }

                    if (MaxPrice.HasValue)
                    {
                        sqlCount += " AND price <= @maxPrice";
                    }

                    if (!Category.Equals("any"))
                    {
                        sqlCount += " AND category=@category";
                    }

                    using (SqlCommand command = new SqlCommand(sqlCount, connection))
                    {
                        command.Parameters.AddWithValue("@search", "%" + Search + "%");
                        command.Parameters.AddWithValue("@category", Category);

                        if (MinPrice.HasValue)
                        {
                            command.Parameters.AddWithValue("@minPrice", MinPrice);
                        }

                        if (MaxPrice.HasValue)
                        {
                            command.Parameters.AddWithValue("@maxPrice", MaxPrice);
                        }

                        decimal count = (int)command.ExecuteScalar();
                        totalPages = (int)Math.Ceiling(count / pageSize);
                    }

                    // Запрос для получения продуктов
                    string sql = "SELECT * FROM books WHERE (title LIKE @search)";

                    if (MinPrice.HasValue)
                    {
                        sql += " AND price >= @minPrice";
                    }

                    if (MaxPrice.HasValue)
                    {
                        sql += " AND price <= @maxPrice";
                    }

                    if (!Category.Equals("any"))
                    {
                        sql += " AND category=@category";
                    }

                    sql += " ORDER BY id DESC";
                    sql += " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@search", "%" + Search + "%");
                        command.Parameters.AddWithValue("@category", Category);
                        command.Parameters.AddWithValue("@skip", (page - 1) * pageSize);
                        command.Parameters.AddWithValue("@pageSize", pageSize);

                        if (MinPrice.HasValue)
                        {
                            command.Parameters.AddWithValue("@minPrice", MinPrice);
                        }

                        if (MaxPrice.HasValue)
                        {
                            command.Parameters.AddWithValue("@maxPrice", MaxPrice);
                        }

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

                                productsList.Add(productInfo);
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
