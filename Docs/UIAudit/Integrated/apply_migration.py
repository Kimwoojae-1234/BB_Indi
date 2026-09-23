"""Prepare/apply the reviewed Unity candidates while preserving existing object IDs.

Run from the repository root. Unity must have processed begin-apply before --apply,
and end-apply must follow it. Backups and prepared files stay in ignored Library.
"""
import argparse
import hashlib
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
AUDIT = ROOT / "Docs/UIAudit/Integrated"
STAGE = ROOT / "Library/UGUIMigration/prepared"
BACKUP = ROOT / "Library/UGUIMigration/before-apply"
PLAN = json.loads((AUDIT / "candidate-plan.json").read_text())
TYPES = dict.fromkeys(("UILabel", "UISprite", "UITexture", "UIWidget"), "GameUIElement")
TYPES.update({"UIPanel": "GameUIPanel", "UITweener": "GameUITween", "TweenAlpha": "GameUITweenAlpha",
              "TweenPosition": "GameUITweenPosition", "TweenScale": "GameUITweenScale", "TweenRotation": "GameUITweenRotation",
              "UIGrid": "GameUIGrid", "UIScrollView": "GameUIScroll", "UISpriteAnimation": "GameUISpriteAnimation", "NGUITools": "GameUIRoot"})


def prepare():
    changes = {}
    # All surviving source identities, including Transforms replaced by RectTransforms,
    # retain their original IDs. Only new Unity UI components receive new IDs.
    for asset in PLAN["assets"]:
        text = (ROOT / asset["candidate"]).read_text(encoding="utf-8-sig")
        ids = {int(x) for x in re.findall(r"^--- !u!\d+ &(-?\d+)", text, re.M)}
        identity = {x["replacement"]: x["original"] for x in asset["identities"]}
        removed = {x["original"]: x["replacement"] for x in asset["components"]}
        keep_ids = {identity.get(x, x) for x in ids - removed.keys()}
        assert len(keep_ids) == len(ids - removed.keys()), "Object ID collision"
        final = {x: identity.get(x, x) for x in ids}
        final.update({old: identity.get(new, new) for old, new in removed.items()})
        blocks = re.split(r"(?=^--- !u!)", text, flags=re.M)
        output = []
        for block in blocks:
            header = re.match(r"--- !u!(\d+) &(-?\d+)", block)
            if header and int(header[2]) in removed:
                continue
            if header and header[1] == "1":
                block = re.sub(r"^  - component: \{fileID: (-?\d+)\}\n", lambda m: "" if int(m[1]) in removed else m[0], block, flags=re.M)
            # External GUID references are preserved; only local PPtrs are rewritten.
            block = re.sub(r"\{fileID: (-?\d+)\}", lambda m: "{fileID: " + str(final.get(int(m[1]), int(m[1]))) + "}", block)
            block = re.sub(r"^(--- !u!\d+ &)(-?\d+)", lambda m: m[1] + str(identity.get(int(m[2]), int(m[2]))), block, flags=re.M)
            output.append(block)
        text = "".join(output)
        local_refs = {int(x) for x in re.findall(r"\{fileID: (-?\d+)\}", text)} - {0}
        source = (ROOT / asset["source"]).read_text(encoding="utf-8-sig")
        old_ids = {int(x) for x in re.findall(r"^--- !u!\d+ &(-?\d+)", source, re.M)}
        old_missing = {int(x) for x in re.findall(r"\{fileID: (-?\d+)\}", source)} - old_ids - {0}
        assert not local_refs - keep_ids - old_missing, "New dangling local PPtr: " + asset["source"]
        changes[asset["source"]] = text

    controllers = {p for a in PLAN["assets"] for p in a["controllers"]}
    controllers.update("Assets/MainGame/Script/" + p for p in (
        "Character/Catcher.cs", "Character/Runner.cs", "Character/Fielder.cs", "Baseball/Field.cs",
        "Manager/CameraManager.cs", "Manager/BallPlayManager.cs", "UI/UGUI/FieldOverlayCanvas.cs"))
    for name in sorted(controllers):
        path = ROOT / name
        source = path.read_text(encoding="utf-8-sig")
        mapping = TYPES.copy()
        if path.name in ("UIScoreBoard.cs", "ControlRunner.cs", "pitchingSelectButton.cs"):
            # The earlier converters still inspect these unused legacy fields.
            for primitive in ("UILabel", "UISprite", "UITexture", "UIWidget"):
                mapping.pop(primitive)
        text = re.sub(r"\b(" + "|".join(mapping) + r")\b", lambda m: mapping[m[0]], source)
        if path.name == "UIScoreBoard.cs":
            text = text.replace("public UILabel timerLabel", "public GameUIElement timerLabel").replace("public UISprite timerGauge", "public GameUIElement timerGauge").replace("public UILabel batterTimer", "public GameUIElement batterTimer")
        if path.name in ("infoCard.cs", "QuickPlayerNameBar.cs"):
            # The old optional loader always returns null. Keep the converted prefab font.
            text = re.sub(r"^\s*(overallLabel|overall)\.bitmapFont = Util\.GetOverallFont\(overallNum\);", "\n            // The optional overall-font loader is disabled; retain the prefab font.", text, flags=re.M)
        if text != source:
            changes[name] = text.replace("using UnityEngine;", "using BaseBall.BallPlay.UGUI;\nusing UnityEngine;", 1)

    if not (BACKUP / "Assets/MainGame/Script/util/Util.cs").exists():
        name = "Assets/MainGame/Script/util/Util.cs"
        source = (ROOT / name).read_text(encoding="utf-8-sig")
        text = "using BaseBall.BallPlay.UGUI;\n" + source
        text = text.replace("Transform[] ts = obj.GetComponentsInChildren<Transform>();", "foreach (var element in obj.GetComponentsInChildren<GameUIElement>()) element.color = col;\n            Transform[] ts = obj.GetComponentsInChildren<Transform>();")
        pos = text.index("        public static void SetUILabelColor(UILabel")
        text = text[:pos] + """        public static void SetUILabelColor(GameUIElement label, int value)
            {
                Color[] colors = { new Color(.74f, .74f, .74f), new Color(.455f, .588f, .984f), Color.green, new Color(1, .9f, 0), Color.red };
                label.color = colors[MyMath.SetMinMax(value / 200, 0, 4)];
            }
            public static void SetSpritePixelPerfect(GameUIElement sprite, string name, bool pixelPerfect = true)
            {
                sprite.spriteName = name; if (pixelPerfect) sprite.MakePixelPerfect();
            }
    
    """ + text[pos:]
        for method, body in {
            "SetTweenerStart": "native.ResetToBeginning(); native.PlayForward();",
            "SetTween": "obj.SetActive(true); native.ResetToBeginning(); native.PlayForward();",
            "SetTweenReverse": "native.PlayReverse();",
        }.items():
            needle = "public static void " + method + "(GameObject obj)\n        {"
            assert needle in text
            text = text.replace(needle, needle + "\n            var native = obj.GetComponent<GameUITween>();\n            if (native != null) { " + body + " return; }")
        changes[name] = text

    manifest = []
    for name, text in changes.items():
        original = (ROOT / name).read_bytes()
        backup, stage = BACKUP / name, STAGE / name
        backup.parent.mkdir(parents=True, exist_ok=True)
        stage.parent.mkdir(parents=True, exist_ok=True)
        if backup.exists():
            assert backup.read_bytes() == original, "Original already changed: " + name
        else:
            backup.write_bytes(original)
        # Keep original UTF-8 BOM and line endings for source files.
        newline = "\r\n" if b"\r\n" in original else "\n"
        data = text.replace("\r\n", "\n").replace("\n", newline).encode("utf-8")
        if original.startswith(b"\xef\xbb\xbf"):
            data = b"\xef\xbb\xbf" + data
        stage.write_bytes(data)
        manifest.append({"path": name, "before": hashlib.sha256(original).hexdigest(), "after": hashlib.sha256(data).hexdigest()})
    (STAGE / "manifest.json").write_text(json.dumps(manifest, indent=2))
    print("Prepared", len(manifest), "files; originals unchanged.")


def apply():
    manifest = json.loads((STAGE / "manifest.json").read_text())
    for item in manifest:
        assert hashlib.sha256((ROOT / item["path"]).read_bytes()).hexdigest() == item["before"], "Changed since preparation: " + item["path"]
        assert hashlib.sha256((STAGE / item["path"]).read_bytes()).hexdigest() == item["after"]
    for item in manifest:
        (ROOT / item["path"]).write_bytes((STAGE / item["path"]).read_bytes())
    (AUDIT / "apply-manifest.json").write_text(json.dumps(manifest, indent=2))
    print("Applied", len(manifest), "files. Backups:", BACKUP)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--apply", action="store_true")
    parser.add_argument("--additional", action="store_true")
    args = parser.parse_args()
    if args.additional:
        paths = set((AUDIT / "additional-assets.txt").read_text().splitlines())
        PLAN["assets"] = [a for a in PLAN["assets"] if a["source"] in paths]
        STAGE = ROOT / "Library/UGUIMigration/prepared-additional"
    apply() if args.apply else prepare()
