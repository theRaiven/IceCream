using System;
using IceCream.Core.Profiles;

namespace IceCream.Core.Services;

public sealed class VolumeCalculator
{
    /// <summary>
    /// Вычисляет объём всего мороженого.
    /// Каждая часть интегрируется отдельно.
    /// </summary>
    /// <param name="profile">Профиль мороженого.</param>
    /// <param name="intervalsPerPart">
    /// Чётное положительное количество промежутков для каждой части.
    /// </param>
    public double Calculate(IceCreamProfile profile, int intervalsPerPart = 1000)
    {
        ArgumentNullException.ThrowIfNull(profile);

        double baseVolume = Calculate(
            profile.BaseProfile,
            intervalsPerPart);

        double capVolume = Calculate(
            profile.CapProfile,
            intervalsPerPart);

        double volume = baseVolume + capVolume;

        if (!double.IsFinite(volume))
        {
            throw new InvalidOperationException("При вычислении объёма произошло переполнение.");
        }

        return volume;
    }

    /// <summary>
    /// Разбиваем высоту на чётное количество промежутков n, по шаге h = (maxZ - minZ) / n
    /// Вычисляем площади кругов S0, ... , Sn
    /// и складываем с весами V = h/3 * (S0 + 4*S1 + 2*S2 + 4*S3 + ... + 4*S_(n-1) + Sn)
    /// </summary>
    /// <param name="profile"> Профиль мороженого. </param>
    /// <param name="intervals"> Чётное положительное количество промежутков. </param>
    /// <returns> Объём мороженого. </returns>
    public double Calculate(IRadialProfile profile, int intervals)
    {
        ArgumentNullException.ThrowIfNull(profile);

        // а надо ли....
        if (profile is IceCreamProfile iceCream)
        {
            return Calculate(iceCream, intervals);
        }

        if (intervals < 2 || intervals % 2 != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(intervals), "Количество промежутков должно быть чётным и не меньше двух.");
        }

        double minZ = profile.MinZ;
        double maxZ = profile.MaxZ;
        double height = maxZ - minZ;

        if (!double.IsFinite(minZ) || !double.IsFinite(maxZ) || !double.IsFinite(height) || height <= 0)
        {
            throw new ArgumentException("Профиль должен иметь конечный положительный диапазон высот.", nameof(profile));
        }

        double step = height / intervals;

        double sum = GetSectionArea(profile, minZ) + GetSectionArea(profile, maxZ);

        for (int i = 1; i < intervals; i++)
        {
            double z = minZ + height * ((double)i / intervals);

            int weight = i % 2 == 0 ? 2 : 4;

            sum += weight * GetSectionArea(profile, z);
        }

        double volume = step / 3 * sum;

        if (!double.IsFinite(volume))
        {
            throw new InvalidOperationException("При вычислении объёма произошло переполнение.");
        }

        return volume;
    }

    private static double GetSectionArea(IRadialProfile profile, double z)
    {
        double radius = profile.GetRadius(z);

        if (!double.IsFinite(radius) || radius < 0)
        {
            throw new InvalidOperationException("Профиль должен возвращать конечный неотрицательный радиус.");
        }

        return Math.PI * radius * radius;
    }
}
