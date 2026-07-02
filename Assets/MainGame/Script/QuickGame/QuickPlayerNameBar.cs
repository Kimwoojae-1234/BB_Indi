using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class QuickPlayerNameBar : MonoBehaviour
    {
        public UISprite bg;
        public UILabel num;
        public UILabel name;
        public UILabel overall;
        public UISprite logo;
        public GameObject hitFlag;
        public GameObject focusObj;


        private UILabel resultLabel;

        private bool bPitcher;
        private int team;
        private float yPos;
        private bool bCurOffense;

        private IEnumerator focus, deFocus;


        public void setInit(CPlayer player, int teamIndex, bool bOffense)
        {
            if (player == null) return;
            yPos = transform.localPosition.y;
            bCurOffense = bOffense;
            team = teamIndex;  
            name.text = player.getName();
#if _Test_Local
            logo.spriteName = "logo_" + Random.Range(1, 10);     
            bPitcher = (player.getCurPos() == CPlayer._PITCHER?true:false);
            int overallNum = Random.Range(80,140);            
#else
            Util.SetSpritePixelPerfect(logo, "logo_" + (int)player.getPlayerData().eTeam, false); //logo.spriteName = "logo_" + (int)player.getPlayerData().eTeam;     //
            bPitcher = (player.getCard().PlayerType == WebConnector.PlayerType.Pitcher ? true : false);
            int overallNum;
            if (bPitcher == true) overallNum = Utils.TeamPowerUtils.calCardPower(player.getCard());                
            else overallNum = Utils.TeamPowerUtils.calCardPower(player.getCard());
#endif      
            overall.bitmapFont = Util.GetOverallFont(overallNum);
            overall.text = overallNum.ToString();

            if (bOffense == true)
            {
                num.text = string.Format("No.{0}", (player.getOrder() + 1));
            }
            else
            {
                if (bPitcher == true)
                {
                    num.text = Util.getPitcherposString(player);
                }
                else
                {
                    num.text = Util.GetPositionString2(player.getCurPos());
                }
            }

            setFocus(false);

            resultLabel = hitFlag.transform.Find("hitlabel").GetComponent<UILabel>();

            hitFlag.SetActive(false);
        }


        private readonly Color focusColor =  new Color(1, 1, 1);
        private readonly Color normalColor = new Color(0.729f, 0.729f, 0.729f);


        public void setFocus(bool bFocus)
        {
            if (bFocus == true)
            {
                //bg.spriteName = "simul_table2";
                focus = focusBar();
                StartCoroutine(focus);
            }
            else
            {
                //bg.spriteName = "simul_table1";
                deFocus = deFocusBar();
                StartCoroutine(deFocus);
            }
        }


        public bool setHitFlag(SimulResultState result, QuickPlayerNameBar next)
        {
            bool bActive = true;

            if (result == SimulResultState.FourBall) resultLabel.text = "BB";
            else if (result == SimulResultState.Single || result == SimulResultState.SingleOneError || result == SimulResultState.InfieldSingle || result == SimulResultState.BuntSingle) resultLabel.text = "HIT";
            else if (result == SimulResultState.Double || result == SimulResultState.DoubleOneError) resultLabel.text = "Double";
            else if (result == SimulResultState.Triple || result == SimulResultState.TripleOneError) resultLabel.text = "Triple";
            else if (result == SimulResultState.HomeRun) resultLabel.text = "HR";
            else bActive = false;

            if (bActive == false)
            {
                //바로 다음 타자
                setFocus(false);
                next.setFocus(true);
                return false;
            }
            else
            {
                //힛포커스
                StartCoroutine(setHitMark(next));
                return true;
            }
        }

        private IEnumerator setHitMark(QuickPlayerNameBar next)
        {
            //쉐이크
            Util.SetTween(hitFlag);// hitFlag.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            setFocus(false);
            next.setFocus(true);
        }


        private IEnumerator focusBar()
        {
            if (deFocus != null) StopCoroutine(deFocus);

            if (bCurOffense) TweenPosition.Begin(gameObject, 0.1f, new Vector3(team == 0 ? -12 : 12, yPos, 0));
            else transform.localPosition = new Vector3(0, yPos, 0);

            yield return new WaitForSeconds(0.1f);

            focusObj.SetActive(true);
            TweenAlpha.Begin(focusObj, 0.15f, 1);

            yield return new WaitForSeconds(0.15f);

            num.color = focusColor;
            name.color = focusColor;
        }

        private IEnumerator deFocusBar()
        {
            if (focus != null) StopCoroutine(focus);

            if (bCurOffense) TweenPosition.Begin(gameObject, 0.1f, new Vector3(0, yPos, 0));
            else transform.localPosition = new Vector3(0, yPos, 0);                

            //상대 포지션 유지하기 위한 뻘짓
            float curTime = 0;
            Vector3 pos = hitFlag.transform.position;
            while (curTime < 0.1f)
            {                
                curTime += Time.deltaTime;                
                yield return new WaitForEndOfFrame();
                hitFlag.transform.position = pos;
            }
            //0.1초동안...

            hitFlag.transform.localPosition = new Vector3(team == 0 ? -25 : 289, 0, 0);
            TweenAlpha.Begin(focusObj, 0.1f, 0);

            yield return new WaitForSeconds(0.1f);

            num.color = normalColor;
            name.color = normalColor;

            yield return new WaitForSeconds(0.1f);
            focusObj.SetActive(false);
        }

    }
}