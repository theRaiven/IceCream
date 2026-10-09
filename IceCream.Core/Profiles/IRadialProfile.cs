
namespace IceCream.Core.Profiles;

public interface IRadialProfile
{
    /// <summary>
    /// Минимальное значение Z
    /// </summary>
    double MinZ { get; }
    /// <summary>
    /// Максимальное значение Z
    /// </summary>
    double MaxZ { get; }
    /// <summary>
    /// Возвращает радиус профиля в зависимости от координаты Z
    /// </summary>
    /// <param name="z">Координата Z</param>
    /// <returns>Радиус профиля</returns>
    double GetRadius(double z);
}