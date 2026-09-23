# UGUI 최상위 구조 변경 — 1단계 배치·의존 관계 조사

2026-09-24 KST. **1단계 조사와 기준 자료 작성 완료. 2단계 Canvas 구조 변경은 아직 적용하지 않았다.**

사용자 지시에 따라 아래 순서를 한 단계씩 진행한다. 이번 변경은 문서와 조사 자료만 포함한다.

1. 현재 배치 및 카메라·좌표·트윈·계층 경로·직렬화 참조 조사 ← 이번 단계
2. UGUI 루트 Canvas·CanvasScaler·GraphicRaycaster 및 표시 계층 구성
3. RectTransform·앵커 배치 전환, 위젯별 Canvas 통합
4. 알파·스크롤·마스크·입력·트윈 연결 정리
5. 경기 HUD → 조작/판정/선수 정보 → 팝업/교체 → Quick/로딩/동적 UI 적용
6. Unity 통합 검증, 해상도·모드 전환 확인 및 정리

## 기준 버전과 참고 구조

- 작업 기준: `ngui-to-ugui-migration`, `00aabf658210a9f920761bf290d0d69c6090aa5d` (`feat: 경기 실사용 UI의 UGUI 일괄 전환`).
- 현재 구현은 UGUI 표시·입력 컴포넌트를 사용하지만, 최상위 배율·좌표·패널 관계는 기존 NGUI 방식에 맞춘 어댑터 구조다. 이전 일괄 전환 완료와 이번 최상위 구조 변경 완료를 구분한다.
- 참고 프로젝트: `D:/PROJECTS/Git/BeachBaseballWorkspace/client`, 로컬 HEAD `b221ed12328fbf862ec8c495d9fdde85e1315270`. 원격 최신 상태를 새로 가져온 것은 아니다.
- 참고 씬 `Assets/Scenes/BallPlay.unity`의 `UI_Cavas`에는 RectTransform, Screen Space Camera Canvas, CanvasScaler, GraphicRaycaster가 있다. CanvasScaler는 Scale With Screen Size, 기준 1170×2532, 가로 맞춤이다.
- 참고할 부분은 **Canvas 중심의 계층과 책임 분리**다. 현재 게임은 가로 1280×720 기준이며, 참고 프로젝트의 세로 해상도·가로 맞춤 설정을 그대로 옮기지 않는다.

## 남긴 자료와 읽는 방법

| 자료 | 내용 |
|---|---|
| [layout-baseline.json](layout-baseline.json) | 19개 전환 프리팹 + MainCamera 프리팹 + BallPlay/MainLoading 씬, 총 22개 자산의 저장 상태와 SHA-256 |
| [dependency-candidates.csv](dependency-candidates.csv) | 77개 스크립트에서 추출한 의존 코드 후보 765행. 파일·행·분류·원문 포함 |
| [validation.txt](validation.txt) | 이번 단계의 자산 해시·연결·집계·소스 행·문서 링크 검사 결과 |
| [이전 통합 기록](../Integrated/PROGRESS.md) | 2026-09-23 구현 및 Play Mode 검사 기록 |
| [기존 참조 매핑](../Integrated/source-references.json) | 이전 컴포넌트 전환 시 사용한 GUID/fileID 참조 기준. 이번 구조 변경의 새 검사 결과는 아님 |

JSON 수집 시각은 `2026-09-23T15:50:46Z`(KST 2026-09-24 00:50:46)다. 비활성 오브젝트도 포함했다. `activeSelf`는 자산의 자체 활성 값이며 실제 실행 중 표시 여부와 다르다.

`nodes`는 GameObject fileID 문자열을 키로 사용한다. 위치·회전·배율·부모 Transform과 RectTransform 값을 저장했고, `widgets`에 크기·피벗·색·깊이·Graphic/Canvas 참조를 기록했다. 같은 이름의 `Panel` 형제가 있으므로 **경로 문자열만으로 대상을 식별하지 말고 자산 GUID와 fileID를 함께 사용한다.**

카메라의 마스크 값은 `maskBits`, 뷰포트는 `viewport`에 있다. `panels`, `tweens`, `pointers`, `animators`, `nativeViews`, `mixedRenderers`의 `node`로 해당 오브젝트를 찾는다. 트윈 완료 및 포인터 콜백의 대상·매개변수도 `serializedActions`에 보관했다. 씬 프리팹 인스턴스 변경값은 `sceneInstances.serializedOverrides`에서 확인한다.

이 JSON은 선택한 배치·연결 필드의 조사 자료다. 모든 Graphic 자식이나 모든 게임 컨트롤러 필드를 복제한 씬 덤프는 아니다. 생략된 필드와 애니메이션 바인딩은 기준 커밋의 원본 자산을 함께 확인한다. CSV 역시 실행 경로 확정 목록이 아니다. 주석은 제외했지만 조건부 컴파일·레거시 분기·런타임 생성 코드를 포함한다.

## 자산 구성

다음 수치는 저장된 프리팹 기준이며, 씬 인스턴스를 펼쳐 중복 집계하지 않았다. 실행 중 동적 생성 수와도 구분한다.

| 대상 | Canvas | GameUIElement | GameUIPanel | GameUITween | GameUIPointer | Animator |
|---|---:|---:|---:|---:|---:|---:|
| IngameUIPrefab | 1,496 | 1,448 | 33 | 58 | 49 | 6 |
| QuickSimulatorPrefab | 466 | 454 | 16 | 60 | 15 | 2 |
| ControlUIPrefab | 0 | 0 | 0 | 6 | 0 | 0 |
| loadingPagePrefab | 11 | 11 | 2 | 0 | 0 | 0 |
| quickBuff / actionBuff | 10 | 10 | 0 | 8 | 0 | 0 |
| QuitPopup / consectiveGame | 18 | 14 | 1 | 1 | 4 | 0 |
| 스킬 배경 6종 | 8 | 8 | 0 | 4 | 0 | 0 |
| battingViewPrefab | 0 | 0 | 0 | 2 | 0 | 0 |
| 추적 프리팹 4종 | 10 | 10 | 0 | 9 | 0 | 0 |
| **19개 합계** | **2,019** | **1,955** | **52** | **148** | **68** | **8** |

2,019개 Canvas는 모두 World Space이고 CanvasScaler는 0개다. 68은 포인터 컴포넌트 수이며 이전 검사에서 기록한 유효 콜백 54개와 다른 집계다. ControlUIPrefab에는 tk2d 조작 표시가 있으므로 Canvas가 0개인 것을 빈 UI로 해석하지 않는다.

## 현재 계층·배치·배율

주요 경기 UI의 구조는 다음과 같다. 핵심 그룹만 표시했으며 전체 목록은 JSON을 기준으로 한다.

```text
IngameUIPrefab (Transform, GameUIRoot, GameUIPanel, IngameUI)
└─ Camera (Transform, Camera)
   ├─ Panel (GameUIPanel depth 3)
   │  ├─ battingview (점수판, 선수 정보, 타격 판정)
   │  ├─ fieldview (미니맵, 득점, 수비 판정)
   │  ├─ control (주자 조작, 구종 선택)
   │  ├─ event (이닝 변경, 시작, 라인업)
   │  ├─ playerChange (일시정지, 교체, 교체 연출)
   │  └─ skill (좌우 스킬, 좌우 캡처 카메라)
   ├─ UI_PopupConfirm (depth 10, 기본 비활성)
   ├─ Panel (동명 형제, depth 23, 기본 비활성)
   └─ Chatting (depth 7, 기본 비활성)
```

주요 루트의 기준 해상도는 모두 1280×720이지만 배율 규칙이 다르다. `aspect = Screen.width / Screen.height`, 루트 배율은 `2 / height`다.

| 대상 | 현재 GameUIRoot 설정 | 현재 계산 | 후속 CanvasScaler에서 보존할 동작 |
|---|---|---|---|
| 경기 HUD | fitWidth=false, fitHeight=true | height=720 | 높이 맞춤 |
| Quick / 로딩 | fitWidth=true, fitHeight=true | height=max(720, 1280/aspect) | 가로·세로 모두 수용하는 맞춤(Expand 대응 후보) |

이 대응은 코드 계산에 근거한 설계 입력이며 화면 검증을 마친 설정값은 아니다. 16:9에서는 세 루트 모두 배율이 약 0.002777778이다. 긴 가로 화면과 좁은 화면에서는 화면별 맞춤 정책 차이를 유지해야 한다.

| 대상 | 저장된 대표 배치·표시 순서 |
|---|---|
| 경기 루트 | 프리팹 위치 (-9878.9, -10000, 0). 주 Panel 및 위 6개 그룹의 localPosition=(0,0,0), localScale=(1,1,1) |
| Quick 루트 | 프리팹 위치 (-715.47876, 350.5037, 0), BallPlay 씬 오버라이드 위치 (-10000, -10000, 0) |
| Quick 주 Panel | depth=22. board=(-2,239,0), field=(0,-302,0), lineup=(0,0,0) |
| Quick 팝업 | Camera 아래 WaitPopup/ReconnectPopup/Popup, depth=23, 기본 비활성. WaitPopup 배율은 1.0008 |
| 로딩 루트 | 프리팹 위치 (-326.2749, 635.31885, 0). Camera/Panel/loadingBack 중심 (0,0,0), 주 Panel depth=0 |
| 개별 표시 요소 | JSON의 node 위치·배율과 widget의 mWidth/mHeight/mPivot/mDepth를 함께 사용. 단순 부모 좌표만으로 표시 영역을 비교하지 않음 |

프리팹 원본 위치, 씬 오버라이드 위치, 실행 중 계산된 위치는 서로 다를 수 있다. 새로운 루트에 현재 큰 월드 좌표를 그대로 복사하기 전에 화면상 위치로 환산해야 한다.

## 카메라와 혼합 렌더링

| 카메라 | 저장된 depth | 투영 / 크기 | 대상 레이어 |
|---|---:|---|---|
| IngameUIPrefab/Camera | 4 | 직교 / 1 | Skill_UI(19), Common_UI(20) |
| QuickSimulatorPrefab/Camera | 6 | 직교 / 1 | Skill_UI(19), Common_UI(20) |
| loadingPagePrefab/Camera | 10 | 직교 / 1 | UI(5) |
| ControlUIPrefab/tk2dCamera | 3 | 직교 / 480 | GAMEUI_LAYER(13) |
| 경기·Quick 좌우 captureCamera | 0 | 직교 / 0.38113075 | Skill_UI(19) |
| MainCamera/camera, batterCamera | 0 / 2 | 직교 / 480 | 경기 및 타자 표시용 마스크. JSON 참조 |
| fieldActiveCamera / fieldZoomCamera | 0 / 0 | 원근 | 경기 필드용 마스크. JSON 참조 |
| MainLoading 씬 Camera | 100 | 직교 / 2114.9258 | 저장 상태는 전체 레이어 |

UI 카메라 세 개의 near/far는 -10/10, 스킬 캡처 카메라는 0.3/10이다. 캡처 카메라는 저장 상태에서 targetTexture가 비어 있어도 실행 중 RenderTexture로 렌더링한다. 이름이나 기본 설정만 보고 제거하면 안 된다. 카메라 활성화와 마스크 변경은 런타임 코드 및 이전 [runtime-snapshot.txt](../Integrated/runtime-snapshot.txt)를 함께 비교한다.

경기 프리팹에는 SkeletonAnimation 9개, tk2dSprite 2개, SpriteRenderer 3개, MeshRenderer 12개가 있다. Quick에는 각각 SkeletonAnimation 17개, SpriteRenderer 2개, MeshRenderer 17개가 있고, Control에는 tk2dSprite 13개, SpriteRenderer 3개, MeshRenderer 13개가 있다. 이 수치는 컴포넌트별 수이며 동일 오브젝트의 MeshRenderer와 SkeletonAnimation 등을 별도 집계했다.

이 혼합 구조 때문에 Canvas를 추가하는 것만으로 부모·카메라 관계를 모두 바꿀 수는 없다. 화면 UI와 Spine/tk2d 연출의 좌표 및 정렬을 연결하는 기준이 필요하다.

## 구조 변경 시 함께 다룰 의존 관계

| 영역 | 확인한 근거 | 변경 시 필요한 처리 |
|---|---|---|
| 루트 배율·카메라 | [GameUIRoot.cs](../../../Assets/MainGame/Script/UI/UGUI/Integrated/GameUIRoot.cs), Resize / Awake / FindCameraForLayer | 매 프레임 루트 배율을 덮어쓴다. 자식 World Space Canvas에 카메라를 지정하며, 자동 탐색은 해당 레이어를 그리는 활성 카메라 중 depth가 가장 큰 것을 선택한다. CanvasScaler 전환 시 중복 배율과 잘못된 카메라 선택 방지 |
| 크기·피벗·여백 | [GameUIElement.cs](../../../Assets/MainGame/Script/UI/UGUI/Integrated/GameUIElement.cs), Apply / LateUpdate | Graphic의 pivot, sizeDelta, localPosition, 반전, 아틀라스 여백, 글자 효과를 재설정한다. RectTransform 배치의 소유 코드를 먼저 정해야 에디터 설정이 덮어써지지 않음 |
| 알파·정렬 | [GameUIPanel.cs](../../../Assets/MainGame/Script/UI/UGUI/Integrated/GameUIPanel.cs), [GameUIRenderOrder.cs](../../../Assets/MainGame/Script/UI/UGUI/Integrated/GameUIRenderOrder.cs) | 부모 알파를 수동 합성하고 중복 적용 방지를 위해 CanvasGroup alpha=1을 유지한다. 정렬은 panel.depth → panel InstanceID → widget.depth 순위다. 같은 depth는 고정된 계층 순서가 아니므로 명시적인 표시 순서로 매핑 필요 |
| 기존 UGUI 표시 | [IngameScoreboardView.cs](../../../Assets/MainGame/Script/UI/UGUI/IngameScoreboardView.cs), [IngameUGUICanvas.cs](../../../Assets/MainGame/Script/UI/UGUI/IngameUGUICanvas.cs), [FieldOverlayCanvas.cs](../../../Assets/MainGame/Script/UI/UGUI/FieldOverlayCanvas.cs) | 이미 UGUI인 점수판·구종·주자·필드 표시도 부모 패널의 알파·순서·카메라를 전달받는다. 상위 어댑터 제거 시 이 전달 경로 함께 교체 |
| 마스크·스크롤 | [GameUIClip.cs](../../../Assets/MainGame/Script/UI/UGUI/Integrated/GameUIClip.cs), [GameUIScroll.cs](../../../Assets/MainGame/Script/UI/UGUI/Integrated/GameUIScroll.cs) | 개별 Canvas마다 마스크 사본을 두고 원본 패널의 월드 위치·회전·배율을 복사한다. Canvas 통합 시 마스크 소유 계층과 스크롤의 content/viewport 연결을 다시 정리 |
| 입력 영역·차단 | [GameUIHitTarget.cs](../../../Assets/MainGame/Script/UI/UGUI/Integrated/GameUIHitTarget.cs), [GameUIPointer.cs](../../../Assets/MainGame/Script/UI/UGUI/Integrated/GameUIPointer.cs) | 원래 3D Collider를 영역과 활성 플래그로 사용한다. 화면→월드→Collider 로컬 좌표로 검사하고 패널 클립을 적용한다. 새 RectTransform 영역·입력 차단과 기존 콜백 대상/scaleTarget 보존 |
| 터치 좌표 | [ControlBattingUI.cs](../../../Assets/MainGame/Script/Control/ControlBattingUI.cs), [ControlPitchingUI.cs](../../../Assets/MainGame/Script/Control/ControlPitchingUI.cs) | 화면 폭을 720 기준으로 환산한다. 타격 setDown에는 -463/+203 보정이 있다. Control의 tk2d 조작 영역을 UI 루트와 같은 좌표로 가정하지 않도록 변환 경계 유지 |
| 입력 생명주기 | GameUIRoot.EnsureInput, GameUIPointer.OnEnable, IngameUGUICanvas.OnEnable | EventSystem 생성 경로가 여러 곳에 있다. 경기 UI 숨김·Quick 전환·씬 로드 중 입력 객체가 함께 꺼지거나 중복 생성되지 않게 정리 |
| 스킬 부모 변경·캡처 | [skillUISetter.cs](../../../Assets/MainGame/Script/UI/ui_etc/skillUISetter.cs), [Util.cs](../../../Assets/MainGame/Script/util/Util.cs) | skillStot을 Spine의 Root/root/text_01/left 또는 right 아래로 옮겼다가 front로 복귀시킨다. 별도 카메라로 640×360 캡처도 수행한다. Screen Space 계층과 Spine 뼈 사이의 좌표·정렬 연결 보존 |
| 미니맵 추적 | [MinimapRunnerView.cs](../../../Assets/MainGame/Script/UI/UGUI/MinimapRunnerView.cs), [UIFieldUI.cs](../../../Assets/MainGame/Script/UI/UIFieldUI.cs) | 주자 이름/바/팀 표시를 서로 다른 표시 계층으로 재배치하고 원래 Transform.TransformPoint를 따라간다. 새 Canvas 좌표로 바꾸어도 추적·겹침 순서 유지. 선수 정보는 ±472,299 위치도 사용 |
| 동적 생성 | [IngameUI.cs](../../../Assets/MainGame/Script/Manager/IngameUI.cs), [QuickSimulator.cs](../../../Assets/MainGame/Script/QuickGame/QuickSimulator.cs), LoadDynamicUI | 경기 쪽은 IngameUI 루트, Quick은 panel 아래에 생성한 후 localPosition/localScale을 설정한다. 동적 Spine 연출과 UI 프리팹의 목적지를 구분해 연결 |
| 씬·모드 전환 | [UILoading.cs](../../../Assets/MainGame/Script/UI/UILoading.cs), QuickSimulator.continueSimul, [BallPlayManager.cs](../../../Assets/MainGame/Script/Manager/BallPlayManager.cs) | 로딩 UI/시뮬레이터는 DontDestroyOnLoad를 사용한다. Quick 진입은 경기 카메라와 IngameUI 루트를 끄며 수동 복귀는 다시 켠다. 루트 참조와 표시 단위 유지 |
| 문자열 경로·직렬화 참조 | CSV의 hierarchy_lookup, JSON의 콜백/Animator, 기존 source-references.json | UIPlayerChange의 bar1/bar2/Label/minus, Quick 팝업의 anim/gaugebar, 카드/시작 연출의 자식 검색 등이 있다. 부모 변경 전후 경로와 GUID/fileID를 대조하고 IngameUI·BallPlayManager의 연결도 검사 |

패널 52개 중 clipping=0은 37개, 소프트 사각 클립(3)은 13개, 텍스처 클립(1)은 2개다. 텍스처 클립은 Quick 스킬 backPanel에 있다. 마스크 없이 공통 Canvas 아래로 이동시키는 방식으로는 현재 표시를 보존할 수 없다.

트윈 148개는 Alpha 37, Position 39, Rotation 59, Scale 13개다. 저장된 Position 트윈 39개는 모두 worldSpace=false다. 부모를 바꾸면 같은 from/to라도 화면 이동이 달라진다. 런타임 Begin 호출과 완료 콜백도 함께 확인해야 한다.

Animator 8개 중 7개는 변환된 overrideController를 참조하고, `fieldview/scoreShow/_active/bg` 한 개는 현재 controller가 비어 있다. 판정·득점·시작·교체·Quick 애니메이션에는 `_active/GameObject/callsign` 등 상대 경로와 활성/위치/색 바인딩이 있다. Animator 루트 아래의 계층을 변경할 때 클립 경로도 대조한다. 루트 위의 이동과 루트 내부 이동을 구분해야 한다.

## 화면 비교 자료와 검증 범위

새 Play Mode 실행이나 새 화면 촬영은 이번 1단계에서 수행하지 않았다. 아래 자료는 **2026-09-23 통합 검사 당시 905×461 화면**이며 1280×720 캡처가 아니다.

| 상태 | 기존 화면 | 이용 범위 |
|---|---|---|
| 경기 표시 | [gameplay-current.png](../Integrated/gameplay-current.png) | HUD·경기 표시 참고 |
| 일시정지 | [gameplay-pause.png](../Integrated/gameplay-pause.png) | 팝업과 겹침 순서 참고 |
| Quick 진행 | [gameplay-quick.png](../Integrated/gameplay-quick.png) | 보드·라인업·선수 표시 참고 |
| 수동 복귀 | [gameplay-return-manual.png](../Integrated/gameplay-return-manual.png) | Quick→수동 전환 후 표시 참고 |

이전 [final-source-manifest.json](../Integrated/final-source-manifest.json)의 구현 파일 199개는 이번 조사에서 모두 같은 SHA-256으로 확인됐다. 따라서 같은 구현의 과거 화면을 참고할 수 있지만, 현재 실행 화면이나 모든 조건부 연출이 검증됐다는 뜻은 아니다. 스킬·선수 교체·로딩의 모든 상태와 다른 화면 비율은 후속 실행 검증이 필요하다.

기존 [applied-checks.txt](../Integrated/applied-checks.txt)의 자산 19개·위젯 1,955개·참조 2,219개 PASS는 이전 검사 결과다. 이번 단계에서는 이를 새로 실행한 검사로 표기하지 않는다.

기존 문제도 구조 변경 회귀와 구분한다. 점수판 일시정지/자동 버튼 Collider 비활성, 일시정지 계속하기 콜백 누락, Quick topUI 비활성이 알려져 있다. 과거 수동↔Quick 전환에는 기존 API를 직접 호출한 검사가 포함되므로 일반 버튼 입력 전체 통과를 의미하지 않는다.

작업 시작 시 이미 변경되어 있던 아래 재질 3개는 `preexistingModifiedFiles`에 해시를 기록했다. 이 단계에서는 수정하지 않았다. 과거 화면과 캐릭터 재질까지 동일하다고 가정하지 않는다.

- `Assets/Resources/MainGame/spineData/bvChar/bvFielder/FIELDER_Material.mat`
- `Assets/Resources/MainGame/spineData/bvChar/bvRunner/runner_Material.mat`
- `Assets/Resources/MainGame/spineData/fieldingview/runner/anim/runnerAnim_Material.mat`

## 2단계로 넘길 조건

1. 가로 1280×720 기준과 화면별 배율 정책을 보존하며 루트 Canvas를 구성한다.
2. 경기 HUD·Quick·로딩의 표시/숨김과 입력 생명주기를 유지한다.
3. Spine/tk2d와 캡처 카메라를 고려해 화면 UI의 카메라·표시 순서를 명시한다.
4. 기존 배율/배치 갱신과 새 CanvasScaler/RectTransform이 동시에 같은 값을 덮어쓰지 않도록 전환 경계를 정한다.
5. 기존 fileID/컨트롤러/Animator/동적 생성 경로를 기준 자료와 대조한다. 같은 이름의 오브젝트는 ID로 구별한다.

이 문서는 1단계의 조사 결과다. 2단계 구현, Unity 컴파일·Play Mode 재검증, 프리팹·씬 저장, 커밋·푸시는 수행하지 않았다.
