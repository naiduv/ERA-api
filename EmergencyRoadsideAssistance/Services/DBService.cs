using EmergencyRoadsideAssistance.Models;
using Npgsql;
using System.Data;
using Dapper;
using System.Collections.Generic;

namespace EmergencyRoadsideAssistance.Services
{
    public class DBService : IDBService
    {
        private readonly IDbConnection _db;

        private readonly ILogger<DBService> _logger;

        public DBService(ILogger<DBService> logger)
        {
            _db = new NpgsqlConnection("Host=host.docker.internal;Port=15432;Username=era;Password=password");
            _logger = logger;
        }

        public async Task UpdateAssistantLocation(Assistant assistant, Geolocation location)
        {
            await _db.ExecuteAsync($@"update assistant set location = point({location.Longitude},{location.Latitude}) where id = {assistant.Id}");
        }

        public async Task<IEnumerable<Assistant>> FindNearestAssistants(Geolocation location, int limit)
        {
            return await _db.QueryAsync<Assistant>($@"select a.id, a.is_reserved, a.location, a.location <@> point({location.Longitude}, {location.Latitude}) as distance from assistant a                                                        
                                                        order by 4 limit {limit};");
        }

        public async Task<IEnumerable<Assistant>> FindNearestUnreservedAssistant(Geolocation location)
        {
            return await _db.QueryAsync<Assistant>($@"select a.id, a.is_reserved, a.location, a.location <@> point({location.Longitude}, {location.Latitude}) as distance from assistant a
                                                        where is_reserved = false
                                                        order by 4 limit 1;");
        }

        public async Task ReserveAssistant(Customer customer, Assistant assistant, Geolocation location)
        {
            var assistantIsReserved = await _db.QuerySingleOrDefaultAsync<bool>($@"select is_reserved from assistant where id = {assistant.Id} and is_reserved = false;");
            var customerHasReservation = await _db.QuerySingleOrDefaultAsync<bool>($@"select has_reservation from customer where id = {customer.Id} and has_reservation = false;");
            if (!assistantIsReserved && !customerHasReservation)
            {
                await _db.ExecuteAsync($@"insert into reservation(customer_id, assistant_id, is_reserved, customer_location, assistant_location, created_on, updated_on) values({customer.Id}, {assistant.Id}, true, point({location.Longitude},{location.Latitude}), point({assistant.Location.X},{assistant.Location.Y}), now(), null) returning is_reserved;
                                      update assistant set is_reserved=true where id = {assistant.Id};
                                      update customer set has_reservation=true where id = {customer.Id};");
            }
        }

        public async Task ReleaseAssistant(Customer customer, Assistant assistant)
        {
            var activeReservationExists = await _db.QuerySingleOrDefaultAsync<bool>($@"select is_reserved from reservation where customer_id = {customer.Id} and assistant_id = {assistant.Id} and is_reserved = true;");
            if (activeReservationExists)
            {
                await _db.ExecuteAsync($@"update reservation set is_reserved=false, updated_on = now() where customer_id = {customer.Id} and assistant_id = {assistant.Id} and is_reserved = true; 
                                      update assistant set is_reserved=false where id = {assistant.Id};
                                      update customer set has_reservation=false where id = {customer.Id};");
            }
        }

        public async Task ReleaseAssistants()
        {
            await _db.ExecuteAsync($@"update reservation set is_reserved=false, updated_on = now() where is_reserved = true; 
                                      update assistant set is_reserved=false;
                                      update customer set has_reservation=false;");
        }

        public async Task<bool> CustomerHasReservation(Customer customer)
        {
            return await _db.QueryFirstOrDefaultAsync<bool>($@"select has_reservation from public.customer where id = {customer.Id};");
        }

        public async Task<Assistant> CustomerReservation(Customer customer)
        {
            return await _db.QueryFirstOrDefaultAsync<Assistant>($@"select a.id, r.is_reserved, a.location from public.reservation r 
                                                                    join public.assistant a on r.assistant_id = a.id 
                                                                    join public.customer c on r.customer_id = c.id 
                                                                    where c.id = {customer.Id} and r.is_reserved = true;");
        }

        public async Task<IEnumerable<Reservation>> GetReservations(bool active)
        {
            var clause = active ? "where is_reserved = true" : "";
            return await _db.QueryAsync<Reservation>($@"select *, assistant_location <@> customer_location as distance from reservation {clause}");
        }


        public async Task<IEnumerable<Assistant>> GetAssistants()
        {
            return await _db.QueryAsync<Assistant>($@"select id, is_reserved, location from assistant");
        }
    }
}