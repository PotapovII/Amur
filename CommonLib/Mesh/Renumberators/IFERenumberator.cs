namespace CommonLib.Mesh
{
    public interface IFERenumberator
    {
        /// <summary>
        /// Метод фронтальной перенумерации 2D сетки
        /// </summary>
        /// <param name="mesh">сетка</param>
        /// <param name="direction">направление фронта</param>
        /// <returns></returns>
        void FrontRenumberation(ref IFEMesh NewMesh, IFEMesh mesh, Direction direction = Direction.toRight);
    }
}
