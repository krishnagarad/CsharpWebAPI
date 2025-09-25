using EventCalendar.Application.Contracts.CivicPlus;
using EventCalendar.Application.Contracts.CivicPlus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventCalendar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventsController : ControllerBase
    {
        private readonly IEventsService _eventsService;
        public EventsController(IEventsService eventsService)
        {
            _eventsService = eventsService;
        }
        [HttpGet]
        public async Task<EventResponseDto> GetEvents(string? orderBy, int top = 20, int skip = 0)
        {
            var queryParams = new Dictionary<string, string>
                {                   
                    { "$top", top.ToString() },
                    { "$skip", skip.ToString()},
                    { "$orderby", string.IsNullOrEmpty(orderBy)?"startDate desc":orderBy },
                };
            var events = await _eventsService.GetEventsAsync(queryParams);
            return events;
        }
        [HttpPost]
        public async Task<EventsDto> CreateEvent(EventsDto events)
        {
            var createdEvent = await _eventsService.CreateEvent(events);
            return createdEvent;
        }
    }
}
