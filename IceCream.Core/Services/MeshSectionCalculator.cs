using System;
using System.Collections.Generic;
using IceCream.Core.Geometry;
using IceCream.Core.Models;

namespace IceCream.Core.Services;

public sealed class MeshSectionCalculator
{
    // пересечение одного треугольника
    private sealed record TriangleSection(Point3D[] Points, bool IsCoplanar);

    // все точки сечения и связи между ними
    private sealed class SectionGraph
    {
        public List<Point3D> Points { get; } = new();
        public HashSet<(int A, int B)> Edges { get; } = new();

        public HashSet<int> TouchPoints { get; } = new();
    }

    /// <summary>
    /// Вычисляет сечение треугольной сетки плоскостью.
    /// </summary>
    public SectionResult Calculate(TriangleMesh mesh, Plane3D plane, double ESP = 1e-8)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(plane);

        if (!double.IsFinite(ESP) || ESP <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ESP), "Допуск должен быть конечным положительным числом.");
        }

        SectionGraph graph = CollectIntersections(mesh, plane, ESP);
        List<SectionContour> contours = BuildContours(graph, plane);

        return new SectionResult(plane, contours);
    }

    private static TriangleSection IntersectTriangle(Point3D a, Point3D b, Point3D c, Plane3D plane, double ESP)
    {
        Point3D[] vertices = { a, b, c };

        double[] distances =
        {
            plane.SignedDistanceTo(a),
            plane.SignedDistanceTo(b),
            plane.SignedDistanceTo(c)
        };

        int[] sides =
        {
            Classify(distances[0], ESP),
            Classify(distances[1], ESP),
            Classify(distances[2], ESP)
        };

        // Все вершины лежат на плоскости с учётом допуска
        if (sides[0] == 0 && sides[1] == 0 && sides[2] == 0)
        {
            return new TriangleSection(
                new[]
                {
                    plane.Project(a),
                    plane.Project(b),
                    plane.Project(c)
                },
                IsCoplanar: true);
        }
        var points = new List<Point3D>();

        // Сначала сохраняем вершины, попавшие на плоскость.
        for (int i = 0; i < 3; i++)
        {
            if (sides[i] == 0)
            {
                AddUniquePoint(points, plane.Project(vertices[i]), ESP);
            }
        }
        // Проверяем рёбра AB, BC и CA.
        for (int i = 0; i < 3; i++)
        {
            int next = (i + 1) % 3;

            // Концы должны находиться строго по разные стороны
            if (sides[i] * sides[next] != -1) continue;

            double t = distances[i] / (distances[i] - distances[next]);

            Point3D point = vertices[i] + (vertices[next] - vertices[i]) * t;

            AddUniquePoint(points, plane.Project(point), ESP);
        }

        return new TriangleSection(points.ToArray(), IsCoplanar: false);
    }
    private static int Classify(double distance, double ESP)
    {
        if (distance > ESP)
            return 1;

        if (distance < -ESP)
            return -1;

        return 0;
    }
    private static void AddUniquePoint(List<Point3D> points, Point3D candidate, double ESP)
    {
        foreach (Point3D point in points)
        {
            if (point.DistanceTo(candidate) <= ESP)
                return;
        }

        points.Add(candidate);
    }

    private static SectionGraph CollectIntersections(TriangleMesh mesh, Plane3D plane, double ESP)
    {
        var graph = new SectionGraph();

        var coplanarEdges = new Dictionary<(int A, int B), int>();
        var coplanarPoints = new HashSet<int>();

        for (int i = 0; i < mesh.TriangleCount; i++)
        {
            var (a, b, c) = mesh.GetTriangle(i);

            TriangleSection section = IntersectTriangle(a, b, c, plane, ESP);
            
            if (section.Points.Length == 0) continue;

            var pointIndices = new int[section.Points.Length];

            for (int j = 0; j < section.Points.Length; j++)
            {
                pointIndices[j] = GetPointIndex(graph.Points, section.Points[j], ESP);
            }

            if (section.IsCoplanar)
            {
                foreach (int index in pointIndices) coplanarPoints.Add(index);

                for (int j = 0; j < 3; j++)
                {
                    int next = (j + 1) % 3;

                    int first = pointIndices[j];
                    int second = pointIndices[next];

                    if (first == second) continue;

                    var edge = MakeEdge(first, second);

                    coplanarEdges.TryGetValue(edge, out int count);
                    coplanarEdges[edge] = count + 1;
                }

                continue;
            }

            if (pointIndices.Length == 1)
            {
                graph.TouchPoints.Add(pointIndices[0]);
            }
            else if (pointIndices.Length == 2)
            {
                int first = pointIndices[0];
                int second = pointIndices[1];

                if (first == second)  graph.TouchPoints.Add(first);
                else                  graph.Edges.Add(MakeEdge(first, second));
            }
        }

        // Убираем внутренние рёбра плоских участков.
        foreach (var pair in coplanarEdges)
        {
            if (pair.Value == 1) graph.Edges.Add(pair.Key);
            else                 graph.Edges.Remove(pair.Key);
        }

        graph.TouchPoints.ExceptWith(coplanarPoints);

        return graph;
    }

    private static int GetPointIndex(List<Point3D> points, Point3D candidate,  double ESP)
    {
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i].DistanceTo(candidate) <= ESP) return i;
        }

        points.Add(candidate);

        return points.Count - 1;
    }

    private static (int A, int B) MakeEdge(int a, int b)
    {
        return a < b ? (a, b) : (b, a);
    }


    private static List<SectionContour> BuildContours(SectionGraph graph, Plane3D plane)
    {
        var contours = new List<SectionContour>();

        var neighbors = new List<int>[graph.Points.Count];

        for (int i = 0; i < neighbors.Length; i++) neighbors[i] = new List<int>();

        foreach (var edge in graph.Edges)
        {
            neighbors[edge.A].Add(edge.B);
            neighbors[edge.B].Add(edge.A);
        }

        // Одинаковый порядок обхода при одинаковых входных данных.
        foreach (List<int> list in neighbors) list.Sort();

        var remainingEdges = new HashSet<(int A, int B)>(graph.Edges);

        // Сначала цепочки от концов и точек ветвления.
        for (int start = 0; start < neighbors.Length; start++)
        {
            if (neighbors[start].Count == 2) continue;

            foreach (int next in neighbors[start])
            {
                if (!remainingEdges.Contains(MakeEdge(start, next))) continue;

                contours.Add(TraceContour(start, next, graph.Points, neighbors, remainingEdges, plane));
            }
        }

        // Остались обычные замкнутые циклы.
        for (int start = 0; start < neighbors.Length; start++)
        {
            foreach (int next in neighbors[start])
            {
                if (!remainingEdges.Contains(MakeEdge(start, next))) continue;

                contours.Add(TraceContour(start, next, graph.Points, neighbors, remainingEdges, plane));
            }
        }

        // Изолированные точки касания.
        for (int i = 0; i < graph.Points.Count; i++)
        {
            if (neighbors[i].Count != 0 || !graph.TouchPoints.Contains(i))
            {
                continue;
            }

            Point3D point = graph.Points[i];

            contours.Add(new SectionContour( new[] { point }, new[] { plane.ToLocal(point) }, isClosed: false));
        }

        return contours;
    }

    private static SectionContour TraceContour(int start, int next, List<Point3D> points, List<int>[] neighbors, HashSet<(int A, int B)> remainingEdges, Plane3D plane)
    {
        var orderedIndices = new List<int> { start };

        int previous = start;
        int current = next;

        bool isClosed = false;

        while (true)
        {
            remainingEdges.Remove(MakeEdge(previous, current));

            if (current == start)
            {
                isClosed = true;
                break;
            }

            orderedIndices.Add(current);

            // Конец цепочки или точка ветвления.
            if (neighbors[current].Count != 2)
                break;

            int following = neighbors[current][0] == previous
                ? neighbors[current][1]
                : neighbors[current][0];

            if (!remainingEdges.Contains(MakeEdge(current, following))) break;

            previous = current;
            current = following;
        }

        var points3D = new Point3D[orderedIndices.Count];
        var points2D = new Point2D[orderedIndices.Count];

        for (int i = 0; i < orderedIndices.Count; i++)
        {
            Point3D point = points[orderedIndices[i]];

            points3D[i] = point;
            points2D[i] = plane.ToLocal(point);
        }

        return new SectionContour(points3D,  points2D, isClosed);
    }
}