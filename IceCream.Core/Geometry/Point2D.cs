namespace IceCream.Core.Geometry;

public readonly record struct Point2D(double X, double Y)
{
    public double DistanceTo(Point2D other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
