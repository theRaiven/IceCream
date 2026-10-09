using System;
using System.Collections.Generic;
using System.Linq;
using IceCream.Core.Geometry;

namespace IceCream.Core.Models;

/// <summary>
/// Представляет результат сечения, содержащий плоскость сечения и список контуров.
/// </summary>
public sealed class SectionResult
{
    public Plane3D Plane { get; }
    public IReadOnlyList<SectionContour> Contours { get; }
    public bool IsEmpty => Contours.Count == 0;

    public SectionResult(Plane3D plane, IReadOnlyList<SectionContour> contours)
    {
        // прочитал, что можно if(...) делать так, надо поменять везде, гле можно, чтобы сократить код
        ArgumentNullException.ThrowIfNull(plane);
        ArgumentNullException.ThrowIfNull(contours);

        SectionContour[] contourArray = contours.ToArray();

        foreach (SectionContour contour in contourArray)
        {
            if (contour is null)
            {
                throw new ArgumentException("Список не должен содержать null.", nameof(contours));
            }
        }

        Plane = plane;
        Contours = Array.AsReadOnly(contourArray);
    }
}
