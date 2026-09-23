# 필드 미니맵·주자 표시·아웃 카운트 UGUI 전환

2026-09-23, Unity 6000.3.9f1 / Android 대상 에디터 / `_Test_Local` 오프라인 데이터. 네 번째 전환 단위다. 현재 실제 사용하는 UI만 전환하고 기존 경기 동작을 우선한다.

## 적용 범위

실제 `IngameUIPrefab`의 `fieldUI/_fieldActive` 안에서 미니맵 배경과 아웃 카운트를 UGUI Canvas·Image로 교체했다. `UIFieldUI.MakeMinimapRunner`가 생성하는 주자도 새 UGUI 프리팹을 사용한다. 팀 아이콘·이름 배경은 Image, 이름은 원본 비트맵 폰트에서 만든 TMP를 사용한다.

- 실제 `Runner`와 연결되는 `miniRunner` 및 `UIFieldUI`의 API를 유지한다. 이동 보간, 초기 베이스 좌표, 아웃 카운트 조건, 개별/전체 삭제 메서드는 원본과 동일하다. [소스 보존 검사](source-preservation.json)
- 여러 주자가 겹칠 때 아이콘 → 이름 배경 → 이름 순서를 유지하도록 표시 계층을 분리한다. 주자 생성·이동·숨김·삭제와 분리한 표시 오브젝트의 수명을 함께 관리한다.
- `Runner.destroyRunner`가 호출하던 NGUI TweenAlpha를 표시 방식에 맞게 전달한다. UGUI 주자는 기존 0.3초·실시간 기준 투명도 효과를 세 표시 요소에 적용하고, 원래 삭제 예약은 유지한다. 주루 규칙과 판정은 변경하지 않았다.
- 필드 UI가 숨겨진 경기 초기화 시점에도 주자를 생성할 수 있다. `NineInningTwoOut`의 이름 숨김 분기를 유지한다.
- 기존 UIPanel은 공존 기간의 카메라·투명도·정렬 전달에 사용한다. 새 표시에는 GraphicRaycaster, Collider, 입력용 EventSystem을 추가하지 않는다.
- 필더 선수 정보, 필드 진루·귀루 조작, 필드 이벤트/득점 연출은 이번 전환 범위 밖이다. 로컬라이징도 아직 진행하지 않았다.

메인 경기 프리팹에서 NGUI UISprite 4개를 제거해 **1,660 → 1,656개**가 됐다. 동적으로 생성되는 주자 프리팹의 NGUI 표시 3개도 UGUI로 대체하지만, 원본 `miniRunner2.prefab`은 다른 레거시 경로의 호환성을 위해 보존했다. 따라서 이 3개를 자산 삭제 수량에 더하지 않는다. 미사용 가능 계층을 포함한 이 수량은 실사용 전환율이 아니다.

기존 오브젝트 중 `_fieldActive`의 자식 목록과 `UIFieldUI` 바인딩만 수정됐고, 나머지는 대상 분기의 교체다. 신규 미해결 로컬 참조는 없다. [프리팹 변경 범위](prefab-change-scope.json), [남은 메인 분기](remaining-main-prefab-branches.json)

## 검증 결과

- 팀 10종 × 이름 3종 × 특수 모드 표시 2종의 **60조합**, 숨겨진 부모 아래 초기화, 축소된 미니맵의 좌표, 표시 계층, 상위 투명도/정렬 전달, 입력 비개입 검사를 통과했다. [정적 검사](presentation-checks.txt)
- 같은 카메라 조건의 1280×720 분리 렌더에서 배경·아이콘·이름·아웃 표시의 배치를 확인했다. NGUI와 UGUI의 텍스처/텍스트 래스터화는 픽셀 완전 일치를 보장하지 않는다. [원본](legacy.png), [UGUI](ugui.png)
- 통제된 Play Mode에서 기존 주자 생성 함수를 사용했다. 4개 베이스의 초기 위치, 4구간 × MOVE/STEAL × 9위치의 **72개 이동 표본**, 이동 대상 외 상태 유지, 아웃 0~3, 필드 숨김/재표시, 투명도, 실제 `Runner.destroyRunner`, 마지막 주자 삭제, 전체 삭제와 재생성을 비교했다.
- 원본 **18개**, UGUI **25개** 검사 항목이 통과했다. UGUI 추가 검사는 분리된 표시의 활성화·투명도·삭제 및 입력 비개입을 확인한다. [원본 동작 검사](Legacy/interaction-checks.txt), [UGUI 동작 검사](UGUI/interaction-checks.txt)
- 원본과 UGUI 모두 기존 tk2d GameView 크기 조회 오류와 사본의 Unity Search 초기화 오류가 있었다. 최종 비교에서 UGUI에만 발생한 신규 오류 서명은 없다. [실행 비교](runtime-comparison.json), [배치 결과](batch-result.txt)
- 컴파일과 실행을 통과한 소스, 프리팹, 새 자산을 원본 프로젝트에 반영했다. 테스트 사본과 소스/프리팹 해시를 대조했고 새 자산 참조도 확인했다. [반영 기록](applied-verification.json), [자산 참조 검사](asset-reference-checks.json)

초기 검사에서 숨겨진 부모를 제외하는 기본 `GetComponentInParent` 호출 때문에 경기 시작 시 주자 생성이 실패했다. 비활성 부모를 포함해 찾도록 수정하고 회귀 검사를 추가한 뒤, 원본/UGUI 전체 검사를 재실행해 통과했다. [수정 전 기록](InactiveInitializationFailure/gameplay-trace.txt)

별도의 초기 정적 검사 실패는 Edit Mode 미리보기에서 일반 MonoBehaviour의 OnDisable/OnDestroy를 자동 실행한다고 가정한 검사 도구 문제였다. 자동 수명 검증은 Play Mode로 옮겼다. [초기 검사 기록](FixtureFailure/presentation-checks.txt)

## 실행 방식과 원본 화면 추가 확인

사용자가 다른 프로젝트 작업에 화면을 사용하므로 원본 Unity 창의 포커스를 가져오지 않았다. `/private/tmp`의 분리 사본을 Unity `-batchmode -force-metal`로 실행했다. 원본의 씬·레이아웃을 조작하거나 사본의 공유 Spine material 변경을 원본에 복사하지 않았다.

배치 모드에서는 기존 `Field.initFielder`의 타격 뷰 메시 로딩 직전 `WaitForEndOfFrame`이 완료되지 않는다. 검사 도구는 필드·타자·투수 데이터 초기화 후 해당 대기 상태에서 경기 진행 코루틴을 멈추고 필드 표시 검사를 시작한다. 게임 코드를 우회 처리로 바꾸지는 않았다. [배치 진입 진단](BatchEntryDiagnostic/gameplay-trace.txt)

위의 초기 결과는 **분리 렌더 + 통제된 배치 Play Mode 검사**다. 이후 사용자가 화면 사용을 허용하여 원본 Unity Game View에서도 추가 확인했다. 사구 출루, 실제 타구→필드 전환, 주자 2명의 진루, 득점 시 주자 제거와 점수 1:0 갱신, 타석 복귀 시 숨김, 공수 교대·주자 재생성, 1회말 내야 타구와 아웃 0→1→2→3, 2회초 재진입을 확인했다. 905×488과 1918×839 화면에서 새 미니맵의 잘림·잔상·잘못된 겹침은 발견하지 못했다. 런타임 구현과 메인 프리팹은 기존 검증본과 동일하다. [원본 화면 검증과 증거](ScreenCheck/README.md)

수비 차례 투구에는 테스트 입력을 사용했으며 실제 포인터 경로는 구종 두 버튼만 부분 확인했다. 일시정지·자동 진행·주루 조작의 실제 입력, 기기·PVP·전체 경기 왕복은 미검증이다. 기존 tk2d/애니메이션 오류 외 같은 호출 경로의 PITCHER_SMILE_01 누락을 추가 관찰해 기록했다. 이 단계에서는 플레이어 빌드를 수행하지 않았다. 원본 Play를 종료했고 시작 전 Logo 씬의 미저장 상태를 유지했다.

Unity 메뉴 `Tools > UI Migration > Run Field Minimap Presentation Checks`로 표시 검사를 다시 실행할 수 있다. `Start Field Minimap Gameplay Check`는 일반 에디터에서 정상 타격 뷰 진입을 기다린 뒤 일회성 경기 상태로 검사한다. `Start Field Screen Check`는 열린 Logo 씬에서 정상 경기 진행을 관찰하며, `With Test Inputs` 변형은 구종 이벤트와 투구 release 전달을 로그에 표시한다. `FieldMinimapBatchChecks.Run`은 명시적으로 표시한 분리 사본에서만 실행되며, 원본→전환본 순서의 검사를 위해 이 단계 적용 전 프리팹이 필요하다. 모든 검사 코드는 `Assets/Editor`에 있어 플레이어 빌드에 포함되지 않는다.
