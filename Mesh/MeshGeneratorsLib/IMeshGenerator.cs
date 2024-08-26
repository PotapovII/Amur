//namespace MeshGeneratorsLib
//{
//    using CommonLib;

//    public interface ISubAreaMeshGenerator
//    {
//        void SetSubAres(HSubAreaMap Area);
//        /// <summary>
//        /// генерация сетки в формате IMesh 
//        /// </summary>
//        /// <returns></returns>
//        IMesh CreateMesh();
//        /// <summary>
//        /// генерация сетки в формате IFEMesh 
//        /// </summary>
//        /// <returns></returns>
//        IFEMesh CreateFEMesh();
//    }

//    /// <summary>
//    /// Герерация базистной сетки в простой подобласти
//    /// </summary>
//    public interface IMeshGenerator
//    {
//        /// <summary>
//        /// генератор сетки в подобласти
//        /// </summary>
//        ISubAreaMeshGenerator generator { set; get; }
//        /// <summary>
//        /// данные об области
//        /// </summary>
//        IHTaskMap mapMesh { set; get; }
//        /// <summary>
//        /// Подготовка к генерации сетки в подобласти
//        /// </summary>
//        /// <param name="subArea">номер подобласти</param>
//        void Build(int subArea=0);
//        /// <summary>
//        /// генерация сетки в формате IMesh 
//        /// </summary>
//        /// <returns></returns>
//        IMesh CreateMesh();
//        /// <summary>
//        /// генерация сетки в формате IFEMesh 
//        /// </summary>
//        /// <returns></returns>
//        IFEMesh CreateFEMesh();
//        /// <summary>
//        /// Загрузка занных ?? сделать внешнею!!!
//        /// </summary>
//        /// <param name="fileName"></param>
//        void LoadData(string fileName);
//    }
//}
