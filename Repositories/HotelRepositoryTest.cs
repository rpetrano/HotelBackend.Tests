using Microsoft.EntityFrameworkCore;
using HotelBackend;
using NUnit.Framework;
using HotelBackend.Tests.Base;

public class HotelRepositoryTests: BaseHotelRepositoryTests
{

    [OneTimeSetUp]
    public void Setup()
    {
        TruncateTestDB();
    } 

    [Test]
    public async Task SearchHotels_ReturnsHotelsWithinRadius_InMemory()
    {
        var options = new DbContextOptionsBuilder<MainDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        using var context = new MainDbContext(options);
        context.Hotels.AddRange(HotelsTestData.GetTestDataMocked());
        context.SaveChanges();

        await _SearchHotels_ReturnsHotelsWithinRadius(context, HotelsTestData.GetCurrentLocationMocked());
    }

    [Test]
    public async Task SearchHotels_ReturnsHotelsWithinRadius_Postgres()
    {
        var options = GetPostgresTestOptions();
        using var context = new MainDbContext(options);

        context.Hotels.AddRange(HotelsTestData.GetTestData());
        context.SaveChanges();

        await _SearchHotels_ReturnsHotelsWithinRadius(context, HotelsTestData.GetCurrentLocation());
    }
}
