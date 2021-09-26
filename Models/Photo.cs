using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PhotoInfoApi.Models
{
    public class Photo
    {
        [Key]
        public long Id { get; internal set; }
        public string Name { get; set; }
        public DateTime DateCreate { get; internal set; } = DateTime.Now;
        [JsonIgnore]
        public string MimeType { get; internal set; }
        [JsonIgnore]
        public string Hash { get; internal set; }
        [JsonIgnore]
        public string Extension { get; internal set; }
        public bool Cover { get; internal set; }
        [NotMapped]
        [JsonIgnore]
        public IFormFile File { get; internal set; }
        public long AlbumId { get; internal set; }
        [ForeignKey("AlbumId")]
        [JsonIgnore]
        public virtual Album Album { get; }
    }
}
