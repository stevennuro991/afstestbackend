namespace Afstest.API.Dtos.Search
{
    public class UserSearchDto
    {
        public string SearchQuery { get; set; } = default!;
    }

    public class CreateSearchHistoryRequest
    {
        public required string SearchQuery { get; set; }
        public required string SearchResult { get; set; }
    }
    
    public class SearchHistoryDto : CreateSearchHistoryRequest
    {
        public Guid SearchHistoryId { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
