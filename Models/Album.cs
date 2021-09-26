using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PhotoInfoApi.Models
{
    public class Album
    {
        [Key]
        public long Id { get; internal set; }
        public string Name { get; set; }
        [JsonIgnore]
        public string Hash { get; internal set; }
        public DateTime DateCreate { get; internal set; } = DateTime.Now;
        public IList<Photo> Photos { get; set; }
    }
}
