#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using BaseBall.BallPlay;
using BaseBall.BallPlay.UGUI;

public static class RemainingUGUIMigration
{
    public const string Output = "Docs/UIAudit/NGUIComplete";
    [Serializable] public sealed class Inventory { public Entry[] assets; }
    [Serializable] public sealed class Entry { public string path; public ComponentInfo[] components; public string[] missingScripts; }
    [Serializable] public sealed class ComponentInfo { public string type, node, settings; public long id; }
    public static void Command(string command)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required.");
        if (command == "check") { RemainingUGUIChecks.Check(); return; }
        if (command == "play-controls") { RemainingUGUIControlChecks.Start(); return; }
        if (command == "play-connections") { CanvasConnectionPlayChecks.StartComplete(); return; }
        if (command == "capture-after") { CaptureResult("after"); return; }
        if (command == "capture-records") { CaptureResult("records"); return; }
        if (command == "dependencies")
        {
            var report = new List<string>();
            foreach (string path in AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith("Assets/") && !p.StartsWith("Assets/NGUI/") && !p.StartsWith("Assets/Editor/") && !AssetDatabase.IsValidFolder(p)))
                foreach (string dependency in AssetDatabase.GetDependencies(path, false).Where(p => p.StartsWith("Assets/NGUI/"))) report.Add(path + " -> " + dependency);
            File.WriteAllLines(Output + "/remaining-plugin-dependencies.txt", report); return;
        }
        throw new ArgumentException(command);
    }
    static void CaptureResult(string stage)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++) if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save scene edits first.");
        var setup = EditorSceneManager.GetSceneManagerSetup(); var previous = RenderTexture.active;
        RenderTexture texture = null; Texture2D pixels = null;
        try
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/MainGame/prefabs/resultUI/resultPrefab.prefab"));
            root.transform.position = Vector3.zero;
            var result = root.GetComponent<ResultUI>(); result.resultMain._active.SetActive(true);
            var board = result.resultMain.board; board.initScoreBoard("Away", "Home", 1, 2);
            int[] away = Enumerable.Repeat(SimulParm.NOPLAY_INNING, 12).ToArray(), home = (int[])away.Clone();
            for (int i = 0; i < 9; i++) { away[i] = i == 3 ? 6 : 0; home[i] = i == 6 ? 2 : 0; }
            board.setResult(away, home, new[] { 6, 14, 1 }, new[] { 2, 7, 1 }, 0);
            if (stage == "records")
            {
                var records=result.playerRecord;
                records._active.SetActive(true); records.batterObj.SetActive(true); records.pitcherObj.SetActive(false);
                records.batterView[0].SetActive(true); records.batterView[1].SetActive(false);
                var scroll=records.batterView[0].GetComponent<GameUIScroll>(); scroll.enabled=false;
                var panel=scroll.GetComponent<GameUIPanel>();
                for (int i=0;i<9;i++)
                {
                    var row=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/MainGame/prefabs/resultUI/resultBatterRecordBarPrefab.prefab"),scroll.transform,false).GetComponent<resultStatBar>();
                    row.transform.localPosition=new Vector3(0,150-42*i,0); row.transform.localScale=Vector3.one;
                    row.bgSpr.SetActive(i%2==0); row.logo.spriteName="logo_1"; row.pos.spriteName="position_"+(i==0?10:i+1);
                    if(!row.pos.sprites.TryGet(row.pos.spriteName,out var positionGlyph)) throw new InvalidOperationException("Missing record position glyph");
                    for(int j=0;j<row.label.Length;j++) row.label[j].text=j==0?(i+1).ToString():j==1?"Player "+(i+1):j==2?"89":"1";
                    row.GetComponent<GameUIDynamicUI>().RefreshUGUIHierarchy();
                    // The fixed nine-player list disables scrolling, so every row must fit.
                    foreach(var element in row.GetComponentsInChildren<GameUIElement>())
                    for(int corner=0;corner<4;corner++)
                    {
                        var pivot=element.pivotOffset;
                        var point=panel.ClipTransform.InverseTransformPoint(element.transform.TransformPoint(new Vector3(((corner&1)-pivot.x)*element.width,((corner>>1)-pivot.y)*element.height,0)));
                        if(point.y<panel.clipRegion.y-panel.clipRegion.w*.5f || point.y>panel.clipRegion.y+panel.clipRegion.w*.5f)
                            throw new InvalidOperationException("Record row clipped: "+i+" "+element.name+" y="+point.y+" clip="+panel.clipRegion);
                    }
                }
                scroll.RefreshContentBounds();
            }
            foreach (var panel in root.GetComponentsInChildren<GameUIPanel>()) GameUIRenderOrder.Register(panel);
            foreach (var element in root.GetComponentsInChildren<GameUIElement>()) GameUIRenderOrder.Register(element);
            foreach (var element in root.GetComponentsInChildren<GameUIElement>()) element.Apply();
            var camera = root.GetComponentInChildren<Camera>();
            texture = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = texture; camera.aspect = 1280f / 720;
            Canvas.ForceUpdateCanvases(); camera.Render();
            RenderTexture.active = texture; pixels = new Texture2D(1280, 720, TextureFormat.RGBA32, false);
            pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); pixels.Apply();
            File.WriteAllBytes(Output + "/result-" + stage + ".png", pixels.EncodeToPNG());
            var colors = pixels.GetPixels32(); int visible = colors.Count(c => !c.Equals(colors[0]));
            File.WriteAllText(Output + "/result-" + stage + "-capture.txt", "Visible pixels=" + visible + " camera=" + camera.transform.position + " size=" + camera.orthographicSize);
            if (visible < 1000) throw new InvalidOperationException("Blank result capture");
        }
        finally
        {
            RenderTexture.active = previous;
            if (pixels != null) UnityEngine.Object.DestroyImmediate(pixels);
            if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
            EditorSceneManager.RestoreSceneManagerSetup(setup);
        }
    }
}
#endif
