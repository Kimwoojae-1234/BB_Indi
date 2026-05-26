using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UI_CardBase : MonoBehaviour
{
    [SerializeField]
    public UISprite spriteBackground; // Card 백그라운드
    private UISpriteAnimation spriteani;
    [SerializeField]
    public UISprite legendLine;
    // 강화 파티클 애니메이션
    [SerializeField]
    public UISprite spriteTeamLogo;   // 팀 로고
    [SerializeField]
    public UITexture texturePlayer;     // 선수 사진
    [SerializeField]
    public UILabel labelPlayerName;     // 선수 이름
    [SerializeField]
    public UISprite spritePlayerName_Bg;
    [SerializeField]
    public UILabel labelPlayerLevel;    // 선수 레벨
    [SerializeField]
    public UISprite spritePlayerLevel_BG;
    [SerializeField]
    public UISprite spritePlayerPosition;   // 선수 포지션 스프라이트
    [SerializeField]
    public UISprite spritePlayerPosition_bg;
    [SerializeField]
    public UISprite spriteReinforce;    // 선수 강화 표시 스프라이트
    [SerializeField]
    public UISprite spriteReinforce_bg;    // 선수 강화 표시 스프라이트
    [SerializeField]
    public UILabel labelPower;//선수 파워 라벨
    [SerializeField]
    public UISprite spriteLock; //잠금표시 스프라이트
    [SerializeField]
    public UISprite Grade_sprite;   //등급 스프라이트 금색 , 은색 별
    [SerializeField]
    public UILabel Grade_label;     //등급 숫자 라벨
    [SerializeField]
    public UISprite spriteNewMark;
    [SerializeField]
    public UISprite new_get_mark;
    [SerializeField]
    private UISprite goldenGlove_sprite;
    protected CardData cardData;

    protected UIFont overallFont;

    /// <summary>
    /// 플레이어가 보유한 카드 정보로 파싱
    /// </summary>
    /// <param name="net_cardInfo"></param>
    public virtual void SetPlayerCard(CardData card_data)
    {
        
    }

    /// <summary>
    /// 게임데이타에 존재하는 정보로 파싱
    /// </summary>
    /// <param name="cardInfo"></param>
    public virtual void SetPlayerCard(card cardInfo)
    {
        
    }

    public void SetPlayerCardEmpty()
    {
        int sprite_num = GetGradeSpriteType(3);

        this.spriteBackground.spriteName = string.Format("bg_{0}grade", sprite_num);
        this.Grade_sprite.spriteName = sprite_num <= 4 ? "star_1" : "star_2";
        this.Grade_label.text = 3.ToString();
        this.labelPower.text = 97.ToString();
        this.spritePlayerPosition.spriteName = DefineEnum.EPosition.B1.ToString();
        bool reinforce_view = true; //최대스킬슬롯이 0이면 강화 안됨으로 강화수치 이미지 보여주지 않음
        this.spriteReinforce.enabled = reinforce_view;
        this.spriteReinforce_bg.enabled = reinforce_view;
        this.labelPlayerLevel.text = 1.ToString();
        this.spritePlayerLevel_BG.spriteName ="lv_bg";
        this.spriteLock.enabled = false;
        this.labelPlayerName.text = "테스트";
        this.spritePlayerName_Bg.spriteName ="name_bg";
        this.spriteNewMark.enabled = false;
        this.spriteTeamLogo.spriteName = WebConnector.TeamCode.SAMSUNG.ToString();
        // DISABLED_MGRS: this.texturePlayer.mainTexture = Mgrs.DataLoad.LoadPlayerTexture(UserData.ETeamCode.SAMSUNG, 20018, WebConnector.PlayerType.Hitter);
    }

    public int GetGradeSpriteType(int grade)
    {
        int grade_num = 0;
        switch (grade)
        {
            case 1:
            case 2:
                grade_num = 2;
                break;
            case 3:
                grade_num = 3;
                break;
            case 4:
                grade_num = 4;
                break;
            case 5:
                grade_num = 5;
                break;
            case 6:
                grade_num = 6;
                break;
            case 7:
                grade_num = 7;
                break;
            case 8:
            case 9:
                break;
            case 10:
                break;
        }
        return grade_num;
    }

    /// <summary>
    /// NewMark를 세팅하는 함수
    /// </summary>
    /// <param name="view"> true:보여줌, false:안보여줌</param>
    public void SetNewMark(bool view)
    {
        this.new_get_mark.enabled = view;
    }

    public int GetCardBGDepth()
    {
        return this.spriteBackground.depth;
    }
    
    public void SetCardDepth(int frontdepth)
    {
        int depth_value = frontdepth;
        this.Grade_label.depth = depth_value;
        this.Grade_sprite.depth = depth_value - 1;
        this.spriteLock.depth = depth_value - 1;
        this.spriteReinforce.depth = depth_value;
        this.spriteReinforce_bg.depth = depth_value - 1;
        this.labelPlayerLevel.depth = depth_value;
        this.spritePlayerLevel_BG.depth = depth_value - 1;
        this.labelPower.depth = depth_value - 1;
        this.spritePlayerPosition.depth = depth_value-1;
        this.spritePlayerName_Bg.depth = depth_value - 1;
        this.texturePlayer.depth = depth_value - 2;
        this.spriteNewMark.depth = depth_value - 1;
        this.spritePlayerName_Bg.depth = depth_value - 3;
        this.spriteTeamLogo.depth = depth_value - 2;
        this.labelPlayerName.depth = depth_value - 2;
        this.spriteBackground.depth = depth_value - 3;
        this.spritePlayerPosition_bg.depth = depth_value - 1;
    }
    
    public void PlusDepth(int plus_value)
    {
        this.Grade_label.depth = this.Grade_label.depth + plus_value;
        this.Grade_sprite.depth = this.Grade_sprite.depth + plus_value;
        this.spriteLock.depth = this.spriteLock.depth + plus_value;
        this.spriteReinforce.depth = this.spriteReinforce.depth + plus_value;
        this.spriteReinforce_bg.depth = this.spriteReinforce_bg.depth + plus_value;
        this.labelPlayerLevel.depth = this.labelPlayerLevel.depth + plus_value;
        this.spritePlayerLevel_BG.depth = this.spritePlayerLevel_BG.depth + plus_value;
        this.labelPower.depth = this.labelPower.depth + plus_value;
        this.spritePlayerPosition.depth = this.spritePlayerPosition.depth + plus_value;
        this.spritePlayerName_Bg.depth = this.spritePlayerName_Bg.depth + plus_value;
        this.texturePlayer.depth = this.texturePlayer.depth + plus_value;
        this.spriteNewMark.depth = this.spriteNewMark.depth + plus_value;
        this.spritePlayerName_Bg.depth = this.spritePlayerName_Bg.depth + plus_value;
        this.spriteTeamLogo.depth = this.spritePlayerName_Bg.depth + 1;
        this.labelPlayerName.depth = this.spritePlayerName_Bg.depth + 1;
        this.spriteBackground.depth = this.spriteBackground.depth + plus_value;
        this.spritePlayerPosition_bg.depth = this.spritePlayerPosition_bg.depth + plus_value;
        this.goldenGlove_sprite.depth = this.goldenGlove_sprite.depth + plus_value;
        this.new_get_mark.gameObject.SetActive(false);
    }

    public CardData GetCardData()
    {
        return this.cardData;
    }

    public void SetGrowAbleMark(bool isView)
    {
        spriteNewMark.enabled = isView;
    }

    public void SetReinforce(card cardDBInfo, int skill_count)
    {
        
    }
}
