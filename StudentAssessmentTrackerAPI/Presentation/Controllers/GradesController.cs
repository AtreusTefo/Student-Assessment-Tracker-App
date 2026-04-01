using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Infrastructure.Data;

namespace StudentAssessmentTracker.Presentation.Controllers
{
    /// <summary>
    /// Read-only endpoint that exposes the seeded Grades lookup table.
    /// Frontend uses this to populate the grade dropdown — teachers cannot
    /// enter free text, preventing inconsistent grade values.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GradesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        /// <summary>Initialises the controller with the database context and mapper.</summary>
        public GradesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>Returns all available grade levels ordered by level number</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<GradeDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GradeDto>>> GetAll()
        {
            var grades = await _context.Grades
                .AsNoTracking()
                .OrderBy(g => g.Level)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<GradeDto>>(grades));
        }
    }
}
