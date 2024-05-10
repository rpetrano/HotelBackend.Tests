using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Net.Http.Json;
using HotelBackend.Models;
using NUnit.Framework.Legacy;
using Microsoft.Extensions.Configuration;
using HotelBackend.Tests.Base;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace HotelBackend.FunctionalTests
{
    // TODO: slow!
    [NonParallelizable]
    public class HotelsControllerFunctionalTests: BaseRealDBTests
    {

        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;

        // TODO: slow
        [SetUp]
        public void Setup()
        {
            TruncateTestDB();

            var options = GetPostgresTestOptions();

            using var context = new MainDbContext(options);

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    // Optional: Configure web host if needed (e.g., ports)
                });

            // TODO: There are better ways to do this, but this is the simplest
            Environment.SetEnvironmentVariable("DATABASE_ENVIRONMENT", "Testing"); 
            _client = _factory.CreateClient();

            // TODO: lazy, not all requests requires the key
            var key = _configuration.GetValue<string>("TestApiKey");
            _client.DefaultRequestHeaders.Add("X-API-KEY", key);
        }

        protected async Task<HttpResponseMessage> InsertHotel(Hotel hotel, bool assertSuccess = true) {
            var result = await _client.PostAsJsonAsync("/api/hotels", hotel);
            if (assertSuccess) 
              Assert.DoesNotThrow(() => result.EnsureSuccessStatusCode());
            return result;
        }

        protected Task<HttpResponseMessage> InsertHotelById(int id, bool assertSuccess = true) {
            var hotel = HotelsTestData.GetTestData().Find(h => h.Id == id);
            Assert.That(hotel != null);
            return InsertHotel(hotel!, assertSuccess);
        }

        protected Task InsertAllMockData() =>
            Task.WhenAll(HotelsTestData.GetTestData().Select(h => InsertHotel(h)));

        [Test]
        public Task TestInsertApi() => InsertAllMockData();

        [Test]
        public async Task SearchHotels_ReturnsHotelsNearby() 
        {
            await InsertAllMockData();
            var searchLocation = HotelsTestData.GetCurrentLocation();

            var resultsAll = new List<HotelSearchResult>();
            var loopExitedOnTime = false;

            // There are only 2 pages (15 hotels) in the test data,
            // But let's see if the API will properly return NO_CONTENT when there are no more hotels
            for (int page = 0; page < 3; page++) {
              var response = await _client.GetAsync($"/api/search?lat={searchLocation.Y}&lon={searchLocation.X}&page={page}");
              var result = response?.Content.ReadFromJsonAsync<List<HotelSearchResult>>().Result;
              if (result == null || result.Count == 0) {
                loopExitedOnTime = true;
                break;
              }

              resultsAll.InsertRange(resultsAll.Count, result);
            }

            Assert.That(loopExitedOnTime);

            var expectedIds = HotelsTestData.GetTestData().Select(h => h.Id).ToList();
            var resultIds = resultsAll.Select(r => r.Hotel.Id).ToList();

            // Test all elements are present, no duplicates
            // TODO: legacy method
            CollectionAssert.AreEquivalent(expectedIds, resultIds);

            // Test all elements are ordered by score
            var sortedResultIds = resultsAll.OrderByDescending(r => r.Score).Select(r => r.Hotel.Id).ToList();
            CollectionAssert.AreEqual(sortedResultIds, resultIds);
        }

        [Test]
        public async Task GetHotels_All()
        {
            await InsertAllMockData();

            var response = await _client.GetFromJsonAsync<List<Hotel>>($"/api/hotels");
            Assert.That(response != null);
            Assert.That(response!.Count == HotelsTestData.GetTestData().Count);
        }

        [Test]
        public async Task UpdateHotel_ChangesHotelData()
        {
            await InsertHotelById(1);
            var hotelToUpdate = _client.GetFromJsonAsync<Hotel>("/api/hotels/1").Result;
            Assert.That(hotelToUpdate != null);

            hotelToUpdate!.Name = "Updated Hotel Name";
            var response = await _client.PutAsJsonAsync($"/api/hotels/{hotelToUpdate.Id}", hotelToUpdate);

            Assert.DoesNotThrow(() => response.EnsureSuccessStatusCode());

            var updatedHotel = _client.GetFromJsonAsync<Hotel>("/api/hotels/1").Result;
            Assert.That(hotelToUpdate != null);
            Assert.That(updatedHotel!.Name == hotelToUpdate!.Name);
        }

        [Test]
        public async Task DeleteHotel_RemovesHotel()
        {
            await InsertHotelById(1);

            var response = await _client.DeleteAsync($"/api/hotels/1");
            response.EnsureSuccessStatusCode();

            var deletedHotel = _client.GetAsync("/api/hotels/1").Result; 
            Assert.That(deletedHotel.IsSuccessStatusCode == false);
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.