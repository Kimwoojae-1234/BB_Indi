using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class ShopProduct
{
    public string id, path, costPath, amountPath, currency, cashPrice, chest;
    public int cost, gold, gems, chestCount;
}
[Serializable]
public class ShopChest
{
    public string id;
    public int cards, gold;
}
[Serializable]
public class ShopCatalog
{
    public ShopProduct[] products;
    public ShopChest[] chests;
    private static ShopCatalog current;
    public static ShopCatalog Current => current ?? (current = JsonUtility.FromJson<ShopCatalog>(Resources.Load<TextAsset>("Prototype/ShopProducts").text));
    public ShopProduct Find(string id) => products.FirstOrDefault(p => p.id == id);
    public ShopChest Chest(string id) => chests.FirstOrDefault(p => p.id == id);
}
