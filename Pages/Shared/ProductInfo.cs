using System.Data.SqlClient;

namespace BestShop.Pages.Shared
{
    public class ProductInfo
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public decimal Price { get; set; }
        public string Category { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageFileName { get; set; } = "";
        public string CreatedAt { get; set; } = "";

        public Dictionary<String, int> getProductDictionary(string cookies)
        {
            var bookDictionary = new Dictionary<string, int>();

            // Read shopping cart items from cookie
            string cookieValue = cookies;

            if (cookieValue.Length > 0)
            {
                string[] bookIdArray = cookieValue.Split('-');

                for (int i = 0; i < bookIdArray.Length; i++)
                {
                    string bookId = bookIdArray[i];
                    if (bookDictionary.ContainsKey(bookId))
                    {
                        bookDictionary[bookId] += 1;
                    }
                    else
                    {
                        bookDictionary.Add(bookId, 1);
                    }
                }
            }

            return bookDictionary;
        }

        public decimal getBookPrice(string bookID, string _connectionString)
        {
            decimal price = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = "SELECT price FROM books WHERE id=@id";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", bookID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                price = reader.GetDecimal(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return price;
        }
    }
}
