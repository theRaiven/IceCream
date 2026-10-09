namespace IceCream.Core.Geometry;

/// <summary>
/// Представляет трехмерный вектор с координатами X, Y и Z.
/// </summary>
/// <param name="X">Координата X.</param>
/// <param name="Y">Координата Y.</param>
/// <param name="Z">Координата Z.</param>
public readonly record struct Vector3D(double X, double Y, double Z)
{
    public static Vector3D Zero => new(0, 0, 0);
    public static Vector3D UnitX => new(1, 0, 0);
    public static Vector3D UnitY => new(0, 1, 0);
    public static Vector3D UnitZ => new(0, 0, 1);
    public double LengthSquared => X * X + Y * Y + Z * Z;
    public double Length => Math.Sqrt(LengthSquared);
    /// <summary>
    /// Нормализует вектор
    /// </summary>
    /// <returns>возвращая новый вектор с той же направленностью, но длиной равной 1</returns>
    public Vector3D Normalized()
    {
        var length = Length;

        if (!double.IsFinite(length) || length == 0)
        {
            throw new InvalidOperationException("Нельзя нормализовать нулевой или некорректный вектор.");
        }

        return new Vector3D(X / length, Y / length, Z / length);
    }
    /// <summary>
    /// Вычисляет скалярное произведение (dot product) текущего вектора с другим вектором.
    /// </summary>
    /// <param name="other">Другой вектор.</param>
    /// <returns>Скалярное произведение.</returns>
    public double Dot(Vector3D other) => X * other.X + Y * other.Y + Z * other.Z;
    /// <summary>
    /// Вычисляет векторное произведение (cross product) текущего вектора с другим вектором.
    /// </summary>
    /// <param name="other">Другой вектор.</param>
    /// <returns>Векторное произведение.</returns>
    public Vector3D Cross(Vector3D other) => new(
        Y * other.Z - Z * other.Y,
        Z * other.X - X * other.Z,
        X * other.Y - Y * other.X
    );

    public static Vector3D operator +(Vector3D a, Vector3D b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vector3D operator -(Vector3D a, Vector3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Vector3D operator -(Vector3D vector) => new(-vector.X, -vector.Y, -vector.Z);

    public static Vector3D operator *(Vector3D vector, double scalar) => new(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);

    public static Vector3D operator *(double scalar, Vector3D vector) => vector * scalar;

    public static Vector3D operator /(Vector3D vector, double scalar)
    {
        if (scalar == 0)
        {
            throw new DivideByZeroException("Деление на ноль недопустимо.");
        }
        return new Vector3D(vector.X / scalar, vector.Y / scalar, vector.Z / scalar);
    }
}