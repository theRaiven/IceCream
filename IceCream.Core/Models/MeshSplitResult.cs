using IceCream.Core.Geometry;

namespace IceCream.Core.Models;

/// <summary>
/// Представляет результат разделения треугольной сетки секущей плоскостью.
/// </summary>
public sealed class MeshSplitResult
{
    /// <summary>
    /// Плоскость, которой была разделена исходная сетка.
    /// </summary>
    public Plane3D Plane { get; }
    /// <summary>
    /// Часть сетки, расположенная с положительной стороны плоскости.
    /// Для её вершин знаковое расстояние до плоскости неотрицательно.
    /// </summary>
    public TriangleMesh PositiveSide { get; }
    /// <summary>
    /// Часть сетки, расположенная с отрицательной стороны плоскости.
    /// Для её вершин знаковое расстояние до плоскости неположительно.
    /// </summary>
    public TriangleMesh NegativeSide { get; }
    /// <summary>
    /// Инициализирует результат разделения сетки на две части.
    /// </summary>
    /// <param name="plane">Секущая плоскость.</param>
    /// <param name="positiveSide">Часть сетки с положительной стороны плоскости.</param>
    /// <param name="negativeSide">Часть сетки с отрицательной стороны плоскости.</param>
    public MeshSplitResult(Plane3D plane, TriangleMesh positiveSide, TriangleMesh negativeSide)
    {
        ArgumentNullException.ThrowIfNull(plane, nameof(plane));
        ArgumentNullException.ThrowIfNull(positiveSide, nameof(positiveSide));
        ArgumentNullException.ThrowIfNull(negativeSide, nameof(negativeSide));
        Plane = plane;
        PositiveSide = positiveSide;
        NegativeSide = negativeSide;
    }
}
