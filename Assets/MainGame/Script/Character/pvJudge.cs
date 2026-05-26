using UnityEngine;
using System.Collections;
using Spine.Unity;

namespace BaseBall.BallPlay
{
    public class pvJudge : MonoBehaviour
    {
        int initPosX,initPosY;
        float depth;
        private bool bLeftPitcher;

        public SkeletonAnimation anim;

        private int strikeoutType = 1;  //게임시작할때 정해져서 바뀌지 않음

        // Use this for initialization
        void Start()
        {
            //오직 시뮬레이션만 하는 경우에는 초기에 리소스를 설정해주지 않는다
            //배팅뷰용
            strikeoutType = Random.Range(1, 7);
        }

        /*
        // Update is called once per frame
        void Update()
        {

        }*/

        int lastTrack;
        public void judgeAnim(int track, string strAnim, bool bLoop = false)
        {
            if (track != lastTrack) anim.state.ClearTrack(lastTrack);
            anim.skeleton.SetToSetupPose();
            anim.state.SetAnimation(track, strAnim, bLoop);
            anim.timeScale = 1.0f;
            lastTrack = track;
        }

        public void initPosition(bool bLeftPitcher, GameObject _parent)
        {
            this.bLeftPitcher = bLeftPitcher;
            transform.parent = _parent.transform;
            initPosX = (bLeftPitcher ? -185 : 180);
            initPosY = 418;
            depth = -1.5f;
            setIdle();
        }

        /// <summary>
        /// 아이들 상태
        /// </summary>
        public void setIdle()
        {
            transform.localScale = Vector3.one;
            transform.localPosition = new Vector3(initPosX, initPosY, depth);
            judgeAnim(0, "IDLE_0"+Random.Range(1,3), true);
        }

        public void setReady()
        {
            judgeAnim(0, "POSE_01", false);
        }

        public void setReadyBack()
        {
            judgeAnim(0, "POSE_03", false);
        }

        public void setStrike()
        {
            //judgeAnim(0, "STRIKE_0"+Random.Range(1,3), false);
            anim.state.ClearTracks();
            anim.skeleton.SetToSetupPose();
            anim.state.SetAnimation(0, "POSE_04", false);
            anim.state.AddAnimation(0, "STRIKE_0" + Random.Range(1, 3), false,0.2f);
            Invoke("setIdle", 2.2f);
        }

        public void setFoul()
        {
            judgeAnim(0, "FOUL", false);
            Invoke("setIdle", 2.0f);
        }

        public void setStrikeOut(bool bLeftBatter)
        {
            transform.localScale = new Vector3(bLeftBatter?-1:1,1,1);

            int curPosX;
            if (bLeftPitcher == true) curPosX = initPosX;// + (bLeftBatter ? 0 : -100);
            else curPosX = initPosX + (bLeftBatter ? 50 : -80);

            transform.localPosition = new Vector3(curPosX, initPosY, depth);
            judgeAnim(0, "STRIKE_OUT_0" + strikeoutType, false);
            Invoke("setIdle", 2.6f);
        }
    }
}