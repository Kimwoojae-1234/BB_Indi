using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class miniRunner : MonoBehaviour
    {
        bool bActive;
        bool bInitPos;
        public Runner runner;

        public UISprite _team;
        public UILabel _name;

        float xPos, yPos;
        BallPlayManager manager;

        // Use this for initialization
        void Start()
        {
            bInitPos = false;
            bActive = true;
        }



        
        // Update is called once per frame
        void Update()
        {
            if (bActive == true)
            {
                if (manager.playState == PlayState.PLAY_FIELDING_VIEW)
                {
                    move();
                }
            }
        }

        public void set(BallPlayManager manager, Runner runner, int team)
        {
            transform.localScale = Vector3.one;

            this.manager = manager;
            this.runner = runner;
            runner.minimapRunner = gameObject;

            _team.spriteName = "minimap_team" + team;

            if (Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut)
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
            else
            {
                _name.text = runner.pRunner.getName();
            }

            int basePos = runner.currentPos;

            if (basePos == SimulParm.HOMEBASE_INDEX)
            {
                transform.localPosition = new Vector3(0, -84, 0);
            }
            else if (basePos == SimulParm.FIRSTBASE_INDEX)
            {
                transform.localPosition = new Vector3(84, 0, 0);
            }
            else if (basePos == SimulParm.SECONDBASE_INDEX)
            {
                transform.localPosition = new Vector3(0, 84, 0);
            }
            else //if (basePos == SimulParm.THIRDBASE_INDEX) 
            {
                transform.localPosition = new Vector3(-84, 0, 0);
            }
        }



        public void move()
        {
            if (runner == null)
            {
                bActive = false;
                Destroy(gameObject);
                return;
            }

            int[] xStartPos = new int[4] { 0, 84, 0, -84 };
            int[] yStartPos = new int[4] { -84, 0, 84, 0 };

            float offsetX;//, offsetY;

            if (runner.state < RunState.STANDBY || runner.state > RunState.STEAL) return;

            if (runner.destPos == FieldParm.FIRSTBASE_INDEX)
            {
                offsetX =  ((runner.posX - FieldSize.getHomePosX()) * 84) / (FieldSize.getFirstBasePosX() - FieldSize.getHomePosX());
                //offsetY = 0;//((runner.posY - FieldSize.getHomePosY()) * 84) / (FieldSize.getFirstBasePosY() - FieldSize.getHomePosY());

                xPos = xStartPos[0] + offsetX;
                yPos = yStartPos[0] + offsetX;// + offsetY;
            }
            else if (runner.destPos == FieldParm.SECONDBASE_INDEX)
            {
                offsetX = ((runner.posX - FieldSize.getFirstBasePosX()) * 84) / (FieldSize.getSecondBasePosX() - FieldSize.getFirstBasePosX());
                //offsetY = 0;//((runner.posY - FieldSize.getFirstBasePosY()) * 84) / (FieldSize.getSecondBasePosY() - FieldSize.getFirstBasePosY());
                xPos = xStartPos[1] - offsetX;
                yPos = yStartPos[1] + offsetX; // +offsetY;
            }
            else if (runner.destPos == FieldParm.THIRDBASE_INDEX)
            {
                offsetX = ((runner.posX - FieldSize.getSecondBasePosX()) * 84) / (FieldSize.getThirdBasePosX() - FieldSize.getSecondBasePosX());
                //offsetY = 0;//((runner.posY - FieldSize.getSecondBasePosY()) * -84) / (FieldSize.getThirdBasePosY() - FieldSize.getSecondBasePosY());
                xPos = xStartPos[2] - offsetX;
                yPos = yStartPos[2] - offsetX;  //+offsetY;
            }
            else //if (runner.destPos == FieldParm.FIRSTBASE_INDEX)
            {
                offsetX = ((runner.posX - FieldSize.getThirdBasePosX()) * 84) / (FieldSize.getHomePosX() - FieldSize.getThirdBasePosX());
                //offsetY = 0;//((runner.posY - FieldSize.getThirdBasePosY()) * -84) / (FieldSize.getHomePosY() - FieldSize.getThirdBasePosY());
                xPos = xStartPos[3] + offsetX;
                yPos = yStartPos[3] - offsetX; // +offsetY;
            }

            transform.localPosition = new Vector3(xPos, yPos,0);
            bInitPos = false;
        }



        public void initPosition()
        {
            if (bInitPos == false)
            {
                int[] xStartPos = new int[4] { 84, 0, -84, 0 };
                int[] yStartPos = new int[4] { 0, 84, 0, -84 };

                xPos = xStartPos[runner.currentPos];
                yPos = yStartPos[runner.currentPos];

                transform.localPosition = new Vector3(xPos, yPos, 0);
                bInitPos = true;
            }
        }
    }
}