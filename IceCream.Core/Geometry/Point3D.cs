namespace IceCream.Core.Geometry;

public readonly record struct Point3D(double X, double Y, double Z)
{
    public static Point3D Origin => new(0, 0, 0);
    public double DistanceTo(Point3D other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        var dz = Z - other.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
    public static Vector3D operator -(Point3D a, Point3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Point3D operator +(Point3D point, Vector3D vector) => new(point.X + vector.X, point.Y + vector.Y, point.Z + vector.Z);
    public static Point3D operator -(Point3D point, Vector3D vector) => new(point.X - vector.X, point.Y - vector.Y, point.Z - vector.Z);
}

