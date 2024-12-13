using BestShop.MyHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace BestShop.Pages.Auth
{
    [RequireNoAuth]
    public class ForgotPasswordModel : PageModel
    {
        private readonly string _connectionString;
        private readonly string _apiKey;

        [BindProperty, Required(ErrorMessage = "Email jest wymagany"), EmailAddress]
        public string Email { get; set; } = "";

        public string errorMessage = "";
        public string successMessage = "";

        public ForgotPasswordModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _apiKey = configuration["SendGrid:ApiKey"];
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

            // 1) Create a token, 2) Save the token in the database, 3) Send the token via email to the user
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM users WHERE email=@email";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@email", Email);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string firstname = reader.GetString(1);
                                string lastname = reader.GetString(2);

                                string token = Guid.NewGuid().ToString();

                                // Save the token in the database
                                SaveToken(Email, token);

                                // Send the token via email to the user

                                string resetUrl = Url.PageLink("/Auth/ResetPassword") + "?token=" + token;
                                string username = firstname + " " + lastname;
                                string subject = "Resetowanie hasła"; 
                                string message = "Drogi " + username + ",\n\n" + 
                                    "Możesz zresetować swoje hasło, korzystając z poniższego linku:\n\n" + 
                                    resetUrl + "\n\n" +
                                    "Pozdrawiamy"; 

                                EmailSender.SendEmail(_apiKey, Email, username, subject, message).Wait();
                            }
                            else
                            {
                                errorMessage = "Nie znaleziono użytkownika z tym adresem email"; 
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            successMessage = "Sprawdź swoją skrzynkę pocztową i kliknij w link do zresetowania hasła.";
        }

        private void SaveToken(string email, string token)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Delete any old token for this email address from the database

                    string sql = "DELETE FROM password_resets WHERE email=@email";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@email", email);

                        command.ExecuteNonQuery();
                    }

                    // Add token to the database

                    sql = "INSERT INTO password_resets (email, token) VALUES (@email, @token)";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@email", email);
                        command.Parameters.AddWithValue("@token", token);

                        command.ExecuteNonQuery();
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
