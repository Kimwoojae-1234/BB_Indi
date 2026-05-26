using UnityEngine;
using SQLite4Unity3d;
using System.Collections.Generic;
using System;
using System.Collections;
using System.IO;
namespace BaseBall.BallPlay
{
    public class batting_balance
    {
        [PrimaryKey, AutoIncrement]
        public string name { get; private set; }
        public string value { get; private set; }
    }

    public class pitching_balance
    {
        [PrimaryKey, AutoIncrement]
        public string name { get; private set; }
        public string value { get; private set; }
    }

    public class fielding_balance
    {
        [PrimaryKey, AutoIncrement]
        public string name { get; private set; }
        public string value { get; private set; }
    }

    public class running_balance
    {
        [PrimaryKey, AutoIncrement]
        public string name { get; private set; }
        public string value { get; private set; }
    }

    public class localBalance : MonoBehaviour
    {

        public static localBalance Instance_;


        private Dictionary<string, string> batting_balance_db = new Dictionary<string, string>();
        private Dictionary<string, string> pitching_balance_db = new Dictionary<string, string>();
        private Dictionary<string, string> fielding_balance_db = new Dictionary<string, string>();
        private Dictionary<string, string> running_balance_db = new Dictionary<string, string>();

        void Awake()
        {
            Instance_ = this;
        }

        void OnDestroy()
        {
            Instance_ = null;
        }


        public static void SetBattingBalance()
        {
            Instance_.setBattingBalance();
        }


        public static void SetPitchingBalance()
        {
            Instance_.setPitchingBalance();
        }

        public static void SetFieldingBalance()
        {
            Instance_.setFieldingBalance();
        }

        public static void SetRunningBalance()
        {
            Instance_.setRunningBalance();
        }

        private void setBattingBalance()
        {
            BattingMechanism.EYE_VALUE = System.Convert.ToSingle(batting_balance_db["eye_value"]);//. 1.0f; 
            BattingMechanism.CONTACT_VALUE = System.Convert.ToSingle(batting_balance_db["contact_value"]);//1.0f; 
            BattingMechanism.POWER_VALUE = System.Convert.ToSingle(batting_balance_db["power_value"]);//1.0f; 
            BattingMechanism.TANDO_VALUE = System.Convert.ToSingle(batting_balance_db["tando_value"]);//1.0f; 
            BattingMechanism.PERFECT_CONTACT_COEF = System.Convert.ToSingle(batting_balance_db["perfect_contact_coef"]);//0.05f;   //이값이 커지면 퍼펙트 컨택이 잘나옴
            BattingMechanism.GOOD_CONTACT_COEF = System.Convert.ToSingle(batting_balance_db["good_contact_coef"]);//0.15f;      //이값이 커지면 굿 컨택이 잘나옴
            BattingMechanism.NORMAL_CONTACT_COEF = System.Convert.ToSingle(batting_balance_db["normal_contact_coef"]);//0.4f;     //이값이 커지면 노멀 컨택이 잘나옴
            BattingMechanism.BAT_SIZEX = 70;// System.Convert.ToSingle(batting_balance_db["eye_value"]);//70;                 //이값이 커지면 컨택이 쉬워짐
            BattingMechanism.BAT_SIZEY = 50;// System.Convert.ToSingle(batting_balance_db["eye_value"]);//50;                 //이값이 커지면 컨택이 쉬워짐
            BattingMechanism.GANGTA_BATSIZE = System.Convert.ToSingle(batting_balance_db["ganta_batsize"]);//30.8f;         //이값이 커지면 강타 컨택이 쉬워짐
            BattingMechanism.CONTACT_AUTO_RATE = System.Convert.ToSingle(batting_balance_db["contact_auto_rate"]);//1.0f;       //이값이 커지면 오토 혹은 AI플레이의 컨택값이 높아짐
            BattingMechanism.TIMING_GAB = System.Convert.ToSingle(batting_balance_db["timing_gab"]);//0.015f;            //이값이 커지면 퍼펙트 타이밍이 쉬워짐
            BattingMechanism.TIMING_AUTO_RATE = System.Convert.ToSingle(batting_balance_db["timing_auto_rate"]);//1.0f;       //이값이 커지면 오토 혹은 AI플레이의 타이밍값이 높아짐
            BattingMechanism.POWER_RATE = System.Convert.ToSingle(batting_balance_db["power_rate"]);//1.5f;          //이값이 커지면 능력치별 파워증가량이 증가
            BattingMechanism.MIN_POWER = System.Convert.ToSingle(batting_balance_db["min_power"]);//20.0f;          //이값이 커지면 최소 파워값이 증가
            BattingMechanism.TANDO_RANGE = (int)System.Convert.ToSingle(batting_balance_db["tando_range"]);//3000;               //이값이 커지면 홈런이 잘 안나옴
            BattingMechanism.BABIB_SIN = (int)System.Convert.ToSingle(batting_balance_db["babib_sin"]);//30;    //바빕지수: 좋은 코스로 타구가 향할 확률
            BattingMechanism.WRIST_USE = (int)System.Convert.ToSingle(batting_balance_db["wrist_use"]);//25;    //의도적이지 않은 손목사용 : 타이밍과 어긋나는 코스 생산 
        }

        private void setPitchingBalance()
        {
            PitchingMechanism.CONTROL_VALUE = System.Convert.ToSingle(pitching_balance_db["control_value"]);
            PitchingMechanism.GUWEE_VALUE = System.Convert.ToSingle(pitching_balance_db["guwee_value"]);
            PitchingMechanism.BALLSPEED_VALUE = System.Convert.ToSingle(pitching_balance_db["ballspeed_value"]);
            PitchingMechanism.BALLMOVEMENT_VALUE = System.Convert.ToSingle(pitching_balance_db["ballmovement_value"]);
            //PitchingMechanism.PITCH_STAMINA_LOSS = System.Convert.ToSingle(pitching_balance_db["pitch_stamina_loss"]);
            //PitchingMechanism.HIT_STAMINA_LOSS = System.Convert.ToSingle(pitching_balance_db["hit_stamina_loss"]);
            //PitchingMechanism.HR_STAMINA_LOSS = System.Convert.ToSingle(pitching_balance_db["hr_stamina_loss"]);
            //PitchingMechanism.FOURBALL_STAMINA_LOSS = System.Convert.ToSingle(pitching_balance_db["fourball_stamina_loss"]);
            //PitchingMechanism.PICKOFF_STAMINA_LOSS = System.Convert.ToSingle(pitching_balance_db["pickoff_stamina_loss"]);
            PitchingMechanism.MIN_NORMAL_CONTROL_VALUE = System.Convert.ToSingle(pitching_balance_db["min_normal_control_value"]);
            PitchingMechanism.MIN_GOOD_CONTROL_VALUE = System.Convert.ToSingle(pitching_balance_db["min_good_control_value"]);
            PitchingMechanism.MIN_PERFECT_CONTROL_VALUE = System.Convert.ToSingle(pitching_balance_db["min_perfect_control_value"]);
            PitchingMechanism.MISS_PER = (int)System.Convert.ToSingle(pitching_balance_db["miss_per"]);
            PitchingMechanism.USER_CONTROL_SPEED = System.Convert.ToSingle(pitching_balance_db["user_control_speed"]);
            PitchingMechanism.PERFECT_GUWEE_RATE = System.Convert.ToSingle(pitching_balance_db["perfect_guwee_rate"]);
            PitchingMechanism.GOOD_GUWEE_RATE = System.Convert.ToSingle(pitching_balance_db["good_guwee_rate"]);
            PitchingMechanism.NORMAL_GUWEE_RATE = System.Convert.ToSingle(pitching_balance_db["normal_guwee_rate"]);
        }


        private void setFieldingBalance()
        {
            FieldingMechanism.BASIC_FIELD_DELAY = System.Convert.ToSingle(fielding_balance_db["basic_field_delay"]);
            FieldingMechanism.BASIC_THROW_DELAY = System.Convert.ToSingle(fielding_balance_db["basic_throw_delay"]);
            FieldingMechanism.BASIC_FIELDER_SPEED = System.Convert.ToSingle(fielding_balance_db["basic_fielder_speed"]);
            FieldingMechanism.BASIC_THROW_SPEED = System.Convert.ToSingle(fielding_balance_db["basic_throw_speed"]);
            FieldingMechanism.FIELDING_ADJUST_RATE = System.Convert.ToSingle(fielding_balance_db["fielding_adjust_rate"]);
            FieldingMechanism.SPECIAL_GROUNDER_MIN_VALUE = System.Convert.ToSingle(fielding_balance_db["special_grounder_min_value"]);
            FieldingMechanism.SLIDING_CATCH_OFFSET = System.Convert.ToSingle(fielding_balance_db["sliding_catch_offset"]);
            FieldingMechanism.SPECIAL_FLYCATCH_MIN_VALUE = System.Convert.ToSingle(fielding_balance_db["special_flycatch_min_value"]);
            FieldingMechanism.DIVING_CATCH_OFFSET = System.Convert.ToSingle(fielding_balance_db["diving_catch_offset"]);
            FieldingMechanism.HR_STEAL_MAX_VALUE = System.Convert.ToSingle(fielding_balance_db["hr_steal_max_value"]);
        }


        private void setRunningBalance()
        {
            RunningMechnism.BASIC_DELAY = System.Convert.ToSingle(running_balance_db["basic_delay"]);
            RunningMechnism.DELAY_DECREASE_RATE = System.Convert.ToSingle(running_balance_db["delay_decrease_rate"]);
            RunningMechnism.BASIC_SPEED_MINIMUM = System.Convert.ToSingle(running_balance_db["basic_speed_minimum"]);
            RunningMechnism.SPEED_INCREASE_RATE = System.Convert.ToSingle(running_balance_db["speed_increase_rate"]);
            RunningMechnism.BASIC_ACCEL_RATE_MINIMUM = System.Convert.ToSingle(running_balance_db["basic_accel_rate_minimum"]);
            RunningMechnism.SLIDING_RANGE = (int)System.Convert.ToSingle(running_balance_db["sliding_range"]);
            RunningMechnism.RUSH_RANGE = (int)System.Convert.ToSingle(running_balance_db["rush_range"]);
            RunningMechnism.CLOSE_PLAY_RANGE = (int)System.Convert.ToSingle(running_balance_db["close_play_range"]);
            RunningMechnism.BASE_ARRIVE_RANGE = (int)System.Convert.ToSingle(running_balance_db["base_arrive_range"]);
            RunningMechnism.STEAL_DELAY = System.Convert.ToSingle(running_balance_db["steal_delay"]);
            RunningMechnism.STEAL_SPEED = (int)System.Convert.ToSingle(running_balance_db["steal_speed"]);
            RunningMechnism.STEAL_ACCEL = System.Convert.ToSingle(running_balance_db["steal_accel"]);
            RunningMechnism.PICKOFF_SAFE_DELAY = System.Convert.ToSingle(running_balance_db["pickoff_safe_delay"]);
            RunningMechnism.PICKOFF_SAFE_SPEED = System.Convert.ToSingle(running_balance_db["pickoff_safe_speed"]);
            RunningMechnism.PICKOFF_OUT_DELAY = System.Convert.ToSingle(running_balance_db["pickoff_out_delay"]);
            RunningMechnism.PICKOFF_OUT_SPEED = System.Convert.ToSingle(running_balance_db["pickoff_out_speed"]);
            RunningMechnism.OVERRUN_SAFE_LIMIT = System.Convert.ToSingle(running_balance_db["overrun_safe_limit"]);
            RunningMechnism.OVERRUN_DANGER_LIMIT = System.Convert.ToSingle(running_balance_db["overrun_danger_limit"]);
            RunningMechnism.OVERRUN_HOMERUSH_LIMIT = System.Convert.ToSingle(running_balance_db["overrun_homerush_limit"]);
            RunningMechnism.RUNNER_BACK_DELAY = System.Convert.ToSingle(running_balance_db["runner_back_delay"]);
            RunningMechnism.HITTERRUNNER_DELAY_RATE = System.Convert.ToSingle(running_balance_db["hitterrunner_delay_rate"]);
        }

        void Start()
        {
#if _Test_Local
            LoadDB();
#else
        Destroy(gameObject);
#endif
        }

        private SQLiteConnection connection;
        private List<EventDelegate.Callback> list_callBack = new List<EventDelegate.Callback>();

        private void InitMethodList()
        {
            this.list_callBack.Clear();
        }

        public void LoadDB()
        {
            this.InitMethodList();
            //string dbPath = "Resources/MainGame/localbalance/local_balance.db";
            string dbPath = string.Format("{0}/Resources/MainGame/localbalance/{1}", Application.dataPath, "local_balance.db");
            connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadOnly);
            InitBattingDB();
            InitPitchingDB();
            InitFieldingDB();
            InitRunningDB();
        }

        private void InitBattingDB()
        {
            TableQuery<batting_balance> battingTable = connection.Table<batting_balance>();
            for (int i = 0; i < battingTable.Count(); ++i)
            {
                if (this.batting_balance_db.ContainsKey(battingTable.ElementAt(i).name))
                {
                    // 키값이 중복되었을때 에러
                    Debug.Log("Error duplicate Key : batting balance" + battingTable.ElementAt(i).name);
                }
                else
                    this.batting_balance_db.Add(battingTable.ElementAt(i).name, battingTable.ElementAt(i).value);
            }
            battingTable = null;
        }


        private void InitPitchingDB()
        {
            TableQuery<pitching_balance> pitchingTable = connection.Table<pitching_balance>();
            for (int i = 0; i < pitchingTable.Count(); ++i)
            {
                if (this.pitching_balance_db.ContainsKey(pitchingTable.ElementAt(i).name))
                {
                    // 키값이 중복되었을때 에러
                    Debug.Log("Error duplicate Key : pitching balance" + pitchingTable.ElementAt(i).name);
                }
                else
                    this.pitching_balance_db.Add(pitchingTable.ElementAt(i).name, pitchingTable.ElementAt(i).value);
            }
            pitchingTable = null;
        }


        private void InitFieldingDB()
        {
            TableQuery<fielding_balance> fieldingTable = connection.Table<fielding_balance>();
            for (int i = 0; i < fieldingTable.Count(); ++i)
            {
                if (this.fielding_balance_db.ContainsKey(fieldingTable.ElementAt(i).name))
                {
                    // 키값이 중복되었을때 에러
                    Debug.Log("Error duplicate Key : field balance" + fieldingTable.ElementAt(i).name);
                }
                else
                    this.fielding_balance_db.Add(fieldingTable.ElementAt(i).name, fieldingTable.ElementAt(i).value);
            }
            fieldingTable = null;
        }

        private void InitRunningDB()
        {
            TableQuery<running_balance> runningTable = connection.Table<running_balance>();
            for (int i = 0; i < runningTable.Count(); ++i)
            {
                if (this.running_balance_db.ContainsKey(runningTable.ElementAt(i).name))
                {
                    // 키값이 중복되었을때 에러
                    Debug.Log("Error duplicate Key : run balance" + runningTable.ElementAt(i).name);
                }
                else
                    this.running_balance_db.Add(runningTable.ElementAt(i).name, runningTable.ElementAt(i).value);
            }
            runningTable = null;
        }
    }
}
