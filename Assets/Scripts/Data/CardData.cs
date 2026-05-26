using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardData
{
    /// <summary>
    /// 로컬 DB에서 가져오는 데이터 
    /// </summary>
    private card m_dbData;
    /// <summary>
    /// 서버 통신으로 받아오는 데이터
    /// </summary>
    private WebConnector.GameCardInfo m_gameCardInfo;

    public WebConnector.Lineup lineup;
    public card DB_Data { get { return m_dbData;}}
    public WebConnector.GameCardInfo GameCardInfo{ get{ return m_gameCardInfo;} private set{ m_gameCardInfo = value;} }

    public List<OldCode.SkillData> haveSkill = new List<OldCode.SkillData>();
    public bool isNewGet;
    public bool isTraining;
    
    public CardData(WebConnector.GameCardInfo gameCardInfo)
    {
        m_gameCardInfo = gameCardInfo;
        if(m_gameCardInfo != null)
        {
            // DISABLED_MGRS: m_dbData = Mgrs.GameData.GameDB_FindCardByCardID(m_gameCardInfo.cardId);
            ChangeSkillData(m_gameCardInfo.skills);
        }
            
        
    }

    public int GetCardID()
    {
        return m_gameCardInfo.cardId;
    }

    public long GetCardSeq()
    {
        return m_gameCardInfo.cardSeq;
    }

    public int GetCardGrade()
    {
        return m_gameCardInfo.grade;
    }

    public int GetCardLevel()
    {
        return m_gameCardInfo.level;
    }
    
    public WebConnector.CardType GetCardType()
    {
        return m_gameCardInfo.CardType;
    }

    public List<WebConnector.GearType> CardHaveGearType()
    {
        return null;
    }

    public void ChangeGameCardInfo(WebConnector.GameCardInfo info)
    {
        m_gameCardInfo = info;
        ChangeSkillData(info.skills);
    }

    public void ChangeSkillData(List<WebConnector.CardSkill> skill_list)
    {
        haveSkill.Clear();
        if(skill_list != null)
            {
                for (int i = 0; i < skill_list.Count; i++)
                {
                    haveSkill.Add(new OldCode.SkillData(skill_list[i]));
                }
            }
    }
}


