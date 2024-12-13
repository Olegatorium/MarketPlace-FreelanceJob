using BestShop.MyHelpers;
using BestShop.Pages.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace BestShop.Pages.Admin.Products
{
	[RequireAuth(RequiredRole = "admin")]
	public class IndexModel : PageModel
	{
        private readonly string _connectionString;

        public List<ProductInfo> productList = new List<ProductInfo>();

		public string search { get; set; } = "";

		public int page = 1;
		public int totalPages = 0; 
		private readonly int pageSize = 5;

		public string column = "id";
		public string order = "desc";



        public IndexModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");

        }

        public void OnGet()
		{
			search = Request.Query["search"];
			if (search == null)
			{
				search = "";
			}

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

			string[] validColumns = { "id", "title", "price", "category", "created_at" };

			column = Request.Query["column"];

			if (column == null || !validColumns.Contains(column))
			{
				column = "id";
			}

			order = Request.Query["order"];

            if (order == null || !order.Equals("asc"))
            {
                order = "desc";
            }

            try
			{
				using (SqlConnection connection = new SqlConnection(_connectionString)) 
				{
					connection.Open();

					string sqlCount = "SELECT COUNT(*) FROM books";

					if (search.Length > 0)
					{
						sqlCount += " WHERE title LIKE @search";
                    }

                    using (SqlCommand command = new SqlCommand(sqlCount, connection)) 
					{
                        command.Parameters.AddWithValue("@search", "%" + search + "%");
                        decimal count = (int)command.ExecuteScalar();
                        totalPages = (int)Math.Ceiling(count / pageSize);
                    }

                    string sql = "SELECT * FROM books";

					if (!string.IsNullOrWhiteSpace(search))
					{
						sql += " WHERE title LIKE @search";
					}

					sql += $" ORDER BY {column} {order}";

                    sql += " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";

                    using (SqlCommand command = new SqlCommand(sql, connection))
					{
                        command.Parameters.AddWithValue("@skip", (page - 1) * pageSize);
                        command.Parameters.AddWithValue("@pageSize", pageSize);

                        command.Parameters.AddWithValue("@search", "%" + search + "%");

						using (SqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								ProductInfo productInfo = new ProductInfo();
								productInfo.Id = reader.GetInt32(0);
								productInfo.Title = reader.GetString(1);
								productInfo.Price = reader.GetDecimal(2);
								productInfo.Category = reader.GetString(3);
								productInfo.Description = reader.GetString(4);
								productInfo.ImageFileName = reader.GetString(5);
								productInfo.CreatedAt = reader.GetDateTime(6).ToString("MM/dd/yyyy");

								productList.Add(productInfo);
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

