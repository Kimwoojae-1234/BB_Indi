using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OldCode
{
    public class SkillData
    {
        private skill m_dbSkillData;

        private WebConnector.CardSkill m_cardSkillInfo;

        private int[] invoke_rate = null;
        private int[] scope_rate = null;
        private int[] value = null;

        public skill LocalDB_SkillData { get { return m_dbSkillData; } }

        public List<int[]> rate_value = new List<int[]>();
        public List<string> rate_name = new List<string>();
        public List<char> unit = new List<char>();

        public WebConnector.CardSkill CardSkillInfo { get { return m_cardSkillInfo; } private set { m_cardSkillInfo = value; } }

        public SkillData(WebConnector.CardSkill skillinfo)
        {
            m_cardSkillInfo = skillinfo;

            if (m_cardSkillInfo != null)
            {
                // DISABLED_MGRS: m_dbSkillData = Mgrs.GameData.GameDB_FindSkill(m_cardSkillInfo.skillId);
                if (m_dbSkillData == null)
                    return;
                if (m_dbSkillData.invoke_rate1 != 0)
                {
                    invoke_rate = new int[5];
                    invoke_rate[0] = m_dbSkillData.invoke_rate1;
                    invoke_rate[1] = m_dbSkillData.invoke_rate2;
                    invoke_rate[2] = m_dbSkillData.invoke_rate3;
                    invoke_rate[3] = m_dbSkillData.invoke_rate4;
                    invoke_rate[4] = m_dbSkillData.invoke_rate5;
                    rate_name.Add(m_dbSkillData.invoke_name);
                    rate_value.Add(invoke_rate);
                    unit.Add('%');

                }

                if (m_dbSkillData.scope_rate1 != 0)
                {
                    scope_rate = new int[5];
                    scope_rate[0] = m_dbSkillData.scope_rate1;
                    scope_rate[1] = m_dbSkillData.scope_rate2;
                    scope_rate[2] = m_dbSkillData.scope_rate3;
                    scope_rate[3] = m_dbSkillData.scope_rate4;
                    scope_rate[4] = m_dbSkillData.scope_rate5;
                    rate_name.Add(m_dbSkillData.scope_name);
                    rate_value.Add(scope_rate);
                    unit.Add('%');
                }

                if (m_dbSkillData.value1 != 0)
                {
                    value = new int[5];
                    value[0] = m_dbSkillData.value1;
                    value[1] = m_dbSkillData.value2;
                    value[2] = m_dbSkillData.value3;
                    value[3] = m_dbSkillData.value4;
                    value[4] = m_dbSkillData.value5;
                    rate_name.Add(m_dbSkillData.value_name);
                    rate_value.Add(value);
                    unit.Add(' ');
                }

            }
        }

        public int[] GetRankRateValue(int rank)
        {
            int[] return_value = new int[rate_value.Count];
            for (int i = 0; i < return_value.Length; i++)
            {
                return_value[i] = rate_value[i][rank - 1];
            }
            return return_value;
        }

    }

}