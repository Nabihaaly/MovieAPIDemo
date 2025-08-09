using MovieApiDemo.Models;
using System.ComponentModel.DataAnnotations;

namespace MovieApiDemo.ViewModel
{
    public class ActorVM
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
