using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using MovieApiDemo.Data;
using MovieApiDemo.Models;
using MovieApiDemo.ViewModel;
using System.Linq;
using System.Net.Http.Headers;

namespace MovieApiDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly MovieDbContext _context;
        private readonly IMapper _mapper;
        public MovieController(MovieDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Get(int pageIndex=0, int pageSize = 10)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                var movieCount = _context.Movies.Count();
                var movieList = _mapper.Map<List<MovieListVM>>(_context.Movies.Include(x => x.Actors).Skip(pageIndex * pageSize).Take(pageSize).ToList());

                response.Status = true;
                response.StatusMessage = "Showed all record";
                response.Data = new { Movies = movieList, Count = movieCount };

                return Ok(response);

            }
            catch (Exception ex)
            {
                //TODO: dp logging exceptions (if smthn went wrong ask user time wgera masla)
                response.Status = false;
                response.StatusMessage = ex.Message;

                return BadRequest(response);
            }
        }
        [HttpGet("{id}")]
        public IActionResult GetMovieById(int id)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                var movie = _context.Movies.Include(x => x.Actors).Where( x => x.Id ==id ).
                    //Select(x => new MovieDetailVM()
                    //{
                    //    Id = x.Id,
                    //    Title = x.Title,
                    //    Description = x.Description,
                    //    ReleaseDate = x.ReleaseDate,
                    //    Language = x.Language,
                    //    Actors = x.Actors.Select(y => new ActorVM
                    //    {
                    //        Id = y.Id,
                    //        Name = y.Name,
                    //        DateOfBirth = y.DateOfBirth
                    //    }).ToList(),
                    //    CoverImage = x.CoverImage

                    //}).
                    FirstOrDefault();
                if(movie == null)
                {
                    response.Status = false;
                    response.StatusMessage = "Record doesnt exist";
                    return BadRequest(response);
                }

                var movieData = _mapper.Map < MovieDetailVM > (movie);

                response.Status = true;
                response.StatusMessage = "Showed all record";
                response.Data = movieData;

                return Ok(response);

            }
            catch (Exception ex)
            {
                //TODO: dp logging exceptions (if smthn went wrong ask user time wgera masla)
                response.Status = false;
                response.StatusMessage = ex.Message;

                return BadRequest(response);
            }
        }

        [HttpPost]
        public IActionResult Post(CreateMovieVM model)
        {
            // modelstate check krta hai data annotations ky acc hai data 
            // match krengy jo id hai  actors ki wo us table m present hain bh ya nahi 
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                if (ModelState.IsValid)
                {
                    // it keeps only those whose Id is found in model.Actors
                    //x represents each actor in the database. Contains(x.Id) checks if the actor's ID exists in model's list.

                    var actors = _context.Actors.Where(x => model.Actors.Contains(x.Id)).ToList();
                    if (actors.Count() != model.Actors.Count())
                    {
                        response.Status = false;
                        response.StatusMessage = "Invalid actors assigned";

                        return BadRequest(response);
                    }

                    var postedModel = _mapper.Map<Movie>(model);
                    postedModel.Actors = actors;

                    _context.Movies.Add(postedModel);
                    _context.SaveChanges();

                    var responseModel = _mapper.Map<MovieDetailVM>(postedModel);

                    response.Status = true;
                    response.StatusMessage = "created successfully ";
                    response.Data = responseModel;

                    return Ok(response);
                }
                else
                {
                    response.Status = false;
                    response.StatusMessage = "not valid modelState";

                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;

                return BadRequest(response);
            }
        }

        [HttpPut]
        public IActionResult Put(CreateMovieVM model)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                if (ModelState.IsValid)
                {
                    // check id is valid
                    if (model.Id <= 0)
                    {
                        response.Status = false;
                        response.StatusMessage = "not valid id";

                        return BadRequest(response);
                    }

                    var actors = _context.Actors.Where(x => model.Actors.Contains(x.Id)).ToList();
                    if (model.Actors.Count != actors.Count)
                    {
                        response.Status = false;
                        response.StatusMessage = "Invalid actors assigned";

                        return BadRequest(response);
                    }

                    var movieDetails = _context.Movies.Include(x => x.Actors).Where(x => x.Id == model.Id).FirstOrDefault();
                    if (movieDetails == null)
                    {
                        response.Status = false;
                        response.StatusMessage = "Invalid movie record";

                        return BadRequest(response);
                    }

                    movieDetails.Title = model.Title;
                    movieDetails.Description = model.Description;
                    movieDetails.CoverImage = model.CoverImage;
                    movieDetails.ReleaseDate = model.ReleaseDate;
                    movieDetails.Language = movieDetails.Language;

                    // remove actors
                    var removedActors = movieDetails.Actors.Where(x => !model.Actors.Contains(x.Id)).ToList(); // agr movieDetails m 5 actors hain(pehly ky) or model m 3 hain(new data)
                    foreach (var actor in removedActors)
                    {
                        movieDetails.Actors.Remove(actor);
                    }
                    // add actors 
                    var addedActors = actors.Except(movieDetails.Actors).ToList();
                    foreach (var actor in addedActors)
                    {
                        movieDetails.Actors.Add(actor);
                    }
                    _context.SaveChanges();

                    var responseData = new MovieDetailVM()
                    {
                        Id = movieDetails.Id,
                        Title = movieDetails.Title,
                        ReleaseDate = movieDetails.ReleaseDate,
                        Language = movieDetails.Language,
                        Actors = movieDetails.Actors.Select(y => new ActorVM
                        {
                            Id = y.Id,
                            Name = y.Name,
                            DateOfBirth = y.DateOfBirth
                        }).ToList(),
                        CoverImage = movieDetails.CoverImage,
                        Description = movieDetails.Description
                    };

                    response.Status = true;
                    response.StatusMessage = "Updated successfully";
                    response.Data = responseData;

                    return Ok(response);
                }
                else
                {
                    response.Status = false;
                    response.StatusMessage = "not valid modelState";

                    return BadRequest(response);
                }

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = ex.Message;

                return BadRequest(response);
            }
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            BaseResponseModel response = new BaseResponseModel(); 

            try
            {

                var movie = _context.Movies.Where( x => x.Id == id ).FirstOrDefault();
                if( movie == null)
                {
                    response.Status = false;
                    response.StatusMessage = "id not valid";
                    return BadRequest(response);
                }

                _context.Movies.Remove(movie);
                _context.SaveChanges();

                response.Status = true;
                response.StatusMessage = "deleted successfully";

                return Ok(response);    
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.StatusMessage = "smthn went wrong ";

                return BadRequest(response);

                throw;
            }
        }

        [HttpPost]
        [Route("upload-movie-poster")]
        public async Task<IActionResult> UploadMoviePoster(IFormFile imageFile)
        {
            try
            {
                var filename = ContentDispositionHeaderValue
                    .Parse(imageFile.ContentDisposition)
                    .FileName.TrimStart('\"').TrimEnd('\"');
                var newPath = @"C:\Project net\to-delete"; //store our images in this directory

                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }

                string[] allowedImageExtensions = new string[] { ".jpg", ".jpeg", ".png" };

                if (!allowedImageExtensions.Contains(Path.GetExtension(filename)))
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Status = false,
                        StatusMessage = "only .jpg .jpeg, .png extnesions are allowed"
                    });
                }

                string newFileName = Guid.NewGuid() + Path.GetExtension(filename); //Guid.NewGuid() generates a unique filename, preventing name collisions.
                string fullFilePath = Path.Combine(newPath, newFileName);

                //Opens a file stream and saves the uploaded image into the folder.
                using (var stream = new FileStream(fullFilePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                //Returns the URL where the image can be accessed (you may want to serve that folder via static files if needed).
                return Ok(new
                {
                    ProfileImage = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/images/{newFileName}"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponseModel
                {
                    Status = false,
                    StatusMessage = "Error Occured"
                });
            }
        }
    }
}


// viewmodels: Listing the data and seeing particular details of one list itme is diff
// actors ko lenegy viewmodel bnake

// ek viewmodel eg: movies ka hai us m actors hai, so we'll use axctorViewModel in it 
// moovielist and moviewlist details sso we'll inherit 