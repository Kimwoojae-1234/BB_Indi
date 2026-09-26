using TMPro;
using BaseBall.BallPlay.UGUI;
using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class Util
    {
        public static void DestroyChildren(Transform parent)
        {
            while (parent.childCount > 0)
            {
                var child = parent.GetChild(0);
                if (Application.isPlaying)
                {
                    child.SetParent(null, true);
                    Object.Destroy(child.gameObject);
                }
                else Object.DestroyImmediate(child.gameObject);
            }
        }

        public static bool GetPercent(int per)
        {
            if (Random.Range(0, 100) < per)
            {
                return true;
            }
            else return false;
        }



        public static void ChangeLayersRecursively(Transform trans, string name)
        {
            trans.gameObject.layer = LayerMask.NameToLayer(name);
            foreach (Transform child in trans)
            {
                ChangeLayersRecursively(child, name);
            }
        }


        

        public static string GetAvgString(int ab, int hit)
        {
            int avg;
            
            if(ab ==0 || hit==0) avg = 0;
            else avg = hit * 1000 / ab;

            if(avg>=1000) 
                return (avg / 1000) + "." + (avg % 1000).ToString("000");
            else
                return "0." + (avg % 1000).ToString("000");

        }

        public static string GetSlugString(int ab, int hit, int _2B, int _3B, int hr)
        {
            int _1B = hit - (_2B + _3B + hr);

            int ruta = _1B + 2 * _2B + 3 * _3B + 4 * hr;

            int slg;
            if (ab == 0 || ruta == 0) slg = 0;
            else slg = ruta * 1000 / ab;

            if (slg >= 1000)
                return (slg / 1000) + "." + (slg % 1000).ToString("000");
            else
                return "0." + (slg % 1000).ToString("000");

        }

        public static string GetOpsString(int pa, int ab, int hit, int _2B, int _3B, int hr, int bb, int hbb)
        {
            int _1B = hit - (_2B + _3B + hr);
            int ruta = _1B + 2 * _2B + 3 * _3B + 4 * hr;
            int slg;
            if (ab == 0 || ruta == 0) slg = 0;
            else slg = ruta * 1000 / ab;

            int obp;
            int chulu = (hit+bb+hbb);
            if (pa == 0 || chulu == 0) obp = 0;
            else obp = chulu * 1000 / pa;

            int ops = obp + slg;
            if (ops >= 1000)
                return (ops / 1000) + "." + (ops % 1000).ToString("000");
            else
                return "0." + (ops % 1000).ToString("000");
            
        }

        public static void ChangeChildObjColor(GameObject obj, Color col)
        {
            foreach (var element in obj.GetComponentsInChildren<GameUIElement>()) element.color = col;
            Transform[] ts = obj.GetComponentsInChildren<Transform>();
            if (ts == null)
                return;

            foreach (Transform t in ts)
            {
                if (t != null)
                {
                    tk2dBaseSprite spr = t.GetComponent<tk2dBaseSprite>();//.color = new Color(1, 1, 1, 0.1f);
                    if (spr != null)
                    {
                        spr.color = col;
                    }

                    SpriteRenderer spr2 = t.GetComponent<SpriteRenderer>();
                    if (spr2 != null)
                    {
                        spr2.color = col;
                    }

                    TextMesh text = t.GetComponent<TextMesh>();
                    if (text != null)
                    {
                        text.color = col;
                    }

                    GameUIElement uiSpr = t.GetComponent<GameUIElement>();
                    if (uiSpr != null)
                    {
                        uiSpr.color = col;
                    }

                    GameUIElement widget = t.GetComponent<GameUIElement>();
                    if (widget != null)
                    {
                        widget.alpha = col.a;
                    }
                }
            }
        }

        public static void RemoveChild(Transform trans)
        {
            foreach (Transform child in trans)
            {
                if (child != null)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }




        public static GameObject Load(string res, Transform parent, Vector3 pos, string name = null)
        {
            GameObject _obj = GameObject.Instantiate(Resources.Load(res), Vector3.zero, Quaternion.identity) as GameObject;
            if (parent != null)
            {
                _obj.transform.parent = parent.transform;
                _obj.transform.localPosition = pos;
            }
            else
            {
                _obj.transform.position = pos;
            }

            if (name != null)
            {
                _obj.name = name;
            }

            return _obj;
        }

        public static void LoadToast(string str, Transform trans, Vector3 pos, string layer = null )
        {
            /*
            GameObject obj = Util.Load("MainGame/prefabs/ControlUI/toastPrefab", trans, pos);//new Vector3(0, 110, -8));
            obj.transform.FindChild("txt").gameObject.GetComponent<TextMesh>().text = str;
            obj.transform.localScale = new Vector3(1, 1, 1);
            if (layer != null)
            {
                ChangeLayersRecursively(obj.transform, layer);
            }
            GameObject.Destroy(obj, 1.0f);*/
        }


        public static string GetErrString(int inningCount, int errCount)
        {
            int err;
            if (inningCount > 0)
            {
                err = (errCount * 27 * 100) / inningCount;
            }
            else
            {
                err = (errCount > 0 ? 9999 : 0);
            }
            return ((err / 100) + "." + (err % 100).ToString("00"));
        }

        public static string GetWHIPString(int inningCount, int chuluNum)
        {
            if (inningCount == 0) return "99.99";
            int whip = (chuluNum * 100 * 3) / inningCount;
            return ((whip / 100) + "." + (whip % 100).ToString("00"));
        }

        public static void SetUILabelColor(GameUIElement label, int value)
        {
            Color[] colors = { new Color(.74f, .74f, .74f), new Color(.455f, .588f, .984f), Color.green, new Color(1, .9f, 0), Color.red };
            label.color = colors[MyMath.SetMinMax(value / 200, 0, 4)];
        }
        public static void SetSpritePixelPerfect(GameUIElement sprite, string name, bool pixelPerfect = true)
        {
            sprite.spriteName = name; if (pixelPerfect) sprite.MakePixelPerfect();
        }



        public static string GetPositionString(int pos)
        {
            string[] posStr = new string[10] { L10n.T("UI.Label.P"), L10n.T("UI.Label.C"), L10n.T("Baseball.Position.FirstBaseAbbreviation"), L10n.T("Baseball.Position.SecondBaseAbbreviation"), L10n.T("Baseball.Position.ThirdBaseAbbreviation"), L10n.T("Baseball.Position.ShortstopAbbreviation"), L10n.T("Baseball.Position.LeftFieldAbbreviation"), L10n.T("Baseball.Position.CenterFieldAbbreviation"), L10n.T("Baseball.Position.RightFieldAbbreviation"), L10n.T("Baseball.Position.DesignatedHitterAbbreviation") };

            return posStr[pos];
        }

        public static string GetPositionString2(int pos)
        {
            string[] posStr = new string[10] { L10n.T("UI.Label.P"), L10n.T("UI.Label.C"), L10n.T("Baseball.Position.FirstBaseAbbreviation"), L10n.T("Baseball.Position.SecondBaseAbbreviation"), L10n.T("Baseball.Position.ThirdBaseAbbreviation"), L10n.T("Baseball.Position.ShortstopAbbreviation"), L10n.T("Baseball.Position.LeftFieldAbbreviation"), L10n.T("Baseball.Position.CenterFieldAbbreviation"), L10n.T("Baseball.Position.RightFieldAbbreviation"), L10n.T("Baseball.Position.DesignatedHitterAbbreviation") };

            return posStr[pos];
        }

        public static string GetPositionStringEng(int pos)
        {
            string[] posStr = new string[10] { L10n.T("Baseball.Position.DesignatedHitterAbbreviation"), L10n.T("UI.Label.C"), L10n.T("Baseball.Position.FirstBaseAbbreviation"), L10n.T("Baseball.Position.SecondBaseAbbreviation"), L10n.T("Baseball.Position.ThirdBaseAbbreviation"), L10n.T("Baseball.Position.ShortstopAbbreviation"), L10n.T("Baseball.Position.LeftFieldAbbreviation"), L10n.T("Baseball.Position.CenterFieldAbbreviation"), L10n.T("Baseball.Position.RightFieldAbbreviation"), L10n.T("Baseball.Position.DesignatedHitterAbbreviation") };

            return posStr[pos];
        }

        //문자중계용 
        //기타
        public static string getBatterResult(int fIndex, bool bGround, int hitCount, string type)
        {
            ////UnityEngine.//Debug.Log("==========================================>>hitCount = " + hitCount);
            string dir = "";
            if (Mode.gameMode == Mode.GamePlayMode.Ranking)
            {
                if (hitCount > 0)
                {
                    //string[] name = new string[5] { "아웃", "안타", "2루타", "3루타", "홈런" };
                    if (hitCount == 1)
                    {
                        dir = L10n.F("UI.Format.InFrontOfValue", Util.GetPositionString(fIndex));
                        if (fIndex < CPlayer._LEFTFIELDER)
                        {
                            dir += L10n.T("UI.Label.Infield");
                        }
                        dir += L10n.T("Baseball.Stat.HitsShort.Ballplay");
                    }
                    else
                    {
                        if (fIndex == CPlayer._LEFTFIELDER) dir = L10n.T("UI.Label.ToLeftField.Padded");
                        else if (fIndex == CPlayer._CENTERFIELDER) dir = L10n.T("UI.Label.ToCenterField");
                        else if (fIndex == CPlayer._RIGHTFIELDER) dir = L10n.T("UI.Label.ToRightField.Padded");

                        if (hitCount == 2) dir += L10n.T("Baseball.Result.Double");
                        else if (hitCount == 3) dir += L10n.T("Baseball.Result.Triple");
                        else dir += L10n.T("Baseball.Result.HomeRun");
                    }
                }
                else
                {
                    if (bGround == true)
                    {
                        dir = L10n.F("UI.Format.InFrontOfValue", Util.GetPositionString(fIndex));
                    }
                    else
                    {
                        dir = Util.GetPositionString(fIndex) + " ";
                    }
                }
            }
            return (dir + type);
        }


        public static Texture loadBigLogo(int index)
        {
            //return Resources.Load("MainGame/Texture/logo/teambig" + index) as Texture;
            return Resources.Load("TeamLogo/Middle/LOGO00" + index + "_M") as Texture;
        }

        public static Texture loadMiddleLogo(int index)
        {
            return Resources.Load("MainGame/Texture/logo/team" + index) as Texture;
        }

        public static Texture loadSmallLogo(int index)
        {
            return Resources.Load("MainGame/Texture/logo/team_small" + index) as Texture;
        }


        public static Sprite MakeCaptureSprite(Camera camera, int w = 1280, int h = 720)
        {
            RenderTexture tempRT = new RenderTexture(w, h, 24);
            camera.targetTexture = tempRT;
            BaseBall.BallPlay.UGUI.GameUIRoot.RenderForCapture(camera);

            Texture2D virtualPhoto = new Texture2D(w, h, TextureFormat.RGBA32, false);

            RenderTexture.active = tempRT;
            virtualPhoto.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);

            virtualPhoto.Apply();
            RenderTexture.active = null;
            camera.targetTexture = null;

            return Sprite.Create(virtualPhoto, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));

        }



        public static Sprite MakeSolidColorCapture(Camera camera, Color solidColor, int w = 1280, int h = 720)
        {
            RenderTexture tempRT = new RenderTexture(w, h, 24);
            camera.targetTexture = tempRT;
            BaseBall.BallPlay.UGUI.GameUIRoot.RenderForCapture(camera);

            Texture2D virtualPhoto = new Texture2D(w, h, TextureFormat.RGBA32, false);

            RenderTexture.active = tempRT;
            virtualPhoto.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);

            if (solidColor == Color.gray)
            {
                ConvertToGrayscale(virtualPhoto);
            }
            else
            {
                ConvertToSolidColor(virtualPhoto, solidColor);
            }

            virtualPhoto.Apply();
            RenderTexture.active = null;
            camera.targetTexture = null;

            return Sprite.Create(virtualPhoto, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));

        }

        static void ConvertToGrayscale(Texture2D graph)
        {
            Color32[] pixels = graph.GetPixels32();
            for (int x = 0; x < graph.width; x++)
            {
                for (int y = 0; y < graph.height; y++)
                {
                    Color32 pixel = pixels[x + y * graph.width];
                    //if (pixel.a != 0)// && (pixel.r == 0 && pixel.g == 0 && pixel.b == 0) == false)
                    {
                        int p = ((256 * 256 + pixel.r) * 256 + pixel.b) * 256 + pixel.g;
                        int b = p % 256;
                        p = Mathf.FloorToInt(p / 256);
                        int g = p % 256;
                        p = Mathf.FloorToInt(p / 256);
                        int r = p % 256;
                        float l = (0.2126f * r / 255f) + 0.7152f * (g / 255f) + 0.0722f * (b / 255f);

                        float alpha = pixel.a / 255.0f;

                        Color c = new Color(l, l, l, alpha);
                        graph.SetPixel(x, y, c);
                    }
                }
            }
        }


        static void ConvertToSolidColor(Texture2D graph, Color solidColor)
        {
            //
            Color fillColor = solidColor;
            Color[] fillColorArray = graph.GetPixels();
            for (int i = 0; i < fillColorArray.Length; ++i)
            {
                float alpha = fillColorArray[i].a;
                fillColorArray[i] = fillColor;
                fillColorArray[i].a = alpha;
            }
            graph.SetPixels(fillColorArray);
            //
        }


        public static Texture MakeCaptureTexture(Camera camera)
        {
            RenderTexture tempRT = new RenderTexture(1280, 720, 24);
            camera.targetTexture = tempRT;
            camera.Render();


            return tempRT;

        }



        public static string getPitcherposSprite(CPlayer player)
        {
           /* PitcherPosotion pos = (PitcherPosotion)player.getPitcherPosition();
            if (pos == PitcherPosotion.STARTER) return "position_starter";
            else if (pos == PitcherPosotion.SAVE) return "position_close";
            else return "position_releif";*/

            return "position_starter";
        }

        public static string getPitcherposString(CPlayer player)
        {
            /*PitcherPosotion pos = (PitcherPosotion)player.getPitcherPosition();
            if (pos == PitcherPosotion.STARTER) return "선발";
            else if (pos == PitcherPosotion.SAVE) return "마무리";
            else return "중계";*/
            return L10n.T("UI.Label.Sp");
        }


        public static string pitcherAchieve(CPlayer player)
        {
            if (player.getStat(Param.ST_PW) == Param.P_ACHIEVE_COMPLETE)
            {
                return L10n.T("UI.Label.W");
            }
            else if (player.getStat(Param.ST_PL) == Param.P_ACHIEVE_COMPLETE)
            {
                return L10n.T("UI.Label.L");
            }
            else if (player.getStat(Param.ST_SV) == Param.P_ACHIEVE_COMPLETE)
            {
                return L10n.T("UI.Label.S");
            }
            else if (player.getStat(Param.ST_HLD) == Param.P_ACHIEVE_COMPLETE)
            {
                return L10n.T("Baseball.Stat.HitsAbbreviation");
            }
            else if (player.getStat(Param.ST_BS) == Param.P_ACHIEVE_COMPLETE)
            {
                return L10n.T("UI.Label.Bs");
            }
            else
            {
                return "-";
            }
        }

        //임시
        public static string getStadiumName(Mode.StadiumType type)
        {
            if (type == Mode.StadiumType.ChampionsField)
            {
                return L10n.T("UI.Label.GwangjuKiaChampionsField");
            }
            else if (type == Mode.StadiumType.Dome)
            {
                return L10n.T("UI.Label.GocheokSkyDome");
            }
            else if (type == Mode.StadiumType.HanhwaField)
            {
                return L10n.T("UI.Label.DaejeonHanwhaLifeEaglesPark");
            }
            else if (type == Mode.StadiumType.LionsPark)
            {
                return L10n.T("UI.Label.DaeguSamsungLionsPark");
            }
            else if (type == Mode.StadiumType.HappyDream)
            {
                return L10n.T("UI.Label.IncheonSkHappyDreamPark");
            }
            else
            {
                return L10n.T("UI.Label.SeoulJamsilBaseballStadium");
            }
        }

        /*
        public static string RandomAvg()
        {
            float avg = Random.Range(200, 350) / 1000.0f;
            return string.Format("{0:F3}", avg);
        }

        public static string RandomErr()
        {
            float err = Random.Range(100, 450) / 100.0f;
            return string.Format("{0:F2}", err);
        }*/


        public static string GetCurAvg(WebConnector.GameRecordHitter record, int addHit, int addAB)
        {
            float avg = 0;
            int baseH = 0;
            int baseAB = 0;

            if(record != null)
            {
                baseH = record.hH;
                baseAB = record.hAB;
            }
            float totalH = baseH + addHit;
            float totalAB = baseAB + addAB;

            if (totalAB != 0)
            {
                avg = totalH / totalAB;
            }
            return string.Format("{0:F3}", avg);
        }

        public static string GetCurErr(WebConnector.GameRecordPitcher record, int addErr, int addOC)
        {
            float err = 0;
            int baseErr = 0;
            int baseOc = 0;

            if (record != null)
            {
                baseErr = record.pER;
                baseOc = record.pOC;
            }
            float totalErr = baseErr + addErr;
            float totalOc = baseOc + addOC;

            if (baseOc != 0)
            {
                err = (totalErr * 9 * 3) / totalOc;
            }
            return string.Format("{0:F2}", err);
        }


        /*
        public static int getTotalExp(int level, int exp)
        {
            int totalExp = exp;
            if (level > 1)
            {
                for (int i = 1; i < level; i++)
                {
                    // DISABLED_MGRS: totalExp += Mgrs.GameData.GameDB_FindTeamLevel_Exp(i);
                }
            }

            return totalExp;
        }*/

        public static int getPlayerTotalExp(int level, int grade, int exp)
        {
            int totalExp = exp;
            if (level > 1)
            {
                for (int i = 1; i < level-1; i++)
                {
                    // DISABLED_MGRS: totalExp += Mgrs.GameData.FindCardExpDemand(i, grade);
                }
            }

            return totalExp;
        }

        public static void SetTweenerStart(GameObject obj)
        {
            var native = obj.GetComponent<GameUITween>();
            if (native != null) { native.ResetToBeginning(); native.PlayForward(); return; }
            GameUITween tween = obj.GetComponent<GameUITween>();
            tween.ResetToBeginning();
            tween.enabled = true;
            tween.PlayForward();
        }





        public static TMP_FontAsset GetOverallFont(int overallNum)
        {
            /*if (overallNum >= 100)
            {
                // DISABLED_MGRS: return Mgrs.DataLoad.LoadFont("bitmapfont_card_yellow_num").GetComponent<TMP_FontAsset>();
            }
            else
            {
                // DISABLED_MGRS: return Mgrs.DataLoad.LoadFont("bitmapfont_card_silver_num").GetComponent<TMP_FontAsset>();
            }*/
            return null;
        }


        public static void SetTween(GameObject obj)
        {
            var native = obj.GetComponent<GameUITween>();
            if (native != null) { obj.SetActive(true); native.ResetToBeginning(); native.PlayForward(); return; }
            obj.SetActive(true);
            GameUITween tween = obj.GetComponent<GameUITween>();
            tween.ResetToBeginning();
            tween.PlayForward();
        }

        public static void SetTweenReverse(GameObject obj)
        {
            var native = obj.GetComponent<GameUITween>();
            if (native != null) { native.PlayReverse(); return; }
            //obj.SetActive(true);
            GameUITween tween = obj.GetComponent<GameUITween>();
            //tween.ResetToBeginning();
            tween.PlayReverse();
        }

        public static void SetAnimation(Animator anim, string animationName)
        {
            // = gameObject.GetComponent<Animator>();
            anim.enabled = true;
            anim.Rebind();
            anim.Play(Animator.StringToHash(animationName));
        }
    }
}
