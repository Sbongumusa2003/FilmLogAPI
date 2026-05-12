using FilmLogAPI.Models;

namespace FilmLogAPI.Services
{
    public interface IMovieService
    {
        Task<List<MovieDto>> SearchMoviesAsync(string title);
        Task<MovieDto?> GetMovieByTitleAsync(string title);
    }
    public class MovieService : IMovieService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public MovieService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<List<MovieDto>> SearchMoviesAsync(string title)
        {
            var apiKey = _config["OMDb:ApiKey"];
            var baseUrl = _config["OMDb:BaseUrl"];

            var url = $"{baseUrl}/?apikey={apiKey}&s={Uri.EscapeDataString(title)}&type=movie";
            var response = await _httpClient.GetFromJsonAsync<OmdbSearchResponse>(url);

            if (response?.Search == null || response.Response == "False")
                return new List<MovieDto>();

            return response.Search.Select(MapToDto).ToList();
        }

        public async Task<MovieDto?> GetMovieByTitleAsync(string title)
        {
            var apiKey = _config["OMDb:ApiKey"];
            var baseUrl = _config["OMDb:BaseUrl"];

            var url = $"{baseUrl}/?apikey={apiKey}&t={Uri.EscapeDataString(title)}";
            var result = await _httpClient.GetFromJsonAsync<OmdbMovieDetail>(url);

            if (result == null || result.Response == "False") return null;

            return new MovieDto
            {
                ImdbID = result.imdbID ?? string.Empty,
                Title = result.Title ?? string.Empty,
                Year = result.Year ?? string.Empty,
                Poster = result.Poster ?? string.Empty,
                Actors = result.Actors ?? string.Empty,
                Genre = result.Genre ?? string.Empty,
                Plot = result.Plot ?? string.Empty
            };
        }
        private static MovieDto MapToDto(OmdbSearchItem item) => new MovieDto
        {
            ImdbID = item.imdbID ?? string.Empty,
            Title = item.Title ?? string.Empty,
            Year = item.Year ?? string.Empty,
            Poster = item.Poster ?? string.Empty,
            Actors = string.Empty,
            Genre = string.Empty,
            Plot = string.Empty
        };
        private class OmdbSearchResponse
        {
            public List<OmdbSearchItem>? Search { get; set; }
            public string? Response { get; set; }
        }
        private class OmdbSearchItem
        {
            public string? Title { get; set; }
            public string? Year { get; set; }
            public string? imdbID { get; set; }
            public string? Poster { get; set; }
        }
        private class OmdbMovieDetail
        {
            public string? Title { get; set; }
            public string? Year { get; set; }
            public string? imdbID { get; set; }
            public string? Poster { get; set; }
            public string? Actors { get; set; }
            public string? Genre { get; set; }
            public string? Plot { get; set; }
            public string? Response { get; set; }
        }
    }
}