using BestShop.MyHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace BestShop.Pages
{
    public class ContactModel : PageModel
    {
        private readonly string _connectionString;
        private readonly string _apiKey;

        public ContactModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _apiKey = configuration["SendGrid:ApiKey"];
        }

        public void OnGet()
        {
        }

        [BindProperty, Required(ErrorMessage = "Imię jest wymagane")]
        [Display(Name = "Imię*")]
        public string FirstName { get; set; } = "";

        [BindProperty, Required(ErrorMessage = "Nazwisko jest wymagane")]
        [Display(Name = "Nazwisko*")]
        public string LastName { get; set; } = "";

        [BindProperty, Required(ErrorMessage = "Adres e-mail jest wymagany"), EmailAddress]
        [Display(Name = "E-mail*")]
        public string Email { get; set; } = "";

        [BindProperty]
        public string? Phone { get; set; } = "";

        [BindProperty, Required(ErrorMessage = "Temat jest wymagany")]
        [Display(Name = "Temat*")]
        public string Subject { get; set; } = "";

        [BindProperty, Required(ErrorMessage = "Wiadomość jest wymagana")]
        [MinLength(5, ErrorMessage = "Wiadomość powinna mieć co najmniej 5 znaków")]
        [MaxLength(1024, ErrorMessage = "Wiadomość nie powinna mieć więcej niż 1024 znaki")]
        [Display(Name = "Wiadomość*")]
        public string Message { get; set; } = "";

        public List<SelectListItem> SubjectList { get; } = new List<SelectListItem>
        {
            new SelectListItem {Value = "Status zamówienia", Text = "Status zamówienia"},
            new SelectListItem {Value = "Prośba o zwrot", Text = "Prośba o zwrot"},
            new SelectListItem {Value = "Aplikacja o pracę", Text = "Aplikacja o pracę"},
            new SelectListItem {Value = "Inne", Text = "Inne"},
        };

        public string SuccessMessage { get; set; } = "";
        public string ErrorMessage { get; set; } = "";

        public void OnPost()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Proszę wypełnić wszystkie wymagane pola";
                return;
            }

            if (Phone == null) Phone = "";

            // adding message to database 

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = "INSERT INTO messages " +
                        "(firstname, lastname, email, phone, subject, message) VALUES " +
                        "(@firstname, @lastname, @email, @phone, @subject, @message);";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@firstname", FirstName);
                        command.Parameters.AddWithValue("@lastname", LastName);
                        command.Parameters.AddWithValue("@email", Email);
                        command.Parameters.AddWithValue("@phone", Phone);
                        command.Parameters.AddWithValue("@subject", Subject);
                        command.Parameters.AddWithValue("@message", Message);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return;
            }

            //Send email to the client

            string userName = $"{FirstName} {LastName}";
            string emailSubject = "Dotyczy Twojej wiadomości";
            string emailMessage = "Drogi/Droga " + userName + ",\nOtrzymaliśmy Twoją wiadomość. Dziękujemy za kontakt.\n" +
                "Nasz zespół wkrótce się z Tobą skontaktuje.\nZ poważaniem\n\nTwoja wiadomość:\n" + Message;

            EmailSender.SendEmail(_apiKey, Email, userName, emailSubject, emailMessage).Wait();

            ///////////////////////
            
            SuccessMessage = "Twoja wiadomość została poprawnie odebrana";

            FirstName = "";
            LastName = "";
            Email = "";
            Phone = "";
            Subject = "";
            Message = "";

            ModelState.Clear();
        }
    }
}
