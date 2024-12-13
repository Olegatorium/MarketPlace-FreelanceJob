using BestShop.MyHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace BestShop.Pages.Admin.Products
{
	[RequireAuth(RequiredRole = "admin")]
	public class EditModel : PageModel
    {
        private readonly string _connectionString;

        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Tytuł jest wymagany")]
        [MaxLength(100, ErrorMessage = "Tytuł nie może przekraczać 100 znaków")]
        public string Title { get; set; } = "";

        [BindProperty]
        [Required(ErrorMessage = "Cena jest wymagana")]
        public decimal? Price { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Kategoria jest wymagana")]
        public string Category { get; set; } = "";

        [BindProperty]
        [MaxLength(1000, ErrorMessage = "Opis nie może przekraczać 1000 znaków")]
        public string? Description { get; set; } = "";


        [BindProperty]
        public string ImageFileName { get; set; } = "";

        public string errorMessage = "";
        public string successMessage = "";

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        private IWebHostEnvironment _webHostEnvironment;

        public EditModel(IWebHostEnvironment env, IConfiguration configuration)
        {
            _webHostEnvironment = env;
            _connectionString = configuration.GetConnectionString("DefaultConnection");

        }

        public void OnGet()
        {
            string requestId = Request.Query["id"];

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM books WHERE id=@id";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", requestId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Id = reader.GetInt32(0);
                                Title = reader.GetString(1);
                                Price = reader.GetDecimal(2);
                                Category = reader.GetString(3);
                                Description = reader.GetString(4);
                                ImageFileName = reader.GetString(5);
                            }
                            else
                            {
                                Response.Redirect("/Admin/Products/Index");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Response.Redirect("/Admin/Products/Index");
            }
        }

        public void OnPost()
        {
            if (!ModelState.IsValid)
            {
                errorMessage = "Walidacja danych nie powiodła się";
                return;
            }

            // successfull data validation

            if (Description == null) Description = "";

            // if we have a new ImageFile => upload the new image and delete the old image

            string newFileName = ImageFileName;
            if (ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                newFileName += Path.GetExtension(ImageFile.FileName);

                string imageFolder = _webHostEnvironment.WebRootPath + "/images/books/";
                string imageFullPath = Path.Combine(imageFolder, newFileName);
                Console.WriteLine("New image (Edit): " + imageFullPath);

                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    ImageFile.CopyTo(stream);
                }

                // delete old image
                string oldImageFullPath = Path.Combine(imageFolder, ImageFileName);
                System.IO.File.Delete(oldImageFullPath);
                Console.WriteLine("Delete Image " + oldImageFullPath);
            }

            // update the book data in the database
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = "UPDATE books SET title=@title, " +
                        "price=@price, category=@category, " +
                        "description=@description, image_filename=@image_filename WHERE id=@id;";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@title", Title);
                        command.Parameters.AddWithValue("@price", Price);
                        command.Parameters.AddWithValue("@category", Category);
                        command.Parameters.AddWithValue("@description", Description);
                        command.Parameters.AddWithValue("@image_filename", newFileName);
                        command.Parameters.AddWithValue("@id", Id);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            successMessage = "Dane zostały zapisane poprawnie";
            Response.Redirect("/Admin/Products/Index");
        }
    }
}
