
namespace BaseBall.BallPlay
{
    public class SimulSkillResult
    {
        public static int skillFieder;
        public static SimulHitType hitType;

        public static VsResult fieldVs;
        public static CSkill runnerSkill;
        public static CSkill fielderSkill;
        public static bool bHoesimSkillInvalidity;  //회심의 일격에 의한 스킬 무효여부
        public static bool bCounterFieldSkill;      //필드스킬 카운터 여부

        
        /// <summary>
        /// 배팅시 발생하는 스킬 체크
        /// </summary>
        /// <param name="batterSkill"></param>
        /// <param name="pitchPitcherSkill"></param>
        /// <param name="pitchBatterSkill"></param>
        /// <returns></returns>
        public static SimulResultState GetSkillResult(CSkill batterSkill, CSkill pitchPitcherSkill, CSkill pitchBatterSkill, SimulFielder[] fielder)
        {
            bHoesimSkillInvalidity = false;
            bCounterFieldSkill = false;
            if (pitchPitcherSkill != null)
            {
                SkillID index = (SkillID)pitchPitcherSkill.ID;
                if (index == SkillID.hoe_sim_il_gyeog)
                {
                    if (batterSkill != null)
                    {
                        if (batterSkill.ID == (int)SkillID.tteun_geum_po)
                        {
                            //회심의 일격에 의해 강습타구, 뜬금포 무효화
                            bHoesimSkillInvalidity = true;
                        }
                    }
                    return setHeosim();
                }
            }

            

            if (batterSkill != null)
            {
                SkillID index = (SkillID)batterSkill.ID;

                if (index == SkillID.gang_seup_ta_gu)
                {
                    //강습타구
                    return setGangSeupTagu(fielder);
                }
                else if (index == SkillID.bunt_sin)
                {
                    //번트의신
                    return setBuntSin(fielder);
                }
                else if (index == SkillID.tteun_geum_po)
                {
                    //뜬금포
                    return setTtenGeumPo(fielder);
                }

            }


            if (pitchBatterSkill != null)
            {
                SkillID index = (SkillID)pitchBatterSkill.ID;
                if (index == SkillID.mea_nun)
                {
                    return setMeaNun();
                }
            }


            return SimulResultState.NONE;
        }


        //필드 카운터 스킬 발동
        private const int FIELD_COUNTER_PERCENT = 50;

        /// <summary>
        /// 강습타구를 설정한다
        /// </summary>
        /// <returns></returns>
        private static SimulResultState setGangSeupTagu(SimulFielder[] fielder)
        {
            skillFieder = UnityEngine.Random.Range(CPlayer._FIRSTBASEMAN, CPlayer._SHORTSTOP + 1);
            hitType = SimulHitType.Grounder;

            if (MyMath.Percent() < FIELD_COUNTER_PERCENT)
            {
                //대항 필드 스킬
                CPlayer curFielder = fielder[skillFieder].getFielder();
                //야수 스페셜캐치 발동시
                if (curFielder.fieldSkillSuccess(SkillIndex.SpecialCatch) == true)
                {
                    fielderSkill = curFielder.getSkillValue(SkillIndex.SpecialCatch);
                    bCounterFieldSkill = true;
                    return SimulResultState.Grounder;
                }
            }
            
            return SimulResultState.InfieldSingle;
        }

        /// <summary>
        /// 번트의 신을 설정한다
        /// </summary>
        /// <returns></returns>
        private static SimulResultState setBuntSin(SimulFielder[] fielder)
        {
            int per = MyMath.Percent();
            if (per < 33) skillFieder = CPlayer._PITCHER;
            else if (per < 66) skillFieder = CPlayer._FIRSTBASEMAN;
            else skillFieder = CPlayer._THIRDBASEMAN;

            hitType = SimulHitType.Bunt;

            if (MyMath.Percent() < FIELD_COUNTER_PERCENT)
            {
                //대항 필드 스킬
                CPlayer curFielder = fielder[skillFieder].getFielder();
                if (skillFieder == CPlayer._PITCHER)
                {
                    //투수 번트수비 발동시
                    if (curFielder.fieldSkillSuccess(SkillIndex.PitcherBuntFielding) == true)
                    {
                        fielderSkill = curFielder.getSkillValue(SkillIndex.PitcherBuntFielding);
                        bCounterFieldSkill = true;
                        return SimulResultState.Grounder;
                    }
                }
                else
                {
                    //야수 특급송구 발동시
                    if (curFielder.fieldSkillSuccess(SkillIndex.SpecialThrow) == true)
                    {
                        fielderSkill = curFielder.getSkillValue(SkillIndex.SpecialThrow);
                        bCounterFieldSkill = true;
                        return SimulResultState.Grounder;
                    }
                }
            }

            return SimulResultState.BuntSingle;
        }


        //홈런스틸 발동
        private const int HR_STEAL_PERCENT = 10;
        /// <summary>
        /// 뜬금포를 설정한다
        /// </summary>
        /// <returns></returns>
        private static SimulResultState setTtenGeumPo(SimulFielder[] fielder)
        {
            skillFieder = UnityEngine.Random.Range(CPlayer._LEFTFIELDER, CPlayer._RIGHTFIELDER + 1);
            hitType = SimulHitType.Fly;

            //대항수비 쇠그물수비 발동시
            //if (MyMath.Percent() < HR_STEAL_PERCENT)
            {
                CPlayer curFielder = fielder[skillFieder].getFielder();
                if (curFielder.fieldSkillSuccess(SkillIndex.HomerunSteal) == true)
                {
                    fielderSkill = curFielder.getSkillValue(SkillIndex.HomerunSteal);
                    bCounterFieldSkill = true;
                    return SimulResultState.FlyOut;
                }
            }

            return SimulResultState.HomeRun;
        }

        /// <summary>
        /// 회심의 일격을 설정한다
        /// </summary>
        /// <returns></returns>
        private static SimulResultState setHeosim()
        {
            skillFieder = UnityEngine.Random.Range(CPlayer._FIRSTBASEMAN, CPlayer._SHORTSTOP + 1);
            hitType = SimulHitType.Grounder;
            return SimulResultState.Grounder;
        }

        /// <summary>
        /// 매의눈을 설정한다.
        /// </summary>
        /// <returns></returns>
        private static SimulResultState setMeaNun()
        {
            skillFieder = UnityEngine.Random.Range(CPlayer._LEFTFIELDER, CPlayer._RIGHTFIELDER + 1);
            hitType = SimulHitType.Liner;

            int per = MyMath.Percent();
            if (per < 33) return SimulResultState.Single;
            else if (per < 90) return SimulResultState.Double;
            else return SimulResultState.Triple;
        }


        /// <summary>
        /// 그라운드 스킬을 검색할 확률
        /// </summary>
        private const int GROUNDER_SKILL_PERCENT = 25;
        /// <summary>
        /// 그라운드시 발생하는 필드 스킬 체크
        /// </summary>
        /// <param name="runner"></param>
        /// <param name="fielder"></param>
        /// <param name="fIndex"></param>
        /// <returns></returns>
        public static SimulResultState GetGrounderFieldSkill(CPlayer runner, CPlayer fielder, int fIndex, bool bChrisma)
        {
            fieldVs = VsResult.None;
            runnerSkill = null;
            fielderSkill = null;
            SimulResultState skillResult = SimulResultState.NONE;

            if (fIndex < CPlayer._LEFTFIELDER)
            {
                int per = MyMath.Percent();

                if (per < GROUNDER_SKILL_PERCENT || bChrisma == true) //25%확률로 체크
                {
                    //스페셜 캐치
                    if (fielder.fieldSkillSuccess(SkillIndex.SpecialCatch) == true)
                    {
                        fielderSkill = fielder.getSkillValue(SkillIndex.SpecialCatch);
                        return SimulResultState.Grounder;
                    }
                    else
                    {
                        //스페셜 송구
                        if (fielder.fieldSkillSuccess(SkillIndex.SpecialThrow) == true)
                        {
                            fielderSkill = fielder.getSkillValue(SkillIndex.SpecialThrow);
                            if ((fIndex == CPlayer._SHORTSTOP && fIndex == CPlayer._SECONDBASEMAN && fIndex == CPlayer._THIRDBASEMAN) &&
                                (runner.fieldSkillSuccess(SkillIndex.RunnerTurbo) == true && MyMath.Percent() < 20))
                            {
                                //주루센스 vs
                                runnerSkill = runner.getSkillValue(SkillIndex.RunnerTurbo);
                                bool bOffenseWin = SimulParm.checkOffenseSkillWin(runner.getSkillRank(SkillIndex.RunnerTurbo), fielder.getSkillRank(SkillIndex.SpecialThrow));
                                fieldVs = bOffenseWin ? VsResult.OffenseWin : VsResult.DefenseWin;
                                if (bOffenseWin == true)
                                {
                                    return SimulResultState.InfieldSingle;
                                }
                                else
                                {
                                    return SimulResultState.Grounder;
                                }
                            }
                            else
                            {
                                return SimulResultState.Grounder;
                            }
                        }
                        else
                        {
                            //주루센스
                            if ((fIndex == CPlayer._SHORTSTOP && fIndex == CPlayer._SECONDBASEMAN && fIndex == CPlayer._THIRDBASEMAN) &&
                                runner.fieldSkillSuccess(SkillIndex.RunnerTurbo) == true)
                            {
                                runnerSkill = runner.getSkillValue(SkillIndex.RunnerTurbo);
                                return SimulResultState.InfieldSingle;
                            }
                        }
                    }
                }

            }
            return skillResult;
        }


        /// <summary>
        /// 플라이 스킬을 검색한 확률
        /// </summary>
        private const int FLY_SKILL_PERCENT = 15;

        /// <summary>
        /// 플라이시 발생하는 스킬 체크
        /// </summary>
        /// <param name="fielder"></param>
        /// <param name="fIndex"></param>
        /// <returns></returns>
        public static SimulResultState GetFlyFieldSkill(CPlayer fielder, int fIndex, bool bChrisma)
        {
            fieldVs = VsResult.None;
            runnerSkill = null;
            fielderSkill = null;
            SimulResultState skillResult = SimulResultState.NONE;
            if (fIndex >= CPlayer._LEFTFIELDER)
            {
                int per = MyMath.Percent();

                if (per < FLY_SKILL_PERCENT || bChrisma == true) //15%확률로 체크 혹은 카리스마 발동시
                {
                    //쇠그물 수비 발생
                    if (fielder.fieldSkillSuccess(SkillIndex.DivingCatch) == true)
                    {
                        fielderSkill = fielder.getSkillValue(SkillIndex.DivingCatch);
                        return SimulResultState.FlyOut;
                    }
                }
            }


            return skillResult;
        }
    }

}