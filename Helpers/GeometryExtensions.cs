// NTS (NetTopologySuite) ignores SRID values during operations. It assumes a planar coordinate system.
// PostGIS for Geography types uses SRID 4326 and the distance returned is in meters. (ref. https://postgis.net/docs/ST_Distance.html)
// NTS uses cartesian coordinates. This means that if you specify coordinates in terms of longitude and latitude, some client-evaluated values like distance, length, and area will be in degrees, not meters.
// This is a problem when the database is mocked, because the distance calculation will be inconsistent with that of the real database.

// There's a couple of ways to go about it:
// 1) Make application and DB work with SRID like 32633 and take planar coordinates as input. In the specification it's not specified how the application should take its coordinates.
// 2) Use Geology type in PostGIS. Convert angular degrees to meters in the application.
// 3) Convert geographic coordinates to planar coordinates when mocking the database.
//
// SRID 4326 is an industry standard, so the first option can be crossed out. We would be making it hard for the users and/or frontend developers.
// Since this issue only exists when the database is being mocked, and since all the spatial databases I know of have support for geographic coordinates, the second option can be crossed out as well.

// This leaves us with the following conversion class to Project the coordinates to SRID 2855. This will make the distance calculation consistent with the real database.
// The only downside is that it has to be manually used in the tests.
//
// The code a very slight modification from: https://learn.microsoft.com/en-us/ef/core/modeling/spatial#srid-ignored-during-client-operations
// Changes: Add SRID 32633 (WGS 84 / UTM zone 33N)
//          Add SRID 3756 (HTRS96 / Croatia TM)
//          Default source SRID set to 4326 if not set
using NetTopologySuite.Geometries;
using ProjNet;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

public static class GeometryExtensions
{
    private static readonly CoordinateSystemServices _coordinateSystemServices
        = new CoordinateSystemServices(
            new Dictionary<int, string>
            {
                // Coordinate systems:

                [4326] = GeographicCoordinateSystem.WGS84.WKT,

                // This coordinate system covers the area of our data.
                // Different data requires a different coordinate system.
                [2855] =
                    @"
                        PROJCS[""NAD83(HARN) / Washington North"",
                            GEOGCS[""NAD83(HARN)"",
                                DATUM[""NAD83_High_Accuracy_Regional_Network"",
                                    SPHEROID[""GRS 1980"",6378137,298.257222101,
                                        AUTHORITY[""EPSG"",""7019""]],
                                    AUTHORITY[""EPSG"",""6152""]],
                                PRIMEM[""Greenwich"",0,
                                    AUTHORITY[""EPSG"",""8901""]],
                                UNIT[""degree"",0.01745329251994328,
                                    AUTHORITY[""EPSG"",""9122""]],
                                AUTHORITY[""EPSG"",""4152""]],
                            PROJECTION[""Lambert_Conformal_Conic_2SP""],
                            PARAMETER[""standard_parallel_1"",48.73333333333333],
                            PARAMETER[""standard_parallel_2"",47.5],
                            PARAMETER[""latitude_of_origin"",47],
                            PARAMETER[""central_meridian"",-120.8333333333333],
                            PARAMETER[""false_easting"",500000],
                            PARAMETER[""false_northing"",0],
                            UNIT[""metre"",1,
                                AUTHORITY[""EPSG"",""9001""]],
                            AUTHORITY[""EPSG"",""2855""]]
                    ",

                [32633] = @"
                    PROJCS[""WGS 84 / UTM zone 33N"",
                    GEOGCS[""WGS 84"",
                        DATUM[""WGS_1984"",
                            SPHEROID[""WGS 84"",6378137,298.257223563,
                                AUTHORITY[""EPSG"",""7030""]],
                            AUTHORITY[""EPSG"",""6326""]],
                        PRIMEM[""Greenwich"",0,
                            AUTHORITY[""EPSG"",""8901""]],
                        UNIT[""degree"",0.0174532925199433,
                            AUTHORITY[""EPSG"",""9122""]],
                        AUTHORITY[""EPSG"",""4326""]],
                    PROJECTION[""Transverse_Mercator""],
                    PARAMETER[""latitude_of_origin"",0],
                    PARAMETER[""central_meridian"",15],
                    PARAMETER[""scale_factor"",0.9996],
                    PARAMETER[""false_easting"",500000],
                    PARAMETER[""false_northing"",0],
                    UNIT[""metre"",1,
                        AUTHORITY[""EPSG"",""9001""]],
                    AXIS[""Easting"",EAST],
                    AXIS[""Northing"",NORTH],
                    AUTHORITY[""EPSG"",""32633""]]",
                
                [3756] = @"
                    PROJCS[""HTRS96 / Croatia TM"",
                        GEOGCS[""HTRS96"",
                            DATUM[""Croatian_Terrestrial_Reference_System"",
                                SPHEROID[""GRS 1980"",6378137,298.257222101],
                                TOWGS84[0,0,0,0,0,0,0]],
                            PRIMEM[""Greenwich"",0,
                                AUTHORITY[""EPSG"",""8901""]],
                            UNIT[""degree"",0.0174532925199433,
                                AUTHORITY[""EPSG"",""9122""]],
                            AUTHORITY[""EPSG"",""4761""]],
                        PROJECTION[""Transverse_Mercator""],
                        PARAMETER[""latitude_of_origin"",0],
                        PARAMETER[""central_meridian"",16.5],
                        PARAMETER[""scale_factor"",0.9999],
                        PARAMETER[""false_easting"",500000],
                        PARAMETER[""false_northing"",0],
                        UNIT[""metre"",1,
                            AUTHORITY[""EPSG"",""9001""]],
                        AXIS[""Easting"",EAST],
                        AXIS[""Northing"",NORTH],
                        AUTHORITY[""EPSG"",""3765""]]
                ",
            });

    public static Geometry ProjectTo(this Geometry geometry, int to_srid)
    {
        // SRID of 0 is not valid, it means it was not set. Default to 4326.
        var from_srid = geometry.SRID == 0 ? 4326 : geometry.SRID;

        var transformation = _coordinateSystemServices.CreateTransformation(from_srid, to_srid);

        var result = geometry.Copy();
        result.Apply(new MathTransformFilter(transformation.MathTransform));

        return result;
    }

    private class MathTransformFilter : ICoordinateSequenceFilter
    {
        private readonly MathTransform _transform;

        public MathTransformFilter(MathTransform transform)
            => _transform = transform;

        public bool Done => false;
        public bool GeometryChanged => true;

        public void Filter(CoordinateSequence seq, int i)
        {
            var x = seq.GetX(i);
            var y = seq.GetY(i);
            var z = seq.GetZ(i);
            _transform.Transform(ref x, ref y, ref z);
            seq.SetX(i, x);
            seq.SetY(i, y);
            seq.SetZ(i, z);
        }
    }
}