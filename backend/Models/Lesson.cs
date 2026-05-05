using System.Text.Json.Serialization;
namespace backend.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        public string Day { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string LessonName { get; set; }

        public int TeacherId { get; set; }
        [JsonIgnore]
        public Teacher? Teacher { get; set; }
    }
}
