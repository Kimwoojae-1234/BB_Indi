using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SQLite4Unity3d;
using System;
using Utils;

public class player
{
    [PrimaryKey, AutoIncrement]
    public int player_id { get; private set; }
    public string name { get; private set; }
    public string desc { get; private set; }
}

public class card
{
    [PrimaryKey, AutoIncrement]
    //카드아이디
    public int card_id { get; private set; }
    //선수아이디
    public int player_id { get; private set; }
    //선수명
    public string name { get; private set; }
    //년도
    public int year { get; private set; }
    //카드타입
    public string type { get { return this.type; } private set { this.eCardType = (WebConnector.CardType)Enum.Parse(typeof(WebConnector.CardType), value); } }
    public WebConnector.CardType eCardType;
    //구단
    
    //고유포지션
    public string position { get { return this.position; } private set { this.ePosition = (DefineEnum.EPosition)Enum.Parse(typeof(DefineEnum.EPosition), value); } }
    public DefineEnum.EPosition ePosition;
    //골들글러브수상자
    public int gga { get; private set; }
    //시작등급
    public int ori_grade { get; private set; }
    //최대스킬슬롯
    public int max_skill_cnt { get; private set; }
    //사용손
    public string main_hander { get { return this.main_hander; } private set { this.eMainHander = (DefineEnum.EMainHander)Enum.Parse(typeof(DefineEnum.EMainHander), value); } }
    public DefineEnum.EMainHander eMainHander;
    //피부색
    public string skin
    {
        get
        {
            return this.skin;
        }

        private set
        {
            this.PlayerColorType = (DefineEnum.EPlayerColor)Enum.Parse(typeof(DefineEnum.EPlayerColor), value);
        }
    }
    //체형
    public string body
    {
        get
        {
            return this.body;
        }

        private set
        {
            this.PlayerBodyType = (DefineEnum.EPlayerBody)Enum.Parse(typeof(DefineEnum.EPlayerBody),value);
        }
    }
    //체형 Enum
    public DefineEnum.EPlayerBody PlayerBodyType;
    //피부색 Enum
    public DefineEnum.EPlayerColor PlayerColorType;
    //얼굴
    public int face { get; private set; }
    public int total_power { get; private set; }

    //체력
    public int pab_1 { get; private set; }
    //직구
    public int pab_2 { get; private set; }
    //체인지업
    public int pab_3 { get; private set; }
    //슬라이더
    public int pab_4 { get; private set; }
    //커브
    public int pab_5 { get; private set; }
    //포크
    public int pab_6 { get; private set; }
    //직구구종
    public string pff { get; private set; }
    //체인지업구종
    public string pcu { get; private set; }
    //슬라이더구종
    public string psd { get; private set; }
    //커브구종
    public string pcv { get; private set; }
    //포크구종
    public string pfb { get; private set; }
    //파워
    public int hab_1 { get; private set; }
    //컨택
    public int hab_2 { get; private set; }
    //선구
    public int hab_3 { get; private set; }
    //주력
    public int hab_4 { get; private set; }
    //송구
    public int hab_5 { get; private set; }
    //수비
    public int hab_6 { get; private set; }
    //타구각
    public int hab_7 { get; private set; }
    //스킬풀
    public string latent_skills { get; private set; }

    public string GetPlayerYearName()
    {
        return string.Format("{0}{1}", this.name, this.year.ToString().Substring(2));
    }
    
    private WebConnector.PlayerType playertype;
    public WebConnector.PlayerType PlayerType
    {
        get
        {
            this.playertype = CardUtils.detectPlayerTypeFrom(this.card_id);
            return this.playertype;
        }
    }

    public int[] GetSkillPool()
    {
        string[] skills = latent_skills.Split('&');
        int[] return_skills = new int[skills.Length];
        for(int i = 0; i<return_skills.Length; i++)
        {
            int.TryParse(skills[i], out return_skills[i]);
        }
        return return_skills;
    }
}

public class card_exp_demand
{
    //레벨
    public int level { get; private set; }
    //1등급
    public int grade1 { get; private set; }
    //2등급
    public int grade2 { get; private set; }
    //3등급
    public int grade3 { get; private set; }
    //4등급
    public int grade4 { get; private set; }
    //5등급
    public int grade5 { get; private set; }
    //6등급
    public int grade6 { get; private set; }
    //7등급
    public int grade7 { get; private set; }
    //8등급
    public int grade8 { get; private set; }
    //9등급
    public int grade9 { get; private set; }
    //10등급
    public int grade10 { get; private set; }
}

public class card_exp_return
{
    //레벨
    public int level { get; private set; }
    //1등급
    public int grade1 { get; private set; }
    //2등급
    public int grade2 { get; private set; }
    //3등급
    public int grade3 { get; private set; }
    //4등급
    public int grade4 { get; private set; }
    //5등급
    public int grade5 { get; private set; }
    //6등급
    public int grade6 { get; private set; }
    //7등급
    public int grade7 { get; private set; }
    //8등급
    public int grade8 { get; private set; }
    //9등급
    public int grade9 { get; private set; }
    //10등급
    public int grade10 { get; private set; }
}

public class card_levelup_cost
{
    public int grade { get; private set; }

    public int gold { get; private set; }
}

public class card_reinforce_cost
{
    public int grade { get; private set; }

    public int gold { get; private set; }
}

public class card_gradeup_cost
{
    public int grade { get; private set; }

    public int gold { get; private set; }
}

public class card_levelup_jacpot_prob
{
    [PrimaryKey, AutoIncrement]
    public int type { get; private set; }

    public float up_rate { get; private set; }
}

public class CardAbility
{
    // 내 능력치 이름
    public int ablilityName;
    // 내 능력치
    public int ablilityValue;

    public CardAbility(string ability)
    {
        string[] abSplit = ability.Split('=');
        this.ablilityName  = System.Convert.ToInt32(abSplit[0]);
        this.ablilityValue = System.Convert.ToInt32(abSplit[1]);
    }
}



public class card_ability
{
    [PrimaryKey, AutoIncrement]
    public int id { get; private set; }
    public string name { get; private set; }
    public string name_e { get; private set; }
    public string ptich_type { get; private set; }

    private DefineEnum.ECardAbility ecardability;

    public DefineEnum.ECardAbility eCardAbility
    {
        get
        {
            if (ecardability == 0)
            {
                switch(id)
                {
                    case 11:
                    case 12:
                    case 13:
                        ecardability = DefineEnum.ECardAbility.FourSeam_FourSeam;
                        break;
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                    case 18:
                        ecardability = DefineEnum.ECardAbility.Curve_Curve;
                        break;
                    case 19:
                    case 20:
                    case 21:
                    case 22:
                    case 23:
                        ecardability = DefineEnum.ECardAbility.ChangeUp_ChangeUp;
                        break;
                    case 24:
                    case 25:
                    case 26:
                    case 27:
                    case 28:
                        ecardability = DefineEnum.ECardAbility.Slider_Slider;
                        break;
                    case 29:
                    case 30:
                    case 31:
                    case 32:
                        ecardability = DefineEnum.ECardAbility.Fork_Fork;
                        break;
                }
            }
            return ecardability;
        }
    }
}

public class level_card
{
    [PrimaryKey, AutoIncrement]
    public int level { get; private set; }
    public int grade { get; private set; }
    public int exp { get; private set; }
}

public class level_team
{
    [PrimaryKey, AutoIncrement]
    public int level { get; private set; }
    public int exp { get; private set; }
}

public class handertype
{
    [PrimaryKey, AutoIncrement]
    public int id { get; private set; }
    public string name { get; private set; }
}

public class lineup
{
    [PrimaryKey, AutoIncrement]
    public int id { get; private set; }
    public string name { get; private set; }
    public string abbr { get; private set; }
}

public class lineup_pitcher
{
    [PrimaryKey, AutoIncrement]
    public int id { get; private set; }
    public string name { get; private set; }
}

public class position
{
    [PrimaryKey, AutoIncrement]
    public int id { get; private set; }
    public string name { get; private set; }
    public string abbr { get; private set; }
}

public class team
{
    [PrimaryKey, AutoIncrement]
    public string id { get { return this.id; } private set { this.eId = (WebConnector.TeamCode)Enum.Parse(typeof(WebConnector.TeamCode), value); } }
    public WebConnector.TeamCode eId;

    public string name { get; private set; }
    public string name2 { get; private set; }
    public string full_name { get; private set; }
    public string stadium_name { get; private set; }
}

public class init_team
{
    [PrimaryKey]
    public string team { get { return this.team; } private set { this.eTeam = (WebConnector.TeamCode)Enum.Parse(typeof(WebConnector.TeamCode), value); } }
    public WebConnector.TeamCode eTeam;
    public int card_id { get; private set; }
    public string lineup { get { return this.lineup; } private set { this.eLineup = (WebConnector.Lineup)Enum.Parse(typeof(WebConnector.Lineup), value); } }
    public WebConnector.Lineup eLineup;
    public int odr { get; private set; }
    public int reinforce_lev { get; private set; }

}

public class game_const
{
    [PrimaryKey, AutoIncrement]
    public string name { get; private set; }
    public float value { get; private set; }
}

public class season_league
{
    [PrimaryKey, AutoIncrement]
    public int level { get; private set; }
    public string name { get; private set; }
}

public class season_rwd_item
{
    [PrimaryKey, AutoIncrement]
    public int league_lev { get; private set; }
    public int item1 { get; private set; }
    public int item2 { get; private set; }
    public int item3 { get; private set; }
    public int item4 { get; private set; }
    public int item5 { get; private set; }
}

public class toast_message
{
    [PrimaryKey, AutoIncrement]
    public int id { get; private set; }
    public string message { get; private set; }
    public int iconType { get; private set; }
}

public class ok_message
{
    [PrimaryKey, AutoIncrement]
    public int id { get; private set; }
    public string message { get; private set; }
    public string ok_btn { get; private set; }
    public int type { get; private set; }
}

public class titleok_message
{
    [PrimaryKey, AutoIncrement]
    public int id { get; private set; }
    public string title { get; private set; }
    public string message { get; private set; }
    public string ok_btn { get; private set; }
    public int type { get; private set; }
}

public class yn_message
{
    [PrimaryKey, AutoIncrement]
    public int id { get; private set; }
    public string message { get; private set; }
    public string yes_btn { get; private set; }
    public string no_btn { get; private set; }
    public int type { get; private set; }
}

[Obsolete("삭제됨")]
public class setdeck
{
    [PrimaryKey, AutoIncrement]
    public int id { get; private set; }
    public string grade { get; private set; }
    public string name { get; private set; }
    public string type { get; private set; }
    public int type_code { get; private set; }
    public int slot { get; private set; }
    public string cond { get; private set; }
    public int need_cnt { get; private set; }
    public string effect { get; private set; }
    public int eff_val { get; private set; }
    public string eff_target { get; private set; }

    //private Utils.Setdeck.SetdeckNode setdecknode;
    //public Utils.Setdeck.SetdeckNode SetDeckNode
    //{
    //    get
    //    {
    //        if (setdecknode == null)
    //        {
    //            setdecknode = Mgrs.GameData.IGameDB_FindSetDeckNode(this.id);
    //        }
    //        return setdecknode;
    //    }
    //}
}

public class item
{
    [PrimaryKey, AutoIncrement]
    public int item_id { get; private set; }
    public string categ { get { return categ; } private set { eCatg = (DefineEnum.ItemCategory)Enum.Parse(typeof(DefineEnum.ItemCategory), value); } }
    public DefineEnum.ItemCategory eCatg;
    public string type { get { return type; } private set { eType = (WebConnector.ItemType)Enum.Parse(typeof(WebConnector.ItemType), value); } }
    public WebConnector.ItemType eType;
    public string name { get; private set; }
    public string desc { get; private set; }
    public int sell_price { get; private set; }
}

public class item_type
{
    [PrimaryKey, AutoIncrement]
    public int id { get; private set; }
    public string name { get; private set; }
    
}

public class live_play_league
{
    [PrimaryKey, AutoIncrement]
    public int league { get; private set; }
    public string name { get; private set; }
    public int min_point { get; private set; }
    public int rwd_condition { get; private set; }
    public int mileage_point { get; private set; }
    public int match_rwd_coin { get; private set; }
    public int weekly_rwd_coin { get; private set; }
    public int weekly_rwd_ruby { get; private set; }
}

public class live_play_mileage_reward
{
    [PrimaryKey, AutoIncrement]
    public int grade { get; private set; }
    public int point { get; private set; }
}

public class live_play_weekly_rank_reward
{
    [PrimaryKey, AutoIncrement]
    public int ranking { get; private set; }
    public int ruby { get; private set; }
}

public class coupon
{
    [PrimaryKey, AutoIncrement]
    public int coupon_id { get; private set; }
    public string name { get; private set; }
    public int target_money_type { get; private set; }
    public int dc_rate { get; private set; }
}

public class recharge_type
{
    [PrimaryKey, AutoIncrement]
    public int id { get; private set; }
    public string name { get; private set; }
}

public class card_gradeup
{
    [PrimaryKey, AutoIncrement]
    public int grade { get; private set; }
    public int cost_normal { get; private set; }
    public int cost_rare { get; private set; }
    public int cost_hero { get; private set; }
    public int cost_legend { get; private set; }
}

public class mission_main
{
    [PrimaryKey, AutoIncrement]
    public int id { get; private set; }
    public string subject { get; private set; }
    public string desc { get; private set; }
    public int init_goal { get; private set; }
    public int direct_link { get; private set; }
}

public class mission_daily
{
    [PrimaryKey, AutoIncrement]
    public int id { get; private set; }
    public string subject { get; private set; }
    public string desc { get; private set; }
    public int goal { get; private set;}
    public int rwd_item { get; private set; }
    public int direct_link { get; private set; }
}

public class mission_main_step
{
    [PrimaryKey, AutoIncrement]
    public int id { get; private set; }
    public int goal { get; private set; }
    public int rwd_gold { get; private set; }
    public int rwd_item { get; private set; }
}

/// <summary>
/// ai_team이지만 팀창단할때 위와 같이 주어진다. 
/// </summary>
public class ai_team
{
    [PrimaryKey, AutoIncrement]
    public int team_id { get; private set; }
    public int ai_seq { get; private set; }
    public int card_id { get; private set; }
    public int lineup { get; private set; }
    public int odr { get; private set; }
}

public class item_mapping
{
    [PrimaryKey, AutoIncrement]
    public int id { get;  private set;}
    public string name { get; private set; }
    public string image_small { get; private set; }
    public string image_large { get; private set; }
}



public class skill
{
    //스킬 아이디
    [PrimaryKey, AutoIncrement]
    public int id { get; private set; }
    //스킬 이름
    public string name { get; private set; }
    //스킬 설명
    public string desc { get; private set; }
    //스킬 설명(스킬변경창에서)
    public string unknowndesc { get; private set; }

    //스킬 랭크별 발동률1
    public int invoke_rate1 {get; private set;}
    //스킬 랭크별 발동률2
    public int invoke_rate2 { get; private set; }
    //스킬 랭크별 발동률3
    public int invoke_rate3 { get; private set; }
    //스킬 랭크별 발동률4
    public int invoke_rate4 { get; private set; }
    //스킬 랭크별 발동률5
    public int invoke_rate5 { get; private set; }
    //스킬 랭크별 범위율1
    public int scope_rate1 { get; private set; }
    //스킬 랭크별 범위율2
    public int scope_rate2 { get; private set; }
    //스킬 랭크별 범위율3
    public int scope_rate3 { get; private set; }
    //스킬 랭크별 범위율4
    public int scope_rate4 { get; private set; }
    //스킬 랭크별 범위율5
    public int scope_rate5 { get; private set; }
    //스킬 랭크별 수치1
    public int value1 { get; private set; }
    //스킬 랭크별 수치2
    public int value2 { get; private set; }
    //스킬 랭크별 수치3
    public int value3 { get; private set; }
    //스킬 랭크별 수치4
    public int value4 { get; private set; }
    //스킬 랭크별 수치5
    public int value5 { get; private set; }

    public string invoke_name { get; private set; }

    public string scope_name { get; private set; }
    public string value_name { get; private set; }
    
}

public class trophy_info
{
    public int ori_grade { get; private set; }

    public int trophy_id { get; private set; }

    public int trophy_count { get; private set; }

    public int combine_gold { get; private set; }

    public int combine_count { get; private set; }
}



public class skillEffectDesc
{
    public int effect_id;
    public string effect_name;
    public int effect_level;
    public string effect_desc;
}

public class sprite_name
{
    public int id {get; private set;}
    public string type { get; private set; }
    public string name { get; private set; }
}

public class walkoff_play_info
{
    [PrimaryKey]
    public int arena_no { get; private set; }

    public int open_lev { get; private set; }
}

public class training_camp
{
    [PrimaryKey]
    public int id { get; private set; }

    public string name { get; private set; }

    public int open_lvl { get; private set; }
}

public class training_reward
{
    [PrimaryKey]
    public string train_type { get { return string.Empty; } private set {eTrain_type = (WebConnector.TrainingType)Enum.Parse(typeof(WebConnector.TrainingType), value); } }
    public WebConnector.TrainingType eTrain_type;
    public int card_grade {get; private set;}
    public int exp { get; private set; }
    public string items { get; private set; }
}

public class ban_word
{
    public string word { get; private set; }
}

public class synergy_effect_info
{
    public string synergy_type { get { return string.Empty; } private set { eSynergyType = (WebConnector.SynergyType)Enum.Parse(typeof(WebConnector.SynergyType), value); } }
    public WebConnector.SynergyType eSynergyType;

    public int rank { get; private set; }

    public int eff_card_cnt { get; private set; }

    public int eff_ab { get; private set; }

}

public class gear
{
    public int gear_id { get; private set; }

    public string name { get; private set; }

    public int grade { get; private set; }

    public string ab_code1 { get { return string.Empty; } private set { eAb_code1 = (WebConnector.CardAbCode)Enum.Parse(typeof(WebConnector.CardAbCode), value); } }

    public WebConnector.CardAbCode eAb_code1;

    public int ab_val1 { get; private set; }

    //public string ab_code2 { get { return string.Empty; } private set { eAb_code2 = (WebConnector.CardAbCode)Enum.Parse(typeof(WebConnector.CardAbCode), value); } }

    public WebConnector.CardAbCode eAb_code2;
    public int ab_val2 { get; private set; }
}

public class gear_reinforce
{
    public int grade { get; private set; }

    public float add_ab { get; private set; }

    public int cost { get; private set; }

    public int sell_price { get; private set; }

    public int exp_demand1 { get; private set; }

    public int exp_demand2 { get; private set; }

    public int exp_demand3 { get; private set; }

    public int exp_demand4 { get; private set; }

    public int exp_demand5 { get; private set; }

    public int exp_return0 { get; private set; }

    public int exp_return1 { get; private set; }

    public int exp_return2 { get; private set; }

    public int exp_return3 { get; private set; }

    public int exp_return4 { get; private set; }

    public int exp_return5 { get; private set; }
}

public class goods
{
    public int goods_id { get; private set; }

    public string store_type { get; private set; }

    public string name { get; private set; }
}

public class trade_point
{
    [PrimaryKey]
    public int card_grade { get; private set; }

    public int point { get; private set; }

    public int legend_point { get; private set; }
}

public class trade_probability
{
    [PrimaryKey]
    public int min_point { get; private set; }

    public string trade_cls { get; private set; }

    public int retry_ruby { get; private set; }

    public string prob { get; private set; }

    public int GradeIconViewCount()
    {
        if (trade_cls.Equals("SS"))
            return 2;
        else if (trade_cls.Equals("SSS"))
            return 3;
        else
            return 1;
    }
}