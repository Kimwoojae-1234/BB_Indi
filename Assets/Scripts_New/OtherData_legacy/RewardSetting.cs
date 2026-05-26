

public enum RewardType
{
    None = 0,
    Gold = 1,
    Gem = 2,
    Energy = 3,
    XP = 4,
    PowerTP = 5,
    ContactTP = 6,
    VisionTP = 7,
    FieldingTP = 8,
    ThrowingTP = 9,
    SpeedTP = 10,
    SkillPoint = 11,
    PlayerCard = 12,
    SkillCard = 13,
    GearCard = 14,
    Skin = 15,
    Bat = 16,
    GameToken = 17,
    /*Chest = 17,
    RareChest = 18,
    EpicChest = 19,
    LegendChest = 20,
    SkinChest = 21,
    GearChest = 22,
    PlayerChest = 23,*/
}

public class RewardSetting
{
    public RewardType rewardData { get; set; }
    public int index { get; set; }
    public int quantity { get; set; }
    public bool bNewItem { get; set; }


    public RewardSetting()
    {

    }

    public RewardSetting(RewardType type, int idx, int qun)
    {
        rewardData = type;
        index = idx;
        quantity = qun;
    }

}