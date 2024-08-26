namespace BLLib
{
    using CommonLib;
    using System.ComponentModel;

    interface IBedLoadParams
    {
        /// <summary>
        /// Тип граничных условий на границе
        /// </summary>
        [DisplayName("Тип граничных условий")]
        //[Category("Граничные условия")]
        TypeBoundCond typeBC { get; set; }
    }
    //interface IMixBedLoadParams : IBedLoadParams
    //{
    //}
    //interface IABedLoadTask<IP> : IBedLoadTask
    //{
    //    IP Param { get; set; }
    //}

}
