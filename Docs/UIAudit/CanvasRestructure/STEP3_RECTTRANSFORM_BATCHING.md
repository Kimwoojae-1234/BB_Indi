# UGUI 구조 변경 — 3단계 RectTransform 배치와 Canvas 통합

기준: `ngui-to-ugui-migration` / `ae577638` (2단계 완료 커밋), 2026-09-26 KST.

**3단계 적용과 해당 범위의 검증 완료. 4단계는 진행하지 않았다.**

경기·Quick·로딩 프리팹의 배치 정보를 RectTransform으로 옮기고, 표시 순서가 연속인 동일 부모·패널·깊이·레이어의 위젯에 공통 Canvas를 적용했다. 4단계 알파·스크롤·마스크·입력·트윈 처리 개편은 이번 범위가 아니다.

## 배치와 크기 관리

- 원래 UI 오브젝트의 Transform 2,227개를 동일 fileID의 RectTransform으로 바꿨다. 컨트롤러 위치·회전·배율, 원래 부모 및 형제 순서, Animator 상대 경로를 보존했다.
- 위젯 1,913개의 `layoutRect`를 연결했다. 크기와 피벗은 이 RectTransform이 관리하며, `width` / `height` / `SetDimensions` API도 여기에 반영한다. `mWidth` / `mHeight`는 호환용 현재 값으로 동기화한다. 전환된 위젯을 편집할 때는 RectTransform을 사용한다.
- 기존 중앙 기준 배치를 유지하기 위해 앵커를 부모의 피벗에 고정하고 원래 로컬 위치를 anchoredPosition으로 옮겼다. 기존 ScrollRect/Viewport처럼 이미 RectTransform이던 오브젝트는 저장된 anchoredPosition을 보존했다. 가로 1280×720 기준 및 2단계 CanvasScaler 정책을 유지한다.
- 아직 전환하지 않은 나머지 프리팹은 `layoutRect == null`일 때 기존 크기 API를 그대로 사용한다.

## Canvas 통합 범위

아래는 중첩 프리팹 인스턴스를 펼치지 않은 **세 프리팹 소스의 컴포넌트 수**다. 실행 중 추가되는 UI나 draw call/FPS 측정값은 아니다.

| 프리팹 | 새 RectTransform | Canvas 변경 전 → 후 | 공통 Canvas | 공유 위젯 |
|---|---:|---:|---:|---:|
| IngameUIPrefab | 1,691 | 1,497 → 1,069 | 176 | 604 |
| QuickSimulatorPrefab | 522 | 467 → 301 | 62 | 228 |
| loadingPagePrefab | 14 | 12 → 10 | 2 | 4 |
| 합계 | **2,227** | **1,976 → 1,380** | **240** | **836** |

Canvas는 **596개(약 30.2%) 감소**했다. 서로 다른 깊이·부모·렌더 레이어, 순서 사이에 다른 위젯이 들어오는 구간, 입력 및 기존 네이티브 표시용 Canvas는 독립 경계로 남겼다. 모든 위젯을 하나의 Canvas로 합친 상태는 아니다.

```text
기존 공통 부모 (RectTransform)
├─ 위젯 A (RectTransform + GameUIElement, 원래 컨트롤러 계층)
├─ 위젯 B (RectTransform + GameUIElement)
└─ UI Batch <depth> (Canvas + GameUICanvasBatch)
   ├─ Presentation A (기존 Graphic·글자 효과·마스크)
   └─ Presentation B
```

공유 대상은 변환기가 생성했던 표시 컨테이너다. 기존 게임 제어 오브젝트는 재배치하지 않는다. `GameUICanvasBatch`는 표시 컨테이너에 해당 위젯의 로컬 위치·회전·배율과 활성 상태를 전달한다. 같은 부모 공간을 사용하므로 비균일 배율과 회전이 함께 있어도 원래 변환 행렬을 보존한다. 위젯별 마스크 복사와 알파 계산은 기존 동작을 유지하며 4단계에서 다룬다.

실행 중 깊이·부모·레이어가 달라지거나 다른 위젯이 표시 순서 사이에 들어오면 해당 그룹 전체를 개별 Canvas로 전환한다. 이때 표시 컨테이너를 각 원래 소유자 아래로 돌려놓고 카메라와 정렬을 연결한다. 실행 중 Canvas 개수보다 기존 표시 순서를 우선하는 처리다. 개별 위젯 삭제 시 공통 Canvas에 남은 해당 표시 객체도 제거한다.

비활성 상태에서도 원래 패널을 찾도록 했으며, Canvas를 껐다가 켜도 같은 깊이의 정렬 순위가 사라지지 않도록 정렬 판정을 부모 Canvas 계층 기준으로 바꿨다.

부모의 피벗 변경 후에도 공통 Canvas는 부모 원점에 유지된다. 스킬 캡처는 LateUpdate 이전에 호출될 수 있으므로 캡처 직전에 공유 표시를 동기화한다. 이동 직후 캡처가 이전 프레임의 좌표를 사용하지 않도록 하는 연결이다.

## 검증 자료와 한계

- [정적 구조 검사](Step3/static-checks.txt): 기존 컨트롤러·위젯 값·원래 형제 순서·트윈·애니메이션 데이터 및 fileID 보존, 미해결 로컬 참조 없음. 씬 2개를 포함한 다른 조사 자산 19개와 캐릭터 재질 3개도 2단계 상태 유지.
- [화면·좌표·씬·캡처 검사](Step3/projection-checks.txt): **PASS**. 세 프리팹 × 1280×720 / 2340×1080 / 1024×768의 9개 조합 모두 좌표 차이와 RGB 이미지 차이 0. 비활성 위젯도 좌표 검사에 포함한다. 게임 데이터가 없는 프리팹 기본 표시 상태의 비교이며 전체 경기 시나리오 검사는 아니다.
- [기존 참조 검사](Step3/retained-references.txt): 자산 19개, 위젯 1,955개, 기존 GUID/fileID 참조 2,219개 검사. 기존 NGUI 컴포넌트 및 NGUI 애니메이션 바인딩 잔여 검사도 포함한다.
- [Play Mode 검사](Step3/play-checks.txt): **PASS**. 게임과 분리된 실제 UGUI 실행 fixture. 공통 Canvas 렌더링, RectTransform 직접 편집 및 크기 API, 피벗·회전·비균일 배율·이동, GameObject/컴포넌트 및 루트 표시 전환, EventSystem 유지, 깊이·부모 변경과 삭제, 이동 직후 즉시 캡처를 검사했다.
- [변환 매핑](Step3/layout-mapping.json): 전환된 Transform, 제거한 Canvas, 공통 Canvas 구성원과 원래 표시 컨테이너 식별자.

대표 화면: [경기 기본 상태](Step3/IngameUIPrefab-1280x720.png), [Quick](Step3/QuickSimulatorPrefab-1280x720.png), [로딩](Step3/loadingPagePrefab-1280x720.png).

실제 경기 진행·교체/스킬 전 과정·PVP·실기기·성능 측정은 아직 검증하지 않았다. 2단계에서 기록한 기존 null 참조와 tk2d 에디터 Game View 조회 문제도 이번 배치 변환으로 해결되었다고 보지 않는다. 검사기를 통한 Play Mode UI 확인과 6단계 게임 통합 검증은 구분한다.

## 재현

- `python Docs/UIAudit/CanvasRestructure/verify_layout_structure.py`
- Unity 6000.3.9f1 Android 대상, `-batchmode -projectPath <저장소> -buildTarget Android -executeMethod CanvasStructureChecks.CheckStep3 -quit -logFile <로그>`
- Play Mode 검사: 같은 인자에서 `-executeMethod CanvasBatchPlayChecks.Start`를 사용하고 **`-quit`를 제외**한다. 검사기가 Play Mode 종료와 Unity 종료 코드를 처리한다. 일반 에디터에서 자동으로 실행되지 않는다.

좌표 비교 기준은 적용 전 `ae577638`의 세 프리팹에서 캡처한 `Library/UGUIMigration/CanvasLayout`이다. 새 환경에서는 해당 커밋의 프리팹 사본을 `Assets/Editor/CanvasLayoutBaseline`에 두고 `CanvasStructureChecks.CaptureStep3Baseline`으로 기준을 만든 후 임시 사본을 제거한다. 전환 완료 자산을 새 기준으로 덮어쓰려 하면 중단한다.

`apply_layout_structure.py`는 2단계 원본과 일치할 때만 적용되는 변환 기록이며, 이미 변환한 자산에 재실행하는 도구가 아니다. 기존 컨트롤러·재질·Sprite/폰트 자산을 다시 생성하지 않는다. 검사기는 원본 씬/프리팹을 저장하지 않는다.

검사 로그: 로컬 `Library/UGUIMigration/canvas-layout-check.log`, `canvas-layout-play.log`. 다음 단계는 **4번 알파·스크롤·마스크·입력·트윈 연결 정리**다.

Git 업로드 메시지:

```text
feat: 경기 UI RectTransform 배치 전환 및 공통 Canvas 적용

- 경기·Quick·로딩의 UI Transform 2,227개를 RectTransform으로 전환
- 연속 위젯 836개의 Canvas를 공유해 Canvas 596개 감소
- 기존 제어 계층과 표시 순서를 보존하고 동적 변경 시 개별 Canvas로 전환
- 화면 비교·참조 보존·Play Mode 표시 수명 주기 검증 추가
```
