using Moq;
using HotelBackend.Services;
using HotelBackend.Repositories;
using NetTopologySuite.Geometries;
using HotelBackend.Models;
using Microsoft.Extensions.Options;
using NUnit.Framework;

public class HotelServiceTests
{
    [Test]
    public async Task SearchHotels_ReturnsHotelsWithPagination()
    {
        var pageConfig = new List<dynamic>{
            new{ Size = 3, Page = 0, Count = 3, FirstID = 1},
            new{ Size = 5, Page = 1, Count = 5, FirstID = 6},
            new{ Size = 4, Page = 3, Count = 3, FirstID = 13},
            new{ Size = 10, Page = 2, Count = 0, FirstID = -1}
        };

        var mockRepo = new Mock<IHotelRepository>();
        mockRepo.Setup(r => r.SearchHotels(It.IsAny<Point>(), It.IsAny<double>(), It.IsAny<int>()))
            .ReturnsAsync(HotelsTestData.GetTestData());

        var mockCalculator = new Mock<IHotelScoreCalculationService>();
        mockCalculator.Setup(c => c.CalculateHotelScore(It.IsAny<Hotel>(), It.IsAny<Point>()))
            .Returns(0.0);
        
        foreach (var i in pageConfig) {
          // MaxDistance and Limit are not used since the repository is mocked
          var options = Options.Create(new SearchOptions { MaxDistance = -1, Limit = -1, PageSize = i.Size });
          var hotelService = new HotelService(mockRepo.Object, mockCalculator.Object, options);
          var results = await hotelService.SearchHotels(HotelsTestData.GetCurrentLocation(), (int) i.Page);

          var defaultHotel = new Hotel{ Id = -1, Name = "Hotel 1", Price = 0, GeoLocation = new Point(0, 0) };
          var defaultHotelResult = new HotelSearchResult{ Hotel = defaultHotel, Score = 0.0 };

          Assert.That(i.Count == results.Count());
          Assert.That(i.FirstID == results.FirstOrDefault(defaultHotelResult).Hotel.Id);
        }
    }
}