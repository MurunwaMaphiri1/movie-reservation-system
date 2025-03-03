using Microsoft.AspNetCore.Mvc;
using MovieReservationsSystem.Data;
using MovieReservationsSystem.Models.Entities;

namespace MovieReservationsSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeSlotsController: ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TimeSlotsController(ApplicationDbContext context)
        {
            _context = context;
        }
    
        //Get all TimeSlots
        [HttpGet]
        public async Task<ActionResult> GetTimeSlots()
        {
            var allTimeSlots = _context.TimeSlots.ToList();
            return Ok(allTimeSlots);
        }
    
        //Add timeslots
        [HttpPost("add-time-slot")]
        public async Task<IActionResult> AddTimeslots(TimeSlots timeSlots)
        {
            var timeSlot = _context.TimeSlots.Add(timeSlots);
            await _context.SaveChangesAsync();
            return Ok(timeSlot);
        }
        
        //Get timeslotId by time string
        [HttpGet("get-timeSlotId-by-time-string")]
        public async Task<ActionResult> GetTimeSlotIdByTimeString([FromQuery] string timeString)
        {
            var time = _context.TimeSlots.FirstOrDefault(t => t.TimeSlot.ToString() == timeString);
            
            return Ok(time.TimeSlotId);
        }
        
        //Delete Timeslot
        [HttpDelete("delete-time-slot/{id}")]
        public async Task<IActionResult> DeleteTimeSlot(int id)
        {
            var timeSlot = await _context.TimeSlots.FindAsync(id);
            if (timeSlot == null) return NotFound();
            _context.TimeSlots.Remove(timeSlot);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    
        //Update Timeslot
        [HttpPut("update-time-slot/{id}")]
        public async Task<IActionResult> UpdateTimeSlot([FromQuery] int id, TimeSlots timeSlot)
        {
            if (id != timeSlot.TimeSlotId) return BadRequest();
            _context.Update(timeSlot);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }    
}
