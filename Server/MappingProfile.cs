using AutoMapper;
using MovieApiDemo.Models;
using MovieApiDemo.ViewModel;

namespace MovieApiDemo
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Movie, MovieListVM>();
            CreateMap<MovieListVM, Movie>();
            CreateMap<Movie, MovieDetailVM>();
            CreateMap<CreateMovieVM, Movie>().ForMember( x=> x.Actors, y => y.Ignore()); //actor tyoe mismatch hai islye isko yaahn hanle nh krrhy 

            CreateMap<Actor, ActorDetailsVM>();
            CreateMap<Actor, ActorVM>();
            CreateMap<ActorVM, Actor>();

        }


    }
}
