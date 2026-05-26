using UnityEngine;
using System.Collections;
using Spine.Unity;

namespace BaseBall.BallPlay
{
    public class HitEffect : MonoBehaviour
    {

        private static HitEffect instance_;

        private SkeletonAnimation hitEffectAnim;

        void Awake()
        {
            instance_ = this;
        }

        void OnDestroy()
        {
            instance_ = null;
        }

        // Use this for initialization
        void Start()
        {
            hitEffectAnim = GetComponent<SkeletonAnimation>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public static void SetHitEffect(string effectName, float x, float y, float scale = 100, float tScale = 1.0f)
        {
            instance_.setHitEffect(effectName, x, y, scale, tScale);
        }

        public static void SetColor(Color color)
        {
            instance_.setColor(color);
        }


        private void setHitEffect(string effectName, float x, float y, float scale, float tScale)
        {
            StartCoroutine(hitEffect(effectName, x, y, scale, tScale));
        }

        private IEnumerator hitEffect(string effectName, float x, float y, float scale, float tScale)
        {
            hitEffectAnim.transform.localPosition = new Vector3(x, y + 197, -90);
            hitEffectAnim.transform.localScale = new Vector3(scale, scale, 100);
            //hitEffectAnim.transform.localEulerAngles = new Vector3(0, 0, 0);

            hitEffectAnim.GetComponent<Renderer>().enabled = true;

            hitEffectAnim.state.ClearTracks();
            hitEffectAnim.skeleton.SetToSetupPose();
            hitEffectAnim.state.SetAnimation(25, effectName, false);
            hitEffectAnim.timeScale = tScale;

            yield return new WaitForSeconds(3.0f);

            hitEffectAnim.GetComponent<Renderer>().enabled = false;
            setColor(new Color(1, 1, 1, 1));
        }

        private void setColor(Color color)
        {
            hitEffectAnim.skeleton.SetColor(color);
        }

    }
}