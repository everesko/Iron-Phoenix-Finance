namespace IFF.Models
{
    public class Salary
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime AccrualDate { get; set; }
        public decimal Accrued { get; set; }
        public decimal Pdfo { get; set; }
        public decimal Pension { get; set; }
        public decimal Military { get; set; }
        public decimal ZfFund { get; set; }
        public decimal Bonus { get; set; }
        public decimal Received { get; set; }
        public int? WorkDays { get; set; }
        public int? VacationDays { get; set; }
        public int? SickDays { get; set; }
    }
}