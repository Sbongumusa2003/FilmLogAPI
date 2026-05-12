using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FilmLogAPI.Services;

namespace FilmLogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;

        public MoviesController(IMovieService movieService)
        {
            _movieService = movieService;
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchMovies([FromQuery] string t)
        {
            if (string.IsNullOrWhiteSpace(t))
                return BadRequest(new { message = "Search title (t) is required." });

            var results = await _movieService.SearchMoviesAsync(t);

            if (results == null || results.Count == 0)
                return NotFound(new { message = "No movies found for that title." });

            return Ok(results);
        }
        [Authorize]
        [HttpGet("detail")]
        public async Task<IActionResult> GetMovieDetail([FromQuery] string t)
        {
            if (string.IsNullOrWhiteSpace(t))
                return BadRequest(new { message = "Title (t) is required." });

            var movie = await _movieService.GetMovieByTitleAsync(t);

            if (movie == null)
                return NotFound(new { message = "Movie not found." });

            return Ok(movie);
        }
    }
}