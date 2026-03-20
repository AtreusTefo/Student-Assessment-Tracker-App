using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Infrastructure.Data;

namespace StudentAssessmentTracker.Presentation.Controllers
{
    /// <summary>
    /// Read-only endpoint that exposes the seeded subjects list.
    /// Used by the front-end to populate the Subject dropdown on teacher registration/edit.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SubjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public SubjectsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>Returns all subjects ordered alphabetically</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubjectDto>>> GetAll()
        {
            var subjects = await _context.Subjects
                .OrderBy(s => s.Name)
                .ToListAsync();
            return Ok(_mapper.Map<IEnumerable<SubjectDto>>(subjects));
        }
    }
}
