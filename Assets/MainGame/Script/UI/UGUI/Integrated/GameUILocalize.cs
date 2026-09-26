using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace BaseBall.BallPlay.UGUI
{
    public sealed class GameUILocalize : MonoBehaviour
    {
        public string key;
        public TextAsset table;
        public GameUIElement element;
        public string CatalogKey => ResolveCatalogKey(key);
        public static string ResolveCatalogKey(string serializedKey)
        {
            // Three old prefabs stored sentences in the key field. Retain their authored meaning.
            switch (serializedKey)
            {
                case "점": return "UI.Label.Points";
                case "리스트는 오전 06:00에 갱신됩니다.": return "UI.Label.TicketsReplenishDailyAtMidnight";
                case "난이도에 따라서 골드를 보상으로 받습니다.": return "Legacy.GoldTicketTip";
                default: return serializedKey;
            }
        }
        void OnEnable() { L10n.LanguageChanged += Localize; Localize(); }
        void OnDisable() { L10n.LanguageChanged -= Localize; }
        public void Localize()
        {
            if (element == null) return;
            if (element.kind != GameUIElement.ElementKind.Sprite)
            {
                element.text = L10n.T(CatalogKey);
                return;
            }
            // Sprite identifiers are resource names; retain the sprite table contract.
            if (table == null) return;
            var rows = Parse(table.text);
            if (rows.Count == 0) return;
            string language = PlayerPrefs.GetString("Language", rows[0].Count > 1 ? rows[0][1] : "");
            int column = Mathf.Max(1, rows[0].IndexOf(language));
            foreach (var row in rows)
                if (row.Count > column && row[0] == key) { if (element.kind == GameUIElement.ElementKind.Sprite) element.spriteName = row[column]; else element.text = row[column]; break; }
        }
        static List<List<string>> Parse(string csv)
        {
            var rows = new List<List<string>>(); var row = new List<string>(); var cell = new StringBuilder(); bool quoted = false;
            for (int i = 0; i < csv.Length; i++)
            {
                char c = csv[i];
                if (c == '"') { if (quoted && i + 1 < csv.Length && csv[i + 1] == '"') { cell.Append(c); i++; } else quoted = !quoted; }
                else if (!quoted && (c == ',' || c == '\n')) { row.Add(cell.ToString().TrimStart('\ufeff')); cell.Clear(); if (c == '\n') { rows.Add(row); row = new List<string>(); } }
                else if (c != '\r' || quoted) cell.Append(c);
            }
            if (cell.Length > 0 || row.Count > 0) { row.Add(cell.ToString()); rows.Add(row); }
            return rows;
        }
    }
}
