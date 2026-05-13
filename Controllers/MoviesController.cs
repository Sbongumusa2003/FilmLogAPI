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
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(IMovieService movieService, ILogger<MoviesController> logger)
        {
            _movieService = movieService;
            _logger = logger;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchMovies([FromQuery] string t)
        {
            if (string.IsNullOrWhiteSpace(t))
                return BadRequest(new { message = "Search title (t) is required." });

            try
            {
                var results = await _movieService.SearchMoviesAsync(t);

                if (results == null || results.Count == 0)
                    return NotFound(new { message = "No movies found for that title." });

                return Ok(results);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Configuration error in SearchMovies");
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in SearchMovies for query: {Query}", t);
                return StatusCode(500, new { message = "An error occurred while searching for movies." });
            }
        }

        [Authorize]
        [HttpGet("detail")]
        public async Task<IActionResult> GetMovieDetail([FromQuery] string t)
        {
            if (string.IsNullOrWhiteSpace(t))
                return BadRequest(new { message = "Title (t) is required." });

            try
            {
                var movie = await _movieService.GetMovieByTitleAsync(t);

                if (movie == null)
                    return NotFound(new { message = "Movie not found." });

                return Ok(movie);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Configuration error in GetMovieDetail");
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetMovieDetail for title: {Title}", t);
                return StatusCode(500, new { message = "An error occurred while fetching movie details." });
            }
        }
    }
}