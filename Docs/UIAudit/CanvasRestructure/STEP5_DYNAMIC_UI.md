# UGUI 구조 변경 — 5단계 나머지 실사용 UI 구조 확대

기준: `ngui-to-ugui-migration` / `00b496e3` (4단계 완료 커밋), 2026-09-26 KST.

**5단계 구현과 해당 범위의 검증 완료. 6단계 전체 경기 통합 검증은 진행하지 않았다.**

앞 단계에서 경기 HUD·조작/판정 표시·선수 정보·교체·Quick·로딩의 상주 UGUI 위젯 1,913개를 RectTransform 기반으로 옮겼다. 이번에는 남은 버프·팝업·스킬 배경·투구 추적 프리팹 14개, 위젯 42개에 같은 배치 기준을 적용했다. 조사한 19개 프리팹의 GameUIElement 1,955개 모두 `layoutRect`가 연결된다. ControlUIPrefab과 battingViewPrefab의 tk2d/Spine 표시·카메라·게임 좌표는 기존 경계를 유지한다.

## 프리팹 적용

기존 Transform fileID를 유지하며 52개를 RectTransform으로 바꿨다. 고정 앵커와 부모 pivot 기준을 사용해 기존 로컬 원점과 트윈 좌표를 보존한다. 게임 제어 오브젝트의 부모·형제 순서와 이름, 컨트롤러 참조, 콜백, from/to 값은 변경하지 않았다.

| 그룹 | 프리팹 | 위젯 | 새 RectTransform | Canvas 전 → 후 |
|---|---:|---:|---:|---:|
| quickBuff / actionBuff | 2 | 10 | 12 | 10 → 8 |
| QuitPopup / consectiveGame | 2 | 14 | 15 | 18 → 14 |
| 스킬 배경 | 6 | 8 | 15 | 8 → 8 |
| 투구 추적 | 4 | 10 | 10 | 10 → 10 |
| 합계 | **14** | **42** | **52** | **46 → 40** |

같은 부모·패널·깊이·레이어이고 표시 순서가 연속인 11개 위젯을 5개 공통 Canvas로 묶어 Canvas 6개를 줄였다. 이는 프리팹 소스의 저장 수이며 실행 중 생성 수나 draw call/FPS 수치가 아니다. 기존 깊이/활성/부모 변경에 따른 개별 Canvas 복구는 유지한다.

- [범위 및 시작 시 수정 파일](Step5/scope.json)
- [Transform·Canvas·바인딩 매핑](Step5/layout-mapping.json)
- [직렬화 보존 검사](Step5/static-checks.txt)

## 동적 생성 연결

각 대상 프리팹의 RectTransform 루트에 `GameUIDynamicUI`를 추가했다. 별도의 화면 CanvasScaler를 만드는 대신 생성된 위치의 GameUIRoot/Canvas를 사용한다.

- 실제 `Util.Load`가 인스턴스를 생성한 뒤 부모를 설정하는 순서에 맞춰 카메라와 패널을 다시 연결한다. 활성/비활성 자식 위젯도 연결 대상이다.
- 부모 UI 루트가 바뀌면 위젯의 캐시된 패널과 공유 Canvas의 패널, 각 Canvas의 카메라를 갱신한다. 상위 컨테이너 전체의 부모가 바뀐 경우도 확인하며, 캡처 직전 동기화에서도 갱신한다.
- 생성 시 부모에 클리핑이 있으면 해당 위젯의 기존 Graphic/효과를 유지하면서 GameUIClip을 구성한다. 로드 후 클리핑을 켜는 경우도 연결한다. 4단계 마스크 구현을 사용해 RectMask2D/텍스처 Mask 전환과 해제·재설정을 처리한다.
- 다른 UI 루트의 활성 카메라가 더 높은 depth를 가져도 소속 GameUIRoot의 카메라를 우선 사용한다. GameUIRoot가 없는 사용처의 기존 레이어 기반 카메라 탐색은 유지한다.

QuickSimulator의 동적 Spine 연출은 `EffectsParent`, 종료 팝업은 `PopupParent`를 사용하도록 연결했다. 기존 Quick 종료 팝업 생성은 Canvas 바깥의 소유자 루트를 부모로 사용했는데, 이 루트는 2단계 이후 배율 1이므로 UI 픽셀 좌표를 제공하는 Popups 계층으로 바꿨다. 수동 경기 UIPause의 종료 팝업도 같은 Popups 계층을 사용한다. UI 루트가 없는 사용처는 기존 부모를 사용한다. 스킬 배경·버프·투구 추적의 의미 있는 개별 부착 위치는 유지한다.

## 검증 결과

| 검사 | 결과 |
|---|---|
| Unity 컴파일 | Unity 6000.3.9f1, Android 대상 배치 에디터에서 컴파일 후 검사 실행 |
| 변경된 14개 프리팹 | [static-checks.txt](Step5/static-checks.txt): 원래 Transform의 TRS/fileID, 컨트롤러·콜백·트윈·기존 형제 순서 보존, 미해결 로컬 참조 없음 |
| 동적 UI 화면 | [dynamic-checks.txt](Step5/dynamic-checks.txt): 14종 × 3해상도 × 초기/트윈 0.5 표본, **84개 조합 모두 좌표·RGB 차이 0** |
| 기존 상주 UI 화면 | [Main/projection-checks.txt](Step5/Main/projection-checks.txt): 경기·Quick·로딩 × 3해상도, **9개 조합 모두 좌표·RGB 차이 0**. 씬 2개 및 보조 카메라 캡처 검사 포함 |
| 참조 보존 | [retained-references.txt](Step5/retained-references.txt): 프리팹 19개, 위젯 1,955개, 참조 2,219개 및 NGUI 동작/애니메이션 잔여 검사 PASS |
| 동적 UI Play Mode | [play-checks.txt](Step5/play-checks.txt): 실제 Resources/Util.Load 경로의 14개 프리팹·42개 위젯·공통 Canvas 5개, 부모 카메라/패널/클립 연결 검사 PASS |
| 동적 표시·입력 | 같은 Play Mode 보고서: 실제 스킬 배경의 텍스처 마스크/투명 마스크/좌우 반전, 상위 컨테이너의 UI 루트 간 이동, 알파/즉시 캡처, 숨김/깊이 변경/삭제, EventSystem 유지, Quick/수동 팝업 생성 경로와 실제 GraphicRaycaster 검사 PASS |
| 연결 회귀 | [connection-regression-checks.txt](Step5/connection-regression-checks.txt): 4단계 알파·스크롤·마스크·입력·트윈 fixture PASS |
| Canvas 회귀 | [batch-regression-checks.txt](Step5/batch-regression-checks.txt): 3단계 배치/변환/숨김/깊이·부모 변경/삭제 fixture PASS |

동적 화면 비교는 검사에서 원래 게임 오브젝트를 활성화한 분리된 Canvas fixture다. 초기 값과 각 트윈의 0.5 지점을 비교하며 전체 연출 타임라인 검사는 아니다. Play Mode에서는 실제 리소스와 생성 API를 사용하지만 경기 데이터를 구동하지 않는다. 팝업 raycast는 렌더러 등록 후 확인하며 실제 종료 콜백은 호출하지 않는다. OS 포인터·기기 터치·전체 경기/교체/스킬/PVP 왕복은 6단계에 남아 있다. 기존 null 폰트/아틀라스/콜백과 tk2d 에디터 경고는 이전 기록과 구분한다.

세 상주 UI 프리팹, Control/BattingView/MainCamera 프리팹, 씬 2개는 4단계와 동일하다. 작업 시작 시 수정되어 있던 FIELDER_Material과 runnerAnim_Material 두 파일은 시작 시 SHA-256과 비교해 원래 편집 내용을 그대로 보존했다.

## 재현

- 직렬화 검사: `python Docs/UIAudit/CanvasRestructure/verify_expansion_structure.py`
- 화면·참조: Unity 배치 인자에 `-executeMethod CanvasExpansionChecks.Check -quit`
- Play Mode: `CanvasExpansionPlayChecks.Start`, `CanvasConnectionPlayChecks.StartStep5`, `CanvasBatchPlayChecks.StartStep5`를 각각 실행하고 **-quit를 제외**한다. 검사기가 종료 코드를 반환한다.
- 열린 에디터: 기존 `Library/UGUIMigration/request.txt` 명령 통로의 `check-expansion`, `play-expansion`, `play-expansion-connections`, `play-expansion-batches`를 하나씩 실행한다. Play Mode의 최종 성공 여부는 전용 보고서에서 확인한다.
- 새 환경의 화면 기준은 `00b496e3`의 프리팹 및 런타임 코드에서 5단계 검사 진입점만 추가한 후 `CanvasExpansionChecks.CaptureBaseline`으로 저장한다. `Library/UGUIMigration/CanvasExpansionMain`, `CanvasExpansion`을 사용하며 이미 존재하면 덮어쓰지 않는다. 전환 완료 코드를 새 기준으로 캡처하지 않는다.

검사기는 씬/프리팹을 저장하지 않는다. 열린 에디터에서는 저장되지 않은 씬 변경과 Play Mode 상태를 확인하고 검사 후 씬 구성을 복원한다. 이전 단계 보고서는 덮어쓰지 않았다. `apply_expansion_structure.py`는 4단계 원본과 일치할 때만 사용하는 변환 기록이며 이미 적용한 자산에 재실행하는 도구가 아니다.

다음은 **6번 Unity 전체 경기 통합 검증과 정리**다.

Git 업로드 메시지:

```text
feat: 동적 UI 14종에 UGUI 배치와 생성 계층 적용

- 버프·팝업·스킬 배경·투구 추적의 RectTransform 및 공통 Canvas 확대
- 생성·부모 변경 시 소속 카메라·패널·마스크 연결 갱신
- Quick 연출과 경기 종료 팝업을 Effects/Popups 계층에 연결
- 93개 화면 비교와 동적 생성·입력·기존 연결 회귀 검사 추가
```
