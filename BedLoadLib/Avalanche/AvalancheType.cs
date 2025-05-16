namespace BedLoadLib
{
    using System.ComponentModel;
    public enum AvalancheType
    {
        [Description("Не использовать лавинную модель")]
        NonAvalanche = 0,
        [Description("Балансная лавинная модель на произвольной сетке")]
        AvalancheSimple,
        [Description("Балансная лавинная модель на регулярной сетке")]
        AvalancheSimpleOld,
        [Description("Дифференцальная лавинная модель")]
        AvalancheQuad_2006,
        [Description("Дифференцальная лавинная модель")]
        AvalancheQuad_2021
    }
}
