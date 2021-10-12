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
using DotnetCoreApiPhotoGallery.Models;
using ExifPhotoReader;


namespace DotnetCoreApiPhotoGallery.Controllers
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
        public async Task<ActionResult<Album>> GetPhotoByAlbum(long id)
        {
            Album album = await _context.Album
                .Include(x => x.Photos)
                .OrderBy(x=>x.DateCreate)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (album == null)
            {
                return NotFound();
            }

            return album;
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
            _context.Entry(photo).Property(x => x.AlbumId).IsModified = false;
            _context.Entry(photo).Property(x => x.Cover).IsModified = false;
            _context.Entry(photo).Property(x => x.Extension).IsModified = false;
            _context.Entry(photo).Property(x => x.Hash).IsModified = false;
            _context.Entry(photo).Property(x => x.MimeType).IsModified = false;

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

        // PUT: api/Photo/5
        [HttpPut("SetCoverAlbum/{id}")]
        public async Task<IActionResult> SetCoverAlbum(long id, Photo photo)
        {                        
            try
            {
                Photo photoOld = await _context.Photo.Where(x=>x.Cover == true).Where(x=>x.AlbumId == photo.AlbumId).FirstOrDefaultAsync();
                
                if(photoOld != null)
                {
                    photoOld.Cover = false;
                }

                Photo photoNew = await _context.Photo.FindAsync(id);

                photoNew.Cover = true;

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
        public async Task<ActionResult<Photo>> PostPhoto()
        {
            try
            {
                IFormFileCollection files = Request.Form.Files;

                Request.Form.TryGetValue("albumId", out var albumId);

                Album album = await _context.Album.FindAsync(Int64.Parse(albumId));

                foreach (IFormFile file in files)
                {
                    Photo photo = new Photo
                    { 
                        Name = Path.GetFileNameWithoutExtension(file.FileName),
                        Extension = Path.GetExtension(file.FileName),
                        MimeType = file.ContentType,
                        Hash = Guid.NewGuid().ToString("N"),
                        AlbumId = album.Id
                    };

                    this.UploadFile(file, photo.Hash, album.Hash);

                    _context.Photo.Add(photo);
                    
                    await _context.SaveChangesAsync();
                }

                return StatusCode(StatusCodes.Status201Created);
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
                Photo photo = await _context.Photo
                    .Include(x => x.Album)
                    .FirstOrDefaultAsync(x => x.Id == id);

                string file = $"{path}{photo.Album.Hash}\\{photo.Hash}";

                if (thumb == 1)
                {
                    file = $"{file}.thumb";
                }

                if (!PhotoExists(id))
                    return NotFound();

                MemoryStream memory = new MemoryStream();
                using (var stream = new FileStream(file, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                return File(memory, GetExtension()[photo.Extension.ToLower()], photo.Name);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("Properties/{id}")]
        public async Task<ActionResult<ExifImageProperties>> GetProperties(long Id)
        {
            Photo photo = await _context.Photo
                .Include(x => x.Album)
                .FirstOrDefaultAsync(x => x.Id == Id);

            if (photo == null)
                return NotFound();

            string file = $"{path}{photo.Album.Hash}\\{photo.Hash}";

            ExifImageProperties exifImage = ExifPhoto.GetExifDataPhoto(file);

            return exifImage;
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

        public record ExifProperties()
        {
           public int ISO { get; set; }
           public string ShutterSpeedValue {  get; set; }
           public string MaxApertureValue {  get; set; }

        }
    }
}