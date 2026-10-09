namespace IceCream.Core.Geometry;

/// <summary>
/// Полученная после триангуляции сетка
/// </summary>
public sealed class TriangleMesh
{
    public IReadOnlyList<Point3D> Vertices { get; }
    public IReadOnlyList<int> Indices { get; }
    public int TriangleCount => Indices.Count / 3;
    /// <summary>
    /// Инициализирует новый экземпляр класса TriangleMesh с заданными вершинами и индексами.
    /// </summary>
    /// <param name="vertices">Список вершин</param>
    /// <param name="indices">Список индексов</param>
    public TriangleMesh(IReadOnlyList<Point3D> vertices, IReadOnlyList<int> indices)
    {
        if (vertices == null) throw new ArgumentNullException(nameof(vertices));
        if (indices == null) throw new ArgumentNullException(nameof(indices));

        Point3D[] vertexArray = vertices.ToArray();
        int[] indexArray      = indices.ToArray();


        if (indexArray.Length % 3 != 0)
        {
            throw new ArgumentException("Количество индексов должно быть кратно трём.",  nameof(indices));
        }

        foreach (Point3D vertex in vertexArray)
        {
            if (!double.IsFinite(vertex.X) || !double.IsFinite(vertex.Y) || !double.IsFinite(vertex.Z))
            {
                throw new ArgumentException("Координаты вершин должны быть конечными.", nameof(vertices));
            }
        }
        foreach (int index in indexArray)
        {
            if (index < 0 || index >= vertexArray.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(indices), "Индекс выходит за границы массива вершин.");
            }
        }

        Vertices = Array.AsReadOnly(vertexArray);
        Indices  = Array.AsReadOnly(indexArray);
    }
    /// <summary>
    /// Возвращает треугольник по его индексу в списке треугольников.
    /// </summary>
    /// <param name="triangleIndex">Индекс треугольника</param>
    /// <returns>Возвращает треугольник по его индексу в списке треугольников</returns>
    public (Point3D A, Point3D B, Point3D C) GetTriangle(int triangleIndex)
    {
        if (triangleIndex < 0 || triangleIndex >= TriangleCount)
        {
            throw new ArgumentOutOfRangeException(nameof(triangleIndex));
        }

        int offset = triangleIndex * 3;

        return (
            Vertices[Indices[offset]],
            Vertices[Indices[offset + 1]],
            Vertices[Indices[offset + 2]]);
    }
}
