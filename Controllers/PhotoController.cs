using System;
using System.Collections.Generic;
using System.Drawing;
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
    public class PhotoController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private static IWebHostEnvironment _webHostEnvironment;
        private static string path;

        public PhotoController(ApiDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            path = $"{_webHostEnvironment.WebRootPath}\\upload\\";
        }

        // GET: api/Photo
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Photo>>> GetPhoto()
        {
            return await _context.Photo
                    .Include(x => x.Album)
                    .ToListAsync();
        }

        // GET: api/Photo/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Photo>> GetPhoto(long id)
        {
            var photo = await _context.Photo.FindAsync(id);

            if (photo == null)
            {
                return NotFound();
            }

            return photo;
        }

        // GET: api/Photo/5
        [HttpGet("Album/{id}")]
        public async Task<ActionResult<List<Photo>>> GetPhotoByAlbum(long id)
        {
            List<Photo> photo = await _context.Photo.Where(x=>x.AlbumId == id).ToListAsync();

            if (photo == null)
            {
                return NotFound();
            }

            return photo;
        }

        // PUT: api/Photo/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPhoto(long id, Photo photo)
        {
            if (id != photo.Id)
            {
                return BadRequest();
            }

            _context.Entry(photo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PhotoExists(id))
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

        // POST: api/Photo
        [HttpPost]
        public async Task<ActionResult<Photo>> PostPhoto([FromForm] Photo photo)
        {
            try
            {
                photo.Name = photo.File.FileName;
                photo.Extension = Path.GetExtension(photo.File.FileName);
                photo.MimeType = photo.File.ContentType;
                photo.Hash = Guid.NewGuid().ToString("N");

                Album album = await _context.Album.FindAsync(photo.AlbumId);

                this.UploadFile(photo.File, photo.Hash, album.Hash);

                _context.Photo.Add(photo);

                await _context.SaveChangesAsync();

                return CreatedAtAction("GetPhoto", new { id = photo.Id }, photo);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // DELETE: api/Photo/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(long id)
        {
            try
            {
                var photo = await _context.Photo
                    .Include(x => x.Album)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (photo == null)
                {
                    return NotFound();
                }

                System.IO.File.Delete($"{path}{photo.Album.Hash}\\{photo.Hash}");
                System.IO.File.Delete($"{path}{photo.Album.Hash}\\{photo.Hash}.thumb");

                _context.Photo.Remove(photo);

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        private bool PhotoExists(long id)
        {
            return _context.Photo.Any(e => e.Id == id);
        }

        private void UploadFile([FromForm] IFormFile objetctFile, string hash, string albumPath)
        {
            try
            {
                if (objetctFile.Length > 0)
                {
                    if (!Directory.Exists(albumPath))
                    {
                        Directory.CreateDirectory(albumPath);
                    }

                    using (FileStream fileStream = System.IO.File.Create($"{path}{albumPath}\\{hash}"))
                    {
                        objetctFile.CopyTo(fileStream);

                        fileStream.Flush();

                        using (Image image = Image.FromStream(fileStream))
                        {
                            new Bitmap(image, 250, (int)(250 * ((float)image.Height / (float)image.Width))).Save(Path.ChangeExtension($"{path}{albumPath}\\{hash}", "thumb"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Uploaded fail: {ex.Message}");
            }
        }

        [HttpGet("DownloadFile/{id}/{thumb?}")]
        public async Task<ActionResult> DownloadFile([FromRoute] long id, int thumb = 0)
        {
            try
            {
                var photo = await _context.Photo
                    .Include(x => x.Album)
                    .FirstOrDefaultAsync(x => x.Id == id);

                string file = $"{path}{photo.Album.Hash}\\{photo.Hash}";

                if (thumb == 1)
                {
                    file = $"{file}.thumb";
                }

                if (!PhotoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    MemoryStream memory = new MemoryStream();
                    using (var stream = new FileStream(file, FileMode.Open))
                    {
                        await stream.CopyToAsync(memory);
                    }
                    memory.Position = 0;

                    return File(memory, GetExtension()[photo.Extension.ToLower()], photo.Name);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        private Dictionary<string, string> GetExtension()
        {
            return new Dictionary<string, string>
            {
                {".jpg", "image/jpeg" },
                {".jpeg", "image/jpeg" },
                {".png", "image/png" },
                {".gif", "image/gif" },
                {".tiff", "image/tiff" }
            };
        }
    }
}