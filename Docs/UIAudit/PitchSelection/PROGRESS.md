# 구종 선택 UGUI 전환

2026-09-23, Unity 6000.3.9f1 / Android 대상 에디터 / `_Test_Local`, 수동 오프라인 경기. 스코어보드 표시 다음 단계다. 전체 NGUI 제거와 로컬라이징은 아직 완료되지 않았다.

## 범위와 동작

`IngameUIPrefab`의 `pitchingSelect/_active`를 UGUI Canvas, Image/RawImage, TMP, Button으로 교체해 실제 경기 프리팹에 적용했다. 기존 `ControlPitchingSelect`의 구종 결정·PVP 송신·자동 선택·준비 완료 로직은 유지하고 `pitchingSelectButton`에 새 표시 연결을 추가한다. 다른 레거시 프리팹에는 기존 경로를 남긴다.

- 5개 버튼, 구종 이미지, 능력치 숫자와 60/80/100 경계의 등급 색상을 보존한다.
- 230×58 입력 영역, 누른 뒤 놓을 때 선택하는 기존 동작(영역 밖에서 놓는 경우 포함), 0.9배 축소, 한 번만 선택되는 잠금을 재현한다.
- 순차 등장, 선택 표시, 확대 링, 선택/비선택 버튼 퇴장과 기존 컨트롤러의 1.45초 닫힘 시점을 유지한다. 재진입 시 이전 코루틴과 포인터 상태는 초기화한다.
- 원본의 depth 순서에 맞춰 링을 모든 버튼 위에 그린다. 부모 NGUI 패널의 alpha와 render queue를 전달한다.
- BallPlay에는 EventSystem이 없어 필요한 경우 StandaloneInputModule과 함께 생성한다. 주자 UI를 옮긴 세 번째 단계부터는 `IngameUI` 아래에서 공유해 개별 UI의 숨김과 무관하게 입력을 유지한다. 기존 EventSystem이 있으면 사용하고, 재활성화 때 중복 생성하지 않는다. 일시정지 상태에서는 CanvasGroup 입력을 차단한다.
- 로컬라이징이나 게임 규칙 변경은 이번 범위에 포함하지 않는다.

NGUI 컴포넌트 40개(UILabel 5, UISprite 15, UITexture 10, UIEventTrigger 5, UIButtonScale 5)와 BoxCollider 5개를 해당 계층에서 제거했다. 게임 프리팹의 기존 오브젝트 중 부모 Transform과 ControlPitchingSelect 두 개만 변경됐고, 나머지는 해당 분기 교체다. 신규 미해결 로컬 참조는 없다. [변경 범위](prefab-change-scope.json)

이 단계 종료 시 메인 경기 프리팹의 NGUI 컴포넌트는 1,688개였다. [당시 분기별 수량](remaining-main-prefab-branches.json)은 비활성 계층도 포함하며 프로젝트 전체 수량과 구분한다. 최신 수량과 공용 입력 처리 변경은 [세 번째 단계](../RunnerControl/PROGRESS.md)를 참조한다.

## 검증

- 표시 960조합(5개 버튼 × 24구종 × 능력치 8값), 1~5개 슬롯의 GraphicRaycaster 입력 영역, callback 연결, 계층의 NGUI·Collider 제거, 효과 순서, 입력 차단, alpha/정렬 검사 통과. [결과](presentation-checks.txt)
- 1280×720 동일 조건의 원본/후보 정적 렌더와 효과 렌더를 비교했다. [원본](legacy.png), [UGUI](ugui.png), [원본 효과](legacy-effects.png), [UGUI 효과](ugui-effects.png). 픽셀 완전 일치를 주장하지 않는다.
- 원본과 전환본을 각각 Logo→MainLobby→MainLoading→BallPlay에서 시작해 1회말 수비 이닝까지 진행했다. 이닝·초말·점수·카운트 체크포인트 22개가 일치했으며, 해당 관찰 구간에서 원본 대비 신규 오류는 없었다. [실행 비교](runtime-comparison.json)
- 두 버전 모두 구종 선택 후 기존 지연에 따라 선택창이 닫히고 `PLAY_BATTING_VIEW`로 진행했다. [원본 검사](Legacy/interaction-checks.txt), [UGUI 검사](UGUI/interaction-checks.txt), [원본 경기 화면](Legacy/menu.png), [UGUI 경기 화면](UGUI/menu.png), [선택 이후](UGUI/after-selection.png)
- 전환본 실제 경기에서 EventSystem 1개와 입력 모듈, UI 카메라, 5개 버튼 등장 위치, EventSystem.RaycastAll의 대상 버튼, 누름 축소 0.9, 다른 터치 해제 무시, 영역 밖 놓기 선택, 전체 선택 잠금, flash→ring→종료를 확인했다.
- 독립 후보 인스턴스에서 release/click/중복 release가 callback을 한 번만 실행하고 재활성화 때 이전 퇴장 코루틴·포인터·축소·효과가 초기화되는지 확인했다. EventSystem 중복도 발생하지 않았다.
- 검사는 Unity 이벤트 직접 전달을 사용했다. OS 마우스·실기기 터치, PVP, 자동 선택 타임아웃, 경기 전체 왕복은 미검증이다. 이 단계의 통과를 프로젝트 전체 동작 보증으로 보지 않는다.
- 공용 변환 헬퍼 변경 후 기존 점수판의 2,560 상태 검사를 재실행해 통과했다.

초기 검증 도구는 Editor 어셈블리의 MonoBehaviour를 런타임 컴포넌트로 생성하지 못해 실패했다. 해당 시도는 [별도 로그](FixtureFailure/gameplay-trace.txt)에 보존했다. 도구를 일반 C# 검사 객체와 기존 매니저의 코루틴 실행으로 수정한 후 위 원본/UGUI 검사를 새로 완료했다. 게임 런타임 코드의 오류와 구분한다.

검사 종료 후 Play Mode를 종료하고 원래 Logo 씬으로 복원했다. 공유 Spine material 3개와 `UserSettings/EditorUserSettings.asset`에는 시작 상태 대비 파일 변경이 없는 것을 확인했다. 사용자의 기존 tk2d 설정·레이아웃·프로젝트 파일 변경은 유지했다.

## 기존 데이터의 제약

현재 atlas에는 `pselect_1`~`pselect_27`이 있지만 enum의 H_FORK(106), UPSHOOT(107)에 해당하는 이미지는 없다. 원본처럼 해당 이름은 빈 표시로 유지한다. 임의로 다른 구종 이름을 대응시키지 않았다.

선수 정보 UI는 Android의 `GIRL_PLAY`에서 비활성화되며 일부 기존 참조가 null이다. 실제 사용 중인 구종 선택을 먼저 옮겼고 선수 정보도 전체 전환 목록에 남아 있다.

## 재현

Unity `Tools > UI Migration`의 `Run Pitch Selection Presentation Checks`, `Start Pitch Selection Gameplay Check`를 사용한다. 후자는 열려 있는 씬을 보존하고 Play Mode 종료 때 복원하며, 원본과 UGUI 로그를 각각 `Legacy`, `UGUI` 폴더에 기록한다. 에디터 검사 전용 이벤트 주입은 `Assets/Editor` 아래에만 있어 플레이어 빌드에 포함되지 않는다.

`Build UGUI Pitch Selection Candidate`와 `Apply Pitch Selection To Game Prefab`은 초기 변환 도구이며 중복 적용을 거절한다.
