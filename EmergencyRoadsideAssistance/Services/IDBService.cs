﻿using EmergencyRoadsideAssistance.Models;

namespace EmergencyRoadsideAssistance.Services
{
    public interface IDBService
    {
        public Task UpdateAssistantLocation(Assistant assistant, Geolocation location);
        Task<IEnumerable<Assistant>> FindNearestAssistants(Geolocation location, int limit);
        Task ReserveAssistant(Customer customer, Assistant assistant);
        Task ReleaseAssistant(Customer customer, Assistant assistant);
        Task<bool> CustomerHasReservation(Customer customer);
        Task<Assistant> CustomerReservation(Customer customer);
        Task<IEnumerable<Reservation>> GetReservations(bool active);
        Task<IEnumerable<Assistant>> GetAssistants();
    }
}