# 투구 전 도루·견제 선택 UGUI 전환

2026-09-23, Unity 6000.3.9f1 / Android 대상 에디터 / `_Test_Local` 수동 오프라인 경기. 세 번째 전환 단위이며, 프로젝트 전체 NGUI 제거와 로컬라이징은 아직 완료되지 않았다.

## 적용 범위

실제 `IngameUIPrefab`의 `runnerControl/_active`를 UGUI Canvas, Image, Text, Button으로 교체했다. 1·2·3루 점유/능력치/스킬 표시, 도루·견제 선택, 선택 시 점멸, 표시·숨김을 포함한다. 필드 뷰의 진루·귀루 조작과 주자 미니맵은 별도 전환 대상이다.

- `ControlRunner`의 기존 명령 API와 도루 연쇄 선택, 견제 전달, PVP 분기를 유지한다. 다른 레거시 프리팹에는 기존 표시 경로를 남긴다.
- 원형 반경 40의 입력 영역과 누르는 순간 실행하는 동작을 유지한다. 해제/click 중복 실행을 막고, 같은 버튼에 두 번째 포인터가 들어와도 중복 실행하지 않는다.
- 원본 버튼끼리 겹치는 영역은 NGUI의 동일 depth로 선택 순서가 보장되지 않았다. UGUI에서는 가까운 베이스 중심을 선택하도록 명확히 했다.
- 기존 프레임당 alpha 0.015의 숨김 속도를 유지하고, 재표시하면 진행 중인 숨김 코루틴을 취소한다. 숨김 시작과 일시정지 시에는 입력을 차단한다.
- 원본 atlas, NanumSquareEB 폰트와 60/80/100 능력치 경계 색상, 흰색 outline 설정을 사용한다. 원본 NGUI 숫자는 현재 Unity의 정적 렌더와 실제 경기에서 검게 표시됐다. UGUI에는 코드에 지정된 등급 색상과 outline이 나타나므로 픽셀 완전 일치가 아니다. 폰트 원본 파일은 변경하지 않았다.
- `runnerControl` 소유자의 UIPanel은 공존 기간의 alpha·카메라·정렬 전달용으로 남긴다. 변환한 `_active` 내부에는 NGUI 또는 Collider가 없다.
- 구종 선택과 주자 선택은 필요한 경우 `IngameUI` 아래에 만든 하나의 EventSystem을 공유한다. 한 UI가 숨겨져도 다른 UI의 입력이 유지되도록 공용 Canvas 처리로 묶었다. 구종 선택의 기존 스크립트 GUID와 직렬화 연결은 유지한다.
- 사용하지 않던 고의사구 버튼은 기존 `Init`과 같이 숨겨 둔다. 로컬라이징과 경기 규칙은 이번 범위에 포함하지 않는다.

NGUI 28개(UILabel 3, UISprite 17, UIEventTrigger 4, TweenAlpha 3, UIButtonScale 1), CapsuleCollider 3개와 BoxCollider 1개를 제거했다. 기존 오브젝트 중 부모 Transform과 ControlRunner 두 개만 수정되고 해당 표시 분기가 교체됐다. 신규 미해결 로컬 참조는 없다. [변경 범위](prefab-change-scope.json)

이 단계 이후 메인 경기 프리팹에는 비활성 계층을 포함해 NGUI **1,660개**가 남는다. 프로젝트 전체 수량과 구분한다. [남은 분기](remaining-main-prefab-branches.json)

## 검증 기록

- 표시 768조합(공격/수비 × 점유 8종 × 스킬 8종 × 능력치 6경계값), 기존 capsule과 비교한 1,092개 입력 위치, 표시 순서·입력 차단·직렬화 연결 검사를 통과했다. [결과](presentation-checks.txt)
- 동일 조건의 1280×720 분리 렌더를 비교했다. [원본](legacy.png), [UGUI](ugui.png), [원본 선택](legacy-selected.png), [UGUI 선택](ugui-selected.png)
- 원본과 전환본의 Play Mode 검사는 Logo→MainLobby→MainLoading→BallPlay로 진입한 뒤 실제 게임의 주자 생성 함수를 사용한다. 경기 진행과 예약 투구를 멈춘 통제된 시나리오에서 Unity 이벤트를 전달하며, 자연스럽게 안타로 출루한 경기 또는 OS 입력 검사가 아니다.
- 원본은 도루 연쇄 선택 12조합, 주자 없음/잘못된 경기 단계/비활성 상태/숨김 중 입력 차단, 숨김 완료, PVP433 견제 금지 조건, 2루 견제 명령의 필드 컨트롤러 전달을 통과했다. [원본 검사](Legacy/interaction-checks.txt)
- 전환본도 위 동작을 통과했다. 실제 경기의 `EventSystem.RaycastAll`이 각 베이스 버튼을 선택했고, 두 번째 포인터/해제/click 중복 실행 방지, 재활성화 시 포인터 초기화, scaled time이 0인 동안의 선택 점멸, 숨김 취소 후 재표시, 일시정지 입력 차단, 구종 선택과 EventSystem 1개 공유 및 서로 숨겨질 때의 입력 유지도 통과했다. [UGUI 검사](UGUI/interaction-checks.txt), [실행 비교](runtime-comparison.json)
- 공용 Canvas와 변환 헬퍼 변경 후 구종 선택 960조합과 점수판 2,560조합 검사를 재실행해 통과했다. [구종 선택](../PitchSelection/presentation-checks.txt), [점수판](../Scoreboard/presentation-checks.txt)
- 기존 구종 선택도 일반 오프라인 경기에서 1회말 수비 이닝까지 진행해 선택→종료→투구 준비와 재진입 검사를 다시 통과했다. 세 번째 단계 이전 실행과 이닝·초말·점수·카운트 체크포인트 22개가 일치했고 신규 오류는 없었다. [회귀 검사](PitchRegression/interaction-checks.txt), [비교](PitchRegression/comparison.json)

초기 재표시 검사는 타격 컨트롤러가 소유한 AI 투구 예약을 멈추지 않아 실패했다. 호출 추적에서 `Pitcher.startPichingAnim3 → getSign → ControlRunner.SetActive(false, true)`가 정상적으로 새 숨김을 요청한 것을 확인했다. 검사 도구에서 해당 예약까지 멈추도록 수정했으며 게임의 투구 로직은 수정하지 않았다. [실패 기록](FixtureFailure/interaction-checks.txt), [진단 기록](FixtureFailureDiagnostic/interaction-checks.txt), [호출 경로](FixtureFailureDiagnostic/activation-cause.txt)

원본 실행에서도 tk2d GameView 크기 조회 오류와 `GETUP_AFTER_SLIDING_S` 애니메이션 누락이 발생했다. 최종 UGUI 통제 검사에는 원본 대비 신규 오류가 없었다. 기존 데이터/플러그인 오류와 UI 이전 회귀를 구분한다. [원본 로그](Legacy/gameplay-trace.txt), [UGUI 로그](UGUI/gameplay-trace.txt)

OS 마우스·실기기 터치, 실시간 PVP, 도루/견제의 최종 세이프·아웃 판정, 자동 플레이, 특수 모드, 전체 경기→결과→로비 왕복은 미검증이다. 통제된 검사 통과를 이 항목들의 통과로 확대하지 않는다.

Play Mode를 종료하고 원래 Logo 씬을 복원했다. 검사 중 텍스처가 바뀐 공유 주자 Spine material 2개는 시작 상태로 복원했고, 기존 사용자 변경은 유지했다. Android 대상 에디터에서 컴파일·실행한 결과이며 플레이어 빌드는 수행하지 않았다.

## 재현

Unity `Tools > UI Migration > Run Runner Control Presentation Checks`와 `Start Runner Control Gameplay Check`를 사용한다. 후자는 Play Mode 안의 일회성 경기 상태를 조정하고 종료 시 원래 씬 구성을 복원한다. 검사 코드는 `Assets/Editor` 아래에 있어 플레이어 빌드에 포함되지 않는다. `Build UGUI Runner Control Candidate`와 `Apply Runner Control To Game Prefab`은 이미 적용된 상태에서 중복 실행을 거절한다.
