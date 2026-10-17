using IceCream.Core.Geometry;
using IceCream.Core.Models;

namespace IceCream.Core.Services;
/// <summary>
/// Разделяет треугольную сетку секущей плоскостью на две части.
/// </summary>
public sealed class MeshPlanEPSlitter
{
    /// <summary>
    /// Вершина многоугольника и её знаковое расстояние до секущей плоскости.
    /// </summary>
    private readonly record struct ClippingVertex(Point3D Point, double Distance);
    /// <summary>
    /// Разделяет треугольную сетку на части, расположенные по разные стороны плоскости.
    /// Треугольники, пересекаемые плоскостью, разрезаются по линии пересечения.
    /// </summary>
    /// <param name="mesh">Исходная треугольная сетка.</param>
    /// <param name="plane">Секущая плоскость.</param>
    /// <param name="EPS">Допуск при сравнении расстояния до плоскости с нулём.</param>
    /// <returns>Две треугольные сетки по разные стороны секущей плоскости.</returns>
    public MeshSplitResult Split(TriangleMesh mesh, Plane3D plane, double EPS = 1e-10)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(plane);

        if (!double.IsFinite(EPS) || EPS <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(EPS), "Допуск должен быть конечным положительным числом.");
        }

        var positiveVertices = new List<Point3D>();
        var positiveIndices = new List<int>();

        var negativeVertices = new List<Point3D>();
        var negativeIndices = new List<int>();

        for(int i = 0; i < mesh.TriangleCount; i++)
        {
            var (a,b, c) = mesh.GetTriangle(i);

            ClippingVertex[] triangle =
            {
                 CreateClippingVertex(a, plane, EPS),
                 CreateClippingVertex(b, plane, EPS),
                 CreateClippingVertex(c, plane, EPS)
            };

            bool isCoplanar = triangle.All(v => Math.Abs(v.Distance) < EPS);

            if(isCoplanar)
            {
                // Полностью лежащий в секущей плоскости треугольник нельзя однозначно
                // отнести к одной из половин. Добавляем его только в положительную,
                // чтобы при одновременной отрисовке двух частей не было наложения граней.
                AddPolygon(triangle, positiveVertices, positiveIndices, EPS);
                continue;
            }

            List<ClippingVertex> positivePolygon = ClipTriangle(triangle, keepPositiveSide: true);
            List<ClippingVertex> negativePolygon = ClipTriangle(triangle, keepPositiveSide: false);

            AddPolygon(positivePolygon, positiveVertices, positiveIndices, EPS);
            AddPolygon(negativePolygon, negativeVertices, negativeIndices, EPS);
        }
        var positiveSide = new TriangleMesh(positiveVertices, positiveIndices);
        var negativeSide = new TriangleMesh(negativeVertices, negativeIndices);

        return new MeshSplitResult(plane, positiveSide, negativeSide);
    }

    /// <summary>
    /// Вычисляет расстояние вершины до плоскости.
    /// Если вершина находится ближе допуска, она проецируется точно на плоскость.
    /// </summary>
    private static ClippingVertex CreateClippingVertex(Point3D point, Plane3D plane, double EPS)
    {
        double distance = plane.SignedDistanceTo(point);

        if (Math.Abs(distance) < EPS)
        {
            // Проецируем точку на плоскость, если она находится ближе допуска.
            return new ClippingVertex(plane.Project(point), 0);
        }
        return new ClippingVertex(point, distance);
    }
   
    /// <summary>
    /// Оставляет часть исходного треугольника с выбранной стороны плоскости.
    /// Результатом может быть пустой многоугольник, треугольник или четырёхугольник.
    /// </summary>
    private static List<ClippingVertex> ClipTriangle(IReadOnlyList<ClippingVertex> triangle, bool keepPositiveSide)
    {
        var result = new List<ClippingVertex>(4);

        for(int i = 0; i < triangle.Count; i++)
        {
            ClippingVertex current = triangle[i];
            ClippingVertex next = triangle[(i + 1) % triangle.Count];
            
            bool currentInside = IsInside(current.Distance, keepPositiveSide);
            bool nextInside = IsInside(next.Distance, keepPositiveSide);

            // если обе вершины находятся с одной стороны плоскости, добавляем вторую вершину в результат
            if (currentInside && nextInside)
            {
                AddLastUnique(result, next);
            }
            // если текущая вершина находится с одной стороны плоскости, а следующая - с другой, добавляем точку пересечения
            else if (currentInside && !nextInside)
            {
                AddLastUnique(result, IntersectEdge(current, next));
            } 
            // и если текущая вершина находится с другой стороны плоскости, а следующая - с одной, добавляем точку пересечения и следующую вершину
            else if (!currentInside && nextInside)
            {
                AddLastUnique(result, IntersectEdge(current, next));
                AddLastUnique(result, next);
            }
        }
        RemoveClosingDuplicate(result);

        return result;
    }

    /// <summary>
    /// Проверяет, принадлежит ли вершина выбранному полупространству.
    /// Точки секущей плоскости принадлежат обеим сторонам.
    /// </summary>
    private static bool IsInside(double distance, bool keepPositiveSide)
    {
        if (keepPositiveSide)
        {
            return distance >= 0;
        }

        return distance <= 0;
    }
    /// <summary>
    /// Находит точку пересечения ребра с секущей плоскостью.
    /// </summary>
    private static ClippingVertex IntersectEdge(ClippingVertex first, ClippingVertex second)
    {
        double denominator = first.Distance - second.Distance;

        if (denominator == 0)
        {
            throw new InvalidOperationException("Невозможно вычислить пересечение ребра с плоскостью.");
        }

        double t = first.Distance / denominator;
        t = Math.Clamp(t, 0, 1);

        Point3D point = first.Point + (second.Point - first.Point) * t;

        return new ClippingVertex(point, 0);
    }
    /// <summary>
    /// Добавляет многоугольник в результирующую сетку, разбивая его на треугольники.
    /// Порядок обхода вершин сохраняется, поэтому ориентация граней не меняется.
    /// </summary>
    private static void AddPolygon(IReadOnlyList<ClippingVertex> polygon, List<Point3D> vertices, List<int> indices, double EPS)
    {
        if (polygon.Count < 3)
        {
            return;
        }

        Point3D first = polygon[0].Point;

        for (int i = 1; i < polygon.Count - 1; i++)
        {
            Point3D second = polygon[i].Point;
            Point3D third = polygon[i + 1].Point;
            // если треугольник вырожденный, пропускаем его
            if (IsDegenerateTriangle(first, second, third, EPS))
            {
                continue;
            }

            AddTriangle(vertices, indices, first, second, third);
        }
    }
    /// <summary>
    /// Проверяет, имеет ли треугольник практически нулевую площадь.
    /// </summary>
    private static bool IsDegenerateTriangle(Point3D a, Point3D b, Point3D c, double EPS)
    {
        Vector3D ab = b - a;
        Vector3D ac = c - a;

        double doubledAreaSquared = ab.Cross(ac).LengthSquared;
        double limit = EPS * EPS;

        return doubledAreaSquared <= limit;
    }
    /// <summary>
    /// Добавляет треугольник в сетку.
    /// Вершины намеренно не объединяются между соседними треугольниками:
    /// это упрощает разрезание и не влияет на форму результирующей поверхности.
    /// </summary>
    private static void AddTriangle( List<Point3D> vertices, List<int> indices, Point3D a, Point3D b, Point3D c)
    {
        int firstIndex = vertices.Count;

        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);

        indices.Add(firstIndex);
        indices.Add(firstIndex + 1);
        indices.Add(firstIndex + 2);
    }
    /// <summary>
    /// Добавляет вершину, если последняя вершина списка не совпадает с ней.
    /// </summary>
    private static void AddLastUnique(List<ClippingVertex> vertices, ClippingVertex candidate)
    {
        if (vertices.Count == 0 || vertices[^1].Point != candidate.Point)
        {
            vertices.Add(candidate);
        }
    }
    /// <summary>
    /// Удаляет последнюю вершину, если она совпадает с первой и лишь замыкает список.
    /// </summary>
    private static void RemoveClosingDuplicate(List<ClippingVertex> vertices)
    {
        if (vertices.Count > 1 && vertices[0].Point == vertices[^1].Point)
        {
            vertices.RemoveAt(vertices.Count - 1);
        }
    }
}

