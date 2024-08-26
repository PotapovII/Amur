//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.09.2021 Потапов И.И.
//---------------------------------------------------------------------------
using System.Collections.Generic;


namespace MeshLib.ATable
{
    interface IAdressTables
    {
        /// <summary>
        /// идентификатор адресной таблицы
        /// </summary>
        uint ATableID { get; }
        /// <summary>
        /// координаты узлов в которых определены неизвестные
        /// кроме того элементы массива MapTabl содержат вектор Mask[mask]
        /// ПРИМЕР MapTabl[ knot ].Mask[ mask ] в котором отмечены
        /// те из неизвестных которые определены в данном узле
        /// </summary>
        List<ITablKnot> MapTable { get; }
        /// <summary>
        /// вектор смещений неизвестных в узлах
        /// показывет адрес "первой" неизвестной в узле
        /// </summary>
        List<uint> ShiftNode { get; }
        /// <summary>
        /// МАССИВ LinkUnknow служит для определения
        /// по номеру неизвестный - индекс массива
        /// который берется из массивов AdrElems,BnAdrElems,BoundKnots
        /// номера узла в массиве MapTabl и номера (маски) неизвесной
        /// ПРИМЕР
        /// адрес неизвестной
        /// uint Adr = BnAdrElems[ fe ].Knots[ k ];
        /// номер узла для определения координат в массиве MapTabl
        /// uint AdrNode = LinkUnknow[ Adr ].Node;
        /// маска неизесной в таблице смещений
        /// uint Mask = LinkUnknow[ Adr ].Mask;
        /// </summary>
        List<IAdressUnknow> LinkUnknow { get; }
        ///////////////////////////////////
        ///     очистка CATable
        /// <summary>
        /// количество степеней свободы (слоев) в таблице
        /// </summary>
        uint CountLayers { get; }
        /// <summary>
        /// количество неизвестных для данного слоя
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        int CountCondition(int mask);
    }
}
