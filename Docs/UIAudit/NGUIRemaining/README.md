# NGUI 제거 전 잔존 의존성 확인

> 이 문서는 **전환 전 상태**를 기록한 자료다. 후속 작업에서 남은 UI를 UGUI로 전환하고 NGUI를 제거했다. 현재 결과와 검증 범위는 [전체 전환 완료 보고서](../NGUIComplete/README.md)를 참조한다.

2026-09-26, 기준 `53f75713e7c4d398b8cd13f795b48102e97454b1`.

**NGUI를 사용하는 UI와 코드가 남아 있다. 현재 상태에서 플러그인을 삭제하면 레거시 결과/테스트 UI의 연결이 끊기고, 전환된 경기 컨트롤러와 에디터 검사 코드도 컴파일되지 않는다.** 이번 요청은 선행 조사로 처리했으며 `Assets/NGUI` 및 게임 코드·씬·프리팹은 변경하지 않았다.

## 실제 실행을 확인한 화면

비 RTTS 로컬 경기 결과 `Assets/Resources/MainGame/prefabs/resultUI/resultPrefab.prefab`에 **NGUI 컴포넌트 537개**가 남아 있다. 점수판, 팀 통계, 경기 기록, 성장/보상 및 끝내기 결과 하위 화면이 포함된다. 이 수치는 비활성 자식을 포함한 프리팹 전체이며 537개가 동시에 표시된다는 뜻은 아니다.

`BallPlayManager.setResult()`는 정상 RTTS 결과를 `RttsManager.CompleteLiveGame()`으로 넘기지만, 비 RTTS 경로에서는 `LoadGameResult()`로 이 프리팹을 생성한다. `CompleteLiveGame()`이 false를 반환하는 경우에도 레거시 경로에 도달할 수 있다. [호출 코드](../../../Assets/MainGame/Script/Manager/BallPlayManager.cs), [RTTS 결과 분기](../../../Assets/Scripts_New/Manager/RttsManager.cs).

직전 6단계에서 이 결과 화면을 실제 표시해 6:2 경기 결과와 로비 복귀를 확인했다. 따라서 이 경로에서는 미사용 자산이 아니다. [결과 화면](../CanvasRestructure/Step6/Retest/result.png), [활성 NGUI 목록](../CanvasRestructure/Step6/Retest/result.txt).

지난 단계의 `LegacyResultScoreboard`는 공유 점수판의 UGUI 전환으로 끊어진 **레거시 NGUI 연결을 복구한 코드**다. 결과 화면 전체를 UGUI로 전환한 것은 아니다. 정상 RTTS의 새 `Popup_GameResult`와 구분한다.

## 자산에서 확인한 나머지 UI

| 대상 | 직접 NGUI 컴포넌트 | 사용 상태 |
|---|---:|---|
| `MainGame/Scene/Scene1.unity` | 26 | 현재 빌드 목록에 활성 등록. `tempSelectPage`의 기존 테스트 선택 UI. 이번 조사에서 직접 진입하지 않음 |
| `MainGame/PVP_Tester/Scene/Login.unity` | 150 | 현재 빌드 목록에 활성 등록. 로그인·팀 선택·매칭 대기 UI. `lobbyManager`, `UI_pvpwaiting`가 UILabel을 사용 |
| `MainGame/PVP_Tester/Scene/Disconnect.unity` | 6 | 현재 빌드 목록에 활성 등록. 연결 종료 UI. 현재 일반 플레이의 진입 여부는 별도 확인 필요 |
| `resultUI/resultBatterRecordBarPrefab`, `resultPitcherRecordBarPrefab` | 각 15 | 레거시 경기 기록 화면이 코드에서 동적 생성 |
| `resultUI/resultPlayerGrowPrefab` | 16 | 레거시 성장 화면이 코드에서 동적 생성 |
| `resultUI/resultLeagueReaderPrefab`, `resultSeasonRewardBarPrefab`, `rewardLightPrefab`, `rewardObjPrefab` | 23 / 7 / 2 / 7 | 레거시 결과·보상 자산. 각 조건의 실제 실행은 미검증 |
| `ControlUI/miniRunner2.prefab` | 3 | 이전 주자 UI. `UIFieldUI`에 UGUI 뷰가 없을 때 사용하는 대체 경로가 남음. 현재 전환 프리팹의 실제 경기는 UGUI 사용 |
| `eventUI/openingPrefab`, `Texture/info`, `gameUI/overallNumPrefab`, `gameUI/popupBgPrefab`, `QuickUI/simulPlayerNameBar` | 55 / 44 / 3 / 6 / 5 | 기존 화면 자산 잔존. 현재 정상 경로에서 사용한다고 확정하지 않음 |
| `Pitch/pitcherPrefab`, `Pitch/PitchSystem` | 각 1 | `icon`/`ballline`의 NGUI TweenScale 잔존. 전자는 자산에서 비활성. 현재 사용 여부 미확정 |
| `ResourcesBundle/Prefabs`의 옛 기록·미션·상점 UI 등 | 목록 참조 | 레거시 보관 자산. 등록·존재만으로 실사용으로 분류하지 않음 |

전체 목록은 [component-assets.json](component-assets.json). 경로가 `Assets/`로 시작하는 항목은 프로젝트 상대 경로다. `serialized_build_reachable=false`는 씬의 저장된 참조를 따라 도달하지 못했다는 뜻이며, `Resources.Load` 등으로 생성되지 않는다는 뜻이 아니다.

## 전환된 화면과 남은 코드 의존성

메인 경기 `IngameUIPrefab`, QuickSimulator, 로딩 프리팹과 BallPlay/MainLoading/Logo/MainLobby/Tutorial/Nickname 씬에서는 **직접 직렬화된 NGUI 컴포넌트가 0개**다. 직전 실행에서 수동/Quick/정지 상태의 활성 NGUI가 없었던 기록과 일치한다. 그러나 다음 코드가 남아 있어 플러그인 전체 제거는 아직 불가능하다.

| 코드 | 남은 의존성 |
|---|---|
| `UI/UIScoreBoard.cs:38` | 이전 표시용 UISprite/UILabel 필드와 UGUI 뷰가 없을 때의 동작 |
| `Control/ControlRunner.cs:74` | UGUI 뷰가 없을 때 UISprite/UILabel을 사용하는 동작 |
| `UI/ui_etc/pitchingSelectButton.cs:9`, `miniRunner.cs:12` | UGUI와 공존하는 이전 NGUI 필드·동작 |
| `Manager/IngameUI.cs:348`, `etc/localBalance.cs:174` | NGUI `EventDelegate.Callback` 타입 |
| `util/Util.cs:571` | UITweener/UIWidget 처리 및 UISprite/UILabel/UIFont 메서드 |
| `etc/tempSelectPage.cs:29` | 현재 부트/RTTS에도 쓰는 클래스에 이전 UISprite/UILabel UI 필드·메서드 잔존 |
| `UI/resultUI/*`, `UI/finalRewardUI/*`, 기존 `Assets/Scripts/*` | 결과·보상·카드·슬롯·팝업의 NGUI 타입 참조 |
| `Assets/Editor/*MigrationChecks`, `*UGUIConverter`, `CanvasIntegrationChecks` 등 | 기존 화면 비교·변환·레거시 결과 검사를 위한 NGUI 타입 참조 |

화면에서 NGUI가 표시되지 않는 상황에서도 C# 필드·인자·메서드가 해당 타입을 참조하면 컴파일에 NGUI가 필요하다. 에디터 전용 코드는 플레이어에서 제외돼도 Unity 에디터 컴파일에는 영향을 준다.

## 집계와 한계

- `Assets/NGUI` 자체를 제외한 YAML 자산에서 NGUI 컴포넌트를 포함한 자산 **87개, 컴포넌트 1,196개**. 폰트·아틀라스·트윈과 비활성 자식을 포함하므로 화면 수/실사용 수가 아니다.
- NGUI 파일을 직접 참조하는 YAML 자산 **107개**. 위 87개와 애니메이션 클립 20개다. [직렬화 참조 목록](serialized-dependencies.json).
- 주석과 문자열을 제외한 코드 검색 후보 74파일. 그중 `KOBManager.Localization` 관련 13파일과 tk2d의 `Camera UICamera` 속성 1파일은 동명 식별자이며 NGUI 사용으로 집계하지 않는다. 나머지 **60파일(에디터 14 포함)**에 타입 참조가 남아 있다. 조건부 컴파일까지 제거 시뮬레이션한 오류 개수는 아니다. [코드 위치](code-candidates.json).
- 바이너리 직렬화 자산 **182개(프리팹 179, asset 3)**는 이번 텍스트 조사로 내부를 판독하지 않았다. `finalResultUI/finalResultPrefab`, 옛 로비·팝업 등의 프리팹도 여기에 포함된다. 따라서 위 수치는 프로젝트 전체 잔존량의 하한이며, 미확인 자산을 미사용/무의존으로 처리하지 않는다. [목록](binary-uninspected.json).
- 이번 턴은 정적 조사이며 새 Play Mode/서버 경기 실행이나 NGUI 삭제 후 컴파일은 수행하지 않았다. 실행 증거는 현재 HEAD에 포함된 직전 6단계 검증 자료다.

## 제거를 위한 선행 작업

1. 비 RTTS 결과 경로를 유지한다면 결과·기록·성장·보상 화면을 UGUI로 전환한다. 사용하지 않을 경로는 연결된 코드/자산과 함께 명시적으로 제외해야 한다.
2. 기존 테스트 씬과 레거시 자산의 유지 범위를 정하고 바이너리 프리팹의 의존성도 확인한다.
3. 전환 완료 컨트롤러의 NGUI 필드·대체 경로와 공통 타입, 에디터 도구의 NGUI 참조를 정리한다.
4. 그 후 플러그인을 제거하고 에디터/플레이어 컴파일, 씬·프리팹 Missing Script, 로컬 및 RTTS 경기 경로를 재검증한다.

재현: `python Docs/UIAudit/audit_ngui_remaining.py`. 출력은 이 폴더의 조사 JSON이며 게임 자산을 읽기만 한다.
