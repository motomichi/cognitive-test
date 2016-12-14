using System.Collections.Generic;

namespace Model
{
    public class Album
    {
        public string albumId { get; set; }
        public string albumName { get; set; }
        public List<Photo> photo { get; set; }
    }

    public class Photo
    {
        public string photoId { get; set; }
        public List<string> source { get; set; }
    }
}
