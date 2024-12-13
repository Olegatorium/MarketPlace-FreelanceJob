using BestShop.MyHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration; // Добавьте этот using
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace BestShop.Pages
{
    [RequireAuth]
    [BindProperties]
    public class ProfileModel : PageModel
    {
        private readonly string _connectionString;

        public string errorMessage = "";
        public string successMessage = "";

        [Required(ErrorMessage = "Imię jest wymagane")]
        public string Firstname { get; set; } = "";

        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        public string Lastname { get; set; } = "";

        [Required(ErrorMessage = "Email jest wymagany"), EmailAddress]
        public string Email { get; set; } = "";

        public string? Phone { get; set; } = "";

        [Required(ErrorMessage = "Adres jest wymagany")]
        public string Address { get; set; } = "";

        public string? Password { get; set; } = "";
        public string? ConfirmPassword { get; set; } = "";

        // Конструктор для получения строки подключения из конфигурации
        public ProfileModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public void OnGet()
        {
            Firstname = HttpContext.Session.GetString("firstname") ?? "";
            Lastname = HttpContext.Session.GetString("lastname") ?? "";
            Email = HttpContext.Session.GetString("email") ?? "";
            Phone = HttpContext.Session.GetString("phone");
            Address = HttpContext.Session.GetString("address") ?? "";
        }

        public void OnPost()
        {
            if (!ModelState.IsValid)
            {
                errorMessage = "Weryfikacja danych nie powiodła się";
                return;
            }

            // Udana weryfikacja danych
            if (Phone == null) Phone = "";

            // Aktualizacja profilu użytkownika lub hasła
            string submitButton = Request.Form["action"];

            if (submitButton.Equals("profile"))
            {
                // Aktualizacja профиля в базе данных
                try
                {
                    using (SqlConnection connection = new SqlConnection(_connectionString))
                    {
                        connection.Open();

                        string sql = "UPDATE users SET firstname=@firstname, lastname=@lastname, " +
                            "email=@email, phone=@phone, address=@address WHERE id=@id";

                        int? id = HttpContext.Session.GetInt32("id");
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@firstname", Firstname);
                            command.Parameters.AddWithValue("@lastname", Lastname);
                            command.Parameters.AddWithValue("@email", Email);
                            command.Parameters.AddWithValue("@phone", Phone);
                            command.Parameters.AddWithValue("@address", Address);
                            command.Parameters.AddWithValue("@id", id);

                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    return;
                }

                // Обновление данных в сессии
                HttpContext.Session.SetString("firstname", Firstname);
                HttpContext.Session.SetString("lastname", Lastname);
                HttpContext.Session.SetString("email", Email);
                HttpContext.Session.SetString("phone", Phone);
                HttpContext.Session.SetString("address", Address);

                successMessage = "Profil został pomyślnie zaktualizowany";
            }
            else if (submitButton.Equals("password"))
            {
                // Weryfikacja hasła и потверждения пароля
                if (Password == null || Password.Length < 5 || Password.Length > 50)
                {
                    errorMessage = "Długość hasła powinna wynosić od 5 do 50 znaków";
                    return;
                }

                if (ConfirmPassword == null || !ConfirmPassword.Equals(Password))
                {
                    errorMessage = "Hasło i potwierdzenie hasła nie pasują do siebie";
                    return;
                }

                // Обновление пароля в базе данных
                try
                {
                    using (SqlConnection connection = new SqlConnection(_connectionString))
                    {
                        connection.Open();

                        string sql = "UPDATE users SET password=@password WHERE id=@id";

                        int? id = HttpContext.Session.GetInt32("id");

                        var passwordHasher = new PasswordHasher<IdentityUser>();
                        string hashedPassword = passwordHasher.HashPassword(new IdentityUser(), Password);

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@password", hashedPassword);
                            command.Parameters.AddWithValue("@id", id);

                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    return;
                }

                successMessage = "Hasło zostało pomyślnie zaktualizowane";
            }
        }
    }
}
