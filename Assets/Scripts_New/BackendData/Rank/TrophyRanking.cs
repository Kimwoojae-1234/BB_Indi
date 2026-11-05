using System;
using System.Collections.Generic;
using BackEnd;
using LitJson;
using UnityEngine;

namespace BackendData.Rank
{
    /// <summary>Publishes committed GrowthInfo trophies to flat fields in the private game table.</summary>
    public sealed class TrophyRanking
    {
        public const string TableName = "KOBGameData";
        public const string ScoreColumn = "Trophy";
        public const string ProfileColumn = "RankProfile";
        public RankTableItem Table { get; private set; }
        private bool publishing;
        private string publishedKey;
        private Action<string> waiting;

        public void Sync(Action<string> completed = null)
        {
            waiting += completed;
            if (publishing) return;
            var data = KOBManager.Backend.GameData.KOBGameData;
            if (string.IsNullOrEmpty(data.GetInDate())) { Finish(UILocalization.Get("ui.722e90c7be04")); return; }
            publishing = true;
            if (Table != null) { Publish(); return; }
            SendQueue.Enqueue(Backend.URank.User.GetRankTableList, result => {
                if (!result.IsSuccess()) { Finish(UILocalization.Get("ui.2a064304338a")); return; }
                try
                {
                    foreach (JsonData row in result.FlattenRows())
                    {
                        var candidate = new RankTableItem(row);
                        if (candidate.table == TableName && candidate.column == ScoreColumn &&
                            candidate.extraDataColumn == ProfileColumn && candidate.rankType == RankType.user && candidate.date == "infinity" &&
                            string.Equals(candidate.order, "desc", StringComparison.OrdinalIgnoreCase))
                        { Table = candidate; break; }
                    }
                }
                catch (Exception e) { Debug.LogException(e); Finish(UILocalization.Get("ui.28825e31c817")); return; }
                if (Table == null) { Finish(UILocalization.Get("ui.21b09bfcb24e")); return; }
                Publish();
            });
        }

        private string CurrentKey()
        {
            var data = KOBManager.Backend.GameData.KOBPublicData;
            return KOBManager.MyInfo.GameData.GrowthInfo.Trophy + "|" + data.ProfileId + "|" + data.CountryCode + "|" + KOBManager.MyInfo.BackkendUserInfo?.nickname;
        }

        private void Publish()
        {
            string key = CurrentKey();
            if (key == publishedKey) { Finish(null); return; }
            var data = KOBManager.Backend.GameData.KOBPublicData;
            int trophy = KOBManager.MyInfo.GameData.GrowthInfo.Trophy;
            SendQueue.Enqueue(Backend.URank.User.UpdateUserScore, Table.uuid, TableName, KOBManager.Backend.GameData.KOBGameData.GetInDate(), data.GetRankingParam(trophy), result => {
                if (!result.IsSuccess())
                {
                    Debug.LogWarning("Trophy ranking sync failed: " + result);
                    Finish(UILocalization.Get("ui.6d62ac4489c0"));
                    return;
                }
                publishedKey = key;
                // A save may finish while a previous score is in flight.
                if (CurrentKey() != key) { Publish(); return; }
                Finish(null);
            });
        }

        private void Finish(string error)
        {
            publishing = false;
            var callbacks = waiting;
            waiting = null;
            callbacks?.Invoke(error);
        }

        public void GetPage(int offset, Action<string, List<RankUserItem>, int> completed)
        {
            if (Table == null) { completed(UILocalization.Get("ui.e72f02671a5c"), null, 0); return; }
            SendQueue.Enqueue(Backend.URank.User.GetRankList, Table.uuid, 50, offset, result => {
                if (!result.IsSuccess()) { completed(UILocalization.Get("ui.4bb69a7836f9"), null, 0); return; }
                List<RankUserItem> rows;
                int total;
                try
                {
                    var json = result.GetFlattenJSON();
                    total = int.Parse(json["totalCount"].ToString());
                    rows = new List<RankUserItem>();
                    foreach (JsonData row in json["rows"]) rows.Add(new RankUserItem(row, TableName, ProfileColumn));
                }
                catch (Exception e) { Debug.LogException(e); completed(UILocalization.Get("ui.741ef7c70dca"), null, 0); return; }
                completed(null, rows, total);
            });
        }

        public void GetMine(Action<string, RankUserItem> completed)
        {
            if (Table == null) { completed(UILocalization.Get("ui.e72f02671a5c"), null); return; }
            SendQueue.Enqueue(Backend.URank.User.GetMyRank, Table.uuid, result => {
                if (!result.IsSuccess())
                {
                    bool unranked = result.GetStatusCode() == "404" && result.GetMessage().Contains("userRank");
                    completed(unranked ? null : UILocalization.Get("ui.61303a764177"), null);
                    return;
                }
                RankUserItem mine;
                try { var rows = result.FlattenRows(); mine = rows.Count == 0 ? null : new RankUserItem(rows[0], TableName, ProfileColumn); }
                catch (Exception e) { Debug.LogException(e); completed(UILocalization.Get("ui.8327a06f92e4"), null); return; }
                completed(null, mine);
            });
        }
    }
}
