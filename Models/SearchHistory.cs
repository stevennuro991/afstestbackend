using System.ComponentModel.DataAnnotations.Schema;

namespace Afstest.API.Models
{
    public class SearchHistory : Entity
    {
        public Guid SearchHistoryId { get; set; }
        public required string SearchQuery { get; set; }
        public required string SearchResult { get; set; }

        //rel
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = default!;
    }
}
