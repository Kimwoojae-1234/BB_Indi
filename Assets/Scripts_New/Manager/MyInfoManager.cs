using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;

public class MyInfoManager : MonoBehaviour
{
    private UserInfo backendUserInfo = null;

    //이놈을 쓰는 이유 -> 서버상에 저장된 SelectBaller를 바꾸지 않은 상태에서 유저의 선택에 의한 Baller선택 정보가 필요할 떄가 있다 UI_Baller UI에서 필요 그러므로 필요하고
    //로비 진입시 다시 서버상의 SelectBaller와 동기화시킨다
    //UI_Baller를 벗어나면 이것을 쓰지 않는다
    public int UISelectedBaller { get; private set; } = KOBConstant.FIRSTBALLER;

    public void SetUISelectedBaller(int idx)
    {
        Debug.Log("SetUISelectedBaller : " + idx);
        UISelectedBaller = idx;
    }

    public UserInfo BackkendUserInfo
    {
        get
        {
            if (backendUserInfo == null)
            {
                var bro = Backend.BMember.GetUserInfo();
                if (!bro.IsSuccess())
                {
                    Debug.LogError("에러가 발생했습니다 : " + bro.ToString());
                    return null;
                }
                string json = bro.GetReturnValuetoJSON()["row"].ToJson();
                backendUserInfo = JsonHelper.DeserializeObject<UserInfo>(json);
            }
            return backendUserInfo;
        }
    }

    public void InitUserInfo()
    {
        backendUserInfo = null;
    }

    private KOBGameData gameData = null;
    public KOBGameData  GameData
    {
        get
        {
            if (gameData == null)
            {
                gameData = KOBManager.Backend.GameData.KOBGameData.DeepCopy();
            }
            return gameData;
        }
    }

    public void UserInfoUpdate()
    {
        gameData = KOBManager.Backend.GameData.KOBGameData.DeepCopy();
    }

    public KOBGameData UserInfoRevert()
    {
        return gameData.DeepCopy();
    }



    //테스트용


    public DeckData MyDeckInfo
    {
        get;
        private set;
    }

    //실제 게임에서는 안쓰고 Test에서만 씀
    public DeckData OppDeckInfo
    {
        get;
        private set;
    }


    public void MakeMyTempDeck(int [] hitter, int [] pitcher, int Rotation)
    {
        Debug.Log("MakeMyTempDeck");
        DeckData data = new DeckData();

        

        //타자임시
        for (int i = 0; i < hitter.Length; i++)
        {
            data.hitter.Add(i, hitter[i]);
            data.hitterLevel.Add(hitter[i], 7);
            data.PosInfo.Add(hitter[i], PositionInfo.C + i);
        }

        //투수임시
        for (int i = 0; i < pitcher.Length; i++)
        {
            data.pitcher.Add(i, pitcher[i]);
            data.pitcherLevel.Add(pitcher[i], 7);
            data.RotaionInfo.Add(pitcher[i], (RotationInfo)i);
            data.PosInfo.Add(pitcher[i], PositionInfo.SP);
        }

        //로테이션 임시
        data.PitcherRotation = Rotation;

        MyDeckInfo = data;
    }

    public void MakeBotTempDeck(int[] hitter, int[] pitcher, int Rotation)
    {
        Debug.Log("MakeBotTempDeck");
        DeckData data = new DeckData();


        //타자임시
        for (int i = 0; i < hitter.Length; i++)
        {
            data.hitter.Add(i, hitter[i]);
            data.hitterLevel.Add(hitter[i], 7);
            data.PosInfo.Add(hitter[i], PositionInfo.C + i);
        }

        //투수임시
        for (int i = 0; i < pitcher.Length; i++)
        {
            data.pitcher.Add(i, pitcher[i]);
            data.pitcherLevel.Add(pitcher[i], 7);
            data.RotaionInfo.Add(pitcher[i], (RotationInfo)i);
            data.PosInfo.Add(pitcher[i], PositionInfo.SP);
        }

        //로테이션 임시
        data.PitcherRotation = Rotation;


        OppDeckInfo = data;
    }

}
