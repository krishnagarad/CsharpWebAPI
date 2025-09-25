using EventCalendar.Application.Contracts.CivicPlus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventCalendar.Application.Contracts.CivicPlus
{
    public interface IEventsService
    {
        Task<EventResponseDto> GetEventsAsync(Dictionary<string, string> queryParams);
        Task<EventsDto?> GetEventByIdAsync(Guid eventId);
        Task<EventsDto> CreateEvent(EventsDto events);
    }
}
