using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSlot : MonoBehaviour {

    public enum ItemType
    {
        PACK,
        GEAR,
        TROPHY,
        GOODS,
        GOLD,
        RUBY,
        HEART,
        MAX,
    }

    public enum ItemSize
    {
        BIG,
        SMALL,
    }

    [SerializeField]
    private UISprite bg;
    [SerializeField]
    private UISprite icon;
    [SerializeField]
    private UILabel have_count_label;
    [SerializeField]
    private UILabel itemName;
    [SerializeField]
    private ItemSize itemSize = ItemSize.SMALL;
    [SerializeField]
    private GameObject GearObject;
    [SerializeField]
    private UISprite gearPlayerType;
    [SerializeField]
    private UILabel gearReinforce_label;
    [SerializeField]
    private UISprite GearGradeStar;
    [SerializeField]
    private UILabel GearGradeLabel;
    private ItemType itemType = ItemType.MAX;
    public ItemType ITEM_TYPE { get { return itemType; } }

    private ItemData itemData;
    private gear gearDBData;
    //private item itemData;
    //private int have_count;
    public void InitItemSlot()
    {
        bg.spriteName = string.Empty;
        icon.spriteName = string.Empty;
        have_count_label.text = string.Empty;
        itemName.text = string.Empty;
        if(GearObject != null)
            GearObject.SetActive(false);
    }

    public void SetItemSlot(int id, int count)
    {
        if (itemData == null)
            itemData = new ItemData();
        // DISABLED_MGRS: this.itemData.itemDB_data = Mgrs.GameData.GameDB_FindItem(id);
        this.itemData.have_count = count;
    }

    public void SetCurrencySlot(ItemType eType, int Get_value)
    {
        if (itemData == null)
            itemData = new ItemData();

        this.itemData.itemDB_data = null;
        this.itemData.have_count = Get_value;
        this.itemName.text = string.Empty;
        this.have_count_label.text = string.Empty;
        this.icon.spriteName = string.Empty;
        this.itemType = eType;
    }

    /// <summary>
    /// 아이템 슬롯 세팅 함수
    /// </summary>
    /// <param name="item_id">아이템, 장비, 트로피 아이디(SeqX)</param>
    /// <param name="etc_count">아이템, 트로피 갯수 또는 장비의 강화 수치</param>
    /// <param name="itemType">아이템 타입</param>
    /// <param name="name_view">이름을 보여줄것인지 안보여줄것인지(true : 보여줌, false : 안보여줌)</param>
    /// <param name="isMini">작은 아이콘인지 큰 아이콘인지(true : 작은아이콘, false : 큰아이콘)</param>
    public void SetItemSlotPrefab(int item_id, int etc_count, ItemType itemType, bool name_view = true, bool isMini = false)
    {
        this.itemType = itemType;
        this.InitItemSlot();
        switch (this.itemType)
        {
            case ItemType.PACK:
            case ItemType.GOODS:
                SetBigsizeItemInfo(item_id, etc_count, isMini);
                break;
            case ItemType.GEAR:
                // DISABLED_MGRS: SetGearItem(item_id, etc_count, Mgrs.UI.GetCurWindow().windowID == WindowID.UI_WindowInventory, name_view);
                break;
            case ItemType.TROPHY:
                SetTrophyInfo(item_id, etc_count);
                break;
            case ItemType.GOLD:
            case ItemType.RUBY:
            case ItemType.HEART:
                SetCurrencyItem(itemType, etc_count, isMini);
                break;
            default:
                break;
        }
        if (name_view == false)
            this.itemName.text = string.Empty; 
    }

    /// <summary>
    /// WebConnector.ItemType으로 아이템 슬롯 세팅하는 함수
    /// </summary>
    /// <param name="item_id">아이템, 장비, 트로피의 아이디값(Seq X)</param>
    /// <param name="etc_count">아이템, 트로피의 갯수 또는 장비의 강화 수치</param>
    /// <param name="itemType">WebConnector.ItemType</param>
    /// <param name="name_view">이름을 보여줄 것인지 안 보여 줄것인지</param>
    /// <param name="isMini">작은 스롯인지 큰 스롯인지</param>
    public void SetItemSlotPrefab(int item_id, int etc_count, WebConnector.ItemType itemType, bool name_view = true, bool isMini = false)
    {
        switch(itemType)
        {
            case WebConnector.ItemType.BoostCard:
            case WebConnector.ItemType.CardPack:
            case WebConnector.ItemType.CardPackRandom:
            case WebConnector.ItemType.CardPackTeam:
            case WebConnector.ItemType.GearPack:
            case WebConnector.ItemType.CardPackTeamYear:
                SetItemSlotPrefab(item_id, etc_count, ItemType.PACK, name_view, isMini);
                break;
            case WebConnector.ItemType.None:
                SetItemSlotPrefab(item_id, etc_count, ItemType.GOODS, name_view, isMini);
                break;
            case WebConnector.ItemType.Trophy:
                SetItemSlotPrefab(item_id, etc_count, ItemType.TROPHY, name_view, isMini);
                break;
        }
    }

    private void SetBigsizeItemInfo(int pack_id, int have_count, bool isMini)
    {
        this.SetItemSlot(pack_id, have_count);

        if(itemData == null)
        {
            bg.spriteName = string.Empty;
            icon.spriteName = string.Empty;
            have_count_label.text = string.Empty;
            itemName.text = string.Empty;
            return;
        }
        
        icon.MakePixelPerfect();
    }

    /// <summary>
    /// 트로피슬롯 세팅함수
    /// </summary>
    /// <param name="trophy_id">순 트로피 아이디 (ex => 1200001)</param>
    /// <param name="have_count">갖고있는 수량</param>
	private void SetTrophyInfo(int trophy_id, int have_count)
    {
        this.SetItemSlot(trophy_id, have_count);
        if (itemData == null)
        {
            bg.spriteName = string.Format("itembg_{0}", 1200001);
            icon.spriteName = string.Empty;
            have_count_label.text = string.Empty;
            itemName.text = string.Empty;
            return;
        }
        
        have_count_label.text = string.Format("X{0}", have_count);
        itemName.text = itemData.itemDB_data.name;
        bg.MakePixelPerfect();
        icon.MakePixelPerfect();
    }

    private void SetCurrencyItem(ItemType eItemType, int get_value, bool isMini)
    {
        this.SetCurrencySlot(eItemType, get_value);
        if(itemData == null)
        {
            bg.spriteName = string.Format("itembg_{0}", 1200001);
            icon.spriteName = string.Empty;
            have_count_label.text = string.Empty;
            itemName.text = string.Empty;
            return;
        }
        string icon_name = string.Empty;
        switch(eItemType)
        {
            case ItemType.GOLD:
                icon_name = isMini == true ? "item_gold_mini" :"item_gold";
                break;
            case ItemType.RUBY:
                icon_name = isMini == true ? "item_ruby_mini" : "item_ruby";
                break;
            case ItemType.HEART:
                icon_name = isMini == true ? "item_heart_mini" : "item_heart";
                break;

        }
        bg.spriteName = string.Format("itembg_{0}", 1200001);
        icon.spriteName = icon_name;
        have_count_label.text = get_value.ToString();
        itemName.text = string.Empty;
        bg.MakePixelPerfect();

    }

    public void InitGearItem()
    {
        this.GearObject.SetActive(true);
        this.icon.spriteName = string.Empty;
        this.gearReinforce_label.text = string.Empty;
        this.itemName.text = string.Empty;
        this.GearGradeLabel.text = string.Empty;
        this.itemName.text = string.Empty;
        this.GearGradeStar.spriteName = string.Empty;
    }

    private void SetGearItem(int gear_id, int reinforce_count, bool isOnlyView, bool isNameView)
    {
        
    }

    public void SetItemCount(int value)
    {
        int have_count = value;
        this.have_count_label.text = string.Format("X{0}", have_count);
    }

    private void SetPackInfo(int pack_id, int have_count)
    {
        
    }


    private void SetGoodsInfo(int goods_id, int have_count)
    {
        
    }

    public void ViewHaveCount(bool view)
    {
        have_count_label.gameObject.SetActive(view);
    }

    public int GetitemId()
    {
        if(this.itemType != ItemType.GEAR)
            return this.itemData.itemDB_data.item_id;
        else
            return this.gearDBData.gear_id;
    }
    public int GetHaveCount()
    {
        return this.itemData.have_count;
    }

    public item GetItemInfo()
    {
        return this.itemData.itemDB_data;
    }

    public int GetImgWidth()
    {
        int width = 0;
        if (itemSize == ItemSize.SMALL)
            width = bg.width;
        else
            width = icon.width;

        return width;
    }

    public int GetImgHeight()
    {
        int height = 0;
        if (itemSize == ItemSize.SMALL)
            height = bg.height;
        else
            height = icon.height;

        return height;
    }

    public void SetItemDepth(int frontdepth)
    {
        int depth_value = frontdepth;
        this.bg.depth = depth_value;
        this.icon.depth = depth_value - 1;
        this.have_count_label.depth = depth_value - 2;
        this.itemName.depth = depth_value - 2;
    }

    public int GetBGDepth()
    {
        if (itemSize == ItemSize.BIG)
            return this.icon.depth;
        else
            return this.bg.depth;
    }

    public void DepthPlus(int value)
    {
        bg.depth = bg.depth + value;
        icon.depth = icon.depth + value;
        have_count_label.depth = have_count_label.depth + value;
        itemName.depth = itemName.depth + value;
    }


    public void SetMaterialDepth(int depth)
    {
        this.bg.depth = this.bg.depth+depth;
        this.icon.depth = this.icon.depth+depth;
        this.gearReinforce_label.depth = this.gearReinforce_label.depth+depth;
        this.itemName.depth = this.itemName.depth+depth;
        this.GearGradeLabel.depth = this.GearGradeLabel.depth+depth;
        this.GearGradeStar.depth = this.GearGradeStar.depth+depth;
        this.itemName.depth = this.itemName.depth+depth;
    }

    public WebConnector.ItemType GetItemType()
    {
        return itemData.itemDB_data.eType;
    }
}
