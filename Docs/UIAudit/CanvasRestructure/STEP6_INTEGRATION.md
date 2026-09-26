# 6단계 — 전체 경기 통합 검증과 정리

2026-09-26, 기준 커밋 `0d694c64` (`ngui-to-ugui-migration`). **로컬 경기 → 결과 → 로비 경로를 검증했고, 결과 점수판에서 발견한 마이그레이션 회귀를 수정했다.** RTTS 서버 결과 저장, PVP, 실기기와 아래 입력/데이터 제약은 남아 있으므로 전체 전환의 최종 정상 동작 합격으로 처리하지 않는다.

## 환경과 실행 방식

- Unity 6000.3.9f1, Windows, Android 대상 에디터 컴파일. Game View 905×461.
- 기존 `_Test_Local` 부트: Logo → MainLobby 초기화 → MainLoading → BallPlay. `ConfigureDongneYagu()`와 난수 seed 230923 사용.
- 기존 수동/자동 전환 API, 투구 선택·릴리스 보조, 일부 구간 `Time.timeScale=8`을 사용했다. 점수·이닝·경기 판정은 주입하지 않았다. 두 실행의 입력 시점이 달라 최종 점수는 각각 9:2, 6:2다.
- `Step6/`는 최초 실행, `Step6/Retest/`는 결과 점수판 수정 후 재실행이다. 최초 실패 증거를 덮어쓰지 않았다.
- 실제 OS 클릭, Unity 이벤트/기존 API 호출, 정적 프리팹 검사를 구분한다. 결과에서 로비 복귀는 `ResultUI.OnClickBacktoLobby()`를 호출한 검사이며 결과 버튼의 실제 클릭 통과를 뜻하지 않는다.
- 검사 종료 후 Play Mode를 중지하고 원래 Logo 씬 배치를 복원했다. 씬을 저장하지 않았으며 `runInBackground`도 이전 값으로 복원했다.

## 결과 점수판 회귀 수정

첫 경기 종료 시 `scoreboard.initScoreBoard()`가 `GameUIElement`를 찾다가 null 예외를 발생시켰다. 공유 `scoreboard`는 UGUI로 전환됐지만, 이전 범위에서 제외했던 레거시 `resultPrefab`은 여전히 NGUI `UILabel`/`UISprite`를 사용하고 있었다. [실패 로그](Step6/gameplay-trace.txt), [실패 화면](Step6/result-failure.png).

레거시 결과 전용 `LegacyResultScoreboard`를 분리하고 `UIResultMain.board`와 결과 프리팹의 스크립트 GUID를 연결했다. 원본 NGUI 결과 점수판의 초기화, 12이닝, R/H/E, X·끝내기 표기 및 팀 표시 동작을 유지했다. 결과 프리팹의 기존 fileID·오브젝트 참조·콜백은 모두 보존했다. QuickSimulator와 공수교대의 UGUI `scoreboard`는 그대로 사용한다.

이 수정은 제외했던 레거시 화면을 새로 UGUI 전환한 것이 아니라, 공유 코드 변경으로 끊어진 기존 연결을 복구한 것이다. 따라서 레거시 결과 화면에서는 활성 NGUI가 관찰되는 것이 예상 동작이다. RTTS의 `Popup_GameResult`와 구분한다.

## 실행 결과

| 시나리오 | 결과와 증거 |
|---|---|
| 수동 경기 진입 및 HUD | 실제 투구·카운트·필드 전환·타석 복귀 확인. [경기 로그](Step6/gameplay-trace.txt), [화면](Step6/manual-playing.png) |
| 일시정지/재개 | 기존 API로 정지·재개와 화면 표시 확인. 실제 상단 버튼 및 계속하기 클릭은 기존 연결 문제로 실패. [정지 화면](Step6/pause.png), [입력 기록](Step6/input-checks.txt) |
| 수동 → Quick → 수동 | 기존 전환 API로 이동. 4회초 수동 복귀 시 원정 1/홈 2점과 HUD 홈:원정 `2:1` 일치. [Quick](Step6/quick.png), [수동 복귀](Step6/manual-return.png) |
| 공수교대 및 수비 | 실제 4회초 아웃 0→3, 4회말 전환과 수비 아웃 진행 확인. 일부 투구는 기존 보조 API 사용. [상태 로그](Step6/integration-trace.txt) |
| 실제 구종 클릭 | OS 포인터로 싱커 선택 후 구종 창이 닫히고 투구 조작 UI 표시됨. 전체 투구 제스처 통과 판정과 구분. [선택 후 화면](Step6/pitch-selected.png) |
| Quick 동적 종료 팝업 | 기존 pause API로 생성, `Canvas/Popups` 부모 확인. **취소는 실제 OS 클릭으로 닫힘/경기 진행 재개 확인**. [화면](Step6/quick-popup.png), [부모/참조](Step6/quick-popup.txt) |
| 스킬 연출 스킵 null 회귀 | 실제 Quick 인스턴스에서 `setHoldFastForward` 진입/해제 3회. `skillStot` 없는 두 UI에서 예외 없이 정리되고 배경/캡처 숨김, timeScale=1 복원. [PASS 기록](Step6/integration-trace.txt). OS 길게 누르기 전체 입력 경로는 미검증 |
| 9회 종료 → 결과 | 수정 후 새 경기에서 6:2 종료. 점수판 이닝 합계와 R/H/E `6/14/1`, `2/7/1` 확인. [결과 화면](Step6/Retest/result.png), [텍스트](Step6/Retest/result.txt) |
| 결과 → 로비 | 기존 로컬 결과 복귀 API로 MainLobby 표시, EventSystem 1개. [로비 화면](Step6/Retest/lobby-return.png), [상태 기록](Step6/Retest/integration-trace.txt) |
| 실행 중 UI 의존 | 캡처한 수동/정지/Quick/종료 팝업에서 활성 NGUI 없음. 관찰한 BallPlay 상태의 EventSystem 1개. 레거시 결과는 위에 설명한 예외 |
| 수정 후 오류 | 재실행 로그에는 기존 tk2d Game View 경고만 기록됨. 결과 점수판 및 스킬 정리의 새 예외 없음. [재실행 로그](Step6/Retest/gameplay-trace.txt) |

`manual-start.png`는 부트 직후 Logo 캡처다. 수동 경기 진입 증거로 사용하지 않는다. Quick 중 숨겨진 수동 HUD의 `display=0:0`은 활성 Quick 점수판의 표시값이 아니다.

## 자동/정적 검증

- 현재 Unity에서 런타임·에디터 어셈블리 컴파일 후 수정한 실제 경기와 검사 명령을 실행했다. Android 플레이어 빌드는 실시하지 않았다.
- 실제 결과 프리팹을 저장 없이 로드해 연결, 양 팀 12이닝 슬롯, 0/숫자/X/끝내기 `3X`, 미진행 이닝 숨김, 합계, 팀 표시 및 현재 이닝 표시 숨김 검사 통과. [검사 결과](Step6/result-scoreboard-checks.txt).
- 결과 프리팹의 변경이 스크립트 GUID 1건뿐인지 검사했다. 기존 Quick/메인 프리팹, UGUI 점수판, 이전 `skillUISetter` 수정과 선수 생성 코드는 기준 커밋과 동일하다. [범위 검사](Step6/static-checks.txt).
- 5단계 직렬화 구조 검사도 재실행해 통과했다. `git diff --check` 통과.

## 기존 문제와 남은 검증

1. **기존 버튼 연결:** 상단 정지/자동 버튼 Collider 비활성, Pause 계속하기에 `pressContinue` 콜백 누락, Quick 수동 전환 버튼의 부모 비활성은 이전 기준본부터 존재한다. 이번 실행에서도 실제 정지/계속하기 입력 실패를 확인했다. API를 통한 전환 통과로 이를 가리지 않는다.
2. **로컬 선수 교체 데이터:** 교체 API 호출 시 `changeController2.Init:46`에서 `player.getCard().abilities` 접근이 실패했다. [재실행 스냅샷](Step6/Retest/manual-data.txt)에서 타자/투수 카드가 null이고 `overRoll`/`nameLabel` UI 참조는 모두 유효함을 확인했다. `_Test_Local`의 카드 없는 선수 생성과 카드 접근은 전환 전부터 동일하다. 임의 능력치를 연결하지 않았으며 정상 카드 데이터의 교체 완료는 미검증이다.
3. **기존 렌더링/애니메이션:** tk2d Game View 조회 경고, DEADBALL2 및 PITCHER_* 애니메이션 누락은 기존 문제로 별도 기록한다. 첫 실행에서 관찰됐으며 이 작업에서 수정하지 않았다.
4. **스킬과 터치:** 현재 로컬 설정의 `SKILL_TYPE=0` 때문에 자연 발생 스킬의 전체 연출은 검증하지 못했다. 정리 메서드의 null 회귀 검사와 구분한다. 실제 길게 누르기, 기기 터치, 모든 타격/주루 제스처와 반복 재진입은 남아 있다.
5. **RTTS/PVP/기기:** RTTS 완료는 `RttsManager.CompleteRound()` 이후 경기 기록·보상을 백엔드에 저장한다. 계정에 저장하는 테스트 경기 실행 승인을 요청한 상태이며 실행하지 않았다. 로컬 레거시 결과 왕복으로 RTTS를 통과 처리하지 않는다. 실시간 PVP·Android 빌드/실기기·다른 비율의 전체 경기도 미검증이다.

다음 검증은 정상 카드 데이터를 사용하는 환경에서 교체를 확인하고, 승인된 테스트 계정으로 RTTS 경기→결과→로비를 실행하는 것이다. 기존 입력 연결 문제를 해결하기 전에는 모든 조작이 정상이라고 판정할 수 없다.

## 재현

- 정적 검사: `python Docs/UIAudit/CanvasRestructure/verify_integration_scope.py`
- 열린 Unity의 기존 명령 통로 `Library/UGUIMigration/request.txt`에 명령을 하나씩 기록하고 `result.txt`에서 해당 명령 결과를 확인한다.
- 편집 모드 결과 프리팹 검사: `integration:check-result`.
- 로컬 경기 시작: `integration:start:새실행명` (영문/숫자/하이픈). 이전 실행 로그가 있으면 덮어쓰지 않고 중단한다. 저장되지 않은 씬 변경이 있으면 시작하지 않는다.
- `integration:auto`, `integration:manual`, `integration:pause`, `integration:resume`, `integration:quick-pause`, `integration:skip-regression`은 기존 API를 사용하는 테스트 명령이다. 실제 버튼 입력과 구분한다.
- `integration:assist`/`assist-off`, `speed8`/`speed1`, `capture:이름`으로 진행과 캡처를 제어한다. 로컬 레거시 결과에 도착한 경우만 `integration:return-lobby`를 실행한다. 종료는 `integration:stop`.

Git 업로드 메시지:

```text
fix: 결과 점수판 호환성 복구 및 경기 UI 통합 검증

- 레거시 결과 점수판을 분리해 UGUI 전환 후 종료 시 null 예외 수정
- 수동·Quick 전환, 9회 종료, 결과 표시와 로비 복귀 검증
- 스킬 연출 스킵 반복 시 null 참조 및 속도 복원 회귀 검사 추가
- 실제 입력과 테스트 API 증거, 기존 문제와 미검증 범위 정리
```
