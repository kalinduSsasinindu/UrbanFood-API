using DMCW.ServiceInterface.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DMCW.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly ITagsService _tagsService;
        public TagsController(ITagsService tagsService)
        {
            _tagsService = tagsService;
        }

        [HttpGet("kind/{kind}")]
        public async Task<IActionResult> GetTagsByKindAsync(string kind)
        {
            var tags = await _tagsService.GetTagsByKindAsync(kind);
            return Ok(tags);
        }
    }
}
