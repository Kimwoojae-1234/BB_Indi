# UGUI 구조 변경 — 2단계 루트 Canvas 구성

작업 기준: `ngui-to-ugui-migration` / `24f700eb` (1단계 조사 커밋), 2026-09-24 KST.

**2단계 루트 Canvas 구성 및 해당 범위의 검증 완료. 3단계는 진행하지 않았다.**

## 적용 구조

경기·Quick·로딩의 원본 프리팹 3개에 Screen Space Camera Canvas, CanvasScaler, GraphicRaycaster를 추가했다. 카메라 아래에 UI를 두던 구조를 다음과 같이 바꿨다.

```text
IngameUIPrefab / QuickSimulatorPrefab / loadingPagePrefab
├─ Camera                       기존 UI 카메라, Canvas와 형제
└─ Canvas                       RectTransform + Canvas + CanvasScaler + GraphicRaycaster
   ├─ HUD
   │  └─ Panel                  기존 주 패널과 내부 컨트롤러·애니메이션 계층
   ├─ Effects                   경기에서 동적으로 생성하는 연출의 부모
   └─ Popups                    기존 Camera 직속 팝업·채팅 그룹
```

최상위 프리팹 오브젝트는 기존 컨트롤러·모드 전환·수명 관리 대상으로 유지한다. 표시 계층의 루트는 새 Canvas다. 최상위 오브젝트의 배율은 1이며, Canvas가 지정된 GameUIRoot는 기존 `2 / height` 배율 계산을 실행하지 않는다.

| 대상 | CanvasScaler | 기존 배치 정책 |
|---|---|---|
| 경기 HUD | Scale With Screen Size, 1280×720, Match Height | 높이 맞춤 유지 |
| Quick / 로딩 | Scale With Screen Size, 1280×720, Expand | 가로·세로 모두 수용하는 정책 유지 |

카메라를 Canvas의 부모로 사용하지 않으며 Canvas의 자식에도 두지 않는다. Canvas planeDistance=1, 카메라 local Z=-1로 기존 UI의 월드 Z=0 평면을 유지한다. 카메라 near/far는 -9/11로 이동해 종전 월드 클리핑 범위(-10~10)를 유지한다. 카메라 depth·cullingMask·직교 크기와 원래 컴포넌트 ID는 유지했다.

BallPlay/MainLoading 씬 파일은 수정하지 않았다. 두 씬이 참조하는 원본 프리팹에서 새 구조를 상속한다. 씬에 기존 루트 배율을 덮어쓰는 오버라이드는 없었다.

## 이번 단계의 연결 수정

- 기존 위젯별 Canvas는 유지하며, 새 Canvas에 편입된 후에도 기존 패널/위젯 sortingOrder를 사용하도록 overrideSorting을 연결했다. 이미 UGUI인 점수판·조작·필드 표시와 동적으로 생성되는 표시도 정렬을 유지한다.
- 같은 패널·위젯 depth가 겹치면 Hierarchy 순서로 Canvas 순위를 명시한다. 공통 Canvas 아래로 옮긴 후 Quick 카드 장식이 선수 이미지를 덮던 문제를 해결했다. 패널과 기존 네이티브 표시의 순위 슬롯은 유지하고, 입력용 Canvas도 해당 위젯 순위를 사용한다.
- `IngameUI.LoadDynamicUI`는 새 Effects 계층에 연출을 생성한다. 프리팹 최상위 배율이 1로 바뀌어도 기존 픽셀 기준 위치·배율을 사용할 수 있다.
- 스킬 보조 카메라의 캡처는 새 루트 Canvas를 일시적으로 World Space로 렌더링한 후 원래 Screen Space Camera 상태로 복원한다. 캡처 중에도 UI와 Spine의 월드 좌표를 유지하며 `finally`에서 복원한다.
- 기존 GameObject·Transform·컨트롤러·카메라 fileID와 프리팹 GUID를 보존했다. 트윈 from/to, 위젯 크기·피벗·색, Animator 아래 상대 경로는 변경하지 않았다.

HUD 안의 판정·시작·스킬 연출과 Quick 주 Panel 내부 구조는 유지했다. Effects 계층으로 모든 연출을 재배치하거나 위젯별 Canvas를 합치는 작업은 후속 단계다. 각 프리팹에 추가된 오브젝트/컴포넌트는 11개이며, 소스 파일 기준 Canvas 3개와 CanvasScaler 3개가 추가되었다.

## 검증 자료

- [정적 구조 검사](Step2/static-checks.txt): 원래 ID·컨트롤러·배치 필드·카메라의 월드 클리핑 영역 보존, 나머지 조사 자산과 기존 재질 변경 보존.
- [좌표·렌더링·씬 검사](Step2/projection-checks.txt): **OVERALL PASS**. 3개 프리팹 × 1280×720 / 2340×1080 / 1024×768의 9개 조합에서 Graphic 화면 경계 차이 1픽셀 미만. 최대 차이는 경기 HUD 0.5713px, Quick 0.0165px, 로딩 0px다. 양쪽 Quick 카드의 동일 깊이 겹침, 스킬 보조 카메라의 UGUI 마커 캡처 및 Canvas 복원, 두 씬의 루트 상속 연결도 통과했다.
- [직렬화 참조 검사](Step2/retained-references.txt): **PASS**. 프리팹 19개, 위젯 1,955개, 기존 참조 2,219개의 GUID/fileID 매핑과 NGUI 잔여 여부 확인. 이전 단계의 검사 파일은 덮어쓰지 않았다.
- [루트 ID 매핑](Step2/root-mapping.json): 각 프리팹의 카메라·Canvas·계층 및 유지한 패널 식별자.

Unity 6000.3.9f1 Android 대상 에디터 컴파일과 배치 검사 종료 코드 0을 확인했다. 1280×720 이미지에는 양방향 1픽셀 주변 색상 범위를 사용한 비교를 적용했다. 허용치는 0~255 색상 기준 평균 잔차 0.01 미만, 8을 넘는 차이가 있는 픽셀 비율 0.01% 이하다. Quick의 최대 평균 잔차는 0.005439, 초과 픽셀은 79/921,600이다. 안티앨리어싱까지 완전히 같은 이미지는 아니며, 나머지 두 해상도의 이미지 차이는 진단 수치로 기록하고 좌표 경계를 검사했다.

대표 화면: [경기 기본 상태](Step2/IngameUIPrefab-1280x720.png), [Quick](Step2/QuickSimulatorPrefab-1280x720.png), [로딩](Step2/loadingPagePrefab-1280x720.png).

렌더링 검사는 게임 데이터를 주입하지 않은 프리팹 저장 상태의 비교다. 경기 HUD의 기본 상태에는 미니맵/아웃 표시만 보이는 영역이 포함된다. 전체 경기 진행, 일시정지/교체 입력, PVP, 실기기 검증이나 6단계 통합 합격을 의미하지 않는다. 검사 중 기존 tk2d의 에디터 Game View 크기 조회 오류 로그가 발생했으며, 캡처 결과 및 명시적인 검사는 통과했다.

BallPlayManager의 `IngameUITrans`는 기준 프리팹과 BallPlay 씬부터 null이다. 해당 직렬화 상태를 보존했으며 새 참조를 임의 연결하지 않았다. 그 필드를 사용하는 기존 경기 경로의 정상 동작을 이번 씬 검사로 보증하지 않는다.

작업 시작 시 이미 변경되어 있던 캐릭터 재질 3개는 수정하지 않았다. 커밋·푸시는 수행하지 않았다.

## 재현 방법

`python Docs/UIAudit/CanvasRestructure/verify_root_structure.py`로 정적 비교를 실행한다.

`apply_root_structure.py`는 1단계 원본 SHA-256이 일치할 때만 적용하는 변환 기록이다. **이미 적용된 자산에 재실행하는 도구가 아니다.**

Unity 검사는 `Assets/Editor/CanvasStructureChecks.cs`의 명시적인 배치 진입점으로 실행한다. `CaptureBaseline`은 적용 전 자산(또는 `Assets/Editor/CanvasStructureBaseline/`의 동일 이름 임시 사본)을 읽어 `Library/UGUIMigration/CanvasStructure`에 기준을 저장하고, `Check`는 현재 자산을 비교한다. 전환 완료 프리팹을 기준으로 덮어쓰려 하면 중단한다. 기준 사본은 `24f700eb`의 프리팹 3개이며 검사 후 `Library/UGUIMigration/CanvasStructure/BaselinePrefabs`로 옮겼다. 새 환경에서는 해당 커밋에서 임시 사본을 추출해 먼저 기준을 캡처해야 한다. 저장된 사용자 씬이나 프리팹을 이 검사기가 수정하지 않는다.

실행 인자: `-batchmode -projectPath <저장소> -buildTarget Android -executeMethod CanvasStructureChecks.Check -quit -logFile <로그 경로>`.

이번 검사 로그는 로컬 `Library/UGUIMigration/canvas-final-editor.log`에 있다. 임시 검사 코드와 중간 진단 이미지는 제거했다. `git diff --check` 및 정적 자산 보존 검사도 통과했다.

다음은 3단계인 RectTransform·앵커 배치 전환과 위젯별 Canvas 통합이다.

Git 업로드 메시지:

```text
feat: 경기 UI 최상위 구조를 UGUI Canvas로 전환

- 경기·Quick·로딩 루트에 CanvasScaler와 HUD/Effects/Popups 계층 구성
- 카메라·동적 연출·보조 캡처 및 동일 깊이 위젯 정렬 연결 보존
- 9개 화면 크기 조합과 기존 참조 2,219개 검증 기록 추가
```
