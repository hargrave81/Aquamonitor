namespace AquaMonitor.Data.Models
{
    /// <summary>
    /// Reading Types
    /// </summary>
    public enum ReadingType : int
    {
        Unknown = 0,
        Ammonia = 1,
        PH = 2,
        Nitrite = 3,
        Nitrate = 4,
        CA2Hardness = 5,
        MG2Hardness = 6,
        TotalHardness = 7,
        Alkalinity = 8,
        WaterTemp = 9,
        FishFeed = 10,
        MacroN_Nitrogen = 11,
        MacroK_Potassium = 12,
        MacroCa_Calcium = 13,
        MacroMg_Magnesium = 14,
        MacroP_Phosphorus =15,
        MacroS_Sulfur = 16,
        MicroCl_Chlorine = 17,
        MicroFe_Iron = 18,
        MicroMn_Manganese = 19,
        MicroB_Boron = 20,
        MicroZn_Zinc = 21,
        MicroCu_Copper = 22,
        MicroMo_Molybdenum = 23,
        DisolvedOxygen = 24
    }
}
