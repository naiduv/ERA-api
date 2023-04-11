using Microsoft.Extensions.Logging;
using Moq;
using EmergencyRoadsideAssistance.Services;
using Xunit;
using EmergencyRoadsideAssistance.Models;
using NpgsqlTypes;
using System.Reflection.Metadata.Ecma335;
using System.Collections.Generic;
using NuGet.Frameworks;
using Dapper;

namespace IntegrationTests
{
    public class IntegrationTests
    {
        [Fact]
        public void ShouldCreateService()
        {
            var mock = new Mock<ILogger<RoadsideAssistanceService>>();
            ILogger<RoadsideAssistanceService> logger = mock.Object;

            var roadsideAssistanceService = new RoadsideAssistanceService(logger, null);
            //ensure service is created
            Assert.NotNull(roadsideAssistanceService);
        }

        List<Assistant> MakeAssistants(int size)
        {
            var result = new List<Assistant>();
            for (int i = 0; i < size; i++)
            {
                result.Add(new Assistant() { Id = 1+i, Distance = 1+i});
            }
            return result;
        }

        [Theory]
        [InlineData(5, 5)]
        [InlineData(4, 4)]
        [InlineData(3, 3)]
        [InlineData(2, 2)]
        public async void ReturnsCorrectNumberOfNearestAssistants(int limit, int expected)
        {
            var dbService = new DBService(null);

            var roadsideAssistanceService = new RoadsideAssistanceService(null, dbService);

            var nearest = await roadsideAssistanceService.FindNearestAssistants(new Geolocation() { Latitude=35, Longitude=-58 }, expected);
            Assert.Equal(limit, nearest.Count());            
        }

        [Fact]
        public async void ShouldUpdateTheCorrectAssistant()
        {
            var dbService = new DBService(null);
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            var roadsideAssistanceService = new RoadsideAssistanceService(null, dbService);
            await roadsideAssistanceService.UpdateAssistantLocation(new Assistant(1), new Geolocation() { Latitude = 10, Longitude = 10 });

            var assistants = await dbService.GetAssistants();
            var assistant = assistants.Where(x => x.Id == 1).First();
            Assert.Equal(10, assistant.Location.Latitude);
            Assert.Equal(10, assistant.Location.Longitude);


            await roadsideAssistanceService.UpdateAssistantLocation(new Assistant(1), new Geolocation() { Latitude = 11, Longitude = 11 });
            assistants = await dbService.GetAssistants();
            assistant = assistants.Where(x => x.Id == 1).First();
            Assert.Equal(11, assistant.Location.Latitude);
            Assert.Equal(11, assistant.Location.Longitude);
        }


        [Fact]
        public async void ShouldReturnTheCorrectClosestAssistant()
        {
            var dbService = new DBService(null);

            var roadsideAssistanceService = new RoadsideAssistanceService(null, dbService);
            await roadsideAssistanceService.UpdateAssistantLocation(new Assistant(1), new Geolocation() { Latitude = 10, Longitude = 10 });

            var nearest = await roadsideAssistanceService.FindNearestAssistants(new Geolocation() { Latitude = 10, Longitude = 10 }, 2);
            Assert.Equal(1, nearest.First().Id);
        }


        [Fact]
        public async void ShouldReserveTheCorrectClosestAssistant()
        {
            var dbService = new DBService(null);

            var roadsideAssistanceService = new RoadsideAssistanceService(null, dbService);
            await roadsideAssistanceService.UpdateAssistantLocation(new Assistant(1), new Geolocation() { Latitude = 10, Longitude = 10 });

            var assistant = await roadsideAssistanceService.ReserveAssistant(new Customer(1), new Geolocation() { Latitude = 10, Longitude = 10 });
            Assert.Equal(1, assistant?.Id);
        }


        [Fact]
        public async void ShouldReleaseTheReservedAssistant()
        {
            var dbService = new DBService(null);

            var roadsideAssistanceService = new RoadsideAssistanceService(null, dbService);
            
            await roadsideAssistanceService.ReleaseAssistant(new Customer(1), new Assistant(1));
            var activeReservations = await dbService.GetReservations(true);
            Assert.Equal(0, activeReservations.Where(x => x.AssistantId==1).Count());
        }


        [Fact]
        public async void ShouldNotReserveAnotherAssistant()
        {
            var dbService = new DBService(null);
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            var roadsideAssistanceService = new RoadsideAssistanceService(null, dbService);
            await roadsideAssistanceService.UpdateAssistantLocation(new Assistant(1), new Geolocation() { Latitude = 10, Longitude = 10 });

            var assistant = await roadsideAssistanceService.ReserveAssistant(new Customer(1), new Geolocation() { Latitude = 10, Longitude = 10 });
            Assert.Equal(1, assistant?.Id);

            assistant = await roadsideAssistanceService.ReserveAssistant(new Customer(1), new Geolocation() { Latitude = 10, Longitude = 10 });
            Assert.Equal(1, assistant?.Id);

            var activeReservations = await dbService.GetReservations(true);
            Assert.Equal(1, activeReservations.Where(x => x.CustomerId==1 && x.AssistantId == 1).Count());
        }
    }
}