using IceCream.Core.Geometry;
using IceCream.Core.Profiles;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace IceCream.Core.Services;

public sealed class SurfaceMeshBuilder
{
    /// <summary>
    /// Строит треугольную сетку для мороженого на основе профиля.
    /// </summary>
    /// <param name="profile"> Профиль мороженого </param>
    /// <param name="baseSegments"> Количество сегментов основания </param>
    /// <param name="capSegments"> Количество сегментов крышки </param>
    /// <param name="radialSegments"> Количество радиальных сегментов </param>
    /// <returns> Треугольная сетка </returns>
    public TriangleMesh Build(IceCreamProfile profile, int baseSegments = 40, int capSegments = 40, int radialSegments = 64)
    {
        ArgumentNullException.ThrowIfNull(profile);

        if (baseSegments < 1)   throw new ArgumentOutOfRangeException(nameof(baseSegments));
        if (capSegments < 1)    throw new ArgumentOutOfRangeException(nameof(capSegments));
        if (radialSegments < 3) throw new ArgumentOutOfRangeException(nameof(radialSegments));

        var vertices = new List<Point3D>();
        var indices  = new List<int>();

        int[] joinRing = BuildBase(profile.BaseProfile, baseSegments, radialSegments, vertices, indices);
        BuildCap(profile.CapProfile, capSegments, radialSegments, vertices, indices, joinRing);

        return new TriangleMesh(vertices, indices);
    }

    /// <summary>
    /// Строит основание мороженого на основе профиля.
    /// </summary>
    /// <param name="profile"> Профиль </param>
    /// <param name="heightSegments"> Количество сегментов по высоте </param>
    /// <param name="radialSegments"> Количество радиальных сегментов </param>
    /// <param name="vertices"> Список вершин </param>
    /// <param name="indices"> Список индексов </param>
    /// <returns> Массив индексов для первого кольца </returns>
    private static int[] BuildBase(IRadialProfile profile, int heightSegments, int radialSegments, List<Point3D> vertices, List<int> indices)
    {
        int[] firstRing = CreateRing(profile, profile.MinZ, radialSegments, vertices);
        return BuildPart(profile, heightSegments, radialSegments, firstRing, vertices, indices);
    }
    /// <summary>
    /// Строит крышку мороженого на основе профиля.
    /// </summary>
    /// <param name="profile"> Профиль </param>
    /// <param name="heightSegments"> Количество сегментов по высоте </param>
    /// <param name="radialSegments"> Количество радиальных сегментов </param>
    /// <param name="vertices"> Список вершин </param>
    /// <param name="indices"> Список индексов </param>
    /// <param name="joinRing"> Массив индексов для кольца, которое соединяется с крышкой </param>
    private static void BuildCap(IRadialProfile profile, int heightSegments, int radialSegments, List<Point3D> vertices, List<int> indices, int[] joinRing)
    {
        // Начальное кольцо уже создано при построении рожка
        BuildPart(profile, heightSegments, radialSegments, joinRing, vertices, indices);
    }
    /// <summary>
    /// Строит часть мороженого на основе профиля.
    /// </summary>
    /// <param name="profile"> Профиль </param>
    /// <param name="heightSegments"> Количество сегментов по высоте </param>
    /// <param name="radialSegments"> Количество радиальных сегментов </param>
    /// <param name="firstRing"> Массив индексов для первого кольца </param>
    /// <param name="vertices"> Список вершин </param>
    /// <param name="indices"> Список индексов </param>
    /// <returns></returns>
    private static int[] BuildPart(IRadialProfile profile, int heightSegments, int radialSegments, int[] firstRing, List<Point3D> vertices, List<int> indices)
    {
        int[] previousRing = firstRing;

        for(int i = 1; i <= heightSegments; i++)
        {
            double t = (double)i / heightSegments;
            double z;
            if( i == heightSegments)
            {
                z = profile.MaxZ; 
            }
            else
            { 
                z = profile.MinZ + t * (profile.MaxZ - profile.MinZ);
            }
            int[] currentRing = CreateRing(profile, z, radialSegments, vertices);
            ConnectRing(previousRing, currentRing, indices);

            previousRing = currentRing;
        }
        return previousRing;
    }
    /// <summary>
    /// Создаёт вершины кольца на заданной высоте.
    /// При нулевом радиусе создаёт одну вершину на оси.
    /// </summary>
    /// <param name="profile"> Профиль </param>
    /// <param name="z"> Высота </param>
    /// <param name="radialSegments"> Количество радиальных сегментов </param>
    /// <param name="vertices"> Список вершин </param>
    /// <returns> Массив индексов для кольца </returns>
    private static int[] CreateRing(IRadialProfile profile, double z, int radialSegments, List<Point3D> vertices)
    {
        double radius = profile.GetRadius(z);
        if (!double.IsFinite(radius) || radius < 0)
        {
            throw new InvalidOperationException("Радиус должен быть конечным неотрицательным числом.");
        }
         
        if (radius == 0)
        {
            // Если радиус равен нулю, создаем одну вершину в центре
            int centerIndex = vertices.Count;
            vertices.Add(new Point3D { X = 0, Y = 0, Z = z });
            return new int[] { centerIndex };
        }

        int[] ringIndices = new int[radialSegments];
        double angleStep = 2 * Math.PI / radialSegments;

        for(int i = 0; i < radialSegments; i++)
        {
            double angle = i * angleStep;

            double x = radius * Math.Cos(angle);
            double y = radius * Math.Sin(angle);

            ringIndices[i] = vertices.Count;
            vertices.Add(new Point3D { X = x, Y = y, Z = z });
        }
        return ringIndices;
    }
    /// <summary>
    /// соединяет два соседних кольца.
    /// Есть три случая: нижний полюс, верхний полюс и два обычных кольца.
    /// </summary>
    /// <param name="lower"> Нижнее кольцо </param>
    /// <param name="upper"> Верхнее кольцо </param>
    /// <param name="indices"> Список индексов </param>
    private static void ConnectRing(int[] lower, int[] upper, List<int> indices)
    {
        if (lower.Length == 0 || upper.Length == 0)
        {
            throw new ArgumentException("Кольца не должны быть пустыми.");
        }

        if (lower.Length == 1 && upper.Length == 1)
        {
            throw new InvalidOperationException("Нельзя построить поверхность между двумя полюсами.");
        }

        if (lower.Length == 1)
        {
            // Нижний полюс
            int polus = lower[0];
            for (int i = 0; i < upper.Length; i++)
            {
                int next = (i + 1) % upper.Length;

                AddTriangle(indices, polus, upper[next], upper[i]);
            }
            return;
        }
        if (upper.Length == 1)
        {
            // Верхний полюс
            int polus = upper[0];
            for (int i = 0; i < lower.Length; i++)
            {
                int next = (i + 1) % lower.Length;
                AddTriangle(indices, lower[i], lower[next], polus);
            }
            return;
        }
        if (lower.Length != upper.Length)
        {
            throw new ArgumentException("Обычные кольца должны содержать одинаковое количество вершин.");
        }
       // Два обычных кольца
        for (int i = 0; i < lower.Length; i++)
        {
            int next = (i + 1) % lower.Length;

            int a = lower[i];
            int b = lower[next];
            int c = upper[i];
            int d = upper[next];

            AddTriangle(indices, a, b, c);
            AddTriangle(indices, b, d, c);
        }
    }
    
    private static void AddTriangle(List<int> indices, int a, int b, int c)
    {
        indices.Add(a);
        indices.Add(b);
        indices.Add(c);
    }
}
