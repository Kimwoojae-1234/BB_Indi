using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OldCode
{
    public class GearData
    {
        private gear gearDBData;

        private WebConnector.GearInfo gearInfo;

        public GearData(WebConnector.GearInfo gear_Info)
        {
            this.gearInfo = gear_Info;
            if (this.gearInfo == null)
                return;

            // DISABLED_MGRS: gearDBData = Mgrs.GameData.FindGearData(this.gearInfo.gearId);

        }

        public GearData(int gear_id)
        {
            this.gearInfo = null;
            // DISABLED_MGRS: gearDBData = Mgrs.GameData.FindGearData(gear_id);
        }

        public int GetGearID()
        {
            return gearDBData.gear_id;
        }

        public long GetGearSeq()
        {
            if (gearInfo == null)
                return 0;
            return gearInfo.gearSeq;
        }

        public void SetGearCardInfo(long cardSeq)
        {
            this.gearInfo.cardSeq = cardSeq;
        }

        public void UpdateGearInfo(WebConnector.GearInfo gear_Info)
        {
            this.gearInfo = gear_Info;
        }

        public long GetGearCardSeq()
        {
            return gearInfo.cardSeq;
        }

        public string GetGearName()
        {
            return gearDBData.name;
        }

        public int GetGearGrade()
        {
            return gearDBData.grade;
        }

        public string GetIconSpriteName()
        {
            string spriteName = ((int)(gearDBData.gear_id * 0.1)).ToString();
            return spriteName;
        }

        public int GetGearReinforceLev()
        {
            if (gearInfo == null)
                return 0;
            return gearInfo.reinforceLev;
        }
    }
}