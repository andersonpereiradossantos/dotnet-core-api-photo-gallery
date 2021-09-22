using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoInfoApi.Models
{
    public class Album
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreate { get; set; } = DateTime.Now;
        public IList<Photo> Photos { get; set; }
    }
}
