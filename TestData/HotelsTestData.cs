using HotelBackend.Models;
using NetTopologySuite.Geometries;

public static class HotelsTestData 
{
    // Lemax HQ
    public static Point GetCurrentLocation() => new Point(15.9500648, 45.769841);

    public static List<Hotel> GetTestData() 
    {
        return new List<Hotel>
        {
           new Hotel { Id = 1, Name = "Hotel Esplanade", Price = 180, GeoLocation = new Point(15.9734132, 45.8053707) }, // Distance: ~4.346 km 
           new Hotel { Id = 2, Name = "Palace Hotel", Price = 120, GeoLocation = new Point(15.9750927, 45.808519) }, // Distance: ~4.718 km 
           new Hotel { Id = 3, Name = "Dubrovnik Hotel", Price = 150, GeoLocation = new Point(15.9716738, 45.8126665) }, // Distance: ~5.048 km 
           new Hotel { Id = 4, Name = "Hotel International", Price = 100, GeoLocation = new Point(15.9714818, 45.7990721) }, // Distance: ~3.65 km
           new Hotel { Id = 5, Name = "Hotel Academia", Price = 135, GeoLocation = new Point(15.9758652, 45.8193549) }, // Distance: ~5.858 km
           new Hotel { Id = 6, Name = "Sundial Boutique Hotel", Price = 130, GeoLocation = new Point(15.9505552, 45.7734781) }, // Distance: ~0.406 km
           new Hotel { Id = 7, Name = "Best Western Premier Hotel Astoria", Price = 110, GeoLocation = new Point(15.9755131, 45.8071905) }, // Distance: ~4.598 km 
           new Hotel { Id = 8, Name = "Hotel Pleso", Price = 75, GeoLocation = new Point(16.0618029, 45.7300663) }, // Distance: ~9.733 km 
           new Hotel { Id = 9, Name = "Hotel Antunović", Price = 140, GeoLocation = new Point(15.8962666, 45.7974706) }, // Distance: ~5.181 km
           new Hotel { Id = 10, Name = "Admiral Hotel", Price = 95, GeoLocation = new Point(15.916527, 45.7947414) }, // Distance: ~3.799 km 
           new Hotel { Id = 11, Name = "Manda Heritage Hotel", Price = 115, GeoLocation = new Point(15.981193, 45.8114483) }, // Distance: ~5.218 km 
           new Hotel { Id = 12, Name = "Timeout Heritage Hotel", Price = 130, GeoLocation = new Point(15.9709341, 45.813236) }, // Distance: ~5.089 km 
           new Hotel { Id = 13, Name = "Canopy by Hilton Zagreb", Price = 165, GeoLocation = new Point(15.9825912, 45.8057242) }, // Distance: ~4.72 km 
           new Hotel { Id = 14, Name = "Sheraton Zagreb Hotel", Price = 170, GeoLocation = new Point(15.9820792, 45.8069731) }, // Distance: ~4.818 km 
           new Hotel { Id = 15, Name = "Hotel Jägerhorn", Price = 120, GeoLocation = new Point(15.9711221, 45.8132324) }, // Distance: ~5.094 km 
        };
    }

    // Returns TestData with Geolocations projected to 23033. See the GeometryExtensions class for more info.
    public static Point GetCurrentLocationMocked() =>
        (Point) GetCurrentLocation().ProjectTo(32633);

    public static List<Hotel> GetTestDataMocked() =>
        GetTestData().Select(h => new Hotel { Id = h.Id, Name = h.Name, Price = h.Price, GeoLocation = (Point)h.GeoLocation.ProjectTo(32633) }).ToList();
}
