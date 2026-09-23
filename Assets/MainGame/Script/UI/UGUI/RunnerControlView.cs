using UnityEngine;
using UnityEngine.UI;

namespace BaseBall.BallPlay
{
    public sealed class RunnerControlView : MonoBehaviour
    {
        public IngameUGUICanvas canvas;
        public ScoreboardSpriteCatalog sprites;
        public RunnerBaseButton[] bases;
        public Image[] occupied;
        public Image[] action;
        public Image[] light;
        public Image[] skill;
        public Text[] speed;
        public GameObject intentionalWalk;
        private readonly float[] pulseTime = new float[3];

        public void SetMode(bool attacking)
        {
            for (int i = 0; i < 3; i++) action[i].sprite = sprites.Get(attacking ? "steal_1" : "pickoff_1").sprite;
        }

        public void SetRunner(int index, bool onBase, int value, bool hasSkill, bool attacking)
        {
            occupied[index].gameObject.SetActive(onBase);
            action[index].gameObject.SetActive(onBase);
            speed[index].gameObject.SetActive(onBase);
            skill[index].gameObject.SetActive(onBase && hasSkill);
            light[index].gameObject.SetActive(false);
            pulseTime[index] = 0;
            occupied[index].sprite = sprites.Get("runnercon_onbase").sprite;
            action[index].sprite = sprites.Get(attacking ? "steal_1" : "pickoff_1").sprite;
            speed[index].text = value.ToString();
            speed[index].color = value >= 100 ? new Color(.74f, .15f, .89f) :
                value >= 80 ? new Color(.96f, .16f, .16f) : value >= 60 ? new Color(.16f, .58f, 1) : new Color(.38f, .45f, .84f);
        }

        public void Select(int index, bool attacking)
        {
            occupied[index].sprite = sprites.Get("runnercon_steal").sprite;
            action[index].sprite = sprites.Get(attacking ? "steal_2" : "pickoff_2").sprite;
            light[index].gameObject.SetActive(occupied[index].gameObject.activeSelf);
        }

        private void Update()
        {
            for (int i = 0; i < 3; i++)
            {
                if (!light[i].gameObject.activeInHierarchy) continue;
                pulseTime[i] += Time.unscaledDeltaTime;
                var color = light[i].color;
                color.a = 1 - Mathf.PingPong(pulseTime[i] / .3f, 1);
                light[i].color = color;
            }
        }
    }
}
