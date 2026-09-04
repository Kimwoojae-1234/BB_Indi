using UnityEngine;
using TMPro;
using System.Collections.Generic;
using static Popup_Promotion;
using DG.Tweening;

public class ResultPlayer : MonoBehaviour
{
    [SerializeField] private RectTransform CharOrigin;
    [SerializeField] private TextMeshProUGUI TrophyGainTxt;
    [SerializeField] private BallerTierSliderComp TierSliderComp;

    [SerializeField]
    private TextMeshProUGUI[] todayRecord;

    [SerializeField]
    private TextMeshProUGUI[] SeasonRecord;

    public RttsProgressReward ProgressReward { get; private set; }

    public void Set(RttsGameResult resultInfo)
    {
        if (resultInfo == null) return;
        ProgressReward = resultInfo.ProgressReward;

        int selectedBaller = KOBManager.MyInfo.GameData.ManageInfo.SelectBaller;
        KOBBaller baller = KOBManager.MyInfo.GameData.GetBaller(selectedBaller);
        if (baller == null) return;

        SetTodayRecord(resultInfo, selectedBaller);
        SetSeasonRecord(selectedBaller);

        foreach (Transform child in CharOrigin) Destroy(child.gameObject);
        GameObject ballerObject = KOBManager.Resource.LoadBaller(selectedBaller, CharOrigin);
        ShowStaticBallerImage(ballerObject);

        int gainTrophy = GetProgressGain(TR_RewardComp.TR_Type.TrophyRoad);
        int displayedTrophy = 0;
        if (KOBManager.MyInfo.GameData.GrowthInfo.Trophy >= KOBConstant.MAX_TROPHY)
        {
            TrophyGainTxt.text = "MAX";
        }
        else
        {
            TrophyGainTxt.text = "+0";
            if (gainTrophy != 0)
            {
                DOTween.To(() => displayedTrophy, value =>
                {
                    displayedTrophy = value;
                    TrophyGainTxt.text = string.Format("{0}{1}", displayedTrophy >= 0 ? "+" : string.Empty, displayedTrophy);
                }, gainTrophy, 1.5f);
            }
        }

        int gainFame = baller.baller_trophy >= KOBConstant.MAX_BALLER_FAME
            ? 0
            : GetProgressGain(TR_RewardComp.TR_Type.BallerReputation);
        float delay = gainTrophy == 0 ? 0f : 1.5f;
        TierSliderComp.SetGainProcess(baller, gainFame, delay, () =>
        {
            KOBManager.Baller.BallerFameUpgradeEvent(res =>
            {
                Intent intent = new Intent();
                intent["PromotionType"] = PromotionType.Baller_Reputation;
                intent.AddIntentData<UIPopup.OnClickAction>(UIPopup.ON_CLOSE, () =>
                {
                    KOBManager.Popup.OpenPopup<Popup_GameResult>();
                });
                KOBManager.Popup.OpenPopup<Popup_Promotion>().Set(intent);
                UI_LobbyRe.lastPlay = UI_LobbyRe.LastPlay.BallerTierUpgrade;
            });
        });
    }

    private static void ShowStaticBallerImage(GameObject ballerObject)
    {
        if (ballerObject == null) return;

        Transform image = ballerObject.transform.Find("Image");
        if (image != null) image.gameObject.SetActive(true);

        Transform videoPlayer = ballerObject.transform.Find("CharacterVideoPlayer");
        if (videoPlayer != null) videoPlayer.gameObject.SetActive(false);
    }

    private void SetTodayRecord(RttsGameResult resultInfo, int selectedBaller)
    {
        List<RttsPlayerGameRecord> myPlayers = null;
        if (resultInfo.FirstTeamIndex == RttsManager.MY_TEAM)
        {
            myPlayers = resultInfo.FirstTeamPlayers;
        }
        else if (resultInfo.SecondTeamIndex == RttsManager.MY_TEAM)
        {
            myPlayers = resultInfo.SecondTeamPlayers;
        }

        RttsPlayerGameRecord record = myPlayers?.Find(player => player.PlayerIndex == selectedBaller);
        SetRecordText(todayRecord, 0, GetRecordValue(record, RttsPlayerGameRecord.Hit).ToString());
        SetRecordText(todayRecord, 1, GetRecordValue(record, RttsPlayerGameRecord.HomeRun).ToString());
        SetRecordText(todayRecord, 2, GetRecordValue(record, RttsPlayerGameRecord.Rbi).ToString());
    }

    private void SetSeasonRecord(int selectedBaller)
    {
        MyRttsInfo rttsInfo = KOBManager.MyInfo.GameData.RttsInfo;
        int leaguePlayerIndex = (RttsManager.MY_TEAM * KOBConstant.PLAYER_RECORD_UNIT) + selectedBaller;
        if (rttsInfo?.LeaguePlayerRecord == null ||
            !rttsInfo.LeaguePlayerRecord.TryGetValue(leaguePlayerIndex, out BatterRecord record) ||
            record == null)
        {
            SetRecordText(SeasonRecord, 0, "N/A");
            SetRecordText(SeasonRecord, 1, ".000(N/A)");
            SetRecordText(SeasonRecord, 2, "0(N/A)");
            SetRecordText(SeasonRecord, 3, "0(N/A)");
            return;
        }

        // 경기 종료 저장이 완료된 뒤 호출되므로 오늘 경기까지 포함한 리그 리더보드를 다시 계산합니다.
        KOBManager.Rtts.InitLeagueLeaderInfo();

        int opsRank = GetLeagueRank(KOBManager.Rtts.OpsLeader, leaguePlayerIndex, true);
        int avgRank = GetLeagueRank(KOBManager.Rtts.AvgLeader, leaguePlayerIndex, true);
        int homeRunRank = GetLeagueRank(KOBManager.Rtts.HomerunLeader, leaguePlayerIndex, false);
        int rbiRank = GetLeagueRank(KOBManager.Rtts.RbiLeader, leaguePlayerIndex, false);

        int encodedAverage = 0;
        if (KOBManager.Rtts.AvgLeader.TryGetValue(leaguePlayerIndex, out int averageValue))
        {
            encodedAverage = averageValue >= KOBConstant.QPA_CONSTANT
                ? averageValue - KOBConstant.QPA_CONSTANT
                : averageValue;
        }

        SetRecordText(SeasonRecord, 0, FormatRank(opsRank));
        SetRecordText(SeasonRecord, 1,
            string.Format("{0}({1})", KOBTextUtil.SetAvgText(encodedAverage / 100), FormatRank(avgRank)));
        SetRecordText(SeasonRecord, 2, string.Format("{0}({1})", record.HR, FormatRank(homeRunRank)));
        SetRecordText(SeasonRecord, 3, string.Format("{0}({1})", record.RBI, FormatRank(rbiRank)));
    }

    private static int GetRecordValue(RttsPlayerGameRecord record, int recordIndex)
    {
        return record?.Record != null && recordIndex >= 0 && recordIndex < record.Record.Length
            ? record.Record[recordIndex]
            : 0;
    }

    private static int GetLeagueRank(
        Dictionary<int, int> leaderboard,
        int playerIndex,
        bool requiresQualifiedPlateAppearances)
    {
        if (leaderboard == null || !leaderboard.TryGetValue(playerIndex, out int playerValue)) return 0;
        if (requiresQualifiedPlateAppearances && playerValue < KOBConstant.QPA_CONSTANT) return 0;

        int rank = 1;
        foreach (KeyValuePair<int, int> entry in leaderboard)
        {
            if (entry.Key == playerIndex) return rank;
            rank++;
        }
        return 0;
    }

    private static string FormatRank(int rank)
    {
        return rank > 0 ? KOBTextUtil.ToOrdinal(rank) : "N/A";
    }

    private static void SetRecordText(TextMeshProUGUI[] fields, int index, string value)
    {
        if (fields == null || index < 0 || index >= fields.Length || fields[index] == null) return;
        fields[index].text = value;
    }

    public int GetProgressBefore(TR_RewardComp.TR_Type type)
    {
        return ProgressReward != null ? ProgressReward.GetBefore(type) : 0;
    }

    public int GetProgressGain(TR_RewardComp.TR_Type type)
    {
        return ProgressReward != null ? ProgressReward.GetGain(type) : 0;
    }

    public int GetProgressAfter(TR_RewardComp.TR_Type type)
    {
        return ProgressReward != null ? ProgressReward.GetAfter(type) : 0;
    }
}
