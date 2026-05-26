using BackEnd;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class BallerManager : MonoBehaviour
{
    /// <summary>
    /// 트로피 증가에 의한 티어 업글 체크
    /// </summary>
    /// <returns></returns>
    public bool TierUpgradeEvent(Action<TResultTierUpgrade> action = null)
    {
        int curMaxTrophy = KOBManager.MyInfo.GameData.GrowthInfo.MaxTrophy;
        int curTier = KOBManager.Backend.Chart.TrophyRoadData.GetCurrentTier(curMaxTrophy);
        Debug.Log("curTier : " + curTier);
        if (curTier > KOBManager.MyInfo.GameData.GrowthInfo.MyTier)
        {
            TRequestTierUpgrade req = new TRequestTierUpgrade()
            {
                Tier = curTier
            };
            KOBManager.DummyNetwork.SendPacket(req, (BackendReturnObject callback, TResponseBase response) =>
            {
                TResultTierUpgrade res = (TResultTierUpgrade)response;
                if (callback?.IsSuccess() == true && res?.isSuccess == true)
                {   
                    action?.Invoke(res);
                }
                else
                {
                    int ErrorCode = res.ErrorCode;
                    Debug.Log("에러코드 : " + ErrorCode);
                }
                KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
            });
            return true; //로비 연출 중단
        }
        return false;
    }


    /// <summary>
    /// 볼러 명성 증가에 의한 명성 업글 체크
    /// </summary>
    /// <returns></returns>
    public bool BallerFameUpgradeEvent(Action<TResultBallerFameUpgrade> action = null)
    {
        KOBBaller selectedBaller = KOBManager.MyInfo.GameData.GetSelectedBaller();
        int ballerFame = selectedBaller.baller_rank;
        int curFame = KOBManager.Backend.Chart.BallerTrophyRoadData.GetFameByTrophy(selectedBaller.baller_trophy);

        if (curFame > ballerFame)
        {
            int selected_idx = KOBManager.MyInfo.GameData.ManageInfo.SelectBaller;

            TRequestBallerFameUpgrade req = new TRequestBallerFameUpgrade()
            {
                baller_idx = selected_idx
            };
            KOBManager.DummyNetwork.SendPacket(req, (BackendReturnObject callback, TResponseBase response) =>
            {
                TResultBallerFameUpgrade res = (TResultBallerFameUpgrade)response;
                if (callback?.IsSuccess() == true && res?.isSuccess == true)
                {
                    //Debug.Log("티어 업글 성공시 계정 티어 업글 연출");                    
                    action?.Invoke(res);
                }
                else
                {
                    int ErrorCode = res.ErrorCode;
                    Debug.Log("에러코드 : " + ErrorCode);
                }
                KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
            });
            return true; //로비 연출 중단
        }
        return false;
    }


    /// <summary>
    /// 볼러를 라인업에 강제로 넣을 필요
    /// </summary>
    /// <param name="noCodtion"> 이값이 true이면 무조건, false이면 433맨만 대체</param>
    /// <returns></returns>
    public void Check_Baller_Put_Lineup(int baller_idx, bool noCodtion, Action action = null)
    {
        Dictionary<int, KOBLineupInfo> LineupList = KOBManager.MyInfo.GameData.DeckInfo.LineupList;

        bool exists = LineupList.Values.Any(x => x.idx == baller_idx); //라인업에 해당 볼러가 있는지 알아내는 여부
        if (exists == false)
        {
            //라인업짜고
            Dictionary<int, KOBLineupInfo> newLineup = SetAutoLineup_SeletedBaller(baller_idx);
            //배팅오더 짜고
            Dictionary<int, KOBLineupInfo> newLineup2 = SetAutoLineup_Order(newLineup);
            //수비위치 정함
            Dictionary<int, KOBLineupInfo> newLineup3 = SetAutoLineup_Position(newLineup2);

            TRequestChangeDeck req = new TRequestChangeDeck()
            {
                NewDeck = newLineup3,
                SelectIdx = baller_idx
            };

            KOBManager.DummyNetwork.SendPacket(req, (BackendReturnObject callback, TResponseBase response) =>
            {
                TResultChangeDeck res = (TResultChangeDeck)response;
                if (callback?.IsSuccess() == true && res?.isSuccess == true)
                {
                    //진행
                    action?.Invoke();
                }
                else
                {
                    int ErrorCode = res.ErrorCode;
                    Debug.Log("에러코드 : " + ErrorCode);
                }
                KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
            });
        }
        else
        {
            //볼러 있는 경우 그냥 진행
            action?.Invoke();
        }

        //KOBManager.MyInfo.GameData.

    }



    /// <summary>
    /// 주전 선수 세팅 -> 선택된 볼러를 라인업에 넣기
    /// </summary>
    /// <param name="selected_idx"></param>
    /// <returns></returns>
    public Dictionary<int, KOBLineupInfo> SetAutoLineup_SeletedBaller(int selected_idx)
    {    
        //딥카피
        //Dictionary<int, KOBLineupInfo> newLineup = KOBManager.MyInfo.GameData.DeckInfo.LineupList.ToDictionary(pair => pair.Key, pair => new KOBLineupInfo(pair.Value));        

        Dictionary<int, KOBBaller> list = KOBManager.MyInfo.GameData.PlayerInfo.BallerList; //현재 내 볼러 리스트
        List<KOBLineupInfo> newLineupList = new List<KOBLineupInfo>();

        //모든 선수를 newLineupList에 넣고
        int pos = 2;
        foreach (KeyValuePair<int, KOBBaller> baller in list)
        {
            KOBBaller player = baller.Value;
            int power = KOBBallerUtil.GetBallerOverallPower(player.idx, player.level);
            KOBLineupInfo info = new KOBLineupInfo()
            {
                idx = player.idx,
                position = pos,
                lineup_power = power,
            };
            newLineupList.Add(info);
            pos++;
        }
        newLineupList = newLineupList.OrderByDescending(x => x.lineup_power).ToList();
        if (newLineupList.Count > 8) newLineupList.RemoveRange(8, newLineupList.Count - 8); //8개 이상이면 제거

        if (selected_idx > 0) //선택된 플레이어 강제 추가 이슈
        {
            int existingIndex = newLineupList.FindIndex(item => item.idx == selected_idx);

            if (existingIndex == -1) // 리스트에 없으면
            {
                //마지막 선수의 포지션 체크
                KOBLineupInfo removeItem = newLineupList[newLineupList.Count - 1];
                int removePos = removeItem.position;

                //선택된 선수 라인업인포 만들기
                KOBBaller SelectedPlayer = KOBManager.MyInfo.GameData.GetBaller(selected_idx);
                int power = KOBBallerUtil.GetBallerOverallPower(SelectedPlayer.idx, SelectedPlayer.level);
                KOBLineupInfo AddItem = new KOBLineupInfo()
                {
                    idx = SelectedPlayer.idx,
                    position = removePos,
                    lineup_power = power,
                };

                //마지막거 빼고, 선택된 놈 넣기
                newLineupList.RemoveAt(newLineupList.Count - 1);
                newLineupList.Add(AddItem);
            }
        }

        int order = 1;
        Dictionary<int, KOBLineupInfo> newLineup = new Dictionary<int, KOBLineupInfo>();
        for(int i = 0; i < newLineupList.Count; i++)
        {
            newLineup.Add(order, newLineupList[i]);
            order++;
        }


        //--> 여기까지는 선수만 바꿈
        return newLineup;
    }


    /// <summary>
    /// 타순 세팅
    /// </summary>
    /// <param name="_cloneList"></param>
    /// <returns></returns>
    public Dictionary<int, KOBLineupInfo> SetAutoLineup_Order(Dictionary<int, KOBLineupInfo> _cloneList)
    {
        Dictionary<int, KOBLineupInfo> Lineup = null;
        if (_cloneList == null)
        {
            Lineup = KOBManager.MyInfo.GameData.DeckInfo.LineupList.ToDictionary(pair => pair.Key, pair => new KOBLineupInfo(pair.Value));            
        }
        else
        {
            Lineup = _cloneList;
        }

        int selected_idx = KOBManager.MyInfo.GameData.ManageInfo.SelectBaller;

        //배팅 파워 계산
        foreach (KeyValuePair<int, KOBLineupInfo> lineup in Lineup)
        {
            KOBBaller cur = KOBManager.MyInfo.GameData.GetBaller(lineup.Value.idx);
            int lineup_power = KOBBallerUtil.GetBallerBattingPower(lineup.Value.idx, cur.level);
            if(lineup.Key == selected_idx)
            {
                lineup_power = (lineup_power * 110) / 100; //선택 선수인 경우 10% 추가 배팅 바워로 타순 정함
            }
            lineup.Value.lineup_power = lineup_power;
        }


        var sortedValues = Lineup
                            .OrderByDescending(x => x.Value.lineup_power) // lineup_power 큰 순서로 정렬
                            .Select(x => x.Value)
                            .ToList();

        // 원하는 순서의 키
        int[] targetKeys = { 3, 2, 1, 4, 5, 6, 7, 8 };

        // 새 Dictionary 생성
        Dictionary<int, KOBLineupInfo> newLineup = new Dictionary<int, KOBLineupInfo>();

        for (int i = 0; i < targetKeys.Length; i++)
        {
            newLineup[targetKeys[i]] = sortedValues[i];
        }

        //키값 기준 오름차순 정렬
        newLineup = newLineup
                   .OrderBy(kvp => kvp.Key)
                   .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);


        return newLineup;
    }

    /// <summary>
    /// 수비 위치 세팅
    /// </summary>
    /// <param name="_cloneList"></param>
    /// <returns></returns>
    public Dictionary<int, KOBLineupInfo> SetAutoLineup_Position(Dictionary<int, KOBLineupInfo> _cloneList)
    {
        Dictionary<int, KOBLineupInfo> Lineup = null;
        if (_cloneList == null)
        {
            Lineup = KOBManager.MyInfo.GameData.DeckInfo.LineupList.ToDictionary(pair => pair.Key, pair => new KOBLineupInfo(pair.Value));
        }
        else
        {
            Lineup = _cloneList;
        }

        //수비 파워 계산
        foreach (KeyValuePair<int, KOBLineupInfo> lineup in Lineup)
        {
            KOBBaller cur = KOBManager.MyInfo.GameData.GetBaller(lineup.Value.idx);
            int lineup_power = KOBBallerUtil.GetBallerFieldingPower(lineup.Value.idx, cur.level); 
            lineup.Value.lineup_power = lineup_power;
        }

        //투수 포지션은 계산 안함
        //bool[] isPositionOn = new bool[9] { true, false, false, false, false, false, false, false, false };
        for (int i = 0; i < isPositionOn.Length; i++) isPositionOn[i] = false;
        List<int> UnDeterminePos = new List<int>();
        UnDeterminePos.Clear();

        Dictionary<int, KOBLineupInfo> Lineup2 = Lineup
            .OrderByDescending(kvp => kvp.Value.lineup_power) // lineup_power 큰 순서로 정렬
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        foreach (KeyValuePair<int, KOBLineupInfo> lineup in Lineup2)
        {
            int key = lineup.Key;
            KOBLineupInfo lineupInfo = Lineup[key]; //여기에 값을 넣어주면 정식값이 됨
            CharacterData charData = KOBManager.Backend.Chart.CharacterData.GetData(lineupInfo.idx);
            KOBPosition realPos = charData.position;
            int posIndex = GetRealPos(realPos);
            if (posIndex == -1)
            {
                //없는 경우 UnDeterminPos에 리스트 포함 하고 -1 값 세팅
                UnDeterminePos.Add(key);
            }
            lineupInfo.position = posIndex;
        }

        if(UnDeterminePos.Count > 0) //정해지지 않은 포지션이 있는 경우
        {
            for(int i=0;i<UnDeterminePos.Count;i++)
            {
                int key = UnDeterminePos[i];
                KOBLineupInfo lineupInfo = Lineup[key]; //여기에 값을 넣어주면 정식값이 됨
                lineupInfo.position = getRemainPos();
            }
        }


        return Lineup;
    }

    static bool[] isPositionOn = new bool[9] { true, false, false, false, false, false, false, false, false };

    static int GetRealPos(KOBPosition realPos)
    {
        int position = -1;
        if (realPos > KOBPosition.Pitcher && realPos <= KOBPosition.Right)
        {
            //한가지 포지션 고정인 경우
            int _realPos = (int)realPos;
            if (isPositionOn[_realPos - 1] == false)
            {
                position = _realPos;
                isPositionOn[_realPos - 1] = true;
            }
        }       
        else if(realPos == KOBPosition.InfieldUtil) //내야 유틸
        {
            int[] utilPos = new int[4] { 6, 4, 5, 3 };
            position = getUtilPos(utilPos);
        }
        else if (realPos == KOBPosition.InfieldCatcherUtil) //포수 포함 내야 유틸
        {
            int[] utilPos = new int[5] { 2, 6, 4, 5, 3 };
            position = getUtilPos(utilPos);
        }
        else if (realPos == KOBPosition.OutfieldUtil) //외야 유틸
        {
            int[] utilPos = new int[3] { 8, 9, 7 };
            position = getUtilPos(utilPos);
        }
        else if (realPos == KOBPosition.OutfieldCatcherUtil) //캐처 포함 외야 유틸
        {
            int[] utilPos = new int[4] { 2, 8, 9, 7 };
            position = getUtilPos(utilPos);
        }
        else if (realPos == KOBPosition.InOutField) //내외야 전부 가능
        {
            int[] utilPos = new int[7] { 6, 8, 4, 5, 9, 7, 3 };
            position = getUtilPos(utilPos);
        }
        else if (realPos == KOBPosition.AllRounder) //전부 가능
        {
            int[] utilPos = new int[8] { 2, 6, 8, 4, 5, 9, 7, 3 };
            position = getUtilPos(utilPos);
        }

        return position;
    }

    static int getUtilPos(int[] pos)
    {
        for (int i = 0; i < pos.Length; i++)
        {
            int _realPos = pos[i];
            if (isPositionOn[_realPos - 1] == false)
            {                
                isPositionOn[_realPos - 1] = true;
                return _realPos;
            }
        }
        return -1;
    }

    static int getRemainPos()
    {
        for (int i = 1; i < isPositionOn.Length; i++)
        {
            if(isPositionOn[i] == false)
            {
                isPositionOn[i] = true;
                return (i + 1);
            }
        }

        Debug.LogError("여기에 이르르면 절대 안됨!!");
        return -1; //여기에 오면 절대 안되
    }


}
