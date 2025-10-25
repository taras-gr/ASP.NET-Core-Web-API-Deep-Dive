using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authorcollections")]
    public class AuthorCollectionsController(
        ICourseLibraryRepository courseLibraryRepository,
        IMapper mapper) : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository = courseLibraryRepository ??
                throw new ArgumentNullException(nameof(courseLibraryRepository));
        private readonly IMapper _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));

        [HttpGet("({authorIds})", Name = "GetAuthorCollection")]
        public async Task<ActionResult<IEnumerable<AuthorForCreationDto>>>
            GetAuthorCollection(
                [ModelBinder(BinderType = typeof(ArrayModelBinder))] 
                [FromRoute] IEnumerable<Guid> authorIds)
        {
            var authorEntities = await _courseLibraryRepository
                .GetAuthorsAsync(authorIds);

            // Check if all athors were found
            if (authorIds.Count() != authorEntities.Count())
            {
                return NotFound();
            }

            var authorsToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            return Ok(authorsToReturn);
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> CreateAuthorCollection(
            IEnumerable<AuthorForCreationDto> authorCollection)
        {
            var authorEntities = _mapper.Map<IEnumerable<Author>>(authorCollection);
            foreach (var author in authorEntities)
            {
                _courseLibraryRepository.AddAuthor(author);
            }
            await _courseLibraryRepository.SaveAsync();

            var authorCollectionToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            var authorIdsAsString = string.Join(",", authorCollectionToReturn.Select(s => s.Id));

            return CreatedAtRoute("GetAuthorCollection",
                new { authorIds = authorIdsAsString },
                authorCollectionToReturn);
        }
    }
}
