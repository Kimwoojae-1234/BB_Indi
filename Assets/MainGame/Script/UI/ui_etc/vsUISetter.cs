using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

namespace BaseBall.BallPlay
{
    public class vsUISetter : MonoBehaviour
    {
        public SkeletonAnimation vsAnim;
        public skillUISetter [] skillUI;

        public GameObject explore;
        //public GameObject weaphon;


        public GameObject loseEffect;

                

        private bool bMyTurn;
        private int my, cpu;

        public void init(int _myID, bool _bMyTurn, int offenseID, int offenseRank, int defenseID, int defenseRank, bool bOffenseWin)
        {
            my = _myID;
            cpu = 1 - _myID;
            bMyTurn = _bMyTurn;

            //내쪽
            int myID = bMyTurn ? offenseID : defenseID;
            int myRank = bMyTurn ? offenseRank : defenseRank;
            
            //상대쪽
            int cpuID = bMyTurn ? defenseID : offenseID;
            int cpuRank = bMyTurn ? defenseRank : offenseRank;

            //내 승리 여부
            bool bMyWin = (bMyTurn ? bOffenseWin : !bOffenseWin);


            StartCoroutine(startAnim(myID, myRank, cpuID, cpuRank, bMyWin));
            
        }

        public float testTime = 0.25f;

        private IEnumerator startAnim(int myID, int myRank, int cpuID, int cpuRank, bool bMyWin)
        {
            //VS연출
            vsAnim.gameObject.SetActive(true);
            vsAnim.state.ClearTracks();
            vsAnim.skeleton.SetToSetupPose();
            vsAnim.state.SetAnimation(0, "VS_SILL", false);

            yield return new WaitForSeconds(0.25f);            
            explore.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            CameraManager.CameraShake(0.25f, 20);
            yield return new WaitForSeconds(0.15f);
            //나
            skillUI[my].init(myID, myRank, true);

            //상대쪽
            skillUI[cpu].init(cpuID, cpuRank, true);


            //지는 연출
            int loseIndex = (bMyWin ? cpu : my);
            loseEffect.transform.localPosition = new Vector3(loseIndex == 0 ? -440 : 440, 110, 0);
            skillUI[loseIndex].lose(loseEffect);


            yield return new WaitForSeconds(1.5f);
            explore.gameObject.SetActive(false);

        }
    }
}