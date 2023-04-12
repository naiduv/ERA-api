using EmergencyRoadsideAssistance.Controllers;
using EmergencyRoadsideAssistance.Models;
using System.Collections.Generic;

namespace EmergencyRoadsideAssistance.Services
{
    public class RoadsideAssistanceService : IRoadsideAssistanceService
    {
        private readonly ILogger<RoadsideAssistanceService> _logger;

        private readonly IDBService _dbService;

        public RoadsideAssistanceService(ILogger<RoadsideAssistanceService> logger, IDBService dBService)
        {
            _logger = logger;
            _dbService = dBService;
        }

        public async Task UpdateAssistantLocation(Assistant assistant, Geolocation assistantLocation)
        {
            await _dbService.UpdateAssistantLocation(assistant, assistantLocation);
        }

        public async Task<SortedSet<Assistant>> FindNearestAssistants(Geolocation geolocation, int limit)
        {
            var list = await _dbService.FindNearestAssistants(geolocation, limit);
            var sortedSet = new SortedSet<Assistant>();
            foreach(var item in list)
            {
                sortedSet.Add(item);
            }

            return sortedSet;
        }

        public async Task ReleaseAssistant(Customer customer, Assistant assistant)
        {
            await _dbService.ReleaseAssistant(customer, assistant);
        }

        public async Task<Assistant?> ReserveAssistant(Customer customer, Geolocation customerLocation)
        {
            var customerHasReservation = await _dbService.CustomerHasReservation(customer);
            if (customerHasReservation)
                return await _dbService.CustomerReservation(customer);

            var nearestAssistants = await _dbService.FindNearestUnreservedAssistant(customerLocation);
            if (nearestAssistants.Count() == 1)
            {
                var nearest = nearestAssistants.First();
                nearest.IsReserved = true;
                await _dbService.ReserveAssistant(customer, nearest);
                return nearest;
            }

            return null;                    
        }

    }
}
