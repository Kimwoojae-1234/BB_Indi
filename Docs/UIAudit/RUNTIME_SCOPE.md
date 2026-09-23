# 실제 사용하는 UI 기준 전환 범위

**재개 작업 적용 결과:** 기준 커밋 `52f01bd1` 이후 경기/Quick/컨트롤/로딩/스킬 배경/동적 추적 UI의 원본 프리팹 19개를 일괄 전환했다. 이 19개에는 NGUI 컴포넌트와 NGUI 애니메이션 바인딩이 남아 있지 않다. 위젯 1,955개와 기존 참조 2,219개를 검사했으며, 실제 경기와 자동 진행에서 활성 NGUI가 없는 상태를 관찰했다. 구현·검증·기존 누락·미검증 항목은 [통합 전환 기록](Integrated/PROGRESS.md)에 정리했다. 아래 네 단위 집계는 재개 이전의 역사적 기록이다.

2026-09-23 사용자 지시: 실제 동작하지 않는 UI는 전환하지 않아도 된다. 이전의 모든 자산 전환 계획보다 이 지시를 우선한다. 미사용 확정은 전환 제외이며 자산 삭제를 뜻하지 않는다.

## 현재 진행률 판정

실사용 대상의 분모가 확정되지 않아 전환율은 **미산정**이다. 전체 자산 3,605→3,508개와 메인 경기 프리팹 1,757→1,660개에서 계산한 2.7%·5.5%는 직렬화 컴포넌트 감소율이며 실사용 기준으로 재사용하지 않는다. 기능 단위와 컴포넌트 단위를 섞어서 계산하지 않는다. 기존 UGUI 화면은 이번 NGUI 전환의 완료 실적으로 넣지 않는다.

적용한 단위는 점수판의 표시 부분, 구종 선택, 투구 전 도루·견제 선택, 필드 미니맵·주자 표시·아웃 카운트다. 네 번째 적용 후 메인 경기 프리팹의 NGUI 자산 컴포넌트 수는 1,656개이며, 이 역시 실사용 진행률이 아니다. 각 단위의 실행 검증 범위와 미검증 항목은 별도 PROGRESS 문서에 기록돼 있다. 실제 입력·전체 경기 왕복까지 검증 완료한 단위 수와도 구분한다.

네 번째 단위는 사본 배치 Play Mode·정적 렌더 이후 원본 Unity 화면도 추가 확인했다. 실제 타구→필드 전환, 두 주자의 진루·득점·제거, 타석 복귀, 공수 교대, 수비 아웃 0→3, 2회초 재진입을 확인했다. 투구는 테스트 입력을 포함하며 실제 포인터는 구종 두 버튼만 부분 확인했다. 나머지 입력·실기기·전체 경기 왕복을 완료한 것으로 집계하지 않는다. [미니맵 전환 기록](FieldMinimap/PROGRESS.md), [화면 검증](FieldMinimap/ScreenCheck/README.md)

## 실행 경로를 읽어 확인한 사항

| 영역 | 확인 내용 | 범위 처리 |
|---|---|---|
| 현재 RTTS 진입 | `RttsManager.launchLiveGame`이 `ConfigureDongneYagu`, `Mode.BeginRttsGame` 후 MainLoading→BallPlay로 진입 | 우선 기준 경로 |
| RTTS 결과 | `BallPlayManager.setResult`에서 `CompleteLiveGame` 성공 시 레거시 `LoadGameResult` 전에 반환 | 정상 RTTS 결과에서는 레거시 resultPrefab 제외 가능. 다른 유지 경로 사용 여부는 별도 확인 |
| 선수 교체 | `UIPause.pressChange`가 `UIPlayerChange.InitPlayerChangeUI`를 호출 | 기본 비활성이라는 이유로 제외 금지. 현재 버튼 연결·실행 확인 필요 |
| 자동/Quick | `UIScoreBoard.setGameSpeedControl`, `BallPlayManager.backToSimulation` 등의 전환 경로 존재 | QuickSimulator를 미사용으로 일괄 제외 금지 |
| 선수 정보 | `UIPlayerInfo.SetActive`는 GIRL_PLAY에서 숨기지만 `Active`는 다시 활성화한다. `ControlManager.EraseFieldUI`, `SkillEffectDisplayManager.setUI`에 호출이 있음 | GIRL_PLAY만 근거로 미사용 확정하지 않음 |
| PVP 채팅 | 경기 초기화 시 Pvp/Pvp433 외에는 비활성화 | 오프라인 RTTS 경로의 대상에서는 제외 가능. 현재 PVP 진입 사용 여부는 별도 확인 |
| 기존 UGUI 로비/새 결과 | 이미 UGUI/TMP 구조 | NGUI 전환 작업의 분모·완료 실적에서 제외 |
| 바이너리/레거시 자산 | 기존 정적 감사로 내부 미확인 자산 159개 | 현재 실행 경로에서 참조되는 경우에만 우선 조사. 미확인 전체를 필수 전환량으로 가산하지 않음 |

기존 `ScoreboardPlayChecks`는 수동 경기를 검사하기 위해 BallPlay 로드 후 Manual, `bSimulationQuickPlay=false`, `bOnlyChanceMode=false`를 설정한다. 이 검사에서 표시되지 않았다는 사실은 일반 RTTS 진입 또는 자동 진행에서 미사용이라는 근거가 되지 않는다.

## 다음 집계 기준

현재 플레이 경로의 진입→수동/자동 전환→일시정지/교체→타격/주루/득점/스킬→공수 교대→결과→로비에서 호출되는 UI를 목록화한다. 숨김 상태를 미사용으로 취급하지 않고 실제 사용, 미사용 확인, 확인 보류로 나눈다. 실제 사용 NGUI의 변경 전 컴포넌트 수와 전환한 수를 같은 기준으로 비교해 적용률을 계산한다. 동작 검증 완료율은 별도로 기록한다.
