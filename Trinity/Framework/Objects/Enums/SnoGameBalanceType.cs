﻿namespace Trinity.Framework.Objects.Enums
{
    public enum SnoGameBalanceType
    {
        AxeBadData = -1,
        ItemTypes = 1,
        Items = 2,
        ExperienceTable = 3,
        Hirelings = 4,
        MonsterLevels = 5,
        Heroes = 7,
        AffixList = 8,
        Inventory = 9,
        MovementStyles = 10,
        Labels = 11,
        LootDistribution = 12,
        RareItemNames = 16,
        Scenery = 17,
        MonsterAffixes = 18,
        MonsterNames = 19,
        SocketedEffects = 21,
        ItemEnchantments = 23,
        HelpCodes = 24,
        ItemDropTable = 25,
        ItemLevelModifiers = 26,
        QualityClasses = 27,
        Handicaps = 28,
        ItemSalvageLevels = 29,
        Recipes = 32,
        SetItemBonuses = 33,
        EliteModifiers = 34,
        ItemTiers = 35,
        PowerFormulaTables = 36,
        ScriptedAchievementEvents = 37,
        ItemDurabilityCostsScalar = 38,
        LootRunQuestTiers = 39,
        ParagonBonuses = 40,
        MadeToOrderTypes = 42,
        MadeToOrderAffixFolders = 43,
        DevilsHandRedeemableSets = 44,
        LegacyItemConversions = 45,
        EnchantItemAffixUseCountCostScalars = 46,
        AffixGroup = 47,
        TieredLootRunLevels = 48
    }

    public enum GameBalanceOffset
    {
        None = 0,
        Items = 0x18,
        Items2 = 0x28,
        ExperienceTable = 0x38,
        ExperienceTableAlt = 0x48,
        HelpCodes = 0x58,
        MonsterLevelTable = 0x68,
        AffixTable = 0x78,
        Heros = 0x88,
        MovementStyles = 0x98,
        Labels = 0xA8,
        LootDistributionTable = 0xB8,
        RareItemNamesTable = 0xC8,
        MonsterAffixesTable = 0xD8,
        RareMonsterNamesTable = 0xE8,
        SocketedEffectsTable = 0xF8,
        ItemDropTable = 0x108,
        ItemLevelModTable = 0x118,
        QualityClassTable = 0x128,
        HandicapLevelTable = 0x138,
        ItemSalvageLevelTable = 0x148,
        Hirelings = 0x158,
        SetItemBonusTable = 0x168,
        EliteModifiers = 0x178,
        ItemTiers = 0x188,
        PowerFormulaTable = 0x198,
        RecipesTable = 0x1A8,
        ScriptedAchievementEventsTable = 0x1B8,
        LootRunQuestTierTable = 0x1C8,
        ParagonBonusesTable = 0x1D8,
        LegacyItemConversionTable = 0x1E8,
        EnchantItemAffixUseCountCostScalarsTable = 0x1F8,
        TieredLootRunLevelTable = 0x208,
        TransmuteRecipesTable = 0x218,
    }

    public enum SnoGameBalanceTypeOffsets
    {
        ItemTypes = 1,
        Items = 2,
        ExperienceTable = 3,
        Hirelings = 4,
        MonsterLevels = 5,
        Heroes = 6,
        AffixList = 7,
        Inventory = 8,
        MovementStyles = 9,
        Labels = 10,
        LootDistribution = 11,
        RareItemNames = 12,
        Scenery = 13,
        MonsterAffixes = 14,
        MonsterNames = 15,
        SocketedEffects = 16,
        ItemEnchantments = 17,
        HelpCodes = 18,
        ItemDropTable = 19,
        ItemLevelModifiers = 20,
        QualityClasses = 21,
        Handicaps = 22,
        ItemSalvageLevels = 23,
        Recipes = 24,
        SetItemBonuses = 25,
        EliteModifiers = 26,
        ItemTiers = 27,
        PowerFormulaTables = 28,
        ScriptedAchievementEvents = 29,
        ItemDurabilityCostsScalar = 30,
        LootRunQuestTiers = 31,
        ParagonBonuses = 32,
        MadeToOrderTypes = 33,
        MadeToOrderAffixFolders = 34,
        DevilsHandRedeemableSets = 35,
        LegacyItemConversions = 36,
        EnchantItemAffixUseCountCostScalars = 37,
        AffixGroup = 38,
        TieredLootRunLevels = 39
    }

    public enum GameBalanceTableId
    {
        ItemsLegendaryOther = 1189,
        ItemsQuests = 130867,
        ItemsLegendary = 170627,
        AffixList = 19737,
        AxeBadData = 19739,
        Characters = 19740,
        ExperienceTable = 19741,
        ExperienceTablePvP = 19742,
        Hirelings = 19744,
        Inventory = 19745,
        InventoryPvP = 19746,
        ItemsArmor = 19750,
        ItemsLegendaryWeapons = 19752,
        ItemsOther = 19753,
        ItemsQuestsBeta = 197880,
        ItemsPagesOfFate = 245193,
        ExperienceTableAlt = 252616,
        HandicapLevels = 256027,
        ItemRequiredLevels = 256158,
        _xx1AffixList = 306532,
        ItemsRandomPlaceholder = 361240,
        ItemSalvageLevels = 361430,
        ItemArtTest = 402453,
        ItemsWeapons = 19754,
        ItemTypes = 19755,
        LabelsGlobal = 19757,
        MonsterAffixes = 19759,
        MonsterLevels = 19760,
        MonsterNames = 19761,
        ParagonBonuses = 286748,
        PowerFormulaTables = 153762,
        RareNames = 19765,
        RecipesBlacksmith = 190073,
        RecipesJeweler = 190074,
        RecipesMystic = 190075,
        SetItemBonuses = 123048,
        SocketedEffects = 19766,
        TransmuteRecipes = 429023,
        x1AffixList = 302484,
        x1EnchantItemAffixUseCountCostScalars = 346698,
        x1LegacyItemConversions = 342632,
        x1TieredLootRunLevels = 380479,
    }
}