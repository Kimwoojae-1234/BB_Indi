"""Verify the Step 6 compatibility fix preserves unrelated UI and serialized IDs."""
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[3]
BASE = "0d694c64"
RESULT = "Assets/Resources/MainGame/prefabs/resultUI/resultPrefab.prefab"
UGUI = "bfde591c7b78b6440b9a4ad6535abf2f"
LEGACY = "9cb05132fe8f4d8d95f7a82bbc1cef70"


def original(path):
    return subprocess.check_output(["git", "show", f"{BASE}:{path}"], cwd=ROOT).decode("utf-8-sig").replace("\r\n", "\n")


def current(path):
    return (ROOT / path).read_text(encoding="utf-8-sig")


old = original(RESULT)
assert old.count(UGUI) == 1
assert current(RESULT) == old.replace(UGUI, LEGACY), "Result prefab changed beyond its scoreboard script GUID"
print("PASS legacy result prefab: only scoreboard script GUID changed; all fileIDs, object references and callbacks preserved")

for path in [
    "Assets/Resources/MainGame/prefabs/QuickUI/QuickSimulatorPrefab.prefab",
    "Assets/Resources/MainGame/prefabs/gameUI/IngameUIPrefab.prefab",
    "Assets/MainGame/Script/UI/ui_etc/scoreboard.cs",
    "Assets/MainGame/Script/UI/ui_etc/skillUISetter.cs",
    "Assets/MainGame/Script/simulate/SimulPlayer.cs",
    "Assets/MainGame/Script/etc/tempPlayerData.cs",
]:
    assert current(path) == original(path), f"Unexpected change: {path}"
print("PASS live UGUI scoreboards, previous skill cleanup fix and offline player generation unchanged")

legacy_owners = []
ugui_owners = []
for path in (ROOT / "Assets/Resources/MainGame/prefabs").rglob("*.prefab"):
    data = path.read_bytes()
    if LEGACY.encode("ascii") in data:
        legacy_owners.append(path.relative_to(ROOT).as_posix())
    if UGUI.encode("ascii") in data:
        ugui_owners.append(path.name)
assert legacy_owners == [RESULT], legacy_owners
assert sorted(ugui_owners) == ["IngameUIPrefab.prefab", "QuickSimulatorPrefab.prefab"], ugui_owners
assert "public LegacyResultScoreboard board;" in current("Assets/MainGame/Script/UI/resultUI/UIResultMain.cs")
assert "public scoreboard board;" in current("Assets/MainGame/Script/QuickGame/QuickSimulator.cs")
assert "public scoreboard board;" in current("Assets/MainGame/Script/UI/UIChangeInning.cs")
print("PASS legacy NGUI result and live UGUI scoreboard consumers are separated")
print("OVERALL PASS")
