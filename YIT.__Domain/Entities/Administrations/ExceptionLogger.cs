
namespace YIT.__Domain.Entities.Administrations
{
    public class ExceptionLogger
    {
        public int Id { get; set; }
        public string? UrlRequest { get; set; }
        public string? ControllerName { get; set; }
        public DateTime LogTime { get; set; }
        public string? UserName { get; set; }
        public string? ExceptionType { get; set; }
        public string? ExceptionMessage { get; set; }
        public string? ExceptionStackTrace { get; set; }
        public string? Source { get; set; }
        public string? TraceIdentifier { get; set; }
    }
}
