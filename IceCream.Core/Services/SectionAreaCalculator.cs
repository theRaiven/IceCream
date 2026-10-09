using System;
using System.Collections.Generic;
using IceCream.Core.Geometry;
using IceCream.Core.Models;

namespace IceCream.Core.Services;

public sealed class SectionAreaCalculator
{
    /// <summary>
    /// Вычисляет площадь сечения.
    /// Контуры должны быть простыми, без взаимных пересечений и касаний.
    /// Вложенные контуры учитываются как отверстия.
    /// </summary>
    public double Calculate(SectionResult section)
    {
        ArgumentNullException.ThrowIfNull(section);

        var closedContours = new List<SectionContour>();

        foreach (SectionContour contour in section.Contours)
        {
            if (contour.IsClosed)
            {
                closedContours.Add(contour);
                continue;
            }

            // Точка или одиночный отрезок имеют нулевую площадь.
            if (contour.Points2D.Count <= 2)
                continue;

            throw new InvalidOperationException(
                "Сечение содержит незамкнутую цепочку. " +
                "Нельзя достоверно вычислить его площадь.");
        }

        double totalArea = 0;

        for (int i = 0; i < closedContours.Count; i++)
        {
            IReadOnlyList<Point2D> points = closedContours[i].Points2D;

            double area = CalculatePolygonArea(points);

            // Считаем, сколько других контуров содержит этот
            int nestingDepth = 0;

            for (int j = 0; j < closedContours.Count; j++)
            {
                if (i == j) continue;

                if (IsInside(points[0], closedContours[j].Points2D)) nestingDepth++;
            }

            // Внешняя область: плюс.
            // Отверстие: минус.
            // Остров внутри отверстия: снова плюс.
            if (nestingDepth % 2 == 0)  totalArea += area;
            else                        totalArea -= area;
        }

        if (!double.IsFinite(totalArea) || totalArea < 0)
        {
            throw new InvalidOperationException("Не удалось получить корректную площадь сечения.");
        }

        return totalArea;
    }

    private static double CalculatePolygonArea(IReadOnlyList<Point2D> points)
    {
        // Сдвиг к первой вершине уменьшает влияние больших абсолютных значений координат
        Point2D origin = points[0];

        double doubledArea = 0;

        for (int i = 0; i < points.Count; i++)
        {
            int next = (i + 1) % points.Count;

            double x1 = points[i].X - origin.X;
            double y1 = points[i].Y - origin.Y;

            double x2 = points[next].X - origin.X;
            double y2 = points[next].Y - origin.Y;

            doubledArea += x1 * y2 - x2 * y1;
        }

        return Math.Abs(doubledArea) / 2;
    }

    /// <summary>
    /// Проверяет, находится ли точка внутри многоугольника.
    /// Предполагается, что точка не лежит на его границе.
    /// </summary>
    private static bool IsInside(Point2D point, IReadOnlyList<Point2D> polygon)
    {
        bool inside = false;

        for (int i = 0; i < polygon.Count; i++)
        {
            int next = (i + 1) % polygon.Count;

            Point2D a = polygon[i];
            Point2D b = polygon[next];

            // Ребро должно пересекать горизонтальный уовень точки
            if ((a.Y > point.Y) == (b.Y > point.Y)) continue;

            double t = (point.Y - a.Y) / (b.Y - a.Y);

            double intersectionX = a.X + t * (b.X - a.X);

            // Считаем пересечения луча, направленного вправо.
            if (intersectionX > point.X) inside = !inside;
        }

        return inside;
    }
}