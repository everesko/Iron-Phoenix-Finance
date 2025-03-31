namespace IFF.Models
{
    public enum UserRole
    {
        Стрілець,
        Фінансист,
        Адміністратор
    }

    public class User
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string FullName { get; set; }
        public DateTime? HireDate { get; set; }
        public bool IsApproved { get; set; }
        public bool HasAccess { get; set; } // Додано поле доступу
    }
}