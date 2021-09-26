using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoInfoApi.Models
{
    public class Photo
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreate { get; set; } = DateTime.Now;
        public string MimeType { get; set; }
        public string Hash { get; set; }
        [NotMapped]
        public IFormFile File { get; set; }
        public long AlbumId { get; set; }
        [ForeignKey("AlbumId")]
        public virtual Album Album { get; }
        public string Extension { get; internal set; }
    }
}
