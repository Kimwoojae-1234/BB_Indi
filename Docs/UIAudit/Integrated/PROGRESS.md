# 경기 UI 일괄 UGUI 전환 기록

2026-09-24 후속 작업: 이 구현은 `00aabf65`로 커밋되었다. 최상위 Canvas 구조 변경은 별도 작업으로 단계별 진행하며, [1단계 배치·의존 관계 조사](../CanvasRestructure/STEP1_BASELINE.md)에 이어 [2단계 루트 Canvas 구성](../CanvasRestructure/STEP2_ROOT_CANVAS.md)과 해당 범위의 검증을 완료했다. 3단계는 진행하지 않았다. 아래는 이전 일괄 전환과 당시 검증 기록이다.

기준: `ngui-to-ugui-migration` / `52f01bd15638130263e69e8f35473937351e2bb9`.
공유 대화의 최종 지시에 따라 남은 실사용 UI를 일괄 적용하고 Unity 통합 검증을 진행했다. 이전 네 단위의 결과와 이번 결과를 구분한다.

최종 적용 검사는 2026-09-23 23:13 KST에 통과했다. 변경된 코드/감사 문서의 `git diff --check`도 통과했다. 최종 구현 파일 199개의 해시는 [final-source-manifest.json](final-source-manifest.json)에 기록했다.

## 적용 범위

- 경기 메인 UI: 선수 정보, 판정/이닝/시작 연출, 타이머, 일시정지/교체/확인창, 조건부 화면.
- QuickSimulator 및 quickBuff/actionBuff, 투타 컨트롤, 로딩 화면.
- 종료/연속 경기 팝업, 스킬 배경 6종, 타구/투구 추적 프리팹 4종, BattingView.
- 원본 프리팹 19개에 `GameUIElement` 1,955개를 적용했다. 프리팹 GUID 및 기존 게임 오브젝트/컨트롤러의 fileID는 보존하고 제거한 NGUI 컴포넌트 참조만 대체했다.
- 렌더링은 Unity Image/RawImage/Text/TMP, 입력은 EventSystem/GraphicRaycaster, 스크롤은 ScrollRect를 사용한다. 기존 컨트롤러 API에 필요한 속성, 트윈, 콜백, 스프라이트 애니메이션은 NGUI를 참조하지 않는 어댑터로 옮겼다.
- 클립 영역/소프트 마스크, 아틀라스 여백/반전, 글자 효과, 패널 알파와 정렬, 원본의 가산 합성을 보존한다. 원본 아틀라스/폰트/애니메이션 파일은 수정하지 않고 변환 자산을 생성했다.

tk2d/Spine 렌더링과 NGUI 플러그인, 미사용으로 확인한 레거시 결과 화면은 유지했다. 정상 RTTS 결과는 `CompleteLiveGame`에서 레거시 결과 로드를 건너뛴다. 로컬라이징은 이번 범위에 포함하지 않았다.

## 검증 증거

| 항목 | 결과/증거 |
|---|---|
| 에디터 컴파일 | Unity 6000.3.9f1, Android 대상, 현재 프로젝트 심볼에서 컴파일 통과. 플레이어 빌드와 실기기 실행은 별도 |
| 원본 대비 변환 비교 | [candidate-checks.txt](candidate-checks.txt): 위젯 1,955개, 트윈 표본 3,108개, 유효 입력 콜백 54개. 기존 null 콜백 22개 별도 집계 |
| 실제 적용 자산 | [applied-checks.txt](applied-checks.txt): 프리팹 19개에서 NGUI 컴포넌트/NGUI 애니메이션 바인딩 없음, CanvasRenderer 확인 |
| 기존 참조 보존 | 2,219개 참조에 대해 GUID/fileID가 원래 대상 또는 명시한 대체 컴포넌트/UGUI 재질과 정확히 일치 |
| 통합 Play Mode | `_Test_Local` 부트→로비→MainLoading→BallPlay, 투구/카운트, 필드 전환과 타석 복귀 확인. [gameplay-trace.txt](gameplay-trace.txt) |
| 일시정지 | UGUI raycast와 pointer 이벤트로 진입, 버튼 배경·문구·빛 효과 표시 확인. [gameplay-pause.png](gameplay-pause.png) |
| 자동 진행 | 수동 경기에서 기존 자동 전환 콜백→Quick 화면, 이닝별 점수/선수 카드/라인업/주자 갱신 확인. [gameplay-quick.png](gameplay-quick.png), [gameplay-manual-auto.txt](gameplay-manual-auto.txt) |
| 수동 복귀 | 별도 실행에서 기존 `QuickSimulator.goToGame()` API 호출→2회초 수동 경기 복귀, 1:0 점수 반영과 판정 표시/카운트 갱신 확인. [gameplay-return-manual.png](gameplay-return-manual.png), [gameplay-trace.txt](gameplay-trace.txt). 숨겨진 버튼의 실제 입력 통과를 의미하지 않음 |
| 실행 중 의존 | 관찰한 수동 경기/일시정지 상태에서 활성 NGUI 컴포넌트 없음. [runtime-snapshot.txt](runtime-snapshot.txt) |

정적 비교 통과와 전체 경기 정상 동작 합격은 구분한다. 테스트 입력, 임시 활성화와 실제 OS 포인터는 [input-checks.txt](input-checks.txt)에 구분하여 기록했다. 투구 보조는 기존 구종 이벤트/투구 API를 호출하며 안타/주자/점수 상태를 주입하지 않는다. Play Mode 종료 시 이전 씬 배치를 복구하고 씬은 저장하지 않는다.

## 통합 검사에서 수정한 문제

- 투구 후 동적으로 로드되는 추적 프리팹의 잔여 NGUI가 새 UI 루트/카메라를 만들며 경기 HUD를 숨겼다. 해당 프리팹 4개와 zoneTrace를 추가 전환했다.
- 투명 입력 그래픽에 CanvasRenderer와 실제 투명 쿼드를 제공해 UGUI raycast 대상으로 등록되도록 했다.
- 별도 Canvas의 마스크, 중첩 알파, 패널 정렬과 카메라 연결을 보완했다. 입력 EventSystem은 경기 UI 표시/숨김과 독립된 씬 루트에 둔다.
- 가산 합성 아틀라스를 일반 알파 합성으로 표시하던 문제를 수정했다. 전용 UGUI 셰이더는 원본 합성과 stencil/RectMask2D를 함께 지원한다.
- 원본의 잘못된 `blink2` 콜백 이름 `506c6179`를 실제 메서드 `Play`로 복원했다.

## 기준본에서 확인된 문제와 검증 한계

- 점수판 상단 일시정지/자동 버튼의 BoxCollider는 기준 커밋부터 꺼져 있다. 자산 상태는 유지하며 테스트에서만 일시적으로 켰다.
- 일시정지의 계속하기 버튼에는 기준본부터 `pressContinue` 콜백이 없다. 실제 포인터 클릭이 복귀하지 않는 상태를 확인했고, 이후 검사는 기존 `UIPause.pressContinue()`를 테스트에서 직접 호출해 이어갔다.
- Quick 수동 복귀 버튼의 상위 `topUI` 오브젝트는 기준본과 전환본 모두 비활성이다. 일반 버튼 입력 검증과 테스트 API를 통한 화면 전환 검증은 구분한다.
- 확인창 폰트 4개와 추적 UI 아틀라스 5개, 입력 null 콜백 22개 등 기존 누락은 [source-issues.txt](source-issues.txt)에 기록했다. 임의 데이터를 연결하지 않았다.
- tk2d의 Unity Game View 크기 조회 오류 및 일부 캐릭터 애니메이션 누락은 기존 자산/코드 문제다. 검사 명령의 예상 실패 예외는 실제 게임 실행 오류와 구분해야 한다.
- PVP 서버, 실기기 터치, Android 플레이어 빌드, 전체 RTTS 경기→결과→로비 왕복은 아직 검증하지 않았다. 이번 오프라인 수동 경기 fixture는 RTTS 전체 경로의 대체 검증이 아니다.

## 재현 및 변경 관리

`Assets/Editor/IntegratedUGUICommand.cs`가 명시적으로 전달한 로컬 명령만 실행한다. `Library/UGUIMigration/request.txt`에 `check-applied`, `start-play-check`, `snapshot`, `stop-play-check` 등을 쓰고 `result.txt`의 해당 명령 결과를 확인한다. `start-play-check`는 저장되지 않은 씬 변경이 있으면 중단한다. `fixture-auto`/`fixture-manual`은 기존 전환 API를 직접 호출하는 테스트이며 버튼 활성 상태는 변경하지 않는다. 이번 검사 종료 후 Play Mode를 종료하고 Logo 씬 배치를 복구했다.

변환 매핑은 [candidate-plan.json](candidate-plan.json), 원본 참조 기준은 [source-references.json](source-references.json), 최초 적용 해시는 [apply-manifest.json](apply-manifest.json)에 보관했다. `apply_migration.py`는 적용 전 원본 해시를 검사하므로 이미 적용된 자산에 재실행하지 않는다. 적용 전 백업과 중간 생성물은 `Library/UGUIMigration`에 보관한다. 기존 사용자 변경은 별도로 유지하며 커밋/푸시는 수행하지 않았다.

비교용 프리팹 19개와 meta는 참조가 없는 것을 확인한 뒤 `Library/UGUIMigration/Candidates`로 옮겼다. 배포 자산에는 중복 비교용 프리팹이 포함되지 않는다. 현재 상태에서 반복 가능한 검사는 `check-applied`이며, `check-candidates`는 적용 이전 사본을 사용한 비교 기록이다. 생성된 네이티브 폰트/스프라이트 카탈로그/클립/재질은 `Assets/MainGame/UI/IntegratedUGUI`에 유지한다.
