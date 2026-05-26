using UnityEngine;
using System.Collections;
using Spine.Unity;

namespace BaseBall.BallPlay
{
    public class bvRunner : MonoBehaviour
    {

        public SkeletonAnimation anim;
        bool bLead;
        bool bMove;
        float xPos, yPos;
        float scale;
        bool bAvailable;

        bool bFirst, bSecond;

        // Use this for initialization
        void Start()
        {
            //anim = gameObject.GetComponent<SkeletonAnimation>();
            bAvailable = false;
            //gameObject.renderer.enabled = false;
            yPos = 100;
            bFirst = false;
            bSecond = false;

            GameObject skeleton = Util.Load("MainGame/prefabs/skeleton/bvRunner/bvRunnerSkeletonPrefab", transform, Vector3.zero, "skeleton");
            skeleton.transform.localScale = new Vector3(170, 170, 100);
            skeleton.layer = LayerMask.NameToLayer("BATTINGVIEW_LAYER");
            anim = skeleton.GetComponent<SkeletonAnimation>();
            anim.gameObject.SetActive(false);
        }

        public void loadTexture(bool bTopInning)
        {
            bFirst = false;
            bSecond = false;
            anim.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (bLead == true)
            {
                xPos += (-60 * Time.deltaTime);
                transform.localPosition = new Vector3(xPos, yPos, -1.81f);
                if (xPos < -175)
                {
                    bLead = false;
                    playAnim("RUNNER_IDLE", true, 1);
                }
            }
            if (bMove == true)
            {
                xPos += (-150 * Time.deltaTime);
                if (bFirst)
                {
                    yPos += (4.9f * Time.deltaTime);
                    scale -= (0.01f * Time.deltaTime);
                    if (xPos < 0)
                    {
                        setBvRunnerMove(false);
                    }
                }
                else if (bSecond)
                {
                    yPos -= (4.9f * Time.deltaTime);
                    scale += (0.010f * Time.deltaTime);

                }

                transform.localPosition = new Vector3(xPos, yPos, -1.81f);
                transform.localScale = new Vector3(scale, scale, 1);
            }
        }


        public void setBvRunnerMove(bool _bMove, bool bFoul = false)
        {
            bLead = false;
            bMove = _bMove;

            if (bMove == true)
            {
                if (bFirst)
                {
                    xPos = 685;
                    yPos = 190;// 406;
                }
                playAnim("RUNNER_RUN", true, 1);
            }
            else
            {
                playAnim("RUNNER_RUN", false, 0);
                if (bFoul == true)
                {
                    if (bFirst)
                    {
                        xPos = 900;// 685;
                        yPos = 190;// 406;// initPosY;
                        scale = 0.38f;
                        transform.localPosition = new Vector3(xPos, yPos, -1.81f);
                        transform.localScale = new Vector3(scale, scale, 1);
                    }
                    else if (bSecond)
                    {
                        xPos = -53;
                        yPos = 207;// 210;// 426;// initPosY;
                        scale = 0.35f;
                        transform.localPosition = new Vector3(xPos, yPos, -1.81f);
                        transform.localScale = new Vector3(scale, scale, 1);
                    }
                }
            }
        }


        public void set1stRunnerInit(bool bAvail, float initScale)//, int initPosY)
        {
            bMove = false;
            //////UnityEngine.//Debug.Log("==============>>bAvailable = " + bAvailable);
            //////UnityEngine.//Debug.Log("==============>>set1stRunnerInit = " + bAvail);
            yPos = 190;// 406;// initPosY;
            if (bAvailable != bAvail)
            {
                //anim.gameObject.GetComponent<Renderer>().enabled = bAvail;
                anim.gameObject.SetActive(bAvail);
                bAvailable = bAvail;
            }


            if (bAvailable == true)
            {
                bFirst = true;
                bSecond = false;
                xPos = 900;// 685;
                scale = initScale;
                transform.localScale = new Vector3(scale, scale, 1);
                transform.localPosition = new Vector3(xPos, yPos, -1.81f);
                playAnim("RUNNER_IDLE", true, 1); //t_normal3
                bLead = false;

            }
        }

        public void set2ndRunnerInit(bool bAvail, float initScale)
        {
            bMove = false;
            yPos = 207;
            if (bAvailable != bAvail)
            {
                //anim.gameObject.GetComponent<Renderer>().enabled = bAvail;
                anim.gameObject.SetActive(bAvail);
                bAvailable = bAvail;
            }


            if (bAvailable == true)
            {
                bFirst = false;
                bSecond = true;
                scale = initScale;
                transform.localScale = new Vector3(scale, scale, 1);
                transform.localPosition = new Vector3(-53, yPos, -1.81f);
                playAnim("RUNNER_IDLE", true, 1);
                bLead = false;
                xPos = -53;
            }
        }

        public void set2ndRunnerLead()
        {
            if (bAvailable == true)
            {
                playAnim("RUNNER_WALK", true, 1);
                bLead = true;
            }
        }

        void playAnim(string strID, bool loop, float timeScale)
        {
            anim.skeleton.SetSlotsToSetupPose();
            anim.state.SetAnimation(0, strID, loop);
            anim.timeScale = timeScale;
        }


        public void loadTexture()
        {
            int team = SimulPlayerManager.myTeamIndex;
            AtlasAsset atlasdata = anim.skeletonDataAsset.atlasAssets[0];
            Material[] materials = atlasdata.materials;
            materials[0].mainTexture = (Texture)Resources.Load("MainGame/spineData/bvChar/bvRunner/team/" + team + "/runner");     
        }

    }
}