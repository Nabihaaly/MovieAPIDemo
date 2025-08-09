using MovieApiDemo.Models;
using System.ComponentModel.DataAnnotations;

namespace MovieApiDemo.ViewModel
{
    public class CreateMovieVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "name of the movie is required")]
        public string Title { get; set; }
        public string Description { get; set; }

        // list of actors
        public ICollection<int> Actors { get; set; }

        [Required(ErrorMessage ="Language not set")]
        public string Language { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string CoverImage { get; set; }

    }
}
