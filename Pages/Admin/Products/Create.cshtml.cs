using BestShop.MyHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace BestShop.Pages.Admin.Products
{
    [RequireAuth(RequiredRole ="admin")]
    public class CreateModel : PageModel
    {
        private readonly string _connectionString;

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
        [Required(ErrorMessage = "Obraz jest wymagany")]
        public IFormFile ImageFile { get; set; }

        public string errorMessage = "";
        public string successMessage = "";

        private IWebHostEnvironment _webHostEnvironment;

        public CreateModel(IWebHostEnvironment env, IConfiguration configuration) 
        {
            _webHostEnvironment = env;
            _connectionString = configuration.GetConnectionString("DefaultConnection");

        }

        public void OnGet()
        {
        }

        public void OnPost()
        {
            if (!ModelState.IsValid)
            {
                errorMessage = "Walidacja danych nie powiodła się";
                return;
            }

            //succesfull data validation

            if (Description == null) Description = "";

            //save the image file on the server

            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            newFileName += Path.GetExtension(ImageFile.FileName);

            string imageFolder = _webHostEnvironment.WebRootPath + "/images/books/";

            string imageFullPath = Path.Combine(imageFolder, newFileName);

            using (var stream = System.IO.File.Create(imageFullPath)) 
            {
                ImageFile.CopyTo(stream);
            }

            //save the new book in the DataBase

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = "INSERT INTO books " +
                    "(title, price, category, description, image_filename) VALUES " +
                    "(@title, @price, @category, @description, @image_filename);";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@title", Title);
                        command.Parameters.AddWithValue("@price", Price);
                        command.Parameters.AddWithValue("@category", Category);
                        command.Parameters.AddWithValue("@description", Description);
                        command.Parameters.AddWithValue("@image_filename", newFileName);

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
