#if UNITY_EDITOR
using System;
using System.IO;
using System.Security.Cryptography;
using BaseBall.BallPlay.UGUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Shared read-only asset inspection helpers. Static labels are reviewed individually;
// there is deliberately no automatic binding by text value.
public static class LocalizationBindingPlan
{
    public static string TextOf(Component c) => c is GameUIElement e?e.text:c is TMP_Text t?t.text:c is Text legacy?legacy.text:c is TextMesh mesh?mesh.text:c is tk2dTextMesh sprite?sprite.text:null;
    public static string Hierarchy(Transform t)=>t.parent==null?t.name+"["+t.GetSiblingIndex()+"]":Hierarchy(t.parent)+"/"+t.name+"["+t.GetSiblingIndex()+"]";
    public static string Hash(string path){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant();}
}
#endif
