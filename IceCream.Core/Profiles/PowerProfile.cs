namespace IceCream.Core.Profiles;
/// <summary>
/// Определяет степенной профиль рожка мороженого
/// </summary>
// в дальнейшем оно будет использоваться для формулы из интернетов r(z) = R(z/H)^n
// где R - радиус верхнего края TopRadius, H - высота рожка Height, n - показатель степени Exponent
public sealed class PowerProfile : IRadialProfile
{
    public double Height { get; }
    public double TopRadius { get; }
    public double Exponent { get; }

    public double MinZ => 0;
    public double MaxZ => Height;

    /// <summary>
    /// Создаёт степенной профиль рожка
    /// </summary>
    /// <param name="height">Высота рожка.</param>
    /// <param name="topRadius">Радиус верхнего края.</param>
    /// <param name="exponent">
    /// Показатель степени: 1 — конус, 0.5 — параболоид, чем меньше показатель, тем более выпуклый профиль, чем боьлше - тем более похож на воронку
    /// </param>
    public PowerProfile(double height, double topRadius, double exponent)
    {
        if (!double.IsFinite(height)    || height <= 0)    throw new ArgumentOutOfRangeException(nameof(height),    "Высота должна быть конечным положительным числом.");
        if (!double.IsFinite(topRadius) || topRadius <= 0) throw new ArgumentOutOfRangeException(nameof(topRadius), "Радиус должен быть конечным положительным числом.");
        if (!double.IsFinite(exponent)  || exponent <= 0)  throw new ArgumentOutOfRangeException(nameof(exponent),  "Показатель степени должен быть конечным положительным числом.");
        Height    = height;
        TopRadius = topRadius;
        Exponent  = exponent;
    }
    /// <summary>
    /// Возвращает радиус рожка на высоте z.
    /// </summary>
    public double GetRadius(double z)
    {
        if (!double.IsFinite(z) || z < MinZ || z > Height)
        {
            throw new ArgumentOutOfRangeException(nameof(z), "Координата Z должна быть конечным числом в диапазоне [0, Height].");
        }
        return TopRadius * Math.Pow(z / Height, Exponent);
    }
}
