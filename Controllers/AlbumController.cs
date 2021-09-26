using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoInfoApi.Models;

namespace PhotoInfoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlbumController : ControllerBase
    {
        private readonly ApiDbContext _context; 
        
        private static IWebHostEnvironment _webHostEnvironment;

        private static string path;

        public AlbumController(ApiDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            
            _webHostEnvironment = webHostEnvironment;

            path = $"{_webHostEnvironment.WebRootPath}\\upload\\";
        }

        // GET: api/Album
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Album>>> GetAlbum()
        {
            try
            {
                List<Album> albums = await _context.Album.Include(x => x.Photos.Where(p=>p.Cover == true)).ToListAsync();

                return albums;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // GET: api/Album/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Album>> GetAlbum(long id)
        {
            var album = await _context.Album
                .Include(x => x.Photos)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (album == null)
            {
                return NotFound();
            }

            return album;
        }

        // PUT: api/Album/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAlbum(long id, Album album)
        {
            if (id != album.Id)
            {
                return BadRequest();
            }

            _context.Entry(album).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AlbumExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Album
        [HttpPost]
        public async Task<ActionResult<Album>> PostAlbum(Album album)
        {
            try
            {
                album.Hash = Guid.NewGuid().ToString("N");
            
                Directory.CreateDirectory($"{path}{album.Hash}");
            
                _context.Album.Add(album);

                await _context.SaveChangesAsync();

                return CreatedAtAction("GetAlbum", new { id = album.Id }, album);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // DELETE: api/Album/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlbum(long id)
        {
            try
            {
                var album = await _context.Album.FindAsync(id);
                if (album == null)
                {
                    return NotFound();
                }

                System.IO.Directory.Delete($"{path}{album.Hash}", true);

                _context.Album.Remove(album);
                await _context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        private bool AlbumExists(long id)
        {
            return _context.Album.Any(e => e.Id == id);
        }
    }
}
