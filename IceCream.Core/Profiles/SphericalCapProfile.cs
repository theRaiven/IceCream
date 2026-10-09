namespace IceCream.Core.Profiles;

public sealed class SphericalCapProfile : IRadialProfile
{
    public double BaseZ { get; }      // высота стыка рожка и мороженого
    public double BaseRadius { get; } // место стыка рожка и мороженого
    public double Height { get; }     // высота самой шапки (шарика мороженного)

    public double SphereRadius { get; }
    public double CenterZ { get; }

    public double MinZ => BaseZ;      // минимальная высота шапки мороженного (нижняя точка)
    public double MaxZ { get; }       // максимальная высота шапки мороженного (верхняя точка)

    /// <summary>
    /// Создаёт профиль шапки мороженного в виде сферической крышки
    /// </summary>
    /// <param name="baseZ">Высота основания</param>
    /// <param name="baseRadius">Радиус основания</param>
    /// <param name="height">Высота шапки</param>
    public SphericalCapProfile(double baseZ, double baseRadius, double height)
    {
        if (!double.IsFinite(baseZ))
        {
            throw new ArgumentOutOfRangeException(nameof(baseZ), "Высота основания должна быть конечным числом.");
        }

        if (!double.IsFinite(baseRadius) || baseRadius <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(baseRadius), "Радиус основания должен быть конечным положительным числом.");
        }

        if (!double.IsFinite(height) || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Высота шапки должна быть конечным положительным числом.");
        }

        BaseZ      = baseZ;
        BaseRadius = baseRadius;
        Height     = height;

        MaxZ = baseZ + height;

        SphereRadius = (baseRadius * baseRadius + height * height) / (2 * height); // (a^2 + h^2) / 2h
        CenterZ = MaxZ - SphereRadius;

        if (!double.IsFinite(MaxZ) || MaxZ <= BaseZ || !double.IsFinite(SphereRadius) || SphereRadius <= 0 || !double.IsFinite(CenterZ))
        {
            throw new ArgumentException("Параметры приводят к некорректным размерам сферы.");
        }
    }
    /// <summary>
    /// Возвращает радиус шапки мороженного на высоте z.
    /// </summary>
    /// <param name="z">Высота</param>
    /// <returns>Радиус</returns>
    public double GetRadius(double z)
    {
        if (!double.IsFinite(z))
        {
            throw new ArgumentOutOfRangeException(nameof(z), "Высота должна быть конечным числом.");
        }

        double esp = 1e-10 * Height;

        if (z < MinZ - esp || z > MaxZ + esp)
        {
            throw new ArgumentOutOfRangeException(nameof(z), "Высота z должна находиться в пределах шапки.");
        }

        if (Math.Abs(z - MinZ) <= esp) return BaseRadius;

        if (Math.Abs(z - MaxZ) <= esp) return 0;

        double t = (z - BaseZ) / Height;

        return Math.Sqrt(1 - t) *
               Math.Sqrt(BaseRadius * BaseRadius + Height * Height * t);
    }
}
