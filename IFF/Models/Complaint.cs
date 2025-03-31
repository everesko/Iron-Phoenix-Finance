namespace IFF.Models
{
    public class Complaint
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public bool IsRoleChangeRequest { get; set; }

        public string DisplayText => $"{Username} ({Date:dd.MM.yyyy HH:mm}): {Text}";
    }
}