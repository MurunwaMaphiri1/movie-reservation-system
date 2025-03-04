using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieReservationsSystem.Data;
using MovieReservationsSystem.Models.Entities;
using StackExchange.Redis;

namespace MovieReservationsSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController: ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public MovieController(IConfiguration configuration, ApplicationDbContext context, ILogger<MovieController> logger)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }
    
        //Get all movies
        [HttpGet]
        public async Task<ActionResult> GetMovies()
        {
            var allMovies = _context.Movies.ToList();
            return Ok(allMovies);
        }
        
        //Get movie by id
        [HttpGet("get-movie/movie-id/{id}")]
        public async Task<ActionResult> GetMovieById(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            return Ok(movie);
        }

        [HttpGet("get-movie/movie-title/{title}")]
        public async Task<ActionResult> GetMovieByTitle(string title)
        {
            var movie = await _context.Movies
                .Where(m => m.Title.ToLower().Contains(title.ToLower()))
                .ToListAsync();

            if (!movie.Any()) return NotFound();
            return Ok(movie);
        }
        
        //Add movie
        [HttpPost("add-movie")]
        public async Task<IActionResult> AddMovie([FromBody] Movies movie)
        {
            var movieToAdd = await _context.Movies.AddAsync(movie);
            await _context.SaveChangesAsync();
            return Ok(movieToAdd);
        }
        
        //Update Movie
        [HttpPut("{id}/update-movie")]
        public async Task<IActionResult> UpdateMovie(int id, [FromBody] Movies movie)
        {
            var movieToUpdate = await _context.Movies.FindAsync(id);
            movieToUpdate.Title = movie.Title;
            movieToUpdate.Description = movie.Description;
            movieToUpdate.Actors = movie.Actors;
            movieToUpdate.Image = movie.Image;
            movieToUpdate.ReleaseDate = movie.ReleaseDate;
            movieToUpdate.Genres = movie.Genres;
            movieToUpdate.ReleaseDate = movie.ReleaseDate;
            movieToUpdate.TicketPrice = movie.TicketPrice;
            movieToUpdate.Trailer = movie.Trailer;
            movieToUpdate.Duration = movie.Duration;
            movieToUpdate.DirectedBy = movie.DirectedBy;
            await _context.SaveChangesAsync();
            return Ok(movieToUpdate);
        }
        
        //Delete Movie
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            _context.Movies.Remove(movie); 
            await _context.SaveChangesAsync();
            return Ok(new {_logger = $"{movie.Title} has been deleted"});
        }
    }
}
