using FilmLogAPI.Data;
using FilmLogAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FilmLogAPI.Repositories
{
    public interface IWatchlistRepository
    {
        Task<List<WatchlistItem>> GetByUserIdAsync(int userId);
        Task<WatchlistItem?> GetByUserAndImdbIdAsync(int userId, string imdbId);
        Task<WatchlistItem> AddAsync(WatchlistItem item);
        /// <summary>Delete by primary key — matches DELETE /api/watchlist/{id}</summary>
        Task<bool> DeleteByIdAsync(int userId, int id);
        /// <summary>Delete by IMDb ID — used internally when marking as watched</summary>
        Task<bool> DeleteByImdbIdAsync(int userId, string imdbId);
    }

    public class WatchlistRepository : IWatchlistRepository
    {
        private readonly ApplicationDbContext _context;

        public WatchlistRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<WatchlistItem>> GetByUserIdAsync(int userId)
        {
            return await _context.WatchlistItems
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();
        }

        public async Task<WatchlistItem?> GetByUserAndImdbIdAsync(int userId, string imdbId)
        {
            return await _context.WatchlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ImdbID == imdbId);
        }

        public async Task<WatchlistItem> AddAsync(WatchlistItem item)
        {
            _context.WatchlistItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> DeleteByIdAsync(int userId, int id)
        {
            var item = await _context.WatchlistItems
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

            if (item == null) return false;

            _context.WatchlistItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByImdbIdAsync(int userId, string imdbId)
        {
            var item = await _context.WatchlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ImdbID == imdbId);

            if (item == null) return false;

            _context.WatchlistItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    // ──────────────────────────────────────────────────────────

    public interface IWatchedRepository
    {
        Task<List<WatchedItem>> GetByUserIdAsync(int userId);
        Task<WatchedItem?> GetByIdAsync(int id);
        Task<WatchedItem?> GetByUserAndImdbIdAsync(int userId, string imdbId);
        Task<WatchedItem> AddAsync(WatchedItem item);
        Task<WatchedItem?> UpdateAsync(WatchedItem item);
        Task<bool> DeleteAsync(int id, int userId);
        Task<bool> ResetTimesWatchedAsync(int id, int userId);
    }

    public class WatchedRepository : IWatchedRepository
    {
        private readonly ApplicationDbContext _context;

        public WatchedRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<WatchedItem>> GetByUserIdAsync(int userId)
        {
            return await _context.WatchedItems
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();
        }

        public async Task<WatchedItem?> GetByIdAsync(int id)
        {
            return await _context.WatchedItems.FindAsync(id);
        }

        public async Task<WatchedItem?> GetByUserAndImdbIdAsync(int userId, string imdbId)
        {
            return await _context.WatchedItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ImdbID == imdbId);
        }

        public async Task<WatchedItem> AddAsync(WatchedItem item)
        {
            _context.WatchedItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<WatchedItem?> UpdateAsync(WatchedItem item)
        {
            var existing = await _context.WatchedItems.FindAsync(item.Id);
            if (existing == null) return null;

            existing.TimesWatched = item.TimesWatched;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var item = await _context.WatchedItems
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

            if (item == null) return false;

            _context.WatchedItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Resets TimesWatched to 1 (not 0 — a movie in the watched list was watched at least once).
        /// The spec says "Reset TimesWatched counter"; resetting to 1 is semantically correct.
        /// </summary>
        public async Task<bool> ResetTimesWatchedAsync(int id, int userId)
        {
            var item = await _context.WatchedItems
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

            if (item == null) return false;

            item.TimesWatched = 1;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}