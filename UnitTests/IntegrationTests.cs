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
    public class IntegrationTests : IDisposable
    {
        public async void Dispose()
        {
            var dbService = new DBService(null);
            dbService.ReleaseAssistants();
        }

        [Fact]
        public void ShouldCreateService()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
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
                result.Add(new Assistant() { Id = 1 + i, Distance = 1 + i });
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
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            var dbService = new DBService(null);
            var roadsideAssistanceService = new RoadsideAssistanceService(null, dbService);

            //return the correct number of nearest assistants 
            var nearest = await roadsideAssistanceService.FindNearestAssistants(new Geolocation() { Latitude = 35, Longitude = -58 }, expected);
            Assert.Equal(limit, nearest.Count());
        }

        [Fact]
        public async void ShouldUpdateTheCorrectAssistant()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            var dbService = new DBService(null);
            var roadsideAssistanceService = new RoadsideAssistanceService(null, dbService);

            //update location of assistant 1 and ensure assistant location is updated
            await roadsideAssistanceService.UpdateAssistantLocation(new Assistant(1), new Geolocation() { Latitude = 10, Longitude = 10 });
            var assistants = await dbService.GetAssistants();
            var assistant = assistants.Where(x => x.Id == 1).First();
            Assert.Equal(10, assistant.Location.X);
            Assert.Equal(10, assistant.Location.X);

            //update location of assistant 1 and ensure assistant location is updated
            await roadsideAssistanceService.UpdateAssistantLocation(new Assistant(1), new Geolocation() { Latitude = 11, Longitude = 11 });
            assistants = await dbService.GetAssistants();
            assistant = assistants.Where(x => x.Id == 1).First();
            Assert.Equal(11, assistant.Location.X);
            Assert.Equal(11, assistant.Location.X);
        }

        [Theory]
        [InlineData(0, 10, 0, 20, 0, 30, 0, 10, 1, 2, 3)]
        [InlineData(0, 10, 0, 20, 0, 30, 0, 21, 2, 3, 1)]
        [InlineData(0, 10, 0, 20, 0, 30, 0, 19, 2, 1, 3)]
        [InlineData(0, 10, 0, 20, 0, 30, 0, 30, 3, 2, 1)] 
        //la1, lo1, la2, lo2, la3, lo3 are the locations of assistants 1, 2, 3
        //cla, clo is the location of the customer
        //expectedFirst, expectedSecond, expectedThird are the expected sorted order of assistants
        public async void ShouldOrderTheNearestAssistantsCorrectly(double la1, double lo1, double la2, double lo2, double la3, double lo3, double cla, double clo, int expectedFirst, int expectedSecond, int expectedThird)
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            var dbService = new DBService(null);
            await dbService.ReleaseAssistants();
            var roadsideAssistanceService = new RoadsideAssistanceService(null, dbService);

            //setup the assistants at the locations provided
            await roadsideAssistanceService.UpdateAssistantLocation(new Assistant(1), new Geolocation() { Latitude = la1, Longitude = lo1 });
            await roadsideAssistanceService.UpdateAssistantLocation(new Assistant(2), new Geolocation() { Latitude = la2, Longitude = lo2 });
            await roadsideAssistanceService.UpdateAssistantLocation(new Assistant(3), new Geolocation() { Latitude = la3, Longitude = lo3 });

            //find the nearest and ensure it is as expected
            var nearest = await roadsideAssistanceService.FindNearestAssistants(new Geolocation() { Latitude = cla, Longitude = clo }, 3);
            Assert.Equal(expectedFirst, nearest.ElementAt(0).Id);
            Assert.Equal(expectedSecond, nearest.ElementAt(1).Id);
            Assert.Equal(expectedThird, nearest.ElementAt(2).Id);

        }


        [Theory]
        [InlineData(15, 25, 0, 0)]
        [InlineData(14, 24, 0.5, 1)]
        [InlineData(93, 93, 0.9, -1.3)]
        [InlineData(0, 0, 0.2, 0.2)]
        [InlineData(-10, -10, 0.1, 0)]
        [InlineData(-90, 0, -1.2, -1.2)]
        [InlineData(0, 90, -2, 0)]
        public async void ShouldFindTheClosestAssistant(double lat, double lng, double latOffset, double langOffset)
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            var dbService = new DBService(null);
            await dbService.ReleaseAssistants();

            var roadsideAssistanceService = new RoadsideAssistanceService(null, dbService);
            //setup the assistant at various locations
            await roadsideAssistanceService.UpdateAssistantLocation(new Assistant(1), new Geolocation() { Latitude = lat, Longitude = lng });

            //find the assistant 1 for customer 1 by placing customer at nearby offset location
            var nearest = await roadsideAssistanceService.FindNearestAssistants(new Geolocation() { Latitude = lat+latOffset, Longitude = lng+langOffset }, 1);
            Assert.Equal(1, nearest.First().Id);
        }

        [Theory]
        [InlineData(15, 25, 0, 0)]
        [InlineData(14, 24, 0.5, 1)]
        [InlineData(93, 93, 0.9, -1.3)]
        [InlineData(0, 0, 0.2, 0.2)]
        [InlineData(-10, -10, 0.1, 0)]
        [InlineData(-90, 0, -1.2, -1.2)]
        [InlineData(0, 90, -2, 0)]
        public async void ShouldReserveTheClosestAssistant(double lat, double lng, double latOffset, double langOffset)
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            var dbService = new DBService(null);
            await dbService.ReleaseAssistants();

            var roadsideAssistanceService = new RoadsideAssistanceService(null, dbService);
            //setup the assistant at various locations
            await roadsideAssistanceService.UpdateAssistantLocation(new Assistant(1), new Geolocation() { Latitude = lat, Longitude = lng });

            //reserve assistant 1 for customer 1 by placing customer at nearby offset location
            var assistant = await roadsideAssistanceService.ReserveAssistant(new Customer(1), new Geolocation() { Latitude = lat + latOffset, Longitude = lng + langOffset });
            Assert.Equal(1, assistant?.Id);
        }

        [Fact]
        public async void ShouldReleaseTheReservedAssistant()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            var dbService = new DBService(null);
            await dbService.ReleaseAssistants();
            var roadsideAssistanceService = new RoadsideAssistanceService(null, dbService);

            //setup assistant one at location 10, 10
            await roadsideAssistanceService.UpdateAssistantLocation(new Assistant(1), new Geolocation() { Latitude = 10, Longitude = 10 });

            //reserve for customer 1 at location 1 and ensure it is assistant 1
            var assistant = await roadsideAssistanceService.ReserveAssistant(new Customer(1), new Geolocation() { Latitude = 10, Longitude = 10 });
            Assert.Equal(1, assistant?.Id);

            //release assistant 1 from customer 1
            await roadsideAssistanceService.ReleaseAssistant(new Customer(1), new Assistant(1));

            //ensure there are not actvice reservations for the assistant
            var activeReservations = await dbService.GetReservations(true);
            Assert.Equal(0, activeReservations.Where(x => x.AssistantId==1).Count());
        }


        [Fact]
        public async void ShouldNotReserveAnotherAssistantForSameCustomer()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            var dbService = new DBService(null);
            await dbService.ReleaseAssistants();
            var roadsideAssistanceService = new RoadsideAssistanceService(null, dbService);

            //setup assistant one at location 10, 10
            await roadsideAssistanceService.UpdateAssistantLocation(new Assistant(1), new Geolocation() { Latitude = 10, Longitude = 10 });

            //reserve for customer 1 at location 1 and ensure it is assistant 1
            var assistant = await roadsideAssistanceService.ReserveAssistant(new Customer(1), new Geolocation() { Latitude = 10, Longitude = 10 });
            Assert.Equal(1, assistant?.Id);

            //reserve again for customer 1 at location 1 and ensure it is assistant 1
            assistant = await roadsideAssistanceService.ReserveAssistant(new Customer(1), new Geolocation() { Latitude = 10, Longitude = 10 });
            Assert.Equal(1, assistant?.Id);

            //ensure customer 1 and assistant 1 are reserved
            var activeReservations = await dbService.GetReservations(true);
            Assert.Equal(1, activeReservations.Where(x => x.CustomerId==1 && x.AssistantId == 1).Count());
        }

        [Fact]
        public async void ShouldNotReserveTheSameAssitantTwice()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            var dbService = new DBService(null);
            await dbService.ReleaseAssistants();
            var roadsideAssistanceService = new RoadsideAssistanceService(null, dbService);

            //setup the 2 assistants fairly close to each other
            await roadsideAssistanceService.UpdateAssistantLocation(new Assistant(1), new Geolocation() { Latitude = 10, Longitude = 10 });
            await roadsideAssistanceService.UpdateAssistantLocation(new Assistant(2), new Geolocation() { Latitude = 10.3, Longitude = 10.3 });

            //reserve an assistant for customer 1 at same location as assistant 1
            var assistant = await roadsideAssistanceService.ReserveAssistant(new Customer(1), new Geolocation() { Latitude = 10, Longitude = 10 });
            //should reserve customer 1 to assistant 1
            Assert.Equal(1, assistant?.Id);

            //reserve an assistant for customer 2 at same location as assistant 1
            assistant = await roadsideAssistanceService.ReserveAssistant(new Customer(2), new Geolocation() { Latitude = 10, Longitude = 10 });
            //should reserve customer 1 to assistant 2
            Assert.Equal(2, assistant?.Id);

            //ensure both customers are reserved to both assistants
            var activeReservations = await dbService.GetReservations(true);
            Assert.Equal(1, activeReservations.Where(x => x.CustomerId == 1 && x.AssistantId == 1).Count());
            Assert.Equal(1, activeReservations.Where(x => x.CustomerId == 2 && x.AssistantId == 2).Count());
        }

    }
}