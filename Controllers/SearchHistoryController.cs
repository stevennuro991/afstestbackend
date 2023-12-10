using Afstest.API.Dtos.Account;
using Afstest.API.Dtos.Common;
using Afstest.API.Dtos.Search;
using Afstest.API.SeedWork;
using Afstest.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Afstest.API.Controllers
{
    [Authorize]
    [Route("api/search-history")]
    [ApiController]
    public class SearchHistoryController : ControllerBase
    {
        readonly SearchService _searchService;
        public SearchHistoryController(SearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet("single/{historyId}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(SearchHistoryDto))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ErrorDto))]
        public async Task<IActionResult> SearchHistory(Guid historyId)
        {
            return Ok((await _searchService.GetSearchHistoryAsync(historyId)));
        }
        
        [HttpGet("{query}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(IEnumerable<UserSearchDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ErrorDto))]
        public async Task<IActionResult> SearchHistory(string query)
        {
            return Ok((await _searchService.GetSearchHistoryAsync(query)));
        }
        
        [HttpPost("paged")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(PaginatedEntities<SearchHistoryDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ErrorDto))]
        public async Task<IActionResult> SearchHistory(PagingRequest request)
        {
            return Ok((await _searchService.GetSearchHistoryAsync(request)));
        }
        
        [HttpPost("create-search-history")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(EntityCreatedResponse<Guid>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ErrorDto))]
        public async Task<IActionResult> CreateSearchHistory(CreateSearchHistoryRequest request)
        {
            return Ok((await _searchService.CreateSearchHistoryAsync(request)));
        }
    }
}
