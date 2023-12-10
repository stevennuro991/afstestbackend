namespace Afstest.API.Dtos.Common
{
    public class EntityCreatedResponse<T>
    {
        public required T Id { get; set; }
    }

    public class BaseDto
    {
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string CreatedBy { get; set; } = default!;
        public string? UpdatedBy { get; set; }
    }

    public class ErrorDto
    {
        public IEnumerable<string> Errors { get; set; }
    }
}
