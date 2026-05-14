using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FilmLogAPI.Models;
using FilmLogAPI.Repositories;

namespace FilmLogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WatchlistController : ControllerBase
    {
        private readonly IWatchlistRepository _watchlistRepo;

        public WatchlistController(IWatchlistRepository watchlistRepo)
        {
            _watchlistRepo = watchlistRepo;
        }

        private int GetUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : 0;
        }

        [HttpGet]
        public async Task<IActionResult> GetWatchlist()
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var items = await _watchlistRepo.GetByUserIdAsync(userId);
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> AddToWatchlist([FromBody] AddMovieDto dto)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var existing = await _watchlistRepo.GetByUserAndImdbIdAsync(userId, dto.ImdbID);
            if (existing != null)
                return Conflict(new { message = "Movie already in watchlist." });

            var item = new WatchlistItem
            {
                ImdbID = dto.ImdbID,
                Title = dto.Title,
                Year = dto.Year,
                Poster = dto.Poster,
                Actors = dto.Actors,
                Genre = dto.Genre,
                Plot = dto.Plot,
                UserId = userId,
                AddedAt = DateTime.UtcNow
            };

            var created = await _watchlistRepo.AddAsync(item);
            return CreatedAtAction(nameof(GetWatchlist), new { id = created.Id }, created);
        }

        /// <summary>
        /// DELETE /api/watchlist/{id}  — removes by primary key (spec requirement)
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> RemoveFromWatchlist(int id)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var deleted = await _watchlistRepo.DeleteByIdAsync(userId, id);
            if (!deleted) return NotFound(new { message = "Movie not found in watchlist." });

            return NoContent();
        }
    }
}