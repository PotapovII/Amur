namespace TestDelaunayGenerator.Boundary
{
    using CommonLib.Geometry;

    public interface IGeneratorBase
    {
        IHPoint[] Generate(BoundaryHill boundary);
    }
}
