namespace IceCream.Core.Profiles;

/// <summary>
/// Определяет профиль мороженного, состоящий из профиля рожка и шапки мороженного
/// </summary>
public sealed class IceCreamProfile : IRadialProfile
{
    public IRadialProfile BaseProfile { get; }     // профиль рожка мороженого
    public SphericalCapProfile CapProfile { get; } // профиль шапки мороженного

    public double MinZ => BaseProfile.MinZ; // минимальная высота профиля (нижняя точка рожка мороженного)
    public double MaxZ => CapProfile.MaxZ;  // максимальная высота профиля (верхняя точка шапки мороженного)

    public double JoinZ => BaseProfile.MaxZ; // точка присоединения шапки к рожку

    /// <summary>
    /// Создаёт профиль мороженного, состоящий из профиля рожка и шапки мороженного
    /// </summary>
    /// <param name="baseProfile">Профиль рожка мороженого</param>
    /// <param name="capProfile">Профиль шапки мороженного</param>
    public IceCreamProfile(IRadialProfile baseProfile, SphericalCapProfile capProfile)
    {
        ArgumentNullException.ThrowIfNull(baseProfile);
        ArgumentNullException.ThrowIfNull(capProfile);

        double baseHeight = baseProfile.MaxZ - baseProfile.MinZ;
        double heightESP = 1e-10 * Math.Max(baseHeight, capProfile.Height);

        if (Math.Abs(baseProfile.MaxZ - capProfile.MinZ) > heightESP)
        {
            throw new ArgumentException("Верх рожка и основание шапки должны находиться на одной высоте.",  nameof(capProfile));
        }

        double joinRadius = baseProfile.GetRadius(baseProfile.MaxZ);

        double radiusESP =  1e-10 * Math.Max(Math.Abs(joinRadius), capProfile.BaseRadius);

        if (!double.IsFinite(joinRadius) || joinRadius <= 0 || Math.Abs(joinRadius - capProfile.BaseRadius) > radiusESP)
        {
            throw new ArgumentException("Радиусы рожка и шапки на стыке должны совпадать.", nameof(capProfile));
        }

        BaseProfile = baseProfile;

        CapProfile = new SphericalCapProfile(
            baseZ:      baseProfile.MaxZ,
            baseRadius: joinRadius,
            height:     capProfile.Height);
    }

    /// <summary>
    /// Создаёт профиль мороженного, состоящий из профиля рожка и шапки мороженного
    /// </summary>
    /// <param name="baseProfile">Профиль рожка мороженого</param>
    /// <param name="capHeight">Высота шапки мороженного</param>
    public IceCreamProfile(IRadialProfile baseProfile, double capHeight) : this(baseProfile, CreateCap(baseProfile, capHeight)) { }

    /// <summary>
    /// Создаёт профиль шапки мороженного на основе профиля рожка и высоты шапки
    /// </summary>
    /// <param name="baseProfile">Профиль рожка мороженого</param>
    /// <param name="capHeight">Высота шапки мороженного</param>
    /// <returns></returns>
    private static SphericalCapProfile CreateCap(IRadialProfile baseProfile, double capHeight)
    {
        ArgumentNullException.ThrowIfNull(baseProfile);

        return new SphericalCapProfile(
            baseZ:      baseProfile.MaxZ,
            baseRadius: baseProfile.GetRadius(baseProfile.MaxZ),
            height:     capHeight);
    }

    /// <summary>
    /// Возвращает радиус профиля мороженного на высоте z.
    /// </summary>
    /// <param name="z">Высота</param>
    /// <returns>Радиус</returns>
    public double GetRadius(double z)
    {
        if (!double.IsFinite(z))
        {
            throw new ArgumentOutOfRangeException(nameof(z), "Высота должна быть конечным числом.");
        }

        if (z < JoinZ)
        {
            return BaseProfile.GetRadius(z);
        }
        else if (z <= MaxZ)
        {
            return CapProfile.GetRadius(z);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(z), "Высота выходит за пределы профиля мороженного.");
        }
    }
}
