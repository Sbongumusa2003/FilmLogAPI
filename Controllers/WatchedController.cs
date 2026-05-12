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
    public class WatchedController : ControllerBase
    {
        private readonly IWatchedRepository _watchedRepo;
        private readonly IWatchlistRepository _watchlistRepo;
        public WatchedController(
            IWatchedRepository watchedRepo,
            IWatchlistRepository watchlistRepo)
        {
            _watchedRepo = watchedRepo;
            _watchlistRepo = watchlistRepo;
        }
        private int GetUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : 0;
        }
        [HttpGet]
        public async Task<IActionResult> GetWatchedList()
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var items = await _watchedRepo.GetByUserIdAsync(userId);
            return Ok(items);
        }
        [HttpPost]
        public async Task<IActionResult> MarkAsWatched([FromBody] AddMovieDto dto)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();
            var existing = await _watchedRepo.GetByUserAndImdbIdAsync(userId, dto.ImdbID);
            if (existing != null)
            {
                existing.TimesWatched++;
                var updated = await _watchedRepo.UpdateAsync(existing);
                return Ok(updated);
            }
            await _watchlistRepo.DeleteByImdbIdAsync(userId, dto.ImdbID);
            var item = new WatchedItem
            {
                ImdbID = dto.ImdbID,
                Title = dto.Title,
                Year = dto.Year,
                Poster = dto.Poster,
                Actors = dto.Actors,
                Genre = dto.Genre,
                Plot = dto.Plot,
                TimesWatched = 1,
                UserId = userId,
                AddedAt = DateTime.UtcNow
            };
            var created = await _watchedRepo.AddAsync(item);
            return CreatedAtAction(nameof(GetWatchedList), new { id = created.Id }, created);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWatched(int id, [FromBody] UpdateWatchedDto dto)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var existing = await _watchedRepo.GetByIdAsync(id);
            if (existing == null || existing.UserId != userId)
                return NotFound(new { message = "Watched item not found." });

            existing.TimesWatched = dto.TimesWatched;
            var result = await _watchedRepo.UpdateAsync(existing);
            return result == null ? NotFound() : Ok(result);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromWatched(int id)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var deleted = await _watchedRepo.DeleteAsync(id, userId);
            if (!deleted) return NotFound(new { message = "Watched item not found." });

            return NoContent();
        }
        [HttpPost("reset/{id}")]
        public async Task<IActionResult> ResetTimesWatched(int id)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var reset = await _watchedRepo.ResetTimesWatchedAsync(id, userId);
            if (!reset) return NotFound(new { message = "Watched item not found." });

            return Ok(new { message = "TimesWatched reset to 0." });
        }
    }
}