namespace RiverLib
{
    public class RiverError
    {
        /// <summary>
        /// Имя ошибки
        /// </summary>
        /// <param name="flagErr"></param>
        /// <returns></returns>
        public static string ErrorName(int flagErr)
        {
            string s = "Ошибка при расчете ";
            string[] ErrName = { s + "сободной поверхности: SolveWaterLevel",
                                 s + "построение КЭ сетки: SetDataForRiverStream",
                                 s + "скоростей потока: SolveVelosity",
                                 s + "придонных касательных напряжений: TausToVols" };
            return ErrName[flagErr];
        }
    }
}
