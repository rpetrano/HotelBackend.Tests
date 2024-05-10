using Moq;
using NUnit.Framework;
using HotelBackend.Models;
using HotelBackend.Services;
using Microsoft.AspNetCore.Mvc;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace HotelBackend.Controllers.Tests
{ 
    public class HotelsControllerTests
    {
        private Mock<IHotelService> _hotelServiceMock;
        private HotelsController _controller;

        [SetUp]
        public void Setup()
        {
            _hotelServiceMock = new Mock<IHotelService>();
            _controller = new HotelsController(_hotelServiceMock.Object);
        }

        [Test]
        public async Task GetHotelById_ReturnsOk_WhenHotelExists()
        {
            var hotelId = 1;
            var expectedHotel = HotelsTestData.GetTestData().Find(h => h.Id == hotelId)!;
            _hotelServiceMock.Setup(s => s.GetHotelById(hotelId)).ReturnsAsync(expectedHotel);

            var result = await _controller.GetHotelById(hotelId);

            Assert.That(result != null && result is OkObjectResult);
            var okResult = (OkObjectResult)result!;
            var returnedHotel = (Hotel)okResult.Value!;

            Assert.That(expectedHotel.Id == returnedHotel.Id);
            Assert.That(expectedHotel.Name == returnedHotel.Name);
        }

        [Test]
        public async Task GetHotelById_ReturnsNotFound_WhenHotelDoesntExists()
        {
            var hotelId = 1;
            var expectedHotel = HotelsTestData.GetTestData().Find(h => h.Id == hotelId);
            _hotelServiceMock.Setup(s => s.GetHotelById(hotelId)).ReturnsAsync(expectedHotel);

            var result = await _controller.GetHotelById(hotelId + 1);
            Assert.That(result is NotFoundResult); 
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.