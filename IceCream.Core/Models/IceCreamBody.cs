using IceCream.Core.Geometry;
using IceCream.Core.Profiles;
using IceCream.Core.Services;

namespace IceCream.Core.Models;

public sealed class IceCreamBody
{
    public IceCreamParameters Parameters { get; }
    public IceCreamProfile Profile { get; }
    public TriangleMesh Mesh { get; }

    public IceCreamBody(IceCreamParameters parameters, SurfaceMeshBuilder meshBuilder, int baseSegments = 40, int capSegments = 40, int radialSegments = 64)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(meshBuilder);

        // Профиль рожка
        var baseProfile = new PowerProfile(
            height: parameters.BaseHeight,
            topRadius: parameters.JoinRadius,
            exponent: parameters.BaseExponent);
        // Профиль мороженного
        var profile = new IceCreamProfile(
            baseProfile,
            capHeight: parameters.CapHeight);

        TriangleMesh mesh = meshBuilder.Build(
            profile,
            baseSegments,
            capSegments,
            radialSegments);

        Parameters = parameters;
        Profile = profile;
        Mesh = mesh;
    }
}