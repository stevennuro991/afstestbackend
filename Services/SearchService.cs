using Afstest.API.Data;
using Afstest.API.Data.QueryExtensions;
using Afstest.API.Dtos.Account;
using Afstest.API.Dtos.Common;
using Afstest.API.Dtos.Search;
using Afstest.API.Models;
using Afstest.API.SeedWork;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Net.NetworkInformation;

namespace Afstest.API.Services
{
    public class SearchService
    {   //Initialzed them in a constructor
        readonly AppDbContext _dbContext;
        readonly CurrentUser _currentUser;
        readonly ILogger<SearchService> _logger;

        public SearchService(AppDbContext dbContext,
            IdentityService identityService,
            ILogger<SearchService> logger)
        {
            _dbContext = dbContext;
            _currentUser = identityService.GetCurrentUser();
            _logger = logger;
        }

        /// <summary>
        /// query = user searches
        /// </summary>
        /// <param name="query">tes</param>
        /// <returns></returns>
        public async Task<IEnumerable<UserSearchDto>> GetSearchHistoryAsync(string query)
        {
            if (string.IsNullOrEmpty(query))
                return Enumerable.Empty<UserSearchDto>();

            return await _dbContext.SearchHistory.Where(p => p.SearchQuery.Contains(query))
                                    .Select(p => new UserSearchDto
                                    {
                                        SearchQuery = p.SearchQuery,
                                    }).ToListAsync();
        }

       public async Task<PaginatedEntities<SearchHistoryDto>> GetSearchHistoryAsync(PagingRequest request)
        {
            var options = request.GetOptions();
            Expression<Func<SearchHistory, bool>> predicate = c => c.SearchQuery!.Contains(options.SearchString!)
                                                  || c.SearchResult.Contains(options.SearchString!);

            var query = _dbContext.SearchHistory.AsNoTracking()
                                        .Where(c => c.UserId == _currentUser.UserId)
                                        .WithSearch(options.SearchString!, predicate)
                                        .Select(s => new SearchHistoryDto
                                        {
                                            SearchHistoryId = s.SearchHistoryId,
                                            SearchQuery = s.SearchQuery,
                                            SearchResult = s.SearchResult,
                                            CreatedOn = s.CreatedOn,
                                        })
                                        .OrderByDescending(s => s.CreatedOn);

            options.SetUpRestOfDto(query);

            var entities = await query.Page(options.PageNum - 1, options.PageSize).ToListAsync();

            return new PaginatedEntities<SearchHistoryDto>
            {
                Entities = entities,
                PagingOptions = options
            };
        }

        public async Task<SearchHistoryDto?> GetSearchHistoryAsync(Guid id)
        {
            return await _dbContext.SearchHistory.Where(s => s.SearchHistoryId == id)
                                    .Select(s => new SearchHistoryDto
                                    {
                                        SearchHistoryId = s.SearchHistoryId,
                                        SearchResult = s.SearchResult,
                                        SearchQuery = s.SearchQuery,
                                    }).FirstOrDefaultAsync();
        }

        public async Task<object> CreateSearchHistoryAsync(CreateSearchHistoryRequest request)
        {
            try
            {
                var searchHistory = new SearchHistory
                {
                    SearchQuery = request.SearchQuery,
                    SearchResult = request.SearchResult,
                    UserId = _currentUser.UserId,
                    CreatedBy = _currentUser.Email
                };

                await _dbContext.AddAsync(searchHistory);
                await _dbContext.SaveChangesAsync();

                return new EntityCreatedResponse<Guid> { Id = searchHistory.SearchHistoryId };
            }
            catch (Exception ex)
            {
                _logger.LogError("Message:{0}, \n\nStackTrace: {1}", ex.Message, ex.StackTrace);
                throw new AfstestPlatformException("failed to create history");
            }
        }
    }
}