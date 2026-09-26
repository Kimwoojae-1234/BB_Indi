# UGUI 구조 변경 — 4단계 알파·스크롤·마스크·입력·트윈 연결

기준: `ngui-to-ugui-migration` / `b611a0d9` (3단계 완료 커밋), 2026-09-26 KST.

**4단계 연결 정리와 해당 범위의 검증 완료. 5·6단계는 진행하지 않았다.**

3단계에서 만든 RectTransform·공통 Canvas 구조에 맞춰 실행 중 상태 전달을 정리했다. 기존 프리팹·씬의 배치, GUID/fileID, 컨트롤러·Animator 경로, 콜백, 트윈의 `from` / `to`는 보존했다. 이번 변경은 공통 UGUI 어댑터와 검사 코드에 적용된다.

## 적용 내용

- **알파:** 위젯과 패널의 알파를 원래 소유자 계층에서 곱하고 Graphic에 한 번 반영한다. 같은 오브젝트에 위젯과 패널이 함께 있어도 둘 다 반영한다. 기존 패널/위젯의 호환용 CanvasGroup 알파는 1로 유지한다. 별도 네이티브 CanvasGroup 알파는 CanvasRenderer가 처리한다. 공유 표시에서 빠진 원래 위젯의 CanvasGroup이 알파 또는 `ignoreParentGroups` 경계를 요구하면 그룹을 개별 Canvas로 돌려 네이티브 계층 처리를 복구한다.
- **스크롤:** 기존 ScrollRect의 content/viewport 참조와 고정 clipAnchor를 유지한다. potential drag 초기화, 휠, 단일 pointer의 드래그/종료를 연결한다. 드래그 소유 버튼이나 스크롤 어댑터를 끄면 드래그와 관성을 취소한다. 정상 드래그 종료의 관성은 유지하며 ResetPosition은 중단 후 상단으로 돌아간다.
- **마스크:** 기존 RectMask2D/Mask 체계를 사용한다. 실행 중 clipRegion·clipSoftness·clipTexture·클리핑 방식 변경을 반영한다. 기존 GameUIClip이 연결된 위젯은 클리핑 해제/재설정 및 부모 변경 시 원래 Graphic을 유지하고 생성된 마스크 컨테이너만 재연결한다. 독립 정렬 Canvas 경계마다 필요한 마스크 체인은 유지하며, 마스크 통합/셰이더 재작성은 하지 않았다.
- **입력:** GraphicRaycaster의 투명 hit target을 기존 Collider 도형 데이터에 맞춰 갱신한다. BoxCollider 크기/중심 변경과 가로 CapsuleCollider의 범위를 반영한다. 표시 크기와 의도적으로 다른 입력 범위는 유지한다. 원래 소유자 계층의 합성 알파, CanvasGroup의 interactable/blocksRaycasts/ignoreParentGroups, Collider 활성 상태를 판정에 사용한다. 클립 영역 밖 입력과 드래그 종료 클릭을 막는다. 텍스처 마스크의 픽셀 알파를 입력 도형으로 샘플링하는 기능은 추가하지 않았다.
- **트윈과 표시 시점:** 기존 위치 트윈은 localPosition/world position 의미를 유지한다. 새 작업에는 `GameUITweenPosition.useAnchoredPosition`을 선택해 anchoredPosition3D를 사용할 수 있다(기존 자산 기본값 false). GameUIElement 갱신을 ScrollRect LateUpdate 뒤로 옮기고, 공통 Canvas 원점을 마스크 계산 전에 맞춘다. 캡처 직전에는 활성 위젯 전체의 알파·클립·표시와 공통 Canvas 정렬을 동기화하여 공유/개별 Canvas 모두 이동·알파 트윈 직후 상태를 반영한다.

`GameUIPanel.alpha` / `GameUIElement.alpha`는 기존 게임 API이고, 이 두 컴포넌트에 연결된 호환용 opacity CanvasGroup을 직접 조절하는 API로 바꾸지는 않았다. 3단계 프리팹에 저장된 Canvas 1,380개 및 공통 그룹 240개는 그대로이며, 실행 중 경계 변경에 따른 개별 Canvas 복구는 동적 처리다.

## 검증

| 검사 | 결과 |
|---|---|
| Unity 컴파일 | 열린 Unity 6000.3.9f1 에디터, Android 대상에서 컴파일 후 검사 실행 |
| 직렬화 보존 | [static-checks.txt](Step4/static-checks.txt): 조사 자산 22개와 캐릭터 재질 3개가 `b611a0d9`와 동일 |
| 기본 화면·좌표·씬·캡처 | [projection-checks.txt](Step4/projection-checks.txt): 세 프리팹 × 1280×720 / 2340×1080 / 1024×768, 9개 조합 모두 좌표·RGB 픽셀 차이 0, 씬 및 보조 카메라 연결 PASS |
| 기존 참조 | [retained-references.txt](Step4/retained-references.txt): 프리팹 19개, 위젯 1,955개, 참조 2,219개 및 NGUI 잔여 검사 PASS |
| 새 연결의 실제 Play Mode | [play-checks.txt](Step4/play-checks.txt): 알파, 부모 pivot 변경, 실제 UGUI raycast, 입력 도형 갱신, ScrollRect 드래그/휠/취소, 마스크 변경·재연결, 위치·알파·크기 트윈 및 완료 콜백, 즉시 캡처 PASS |
| 3단계 회귀 Play Mode | [batch-regression-checks.txt](Step4/batch-regression-checks.txt): 공통 Canvas 표시, RectTransform 크기/회전/비균일 배율, hide/show, 깊이·부모 변경 및 삭제, 즉시 캡처 PASS |

Play Mode 검사는 실제 UGUI 컴포넌트를 사용하는 독립 UI fixture다. 포인터 이벤트는 검사 코드가 전달하고 raycast는 EventSystem/GraphicRaycaster가 수행한다. 실제 OS 마우스·실기기 터치 및 경기 진행 전체를 검증한 것으로 보지 않는다. 6단계 경기/교체·스킬 전 과정/PVP/실기기 통합 검증은 남아 있다. 기존 null 참조와 tk2d 에디터 경고 등 과거 기록의 문제를 이번 단계에서 해결했다고 주장하지 않는다.

9개 PNG는 [Step4](Step4/)에 보관한다. 1~3단계의 검증 파일은 이번 실행 결과로 덮어쓰지 않았다.

## 재현

- 정적 보존: `python Docs/UIAudit/CanvasRestructure/verify_connection_structure.py`
- 열린 에디터: `Library/UGUIMigration/request.txt`에 `check-connections`, `play-connections`, `play-batch-regression` 중 하나를 쓰고 각각 완료 후 다음 명령을 보낸다. 기본 응답은 `result.txt`, Play Mode의 최종 성공 여부는 위의 전용 보고서에서 확인한다. `refresh` 명령으로 파일 변경을 import할 수 있다.
- 에디터가 닫혀 있을 때 배치 검사: 기존 Unity 인자로 `CanvasStructureChecks.CheckStep4 -quit`, Play Mode는 `CanvasConnectionPlayChecks.Start` 또는 `CanvasBatchPlayChecks.StartStep4`를 사용하고 `-quit`를 제외한다.
- 새 환경의 화면 비교 기준은 **4단계 런타임 수정 전 `b611a0d9`**에서 캡처해야 한다. 4단계 검사 진입점만 추가한 기준본에서 `CanvasStructureChecks.CaptureStep4Baseline`으로 `Library/UGUIMigration/CanvasConnections`를 만든다. 기준 폴더가 있으면 덮어쓰지 않는다. 4단계 코드 자체를 새 기준으로 캡처하지 않는다.

열린 에디터 검사는 Play Mode 또는 저장되지 않은 씬 변경이 있으면 시작하지 않는다. 검사 중 임시 씬을 사용하고 종료 시 원래 씬 구성을 복원한다. 사용자 씬/프리팹과 프로젝트 설정은 저장하지 않는다. fixture의 콜백 probe는 `UNITY_EDITOR`에서만 컴파일되어 플레이어에 포함되지 않는다.

다음 단계는 **5번 나머지 실사용 UI 그룹에 구조 전환 확대**다.

Git 업로드 메시지:

```text
fix: UGUI 알파·스크롤·마스크·입력·트윈 연결 정리

- 공통 Canvas와 원래 UI 소유자 사이의 알파·마스크 갱신 보완
- ScrollRect 드래그 초기화·취소·휠 및 입력 영역 상태 연결
- 기존 트윈 좌표를 보존하고 앵커 좌표 옵션과 즉시 캡처 동기화 추가
- 화면 비교·참조 보존·Play Mode 연결 및 Canvas 회귀 검사 추가
```
