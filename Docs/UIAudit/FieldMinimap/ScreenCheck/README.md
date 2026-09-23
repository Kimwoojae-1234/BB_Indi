# 원본 Unity Game View 추가 확인

2026-09-23 / Unity 6000.3.9f1 / Android 대상 에디터 / 오프라인 수동 경기. 이전 사본 배치 검사에 이어 원본 프로젝트 화면을 확인했다. 표시 확인 범위에서는 UGUI 미니맵 회귀를 발견하지 못했다. 전체 경기·모드·입력 검증 완료를 뜻하지 않는다.

## 실행과 보존

`FieldMinimapScreenChecks`는 현재 열린 Logo 씬에서 Play를 시작하고 로비 초기화 후 기존 `ConfigureDongneYagu` 경로로 MainLoading→BallPlay에 진입한다. 타격 결과·주자·아웃·점수를 강제로 설정하지 않는다. 시작 전 Logo 씬의 미저장 상태를 저장하거나 버리지 않았고, 종료 후 `Logo*`와 기존 분할 레이아웃을 화면에서 확인했다.

추가한 코드는 Editor 관찰 도구뿐이다. 이전에 검증한 런타임 소스·메인 프리팹 해시는 모두 동일하며 공유 Spine material 세 개와 EditorUserSettings도 변경되지 않았다. [검사 요약](verification.json)

## 확인한 경기 흐름

| 경로 | 결과와 증거 |
|---|---|
| 사구 출루, 홈→1루 | 1918×839 Game View에서 배경·팀 아이콘·이름·0아웃 표시 확인. [출발](FirstRun/frame-01.png), [1루 도착](FirstRun/frame-05.png) |
| 1회초 실제 타구→필드 | Assisted 실행 126.63초, `contact=True`. 주자 2명과 2아웃 표시가 나옴. [타구 직후](Assisted/frame-06.png) |
| 주자 2명의 진루 | 각 아이콘과 이름이 함께 이동하며 필드 카메라와 별도로 미니맵 위치 유지. [진루](Assisted/frame-18.png) |
| 득점과 주자 제거 | 137.34초 점수 1:0, markers 2→1. 득점 연출과 미니맵의 겹침 순서 확인. [득점 연출](Assisted/frame-24.png) |
| 다음 타석 복귀 | 139.78초 `field=False`, 표시 숨김. 원본 Unity 화면에서 점수판 1:0 및 3루 점유 표시 확인 |
| 공수 교대 | 164.61초 markers=0, 1회말 새 타자 생성 시 markers=1. [교대 중 미니맵 숨김](Assisted/frame-28.png) |
| 1회말 내야 타구·아웃 | 타구 뒤 필드가 다시 나타남. 179.90 / 206.57 / 221.98초 각각 아웃 1 / 2 / 3. 표시등은 1개 / 2개 / 2개로 기존 조건과 일치. 아웃 주자 제거 후 markers=0. [상대팀 주자](Assisted/frame-30.png) |
| 2회초 재진입 | 224.91초 교대 및 초기화 이후 2회초 타격 화면 확인. 점수 1:0 유지, 필드 표시 숨김 |

게임 시각·상태는 [Assisted trace](Assisted/trace.txt)에 기록했다. 스크린샷은 실제 Game View의 ScreenCapture이며 분리 렌더가 아니다. 905×488 분할 Game View와 1918×839 최대화 Game View를 사용했다. 3아웃의 필드 표시등이 2개인 것은 원본 `UIFieldUI`의 두 슬롯 처리다.

## 입력 확인 범위

자동화 도구의 Game View 클릭은 Unity에 down/up이 한 프레임에 들어오고 마우스 좌표가 뷰 밖에 남는 경우가 있었다. 구종 선택뿐 아니라 기존 일시정지 버튼에서도 반응을 확보하지 못했다. 이를 UGUI 결함으로 단정하지 않는다. [진단 요약](input-observation.json), [원시 입력](input.txt)

Assisted 실행은 1회말 첫 구종 선택에 Unity pointer down/up 이벤트를 보내고, 이후 세 투구에 기존 `ControlPitchingUI.setRelease()`를 호출했다. 이 전달은 `TEST_INPUT`으로 기록했다. 1회초 타구·득점 이전에는 이 테스트 입력을 사용하지 않았다. 이후 198.12/198.20초 `button5`, 213.95/214.03초 `button4`에 실제 Input의 down/up과 UGUI raycast hit가 기록됐고, 각각 구종 창에서 투구 단계로 진행했다. 해당 두 선택 사이에는 합성 구종 선택 기록이 없다. 따라서 원본 Game View의 구종 두 버튼은 실제 포인터 경로도 부분 확인했다. [입력 기록](Assisted/input.txt)

일시정지·자동 진행 버튼의 정상 클릭, 구종 모든 버튼, 도루·견제·진루·귀루의 실제 조작, 기기 터치·PVP·전체 경기 결과/로비 왕복은 미검증이다. 수비 차례 투구는 테스트 release API를 포함하므로 전체 수동 조작 성공으로 집계하지 않는다.

## 오류 기록

초기 두 실행의 오류 서명은 이전 NGUI 경기 기준선과 동일했다: tk2d GameView 조회, DEADBALL2 누락, PITCHER_INCONVENENCE_01, PITCHER_SMILE_02/05. Assisted 실행에서는 `PITCHER_SMILE_01` 누락도 관찰했다. 이 정확한 서명은 이전 기준선 로그에 없으므로 새 관찰로 남긴다. 호출 스택은 변경하지 않은 `Pitcher.pitcherCeremony`→`pitcherAnim`이며, 기존 01~05 무작위 애니메이션 선택 계열이다. UGUI 회귀로 볼 근거는 찾지 못했으나 오류가 없는 경기로 보고하지 않는다. 애니메이션 자산 원인과 수정은 별도 확인 대상이다.

이번에 미니맵 표시·수명 처리에서 새 예외는 관찰하지 못했다. 플레이어 빌드·실기기 검증은 수행하지 않았다.
