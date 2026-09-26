# NGUI 제거 후 전체 경기 실행 검증

검증일: 2026-09-26. 기준 커밋 `4cf10ca0` (`ngui-to-ugui-migration`)에 이 문서의 수정 사항을 적용했다. Unity 6000.3.9f1 Windows Editor, Android 선택 상태, `_Test_Local`, 비 RTTS·비 PVP, Game View 1918×999 환경이다.

**판정: 로컬 경기의 시작→이닝 진행→종료→결과·기록→로비 복귀를 확인했다. 전체 조작 정상 판정은 아니다.** 실제 플레이에서 UI 회귀 3건을 수정했다. 기존 버튼 연결, 선수 교체 데이터, tk2d·Spine 문제는 남아 있다. Android 기기와 서버 경기 결과 저장은 검증하지 않았다.

## 완료한 경기

점수는 내 팀:상대팀이다. 모든 경기는 기존 경기 로직으로 진행했으며 점수·이닝·아웃·승패를 주입하지 않았다. 수동 경기는 입력 보조를 사용했고, 긴 진행 구간에는 테스트용 `timeScale=8`을 적용했다. 전체 경기를 사람이 마우스로만 조작한 검사는 아니다.

| 실행 폴더 | 방식과 종료 | 결과 | 확인 사항 |
|---|---|---|---|
| [NGUIGameplayManualFinal](Integration/NGUIGameplayManualFinal/integration-trace.txt) | 수동, 9회초 종료·홈 승리 | 0:3 | 17개 반이닝, 결과의 9회말 X, 기록 탭, 실제 마우스 입력으로 로비 복귀 |
| [NGUIGameplayQuickFinal](Integration/NGUIGameplayQuickFinal/integration-trace.txt) | Quick→수동→Quick, 8회말 콜드게임 | 12:2 | 일시정지 표시 수정, Quick 취소 팝업, 스킬 스킵 반복, 모드 전환 |
| [NGUIGameplayRecordsFixed](Integration/NGUIGameplayRecordsFixed/integration-trace.txt) | Quick, 9회말 종료 | 8:1 | 숨겨진 투수 탭 보정 확인, 긴 타자 목록의 입력 연결 문제 발견 |
| [NGUIGameplayFinal](Integration/NGUIGameplayFinal/integration-trace.txt) | 최종 수정 후 Quick, 9회말 종료 | 14:1 | 양 팀 타자·투수 기록, 실제 휠 입력으로 교체 선수까지 스크롤, 기록 닫기·다음·로비 복귀 |

수동 경기에서는 실제 `ControlBattingUI.swing()` 콜백 80회, `EventSystem.RaycastAll`을 통과한 구종 선택 47회, `ControlPitchingUI.setRelease()` 47회가 실행됐다. HUD 점수 표본 6,919개에서 1초 이상 지속되는 내부 점수와의 불일치는 0건이었다. [집계](Integration/NGUIGameplayManualFinal/full-gameplay-summary.txt)의 `Native pitch selections`는 OS 마우스가 아니라 EventSystem 입력을 뜻한다. 검사기 명칭은 이후 `Raycast pitch selections`로 명확히 했다.

최종 경기 R/H/E는 내 팀 14/19/0, 상대팀 1/11/1이었다. 실제 결과 화면과 스냅샷 값이 일치했다. [최종 결과](Integration/NGUIGameplayFinal/final-result.png), [값](Integration/NGUIGameplayFinal/final-result.txt), [로비 복귀](Integration/NGUIGameplayFinal/final-lobby-return.png).

## 이번에 수정한 UI 문제

### 1. 비활성 UI의 배치 Graphic이 노출됨

일시정지에서 숨겨진 PVP용 ‘선수교체’ 글자가 일반 글자와 겹쳤다. 처음부터 비활성인 위젯은 `OnDisable`을 거치지 않는데, Graphic이 활성 상태의 형제 Canvas에 직렬화되어 남아 있었다.

`GameUICanvasBatch.Apply()`에서 각 위젯 소유자의 활성 상태를 presentation에 동기화했다. 처음 숨겨진 위젯, 첫 활성화, 활성 형제 유지, 재배치·파괴를 Play Mode에서 검사했다.

- [수정 전 검사 실패](gameplay-batch-before.txt) → [수정 후 검사 통과](gameplay-batch-checks.txt)
- [중복 표시](Integration/NGUIGameplayFull/pause-continue.png) → [실제 경기 수정 화면](Integration/NGUIGameplayQuickFinal/pause-fixed.png)

### 2. 숨겨진 기록 탭의 행이 화면 밖으로 밀림

투수 기록 데이터가 존재하는데 탭이 비어 보이거나 맨 아래 일부만 표시됐다. 비활성 탭에서 콘텐츠 크기를 계산할 때 모든 행을 제외했고, 이미 비활성인 `GameUIScroll`을 끄는 것만으로는 native `ScrollRect`가 꺼지지 않았다.

`GameUIScroll`은 탭 상위의 비활성 여부와 목록 내부의 숨긴 행을 구분하여 크기를 계산한다. `GameUIDynamicUI`도 비활성 부모 스크롤을 찾는다. `SetScrollingEnabled()`로 native 상태를 함께 동기화하고, 결과 기록·성장 UI에서 이를 사용한다.

- [수정 전 빈 투수 탭](Integration/NGUIGameplayQuickFinal/quick-records-pitchers.png)
- [최종 내 팀 투수 2명](Integration/NGUIGameplayFinal/my-pitchers-final.png), [상대팀 투수 4명](Integration/NGUIGameplayFinal/opponent-pitchers-final.png)
- raw 투수 기록 수에는 인접 중복 항목이 포함된다. UI의 기존 중복 제거 후 표시 수와 비교했다.

### 3. 동적으로 생성된 기록 행이 스크롤 입력을 전달하지 않음

결과 행 프리팹의 `GameUIPointer.scroll`은 null이다. 행의 hit surface가 입력을 받지만 부모 스크롤로 전달하지 않아 긴 목록의 휠 입력이 작동하지 않았다.

`GameUIPointer`가 입력 시 가장 가까운 부모 `GameUIScroll`을 찾도록 수정했다. 기존 명시적 연결은 우선하고, 진행 중 드래그는 시작한 스크롤로 종료·취소한다. 드래그 도중 부모가 바뀌면 기존 드래그를 취소한다.

회귀 검사에서 실제 GraphicRaycaster로 동적 행을 적중한 뒤 down/begin/drag/up/end와 wheel 이벤트를 전달하여 위치 이동을 확인했다. 명시적 연결 우선 검사도 통과했다. [컨트롤 검사](gameplay-control-checks.txt).

최종 실제 경기에서는 OS 마우스 휠 양방향 이동과 상대팀 타자 11명 중 마지막 교체 선수까지 표시를 확인했다. [스크롤 전](Integration/NGUIGameplayFinal/records-scroll-top.png), [마지막 교체 선수까지 표시](Integration/NGUIGameplayFinal/records-wheel-last-row.png).

**OS 드래그는 통과로 집계하지 않았다.** 자동화의 짧은 drag 호출에서는 위치 이동이 관찰되지 않았다. 드래그 전달 경로는 위 EventSystem 회귀 검사로 확인했으며, 실제 손으로 길게 끌기와 기기 터치는 추가 확인이 필요하다. `records-drag-bottom.png` 및 이전 실행의 `records-opponent-bottom-fixed.png`는 파일명과 달리 드래그 후 이동하지 않은 화면이다.

## 입력과 연출 검증 범위

| 항목 | 방식 | 판정 |
|---|---|---|
| 타격 패드 | 실제 마우스 드래그의 down/up 수신 확인, 이후 전체 경기에서 production swing 콜백 사용 | 입력 수신·경기 진행 확인, 모든 타격 제스처 전수 검사는 아님 |
| 구종 선택·투구 | 실제 마우스로 Slider 선택·투구 패드 드래그 후 수비 전환 관찰 | 통과. [구종 선택](Integration/NGUIGameplayManual9/pitch-selected-native.png), [투구 후](Integration/NGUIGameplayManual9/pitch-release-native.png) |
| 견제 | 실제 1루 견제 버튼 클릭, 수비 카메라 전환 후 선택 화면 복귀 | 통과. [화면](Integration/NGUIGameplayManual9/pickoff-native.png) |
| 수동↔Quick | 기존 모드 전환 API 호출 | 진행·표시 확인. 기존 화면 버튼 정상 판정은 아님 |
| Quick 종료 팝업 취소 | API로 팝업 열기, 실제 마우스로 취소 클릭 | 경기 재개 확인 |
| 스킬 연출 스킵 | 진행 중 Quick의 `setHoldFastForward` 진입·해제 3회 호출 | 빈 슬롯 2개, 연출 숨김, 배속 복원 통과. `skillUISetter.destroyObject` null 예외 재현 안 됨 |
| 결과·기록·팀/투수 탭·닫기·다음 | 실제 OS 마우스 클릭 | 최종 수정 후 통과 |
| 긴 기록 목록 | 실제 OS 휠 양방향 / EventSystem 드래그 회귀 검사 | 각각 통과. OS drag는 위 제한 참조 |

스킬 스킵 결과는 [Quick 실행 로그](Integration/NGUIGameplayQuickFinal/integration-trace.txt)의 `PASS repeated fast-forward enter/exit x3`에 남겼다. 현재 로컬 데이터의 `SKILL_TYPE=0`이므로 자연 발생 스킬의 전체 연출, OS 길게 누르기 입력까지 통과했다는 의미는 아니다.

## 남은 문제

| 문제 | 이번 관찰과 영향 |
|---|---|
| 일시정지·자동·계속하기 입력 연결 | 정지/자동 Collider 비활성, 계속하기 `pressContinue` 콜백 누락, Quick 상단 부모 비활성. 실제 클릭으로 정상 전환되지 않는다. 검증은 API로 우회했다. |
| 로컬 선수 교체 | `changeController2.Init:46`의 `player.getCard().abilities`에서 null 예외. 스냅샷에서 타자·투수 카드가 null이고 UI 필드 참조는 유효했다. 정상 카드 환경의 교체 완료는 미검증이다. |
| tk2d 카메라 | `Editor__GetGameViewSize:371/430`의 경고와 내부 null 예외 로그가 재현된다. 이번 실행에서 경기 종료는 막지 않았다. |
| Spine 애니메이션 | `DEADBALL1/2/3`, `7000_BUNT_NORMAL`, `PITCHER_ANGRY_01`, `PITCHER_INCONVENENCE_01/02/03`, `PITCHER_IRRITATION`, `PITCHER_SMILE_01~05` 관련 오류가 관찰됐다. 해당 연출은 정상 판정하지 않는다. |

버튼 설정과 카드 데이터 문제는 [이전 기준본](../CanvasRestructure/STEP1_BASELINE.md), [이전 경기 검증](../CanvasRestructure/STEP6_INTEGRATION.md)에도 기록되어 있다. 이번 NGUI 제거 이후 처음 생긴 것으로 분류하지 않았다. 원시 오류는 각 실행 폴더의 `gameplay-trace.txt`에 보존했다. 최종 실행에는 tk2d, DEADBALL2, PITCHER_INCONVENENCE_01 오류가 남았다.

PVP, RTTS·서버 기록/보상 저장, 계정 데이터 변경, Android 빌드·실기기 터치, 다른 화면 비율, 모든 스킬·주루·교체 조합, 장시간 성능은 이번 검사 범위 밖이다. 이 결과로 제품 전체 QA 완료를 선언하지 않는다.

## 재실행 및 증거 구분

Unity 편집 모드에서 `Library/UGUIMigration/request.txt`에 명령을 하나씩 기록한다. `result.txt`는 명령 접수/호출 결과이므로 Play Mode 회귀 검사는 **보고서의 PASS와 편집 모드 복귀까지** 확인한다.

```text
integration:check-gameplay-batches
integration:check-gameplay-controls
integration:start:NGUI새실행명
integration:gameplay-assist:on
integration:gameplay-assist:speed8
integration:gameplay-assist:summary
integration:gameplay-assist:off
integration:stop
```

`FullGameplayPlayChecks`는 명시적으로 켰을 때만 로컬 수동 입력을 보조한다. Play Mode 종료 시 해제된다. `integration:auto`, `manual`, `pause`, `resume`, `quick-pause`, `skip-regression`은 생산 API를 이용하는 검사 명령이며 OS 입력과 구분한다. 최종 Play Mode를 종료하고 원래 Logo 편집 씬으로 복귀했다. 컴파일과 C# diff 공백 검사를 통과했다. 커밋·푸시는 하지 않았다.

중간 실행도 보존했다. `NGUIGameplayFull`은 문제 탐색용 부분 경기다. `NGUIGameplayManual9`는 실제 투구·견제 확인 후 보조 입력기의 이전 구종 유지 처리 때문에 재시작했다. `NGUIGameplayManualComplete`는 검사기 컴파일 오타 수정 전 중단한 시도다. 컨트롤 fixture의 새 Graphic은 렌더 프레임 전에는 raycast되지 않아 검사 단계에 프레임 대기를 추가한 뒤 통과했다. 이 검사기 개발 중 실패들을 게임 회귀로 집계하지 않았다. `NGUIGameplayQuickFinal/pause-early-request`는 경기 UI 준비 전 API를 호출한 화면이며 정상 타이밍의 재검증은 `pause-fixed`다.
