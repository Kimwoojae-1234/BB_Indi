**BB_Indi UI 점검 — NGUI → UGUI 전환 및 로컬라이징**

점검일: 2026-09-23. Unity 6000.3.9f1, 현재 에디터 대상 Android 기준.

**최신 적용:** 남은 경기 UI 원본 프리팹 19개를 일괄 UGUI로 전환했다. 위젯 1,955개 비교, 트윈 표본 3,108개, 기존 참조 2,219개 검사를 통과했다. 실제 경기/일시정지/자동 진행 검사 범위와 남은 제한은 [통합 전환 기록](Integrated/PROGRESS.md)을 참조한다. 아래의 기능별 전환 순서와 수량은 재개 이전 기록이다.

**사용자 확정 우선순위: 1순위는 NGUI → UGUI 전환과 기존 기능의 정상 동작 검증이다. 로컬라이징은 전환 안정화 이후에 진행한다.**

**최신 범위 지시:** 현재 실제 사용하는 UI만 전환하고, 미사용이 확인된 UI는 제외한다. 아래 초기 전수 자산 목록과 전체 제거 계획보다 [최신 전환 기준](MIGRATION_VALIDATION.md)과 [실사용 범위 기록](RUNTIME_SCOPE.md)을 우선한다. 기존 2.7%·5.5%는 자산 컴포넌트 감소율이며, 실사용 기준 진행률은 아직 미산정이다.

로비·선수·리그 화면은 이미 UGUI/TMP 기반이다. 최신 지시는 실제 사용하는 경기 UI를 일괄 전환한 뒤 통합 실행 검증하는 방식이다. 번역 데이터 정리, 언어 선택 기능, 다국어 폰트 체계 구축은 선행 조건으로 두지 않는다. 이전에 제안한 “다국어 기반 우선” 순서는 이 우선순위로 대체한다. 경기 조작의 tk2d 연결은 보존하며 전체 재작성은 별도 범위로 남긴다.

전환의 첫 단위와 합격 기준은 [전환·회귀 검증 기준](/Users/munhwaie/Projects/BB_Indi/Docs/UIAudit/MIGRATION_VALIDATION.md)에 기록했다.

아래 수량과 분석은 **전환 전 코드·직렬화 자산의 구조 점검 기준선**이다. 이후 첫 단위인 경기 점수판 표시의 전환을 시작했다. 구현 내용과 실제 검증 범위는 [진행 기록](/Users/munhwaie/Projects/BB_Indi/Docs/UIAudit/Scoreboard/PROGRESS.md)에 별도로 기록한다. 전체 화면 검증 또는 전체 NGUI 제거 완료를 의미하지 않는다.

**확인된 범위와 수량**

| 항목 | 확인 결과 | 해석 |
|---|---:|---|
| 빌드에 활성화된 씬 | 9개 | 이름에 Tester가 있어도 빌드 목록에 포함되면 점검 대상 |
| YAML에서 UI 계열 컴포넌트를 식별한 씬·프리팹 | 291개 | 일부 tk2d 리소스 포함. 화면 개수와 다름 |
| NGUI 컴포넌트가 직접 포함된 YAML 자산 | 106개 / 컴포넌트 3,605개 | 비활성 오브젝트·구형 자산 포함, 런타임 동시 사용량 아님 |
| 위 NGUI 자산 중 빌드 씬의 직렬화 참조를 따라 도달한 자산 | 30개 | Resources 문자열 로딩·바이너리 내부 참조는 이 경로로 계산하지 못함 |
| 프로젝트 C#에서 NGUI 타입을 참조하는 파일 | 88개 | MainGame 81개, Scripts 7개. 주석·문자열을 제외한 정적 후보, 조건부 컴파일 분기 포함 |
| NGUI 스크립트에 바인딩된 YAML 애니메이션 클립 | 19개 | 컴포넌트 교체 시 애니메이션 연결도 전환 필요 |
| 바이너리 형식으로 이번 분석에서 내부를 읽지 못한 프리팹 | 159개 | 미사용으로 판정하지 않았음. 별도 목록 제공 |
| 식별된 텍스트 컴포넌트 | 2,060개 | UILabel 1,002, TextMeshProUGUI 1,035, UGUI Text 23 |
| 코드의 문구 검토 후보 | 257곳 | 숫자 서식·약어·테스트 문구도 포함. 모두 번역 대상이라는 의미는 아님 |

SDK/에셋스토어의 예제 자산은 위 자산 통계에서 제외했다. 직접 참조와 프리팹 내부 인스턴스의 재사용 횟수는 다르다. 특히 159개 바이너리 프리팹 때문에 위 수량은 전체 전환량의 확정 견적이 아니다.

**화면별 전환 지도**

| 영역 | 현재 구조 | 필요한 작업 |
|---|---|---|
| Logo / Nickname / MainLobby | UGUI Canvas, 기준 해상도 2560×1440 | 언어 초기화·팝업·텍스트 바인딩·Safe Area 통일 |
| TutorialScene | UGUI, Constant Pixel Size 설정, 임시 튜토리얼 코드 | 실제 튜토리얼 범위 확정 및 다른 화면과 스케일 정책 정렬 |
| Resources/UI/Window | 로비·선수 목록/상세·인벤토리·상점·패스·랭킹·RTTS·결과 등 UGUI/TMP | NGUI 재작성 없이 문자열과 폰트·레이아웃 개선 가능 |
| Resources/UI/Popup / FrontUI | UGUI/TMP 중심, 일부 UGUI Text | 언어 선택 기능·공통 텍스트 바인딩·팝업 복귀·로딩/오류 문구 정리 |
| IngameUIPrefab | NGUI 컴포넌트 1,757개. UILabel 395, UISprite 1,038, UIPanel 33 포함 | 스코어보드·선수 정보·구종·주루·일시정지·교체·연출을 하위 기능별로 전환 |
| ControlUIPrefab | tk2d 컴포넌트 25개 + NGUI Tween 6개 | 타격/투구 드래그·누름/뗌·좌표 변환을 별도 검증하면서 이전 |
| QuickSimulatorPrefab | NGUI 557개, UILabel 175 | 시뮬레이션 표시·선수 이름·기록·상태 갱신 전환 |
| 레거시 resultPrefab | NGUI 537개, UILabel 205 | 현재 RTTS 결과 경로와 구분하여 사용 모드 확인 후 이전/폐기 결정 |
| PVP Login / Disconnect / Scene1 | 빌드에 포함된 NGUI 씬 | 테스트 전용 여부를 확인해 전환 또는 빌드 제외 여부 결정 |
| Resources/Prefab / ResourcesBundle | 구형 UI·셀·폰트·팝업, 다수 바이너리 | Unity 내부에서 참조/컴포넌트 읽기 후 사용 여부 분류 |

새 UI의 Resources/UI 아래에서는 NGUI 컴포넌트가 직접 포함된 프리팹을 찾지 못했다. 이 영역의 UI 관련 프리팹 81개에는 TMP 계열 컴포넌트가 1,036개 있으며, TMP 텍스트 외 컴포넌트도 포함한 수치다.

현재 RTTS는 [RttsManager](/Users/munhwaie/Projects/BB_Indi/Assets/Scripts_New/Manager/RttsManager.cs:364)에서 MainLoading → BallPlay로 진입한다. Android의 `_Test_Local` 설정에서는 [경기 종료 처리](/Users/munhwaie/Projects/BB_Indi/Assets/MainGame/Script/Manager/BallPlayManager.cs:2271)가 RTTS 결과를 먼저 회수한다. 따라서 레거시 결과 프리팹 전체를 현재 RTTS의 필수 화면으로 계산하면 전환 범위를 과대평가한다. 다른 빌드 대상의 전처리 심볼은 다르므로 별도 확인이 필요하다.

**발견 사항 — 아래 로컬라이징 항목은 전환 안정화 이후 처리**

아래 P1/P2는 해당 기능을 구현할 때의 중요도다. NGUI 이전보다 로컬라이징을 먼저 진행하라는 작업 순위가 아니다. 기존 UI 문제도 전환으로 발생한 회귀와 구분하여 기록한다.

1. **[P1] 언어 선택 화면이 실제 언어 변경을 수행하지 않는다.**

   [Popup_Setting_Language.cs](/Users/munhwaie/Projects/BB_Indi/Assets/Scripts_New/UIPopup/Popup_Setting_Language.cs:5)는 빈 클래스다. 프리팹에서 확인되는 버튼 연결도 닫기 두 곳뿐이다. [GameConfig.ChangeLanguage](/Users/munhwaie/Projects/BB_Indi/Assets/Scripts_Old/Common/GameConfig.cs:276)는 저장값이 없으면 영어를 저장하며 기기 언어 감지는 주석 처리되어 있다. 언어 선택 → 저장 → 현재/캐시된 UI 재바인딩의 공통 흐름이 필요하다.

2. **[P1] 주요 TMP 폰트가 한국어·일본어를 지원하도록 구성되어 있지 않다.**

   실제 UI가 참조하는 LilitaOne 계열 폰트 자산 13개를 확인했다. 12개는 96개 문자, Extended ASCII 버전은 207개 문자가 있으며 한글·가나 글리프가 없다. 모두 로컬 fallback 목록이 비어 있다. [TMP 전역 설정](</Users/munhwaie/Projects/BB_Indi/Assets/TextMesh Pro/Resources/TMP Settings.asset:36>)의 fallback 목록도 비어 있다. [SetFont2](/Users/munhwaie/Projects/BB_Indi/Assets/Scripts_New/Manager/LocalizationManager.cs:86)는 미구현이며 TMP 문자열 조회 시 폰트도 적용하지 않는다. 기존 Font 로더가 요청하는 BlackHanSans-Regular, NotoSansJP-Bold, OPPOSans-B-2는 프로젝트에서 찾지 못했다. 번역문만 바꾸면 누락 글리프가 발생할 위험이 크다. 대상 언어별 TMP 폰트/대체 폰트와 outline material까지 먼저 검증해야 한다.

3. **[P1] 빈 번역에 대한 대체 언어 처리가 없다.**

   [GetLocalizedValue2](/Users/munhwaie/Projects/BB_Indi/Assets/Scripts_New/Manager/LocalizationManager.cs:97)는 키를 찾으면 해당 언어 값을 그대로 반환한다. 값이 빈 문자열이어도 영어/한국어로 대체하지 않는다. 키 자체가 없으면 `String Empty`를 노출한다. 테이블 전체 선형 검색도 매번 수행한다. 키 인덱스, 빈 값 처리, 누락 키 진단, 기본 언어 규칙을 한 경로로 정리해야 한다.

4. **[P1] 번역 치환 인자가 언어별로 다른 항목이 35개다.**

   모두 이번 검사에서는 한국어와 영어 간 차이였다. 예를 들어 `SkillDesc.10003`은 한국어가 `{0}, {1}`, 영어가 `{0}, {2}`를 사용한다. [Popup_SkillSetting](/Users/munhwaie/Projects/BB_Indi/Assets/Scripts_New/UIPopup/Popup_SkillSetting.cs:326)은 값과 백분율을 교대로 전달하므로 인자 차이는 단순 표현 차이가 아니라 잘못된 능력치 표시로 이어질 수 있다. 35개 모두를 오류로 확정한 것은 아니며, 원문·실제 인자 의미를 대조하여 판정해야 한다.

5. **[P2] UGUI 화면에도 하드코딩 문구와 키 직접 출력이 남아 있다.**

   [성장 팝업](/Users/munhwaie/Projects/BB_Indi/Assets/Scripts_New/UIPopup/Popup_StatUpgrade.cs:177)의 `UPGRADE TO POWER LEVEL`, [승급 안내](/Users/munhwaie/Projects/BB_Indi/Assets/Scripts_New/UIPopup/Popup_Promotion.cs:66), [시즌 순위 설명](/Users/munhwaie/Projects/BB_Indi/Assets/Scripts_New/UIComponent/RttsResult/ResultLeagueStading.cs:22), [일정](/Users/munhwaie/Projects/BB_Indi/Assets/Scripts_New/UIComponent/Rtts/ScheduleComponent.cs:53), 로비 프리팹의 SHOP / BALLERS / LEAGUE 등이 대상이다. [TR_RewardComp](/Users/munhwaie/Projects/BB_Indi/Assets/Scripts_New/UIComponent/TrophyRoad/TR_RewardComp.cs:239)와 [BallerAchieveComponent](/Users/munhwaie/Projects/BB_Indi/Assets/Scripts_New/UIComponent/Baller/BallerAchieveComponent.cs:102)는 name_id/desc_id를 직접 표시한다. 코드를 통한 동적 텍스트와 프리팹의 고정 텍스트를 함께 외부화해야 한다.

6. **[P2] 중복 키가 있고 일부는 내용이 다르다.**

   1,134행 중 고유 키는 1,127개다. 중복 키 7개 중 `PopUp_BattleEnd_Result_Invalid`는 INVALID / INVALID GAME, `Ingame.NicePlay`는 NICE PLAY / NICE PLAY!로 다르다. 현 로더는 먼저 발견한 값을 반환한다. Dictionary로 변경하기 전에 기준 행을 정해야 한다.

7. **[P2] 화면 여백 및 팝업 복귀 규칙을 공통화할 필요가 있다.**

   [UIWindow.SafeArea](/Users/munhwaie/Projects/BB_Indi/Assets/Scripts_Old/UI/UIWindow.cs:106)는 좌우 여백을 화면 폭 4.5%로 제한하고 상하 안전 영역은 0/1로 고정한다. 초기화 시 한 번만 적용하며 UIPopup에는 같은 처리가 없다. 한편 [PopupManager](/Users/munhwaie/Projects/BB_Indi/Assets/Scripts_New/Manager/PopupManager.cs:107)는 최초 생성 순서를 저장하고 [BackToLastPopup](/Users/munhwaie/Projects/BB_Indi/Assets/Scripts_New/Manager/PopupManager.cs:143)은 그 목록의 끝에서 두 번째를 복원한다. 캐시된 팝업을 재사용한 뒤 돌아가면 실제 직전 팝업과 달라질 수 있다. 현재 설정 화면 → 언어 화면 흐름을 포함해 재진입 시나리오를 검증해야 한다.

**번역 데이터 준비 상태**

| 언어 | 값이 있는 행 / 전체 1,134행 | 비고 |
|---|---:|---|
| 한국어 | 1,081 | 영어와 함께 초기 적용을 검증할 기반이 있음 |
| 영어 | 1,081 | 값의 존재가 번역 품질을 보장하지 않음 |
| 스페인어 | 736 | 신규 콘텐츠 보강 필요 |
| 일본어 | 678 | 신규 콘텐츠와 폰트 보강 필요 |
| 중국어 간체 | 678 | 신규 콘텐츠와 폰트 보강 필요 |
| 중국어 번체 | 678 | 신규 콘텐츠와 폰트 보강 필요 |
| 프랑스어 | 2 | 사실상 미작성 |
| 독일어·인도네시아어·이탈리아어·포르투갈어·러시아어·태국어·튀르키예어·베트남어 | 각 0 | 열/enum만 존재 |

모든 언어가 빈 행은 53개다. 예약/미사용 키일 수 있으므로 무조건 번역 대상으로 확정하면 안 된다. 위 수치는 중복을 포함한 행 단위 작성률이며, 실제 노출 문구의 번역률과 다르다. 코드에서 동적으로 조합되는 키와 서버에서 받는 차트의 name_id/desc_id는 이번 문자 리터럴 검사만으로 전체 유효성을 보증하지 않는다.

[LocalizationItem.json](/Users/munhwaie/Projects/BB_Indi/Assets/Resources/Localization/LocalizationItem.json)이 현재 새 LocalizationManager의 데이터다. 별도로 구형 [Localization.csv](/Users/munhwaie/Projects/BB_Indi/Assets/Resources/Localization.csv)는 Korea 열과 데이터 232행을 가진다. 이 CSV가 현재 어느 경로에서 실제 로드되는지는 확인되지 않았다. 두 자산의 존재만으로 두 경로가 모두 실행 중이라고 판정하지 않았다. 1차 지원 언어는 아직 확정하지 않았다.

**NGUI 전환 시 함께 옮겨야 하는 것**

| 기존 의존성 | 전환 방향 | 보존할 동작 |
|---|---|---|
| UILabel / UIFont | TextMeshProUGUI / TMP_FontAsset | 정렬·줄바꿈·숫자 폭·언어 폰트·외곽선 |
| UISprite / UIAtlas | Image / SpriteAtlas 및 sprite 매핑 | sliced border·fill·원본 크기·pivot·동적 spriteName 변경 |
| UITexture | RawImage 또는 Image | 동적으로 공급되는 텍스처·UV·크기 |
| UIPanel / UIWidget | RectTransform / CanvasGroup / 필요한 마스크 | alpha·입력 차단·클리핑·depth 순서 |
| UIRoot / UICamera | Canvas / CanvasScaler / EventSystem | 화면 기준 좌표·카메라 순서·터치 전달 |
| NGUI 이벤트 및 EventDelegate | UGUI 이벤트 / UnityEvent 또는 명시적 콜백 | 단순 클릭뿐 아니라 누르기·떼기·드래그·콜백 호출 시점 |
| NGUI Tween / .anim의 mColor 바인딩 | DOTween 또는 새 AnimationClip 바인딩 | 완료 콜백·연출 시간·비활성화 시점 |
| UIGrid / UIScrollView | LayoutGroup / ScrollRect | 셀 정렬·재사용·스크롤 위치 |
| tk2dUIItem / tk2dUIDragItem | 전체 UGUI 통일 범위라면 별도 입력 전환 | 타격 커서·투구 게이지·터치 좌표·누름/뗌 타이밍 |

이는 자동 치환 규칙이 아니라 설계 방향이다. 특히 [ControlBattingUI](/Users/munhwaie/Projects/BB_Indi/Assets/MainGame/Script/Control/ControlBattingUI.cs:22)는 tk2dUIDragItem과 tk2dUIItem을 직접 사용한다. 또한 [인게임 스코어보드](/Users/munhwaie/Projects/BB_Indi/Assets/MainGame/Script/UI/UIScoreBoard.cs:7)와 [새 UGUI 스코어보드](/Users/munhwaie/Projects/BB_Indi/Assets/Scripts_New/UIBaseball/UIScoreBoard.cs:6)는 이름이 같아도 namespace·책임이 다르다. 기존 것을 단순 삭제하거나 이름만 대체하면 안 된다. tk2d의 경기장·캐릭터 렌더링 자산은 UI 이전 여부와 별도로 판단한다.

**확정 우선순위에 따른 작업 순서**

1. **실제 사용 경로와 기존 동작을 기준으로 기록.** 현재 Android RTTS의 로비 → MainLoading → BallPlay → 결과 → 로비를 우선 확인한다. 전환할 하위 기능의 상태·입력·애니메이션을 기록하고 관련 바이너리/중첩 프리팹부터 조사한다. 전체 구형 자산 분류가 끝날 때까지 작은 전환 단위의 작업을 불필요하게 기다리지는 않는다.
2. **스코어보드 표시 부분을 첫 UGUI 전환 단위로 구현.** 팀명·로고·점수·이닝/초말·볼/스트라이크/아웃·주자 표시를 우선 대상으로 한다. 기존 UIScoreBoard의 외부 호출 계약과 경기 상태 갱신 흐름을 보존한다. 같은 클래스의 일시정지·자동 진행·PVP 타이머/송신 로직은 첫 표시 전환과 분리한다. 필요한 폰트·sprite·레이아웃은 현재 화면의 문구와 외관을 재현하는 데 맞춘다.
3. **첫 단위를 실제 경기에서 검증.** 컴파일, 프리팹 참조 검사, 상태별 표시 검사, Play Mode의 실제 갱신·연출·입력 간섭 확인을 통과해야 다음 기능으로 넓힌다. 정적 검사만 끝난 경우에는 런타임 미검증 상태로 표시한다.
4. **나머지 경기 UI를 전환하고 같은 검증 반복.** 선수 정보 → 구종/주루 → 일시정지/선수 교체 → 이벤트/연출 순서로 확장한다. Canvas 정렬·입력 전달·Tween/AnimationClip·활성화/비활성화 시점을 함께 옮긴다. tk2d와 연결된 NGUI Tween을 제거할 때도 기존 터치와 게임 타이밍을 유지한다.
5. **Quick/PVP/구형 결과 및 공통 자산을 처리하고 전체 회귀 검증.** 실제 사용하는 기능은 전환한다. 테스트 씬/레거시 모드는 사용 여부를 확인하고 명시적인 범위 결정 없이 삭제하지 않는다. 유지할 씬/Resources/번들의 NGUI 의존 제거, 빌드, 경기 전체 흐름, 화면 비율과 기기 터치를 검증한다.
6. **NGUI 전환의 정상 동작을 확인한 뒤 로컬라이징.** 그때 공통 언어 서비스·키 바인딩·다국어 폰트·번역 데이터·언어별 레이아웃 검수를 진행한다.

전환 완료 기준은 유지하기로 한 씬/Resources/번들에 NGUI 컴포넌트·스크립트·애니메이션 바인딩이 없고, 기존 화면·입력·연출·경기 진행·결과 복귀가 실제 실행에서 정상 동작하는 것이다. 컴파일 성공이나 컴포넌트 개수 감소만으로 완료 판정하지 않는다. tk2d UI 전체 이전은 NGUI 이전과 구분한다. 로컬라이징 완료 기준은 후속 단계에서 적용한다.

**추가로 남은 검증**

- 바이너리 프리팹 내부와 문자열로 조합되는 Resources 로딩 경로.
- 일부 구형 팝업/보상 카드 및 PVP Login에서 현재 GUID 목록으로 해석되지 않은 스크립트 참조. 실제 missing script인지 Unity에서 확인할 필요가 있으며, 목록만으로 단정하지 않았다.
- 14개 TMP 텍스트 참조에서 GUID `c37f533bd6a442c408ff1c34af680d5d`에 해당하는 로컬 폰트 자산을 찾지 못함. 주로 UserInfo/ChestReward 등에 존재하며 Inspector에서 확인 필요.
- 그림에 포함된 글자, Spine/tk2d 연출의 문자 이미지, 현지화된 리소스 교체 여부.
- 버튼 눌림 영역·대비·작은 글자·레이아웃 잘림·애니메이션 겹침·성능/드로우콜. 실행/기기 검증을 하지 않았으므로 통과 판정을 내리지 않았다.

**근거 파일과 재실행**

| 파일 | 내용 |
|---|---|
| [summary.json](/Users/munhwaie/Projects/BB_Indi/Docs/UIAudit/summary.json) | 집계와 빌드 씬/폴더별 수치 |
| [ui-assets.json](/Users/munhwaie/Projects/BB_Indi/Docs/UIAudit/ui-assets.json) | 자산별 컴포넌트·스케일 설정·직렬화 참조 도달 여부 |
| [ngui-scripts.json](/Users/munhwaie/Projects/BB_Indi/Docs/UIAudit/ngui-scripts.json) | NGUI 타입 참조 C# 88개와 연결 자산 |
| [ngui-animation-clips.json](/Users/munhwaie/Projects/BB_Indi/Docs/UIAudit/ngui-animation-clips.json) | NGUI 바인딩 클립 19개 |
| [binary-assets-uninspected.json](/Users/munhwaie/Projects/BB_Indi/Docs/UIAudit/binary-assets-uninspected.json) | 내부 미검증 바이너리 프리팹 159개 |
| [serialized-texts.json](/Users/munhwaie/Projects/BB_Indi/Docs/UIAudit/serialized-texts.json) | 직렬화 텍스트/오브젝트/폰트 및 컴포넌트 시작 줄 |
| [text-code-candidates.json](/Users/munhwaie/Projects/BB_Indi/Docs/UIAudit/text-code-candidates.json) | 코드 문구 검토 후보 257곳 |
| [localization-data.json](/Users/munhwaie/Projects/BB_Indi/Docs/UIAudit/localization-data.json) | 언어별 작성 수·중복·빈 행·치환 인자 차이 |
| [audit.py](/Users/munhwaie/Projects/BB_Indi/Docs/UIAudit/audit.py) | Python 표준 라이브러리만 사용하는 정적 집계 스크립트 |

프로젝트 루트에서 `python3 Docs/UIAudit/audit.py`를 실행하면 JSON 목록을 갱신한다. Assets/ProjectSettings/Packages를 수정하지 않으며 Unity나 서버를 실행하지 않는다. 보고서의 수동 판단은 자동으로 갱신되지 않는다. 이 도구는 C# 컴파일러나 Unity AssetDatabase를 대신하지 않으며, 간접 의존/런타임 접근성/시각적 정상 동작을 보증하지 않는다.
