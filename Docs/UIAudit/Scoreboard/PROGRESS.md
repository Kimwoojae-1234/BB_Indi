# 점수판 표시 전환 진행 기록

2026-09-23. 기준 커밋 `20670c0b` 위의 작업 디렉터리 변경. Unity 6000.3.9f1, Android 대상 에디터, `_Test_Local`. 로컬라이징은 착수하지 않았다.

## 구현

- `IngameUIPrefab`의 `scoreboard/_active/board` 표시 계층을 실제 UGUI Image 20개, TMP 텍스트 5개, 위치 트윈 3개로 교체했다. 이 계층의 NGUI 컴포넌트 28개를 제거했다.
- `UIScoreBoard`의 기존 공개 호출은 유지하고, `Init`/`BoardUpdate`/`SetActive`를 새 표시 컴포넌트와 연결했다. 아직 이전하지 않은 상단 버튼·타이머는 기존 구현을 사용한다.
- 원본 atlas의 투명 여백, 홀수 크기 이미지의 NGUI 보정, bitmap font의 글리프·kerning, 등장/공격 표시 트윈 곡선을 보존했다.
- 공존 기간의 부모 패널 alpha와 render queue를 CanvasGroup/UGUI 재질에 전달한다. 새 표시에는 GraphicRaycaster가 없고 모든 Graphic의 raycastTarget이 꺼져 있다.
- 프리팹의 기존 직렬화 오브젝트 중 수정된 것은 UIScoreBoard와 board 부모 Transform 2개뿐이다. 나머지는 기존 board 아래 오브젝트의 삭제/신규 생성이다. [변경 범위](prefab-change-scope.json), [참조 검사](serialized-reference-checks.json).

## 수행한 검사

| 항목 | 결과와 범위 |
|---|---|
| 에디터 컴파일 | 새 런타임/에디터 코드 컴파일 후 메뉴 실행 성공 |
| 표시 상태 | 홈/원정 2 × 초/말 2 × 볼 0–4 × 스트라이크 0–3 × 아웃 0–3 × 주자 8 조합 = 2,560개 통과 |
| 추가 검사 | 10개 로고 매핑, 50×50 native 크기, 0/12 점수 매핑, 필수 sprite/font, NGUI 없는 표시 계층, raycast 차단 없음, alpha/queue, 등장 트윈 시작·중단 통과 |
| 분리 렌더링 | 1280×720에서 원본과 후보 캡처, 빈 이미지 감지 포함. 화면을 직접 비교했으며 픽셀 완전 일치를 주장하지 않음 |
| 실제 원본 기준선 | Logo→MainLobby 초기화 후 기존 MainLoading→BallPlay 수동 경기. 점수판 표시, 투구/볼/스트라이크/아웃 변화 확인 |
| 전환 후 실제 경기 | 수동 경기에서 점수판 표시, 투구/카운트/아웃 갱신, 1회초→1회말 교대와 공격 표시 전환 확인. 원본과 이닝/초말/점수/카운트 체크포인트 21개가 순서대로 일치. 관찰 구간에서 기준선 대비 새 오류 없음 |
| PVP·실기기·전체 왕복 | 아직 미검증 |

[자동 검사 결과](presentation-checks.txt), [NGUI 분리 렌더](legacy.png), [UGUI 분리 렌더](ugui.png), [원본 경기 화면](gameplay-legacy.png), [UGUI 경기 화면](gameplay-ugui.png), [원본 수동 경기 로그](manual-gameplay-legacy.txt), [UGUI 수동 경기 로그](manual-gameplay-ugui.txt), [실행 비교](runtime-comparison.json).

Game 뷰에 대한 자동 클릭으로는 로비/경기 버튼 반응을 확인하지 못했다. 실제 포인터 입력은 성공으로 기록하지 않고 별도 확인한다. 에디터 메뉴 실행과 그림 렌더링, 실제 매치의 자동 투구 진행은 이 제한과 구분한다.

## 기준선에서 확인한 기존 문제

- MainLoading을 단독으로 시작하면 `MusicManager.Get()`가 null이라 Batting 초기화가 실패한다. 검증 도구는 Logo 초기화를 먼저 거친다. [단독 실행 로그](standalone-loading-baseline.txt)
- `_Test_Local`은 기본적으로 자동 시뮬레이션이므로 이 HUD를 검증할 수 없다. 검증 도구는 BallPlay의 Start 전에 Manual / bSimulationQuickPlay=false를 설정하고, 두 실행에서 같은 random seed를 사용한다. 이는 에디터 검증 설정이며 프로덕션 경기 규칙은 바꾸지 않는다.
- tk2d의 GameView reflection 경고, `DEADBALL2` 애니메이션 누락, 일부 투수 애니메이션 이름 누락이 원본 수동 경기에서 발생했다.
- 원본 빠른 시뮬레이션 중 `skillUISetter.destroyObject` null 예외도 관찰했다. [원본 자동 시뮬레이션 로그](quick-simulation-baseline.txt)

위 항목은 전환 회귀와 구분하며, 이 점수판 변경에서 게임/애니메이션 코드를 수정하지 않았다.

## 재현 도구

Unity `Tools > UI Migration` 메뉴:

1. `Run Scoreboard Presentation Checks`: 상태·참조·비간섭 검사와 분리 렌더. 실제 경기 검증을 대체하지 않는다.
2. `Start Offline Gameplay Check`: 열려 있는 씬을 보존하고 Logo 부트 후 수동 경기로 진입한다. 씬에 미저장 변경이 있으면 실행을 거절한다. Play Mode 종료 시 원래 씬 구성을 복원한다.
3. `Capture Gameplay Frame`: 실제 Game 뷰 프레임을 저장한다.

`Build UGUI Scoreboard Candidate`와 `Apply Scoreboard To Game Prefab`은 초기 변환 도구다. 이미 이전한 대상에는 중복 적용을 거절한다. 새 표시 프리팹은 `Assets/MainGame/UI/ScoreboardUGUI/ScoreboardDisplay.prefab`에 있다.

## 다음 합격 기준

실제 포인터 입력·득점·결과 복귀 및 지원 모드·목표 기기 검증은 누적 미검증 항목으로 추적한다. 사용자의 단계별 진행 요청에 따라 [구종 선택](../PitchSelection/PROGRESS.md) 전환을 이어갔으며, 이 남은 항목들은 최종 합격 전에 확인한다. 전체 전환 완료나 출시 검증 완료로 판정하지 않는다.

다음 구종 선택 단계 시작 전에 Play Mode가 종료된 것을 확인했다. 앞선 검증에서만 바뀐 공유 Spine material 3개의 텍스처 참조와 최근 씬 목록을 시작 상태로 복원했다. 시작부터 변경돼 있던 `Assets/-tk2d.asset`, 레이아웃, `.vscode`, solution 파일은 사용자의 기존 변경으로 취급한다. 실제 포인터 입력 확인은 여전히 미검증이다.
