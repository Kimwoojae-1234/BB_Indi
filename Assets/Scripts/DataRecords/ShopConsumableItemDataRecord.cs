using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopConsumableItemData
{
	public enum Types
	{
		Bat = 5,
		Ball = 10,
	}
	public bool IsBat()
    {
		return (ConsumableItemType == (int)Types.Bat);
	}
	public bool IsBall()
	{
		return (ConsumableItemType == (int)Types.Ball);
	}
	public int Idx { get; set; } // 상품 아이템 인덱스
	public short ConsumableItemType { get; set; } // 
	public int DataIdx { get; set; } // 아이템 IDX
	public short P1BundleQuantity { get; set; } // Pack 1 - 구성품 수량
	public short P1PriceType { get; set; } // 
	public int P1Price { get; set; } // Pack 1 - 가격
	public short P2BundleQuantity { get; set; } // Pack 2 - 구성품 수량
	public short P2PriceType { get; set; } // 
	public int P2Price { get; set; } // Pack 2 - 가격
	public short P3BundleQuantity { get; set; } // Pack 3 - 구성품 수량
	public short P3PriceType { get; set; } // 
	public int P3Price { get; set; } // Pack 3 - 가격
	public byte InStock { get; set; } // 상품판매여부
	public byte ShopStock { get; set; }  //상점에서 판매 여부}
	public short IngameBundleQuantity { get; set; } // 인게임 판매 수량
	public short IngamePriceType { get; set; } // 인게임 판매 재화 설정
	public int IngamePrice { get; set; } // 인게임 가격
}


public class ShopConsumableItemDataRecord : BaseDataRecord
{
    public ShopConsumableItemData[] ShopConsumableItemData;
}
