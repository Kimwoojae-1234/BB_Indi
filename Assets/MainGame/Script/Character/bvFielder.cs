using UnityEngine;
using System.Collections;
using Spine.Unity;

namespace BaseBall.BallPlay
{
    public class bvFielder : MonoBehaviour
    {

        public SkeletonAnimation anim;


        // Use this for initialization
        void Start()
        {
            //GameObject skeleton = Util.Load("MainGame/prefabs/skeleton/bvFielder/bvFielderSkeletonPrefab", transform, new Vector3(0, 0, -0.01f), "skeleton");
            //skeleton.transform.localScale = new Vector3(130, 130, 100);
            //skeleton.layer = LayerMask.NameToLayer("BATTINGVIEW_LAYER");
            //anim = skeleton.GetComponent<SkeletonAnimation>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        /*
        void playAnim(string strID, bool loop, float timeScale = 1.0f)
        {
            anim.state.ClearTracks();              
            anim.skeleton.SetToSetupPose();
            anim.state.SetAnimation(0, strID, loop);
            anim.timeScale = timeScale;
        }*/


        public void loadTexture()
        {
            int team = SimulPlayerManager.cpuTeamIndex;
            AtlasAsset atlasdata = anim.skeletonDataAsset.atlasAssets[0];
            Material[] materials = atlasdata.materials;
            ////Debug.Log("=========================>>" + ("MainGame/spineData/bvChar/bvFielder/team/" + team + "/FIELDER"));
            materials[0].mainTexture = (Texture)Resources.Load("MainGame/spineData/bvChar/bvFielder/team/" + team + "/FIELDER");                    
        }

        string strID = "FIELDER_IDLE";
        public void setReady()
        {
            //playAnim("tt_normal", false, 1.0f);
            //playAnim("FIELDER_IDLE", true, 1.0f);

            if (strID.Equals("FIELDER_IDLE") == false)
            {
                strID = "FIELDER_IDLE";
                anim.state.ClearTracks();
                anim.skeleton.SetToSetupPose();// .SetToSetupPose();
                anim.state.SetAnimation(0, strID, true);
            }
        }


        public void setFielding()
        {
            //playAnim("FIELDER_READY", false, 1.0f);
            //playAnim("FIELDER_READY", false);
            if (strID.Equals("FIELDER_READY") == false)
            {
                strID = "FIELDER_READY";
                anim.state.ClearTracks();
                anim.state.SetAnimation(1, strID, false);
            }
        }


    }
}