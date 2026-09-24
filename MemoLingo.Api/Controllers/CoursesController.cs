using MemoLingo.Api.Models;
using MemoLingo.Application.Models;
using MemoLingo.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace MemoLingo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CourseModel>), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 500)]
        public async Task<IActionResult> Get([FromQuery] int? userId)
        {
            var courses = await _courseService.GetTrackAsync(userId);
            return Ok(courses);
        }

        [HttpGet("user-courses")]
        [ProducesResponseType(typeof(IEnumerable<UserCourseModel>), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 500)]
        public async Task<IActionResult> GetUserCourses([FromQuery] int? userId)
        {
            var courses = await _courseService.GetUserCoursesAsync(userId);
            return Ok(courses);
        }

        [HttpGet("available-languages")]
        [ProducesResponseType(typeof(IEnumerable<LanguageModel>), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 500)]
        public async Task<IActionResult> GetAvailableLanguages([FromQuery] int? userId)
        {
            var languages = await _courseService.GetAvailableLanguagesAsync(userId);
            return Ok(languages);
        }

        [HttpPost("enroll")]
        [ProducesResponseType(typeof(UserCourseModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        public async Task<IActionResult> Enroll([FromBody] EnrollCourseModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorResponseModel { Errors = "Modelo inválido" });

            try
            {
                var course = await _courseService.EnrollAsync(model.UserId, model.LanguageId);
                return Ok(course);
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                return BadRequest(new ErrorResponseModel { Errors = ex.Message });
            }
        }

        [HttpPut("active-course")]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        public async Task<IActionResult> SetActiveCourse([FromBody] EnrollCourseModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new ErrorResponseModel { Errors = "Modelo inválido" });

            var updated = await _courseService.SetActiveCourseAsync(model.UserId, model.LanguageId);

            if (!updated) return BadRequest(new ErrorResponseModel { Errors = "Curso não encontrado para o usuário" });

            return Ok();
        }
    }
}
