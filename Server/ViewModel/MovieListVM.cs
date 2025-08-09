using System.ComponentModel.DataAnnotations;

namespace MovieApiDemo.ViewModel
{
    public class MovieListVM
    {
        public int Id { get; set; }
        public string Title { get; set; }

        // list of actors
        public List<ActorVM> Actors { get; set; }
        public string Language { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string CoverImage { get; set; }

    }
}
