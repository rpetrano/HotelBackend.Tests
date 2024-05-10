using HotelBackend.Models;
using HotelBackend.Repositories;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace HotelBackend.Tests.Base {
  public abstract class BaseHotelRepositoryTests: BaseRealDBTests
  {
      protected async Task _SearchHotels_ReturnsHotelsWithinRadius(MainDbContext context, Point currentLocation) {
          var testConfig = new List<dynamic>{
              new{ Radius = 300, Limit = 5, IDs = new int[] {} },
              new{ Radius = 300, Limit = 100, IDs = new int[] {} },
              new{ Radius = 4000, Limit = 5, IDs = new int[] {4,6,10} },
              new{ Radius = 4000, Limit = 100, IDs = new int[] {4,6,10} },
              new{ Radius = 5000, Limit = 5, IDs = new int[] {1,4,6,7,10} },
              new{ Radius = 5000, Limit = 100, IDs = new int[] {1,2,4,6,7,10,13,14} },
          };

          var repository = new HotelRepository(context);

          var defaultHotel = new Hotel{ Id = -1, Name = "Hotel 1", Price = 0, GeoLocation = new Point(0, 0) };

          foreach (var i in testConfig) {
              var results = await repository.SearchHotels(currentLocation, (double) i.Radius, (int) i.Limit);

              Assert.That(i.IDs.Length == results.Count());
              Assert.That(new HashSet<int>(results.Select(h => h.Id)).SetEquals(new HashSet<int>(i.IDs)));
          }
      }
  }
}