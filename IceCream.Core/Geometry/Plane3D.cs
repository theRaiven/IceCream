namespace IceCream.Core.Geometry;
/// <summary>
/// Секущая плоскость в трехмерном пространстве, определяемая точкой и нормалью.
/// </summary>
public sealed class Plane3D
{
    public Point3D Origin { get; }
    public Vector3D Normal { get; }

    /// <summary>
    /// Первая единичная ось внутри плоскости.
    /// </summary>
    public Vector3D AxisU { get; }
    /// <summary>
    /// Вторая единичная ось внутри плоскости.
    /// </summary>
    public Vector3D AxisV { get; }

    /// <summary>
    /// Инициализирует новую плоскость в трехмерном пространстве с заданной точкой и нормалью.
    /// </summary>
    /// <param name="origin">Точка на плоскости.</param>
    /// <param name="normal">Нормаль плоскости.</param>
    /// <exception cref="ArgumentException"></exception>
    public Plane3D(Point3D origin, Vector3D normal)
    {
        if (!double.IsFinite(origin.X) || !double.IsFinite(origin.Y) || !double.IsFinite(origin.Z))
        {
            throw new ArgumentException("Координаты точки должны быть конечными.", nameof(origin));
        }
        Origin = origin;
        Normal = normal.Normalized();

        // выбираем вспомогательное направление, которое не будет почти параллельно нормали.
        Vector3D reference = Math.Abs(Normal.X) < 0.9 // пока условно. нужно подумать как изменять
            ? Vector3D.UnitX
            : Vector3D.UnitY;

        // проецируем вспомогательное направление на плоскость.
        AxisU = (reference - Normal * reference.Dot(Normal)).Normalized();

        // получаем вторую ось, перпендикулярную первой и нормали.
        AxisV = Normal.Cross(AxisU).Normalized();
    }
    /// <summary>
    /// Вычисляет знаковое расстояние от точки до плоскости
    /// отрицательно - точка находится по направлению нормали,
    /// положительно - в противоположном направлении
    /// </summary>
    /// <param name="point">Точка, для которой вычисляется расстояние.</param>
    /// <returns>Знаковое расстояние от точки до плоскости.</returns>
    public double SignedDistanceTo(Point3D point) => Normal.Dot(point - Origin);
    /// <summary>
    /// Проецирует точку на плоскость
    /// </summary>
    /// <param name="point">Точка, которую нужно спроецировать.</param>
    /// <returns>Ближайшая точка на плоскости.</returns>
    public Point3D Project(Point3D point) => point - Normal * SignedDistanceTo(point);
    /// <summary>
    /// Создает плоскость, проходящую через заданное значение координаты X
    /// </summary>
    /// <param name="x">Значение координаты X.</param>
    /// <returns>Плоскость, проходящая через заданное значение координаты X.</returns>
    public static Plane3D AtX(double x) => new(new Point3D(x, 0, 0), Vector3D.UnitX);
    /// <summary>
    /// Создает плоскость, проходящую через заданное значение координаты X
    /// </summary>
    /// <param name="x">Значение координаты X.</param>
    /// <returns>Плоскость, проходящая через заданное значение координаты X.</returns>
    public static Plane3D AtY(double y) => new(new Point3D(0, y, 0), Vector3D.UnitY);
    /// <summary>
    /// Создает плоскость, проходящую через заданное значение координаты X
    /// </summary>
    /// <param name="x">Значение координаты X.</param>
    /// <returns>Плоскость, проходящая через заданное значение координаты X.</returns>
    public static Plane3D AtZ(double z) => new(new Point3D(0, 0, z), Vector3D.UnitZ);

    /// <summary>
    /// Возвращает локальные координаты проекции точки на плоскость.
    /// </summary>
    public Point2D ToLocal(Point3D point)
    {
        Vector3D offset = point - Origin;

        return new Point2D(offset.Dot(AxisU), offset.Dot(AxisV));
    }
    /// <summary>
    /// Преобразует локальные координаты в точку на плоскости.
    /// </summary>
    public Point3D ToGlobal(Point2D point)
    {
        return Origin + AxisU * point.X + AxisV * point.Y;
    }
}
