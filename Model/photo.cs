using System.Collections.Generic;

namespace FacebookAPIModel
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

    public class Analysis
    {
        public string userId { get; set; }
        public string albumId { get; set; }
        public string photoId { get; set; }
        public string source { get; set; }
        public string visionResult { get; set; }
    }

}
