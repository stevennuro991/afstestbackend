namespace Afstest.API.SeedWork
{
   public class PagingRequest
    {
        //public string? SortField { get; set; } = string.Empty;
        //public string? SortDirection { get; set; } = "Desc";
        public string? SearchString { get; set; } = string.Empty;
        public int PageNum { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public PagingOptions GetOptions()
        {
            return new()
            {
                //SortField = SortField,
                //SortDirection = SortDirection,
                SearchString = SearchString,
                PageNum = PageNum,

            };
        }
    }
    public record PagingOptions
    {
        public string? SortField { get; set; } = string.Empty;
        public string? SortDirection { get; set; } = "Desc";
        public string? SearchString { get; set; } = string.Empty;

        private const int _defaultPageSize = 15;

        public int PageNum { get; set; } = 1;
        public double TotalCount { get; set; }

        public int PageSize { get; set; } = _defaultPageSize;

        public int[] PageSizes = new[] { _defaultPageSize, 30, 50, 100, 500, 1000 };

        public int NumPages { get; set; } = 1;


        #region helpers
        public void SetUpRestOfDto<T>(IQueryable<T> query)
        {
            TotalCount = query.Count();

            if (TotalCount > 0)
                NumPages = (int)Math.Ceiling(TotalCount / PageSize);

            //used if PageNum posted value is greater than the calculated number of pages
            PageNum = TotalCount == 0 ? 1 : Math.Min(Math.Max(1, PageNum), NumPages);
        }


        //Validations
        public void ValidateOptions<TQueryObject>()
        {
            if (string.IsNullOrEmpty(SortDirection) || (SortDirection.ToUpper() != "DESC" && SortDirection.ToUpper() != "ASC"))
            {
                throw new AfstestPlatformException("sortDirection field invalid");
            }

            if (!string.IsNullOrEmpty(SortField) && typeof(TQueryObject).GetProperties().Any(p => p.Name == SortField) is false)
            {
                throw new AfstestPlatformException("sortField invalid");
            }
        }

        #endregion helpers
    }

    public record PaginatedEntities<T>
    {
        public required PagingOptions PagingOptions { get; set; } = default!;
        public required IEnumerable<T> Entities { get; set; } = Enumerable.Empty<T>();
    }
}
