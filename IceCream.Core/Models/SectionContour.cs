using System;
using System.Collections.Generic;
using System.Linq;
using IceCream.Core.Geometry;

namespace IceCream.Core.Models;

/// <summary>
/// Представляет контур с точками в 3D и 2D пространстве, а также информацию о том, замкнут ли контур.
/// </summary>
public sealed class SectionContour
{
    public IReadOnlyList<Point3D> Points3D { get; }
    public IReadOnlyList<Point2D> Points2D { get; }
    public bool IsClosed { get; }

    
    public SectionContour(IReadOnlyList<Point3D> points3D, IReadOnlyList<Point2D> points2D, bool isClosed)
    {
        // многа праверак, что он норм 
        ArgumentNullException.ThrowIfNull(points3D);
        ArgumentNullException.ThrowIfNull(points2D);

        Point3D[] array3D = points3D.ToArray();
        Point2D[] array2D = points2D.ToArray();

        if (array3D.Length != array2D.Length)
        {
            throw new ArgumentException("Количество точек в 3D и 2D должно совпадать.");
        }

        if (array3D.Length == 0)
        {
            throw new ArgumentException("Контур должен содержать хотя бы одну точку.");
        }

        if (isClosed && array3D.Length < 3)
        {
            throw new ArgumentException("Замкнутый контур должен содержать минимум три точки.");
        }

        foreach (Point3D point in array3D)
        {
            if (!double.IsFinite(point.X) || !double.IsFinite(point.Y) || !double.IsFinite(point.Z))
            {
                throw new ArgumentException( "Координаты должны быть конечными.",  nameof(points3D));
            }
        }

        foreach (Point2D point in array2D)
        {
            if (!double.IsFinite(point.X) || !double.IsFinite(point.Y))
            {
                throw new ArgumentException("Координаты должны быть конечными.",nameof(points2D));
            }
        }

        Points3D = Array.AsReadOnly(array3D);
        Points2D = Array.AsReadOnly(array2D);
        IsClosed = isClosed;
    }
}
