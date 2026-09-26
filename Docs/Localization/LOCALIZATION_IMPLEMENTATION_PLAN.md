# BB_Indi 로컬라이징 적용 작업 지시서

작성 기준: 2026-09-26 / `ngui-to-ugui-migration` / `56331867` / Unity `6000.3.9f1`

대상 프로젝트: `D:\PROJECTS\Git\BB_Indi`

참조 프로젝트: `D:\PROJECTS\Git\BeachBaseballWorkspace\client`

이 문서는 최초 조사 기록이다. **최신 적용 원칙은 고정 UI만 자동 로컬라이징하며, 코드가 제어하거나 외부 데이터가 채우는 텍스트에는 로컬라이즈 컴포넌트를 붙이지 않는 것이다.** 코드의 고정 안내문은 필요한 시점에 번역을 조회해 한 번 대입한다. 해시형 키는 의미 있는 키로 교체했고 닉네임 등 샘플 값의 번역 항목은 삭제했다. 아래의 초기 일괄 연결 제안보다 `Table/README.md`의 규칙과 `IMPLEMENTATION_RESULT.md`의 실제 검증 결과를 우선한다. 다국어 출시·폰트 확장은 영어 적용 이후의 과제다.

## 1. 적용 방향

**`Table/Localization_master_v2.xlsx`를 번역 원본으로 만들고, 검증된 JSON을 현재 프로젝트의 기존 Resources 경로로 익스포트한다. `KOBManager.Localization`을 공통 조회·언어 변경 창구로 개선하고 로비, RTTS, 경기 UI가 이 창구를 사용하도록 한다.**

```text
Table/Localization_master_v2.xlsx
  Public 시트: key + 언어별 문구 + 관리 정보
             ↓ 검증 및 익스포트
Assets/Resources/Localization/LocalizationItem.json
             ↓ 한 번 로드, 키 인덱스 생성
KOBManager.Localization
             ↓ 문자열 조회 / 포맷 / 언어 변경 이벤트
고정 UI 바인더 · 동적 UI 컨트롤러 · GameUIElement
```

참조 프로젝트에서 가져올 것은 키 기반 데이터 구조, 엑셀에서 JSON으로 내보내는 운영 방식, 언어 변경 이벤트, Inspector 키 선택 편의 기능이다. BB_Indi의 현재 매니저·차트·UI 구조에 맞추어 적용한다.

후속 요청에 따른 현재 기준 언어는 영어다. 한글이 섞인 UI·코드도 영어 문구와 키로 옮긴다. 기존 다른 언어의 번역은 보존하되, 각 언어의 번역과 폰트 검증이 끝났을 때 선택 목록에 노출한다.

## 2. 현재 상태와 반드시 해결할 사항

### 2.1 참조 프로젝트에서 확인한 구조

| 항목 | 확인 내용 | BB_Indi 적용 지시 |
|---|---|---|
| 마스터 엑셀 | `Table/Localization_master_v2.xlsx`, 단일 `Public` 시트, A1:F1 = `key, Eng, Kor, Jpn, ChnT, Esp` | 시트명과 앞 6개 열의 구조를 따른다 |
| 분류별 원본 | `Localization_common_v2.xlsx`, `Localization_ui_v2.xlsx`, `Localization_character_v2.xlsx`, `Localization_event_v2.xlsx` | 처음에는 BB_Indi 마스터 한 파일에서 Category로 구분한다. 편집 원본을 중복 관리하지 않는다 |
| 편집용 시트 | 분류별 파일에 `진행상황, key, Eng, Kor, Jpn, ChnT, Esp, Comment`가 있음 | 검수 상태와 문맥을 마스터 관리 열에 기록한다 |
| 마스터 집계 | `IMPORTRANGE` 등을 포함한 외부 Google Sheets 수식이 `__xludf.DUMMYFUNCTION` 형태로 저장됨 | 로컬 엑셀 원본은 값으로 관리한다. 외부 시트 수식이나 저장된 계산값에 의존하지 않는다 |
| 마스터 저장값 | 데이터 1,961행, 고유 키 1,959개. `ERR_GAME_OVR_UNAVAILABLE`, `CMN_TOUCH_SCREEN` 중복 | 참조도 정제된 정답으로 간주하지 않고 필요한 문구만 검토한다 |
| 런타임 JSON | `Assets/Resources/Json/Sheets/LocalizationItem.json`, 루트 `LocalizationItem` 배열 | BB_Indi에서는 기존 `Resources/Localization/LocalizationItem` 경로를 유지한다 |
| 언어 갱신 | `LocalizationManager.OnLanguageChanged`, `TextMeshInitializer` 구독 | 동일한 책임을 BB_Indi 매니저와 UI 바인더에 구현한다 |
| 편집 지원 | `LocalizedString`, `LocalizeAttribute`, 키 드로어·사용처 검색 | BB_Indi 데이터 경로와 매니저를 참조하도록 필요한 기능만 적용한다 |

**참조 익스포터를 그대로 복사해서 실행하면 안 된다.** `Table/export_localization.py`는 기본 입력을 `Localization_*.xlsm`, 시트를 `Localization`, 언어를 `Eng/Kor/Jpn/ChnT`로 가정한다. 출력도 `Resources/Localization/LocalizationItem.json`이어서 참조 클라이언트가 실제 읽는 `Resources/Json/Sheets/LocalizationItem.json`과 다르다. 명시적으로 xlsx 파일을 전달해도 `Public` 시트 규격과 맞지 않는다.

참조 런타임 JSON은 1,940행으로, 마스터의 저장값 1,961행과도 일치하지 않았다. 기존 Python 스크립트와 PSDataTool 자료만으로 현재 마스터를 런타임 JSON으로 만드는 최신 실행 절차는 확인되지 않았다. BB_Indi에는 입력·출력 경로가 명확한 자체 익스포트 절차를 만든다.

### 2.2 BB_Indi에서 확인한 상태

| 영역 | 현황 | 필요한 작업 |
|---|---|---|
| 공통 접근 | `Assets/Scripts_New/Manager/KOBManager.cs`에서 LocalizationManager를 지연 생성 | 이 진입점을 유지하고 초기화 순서를 명확히 한다 |
| 문자열 조회 | `Assets/Scripts_New/Manager/LocalizationManager.cs`가 매번 배열을 선형 검색 | 로드 시 Dictionary를 생성하고 조회 경로를 통합한다 |
| 누락 처리 | 없는 키는 `String Empty`, 해당 언어가 비어 있으면 빈 문자열 반환 | 대체 언어 및 누락 진단 규칙을 추가한다 |
| JSON 모델 | `Assets/Scripts_Old/DataRecords/LocalizationDataRecord.cs`, 키와 15개 언어 필드 | 기존 키·언어 값을 손실 없이 엑셀로 이관한다 |
| 언어 저장 | `GameConfig.CurrentLanguage`, PlayerPrefs `RegistLanguae` | 기존 저장값을 읽고 한 서비스에서 언어 상태를 변경한다 |
| 첫 실행 | `GameConfig.ChangeLanguage()`가 영어를 강제 저장. 기기 언어 처리는 주석 상태 | 저장값 → 지원 기기 언어 → 영어 순서로 선택한다 |
| 언어 팝업 | `Popup_Setting_Language.cs`는 빈 클래스. 설정 팝업에서 열기 경로는 존재 | 언어 목록, 선택, 저장, 현재 UI 갱신을 구현한다 |
| TMP 폰트 | `SetFont2()` 미구현, TMP 전역 fallback 목록 비어 있음 | 실제 폰트 글리프·fallback·재질을 구성하고 검증한다 |
| 일반 Text 폰트 | 로더가 요청하는 `BlackHanSans-Regular`, `NotoSansJP-Bold`, `OPPOSans-B-2` 파일을 검색에서 찾지 못함 | 현재 보유 폰트 확인 후 실제 존재하는 자산으로 매핑한다 |
| 별도 CSV 구현 | `GameUILocalize`가 CSV를 읽고 PlayerPrefs `Language`를 사용 | 사용처 확인 후 공통 언어 상태·조회로 연결한다 |
| 문자열 작성 위치 | C# 하드코딩, 프리팹 고정 문구, 차트의 name_id/desc_id 직접 출력이 혼재 | 세 종류를 함께 조사·전환한다 |

`GameUILocalize`는 현재 여러 구형 프리팹에 직렬화되어 있다. 이 사실만으로 해당 프리팹이나 CSV가 현재 경기 경로에서 사용된다고 판정하지 않는다. 활성 경로와 비활성/보관 자산의 구분은 0단계에서 확정한다.

### 2.3 데이터 기준 수치

현재 `Assets/Resources/Localization/LocalizationItem.json`은 **1,134행 / 고유 키 1,127개**다. 아래 수치는 중복을 포함한 행 단위이며 실제 화면 번역 완료율은 아니다.

| 언어 | 값이 있는 행 | 빈 행 |
|---|---:|---:|
| Eng | 1,081 | 53 |
| Kor | 1,081 | 53 |
| Esp | 736 | 398 |
| Jpn | 678 | 456 |
| ChnT | 678 | 456 |
| ChnS | 678 | 456 |
| Fra | 2 | 1,132 |
| Deu, Idn, Ita, Prt, Rus, Tha, Tur, Vnm | 각각 0 | 각각 1,134 |

중복 키 7종은 `ButtonTxt.ByLevel`, `PopupBody.GoogleLogin`, `PopUp_BattleEnd_Result_Win`, `PopUp_BattleEnd_Result_Invalid`, `PopUp_BattleEnd_Result_Tie`, `Ingame.NicePlay`, `Ingame.YouWin`이다. 예를 들어 `PopUp_BattleEnd_Result_Invalid`는 `INVALID`/`INVALID GAME`, `Ingame.NicePlay`는 `NICE PLAY`/`NICE PLAY!`로 다르다. 현재 로더는 먼저 나온 행을 사용하므로 Dictionary 전환 전에 기준 문구를 정해야 한다.

별도 `Assets/Resources/Localization.csv`는 `KEY,Korea` 헤더, 데이터 232행 / 고유 키 231개이며 `FILTER_NAME`이 중복이다. JSON 키와의 교집합은 0개다. 한국어 CSV를 영어 열로 잘못 이관하지 않도록 `Korea → Kor`를 명시한다.

기존 `Docs/UIAudit/localization-data.json`에는 치환 인자 차이 35건이 기록되어 있다. 이것은 이전 감사 결과다. 구현 시 현재 문구와 호출부의 인자 의미를 대조하여 다시 판정하며, 인자 집합 차이를 모두 번역 오류로 단정하지 않는다.

## 3. 엑셀·익스포트 규격

### 3.1 번역 원본

신규 원본은 `Table/Localization_master_v2.xlsx`로 정한다. 첫 구현은 하나의 `Public` 시트를 직접 편집하며, 참조 마스터처럼 첫 행이 헤더이고 두 번째 행부터 데이터다.

| 열 | 헤더 | 규칙 |
|---|---|---|
| A | key | 대소문자를 구분하는 고유 문자열. 앞뒤 공백 금지 |
| B:F | Eng, Kor, Jpn, ChnT, Esp | 참조 마스터의 순서 유지 |
| G:P | ChnS, Fra, Deu, Idn, Ita, Prt, Rus, Tha, Tur, Vnm | 기존 BB_Indi 번역 보존. 열이 있다는 이유로 언어를 활성화하지 않음 |
| Q | Category | Common, Lobby, Settings, RTTS, Game, Popup, Character, Skill, Equip, Tutorial, Legacy 등 |
| R | Status | 이관, 번역중, 검수완료 등 작업 상태. 익스포트 포함 여부와 무관 |
| S | Context | 화면 용도, `{0}` 등의 의미, 길이 제약, 리치 텍스트 주의점 |
| T | Source | 원본 JSON/CSV의 행 또는 코드·프리팹·차트의 사용처 |

`Public`의 키가 있는 행은 전부 런타임 출력 대상이다. 검수 상태 때문에 행이 조용히 누락되는 규칙을 두지 않는다. 미사용이 확인된 원문을 보관할 필요가 생기면 `_Archive` 시트에 보관하고, 익스포터는 명시적으로 `Public`만 읽는다. 사용 여부가 불명확한 키는 임의로 보관 시트로 옮기거나 삭제하지 않는다.

헤더·키 열 고정, 필터, 번역 셀 줄바꿈, 읽을 수 있는 열 너비를 설정한다. 빈 번역과 중복 키를 눈으로 확인할 수 있게 표시한다. 번역 데이터 셀은 문자열 값으로 저장하며 수식은 사용하지 않는다.

### 3.2 키와 문구 이관 규칙

1. **기존 키는 유지한다.** `UI.BallerListTitle`, `SkillDesc.10003` 등 코드·차트가 이미 참조하는 키를 스타일 통일 목적으로 변경하지 않는다.
2. 신규 키는 `UI_SETTINGS_LANGUAGE_TITLE`, `UI_RTTS_TODAY_MATCH`, `UI_GAME_INNING_TOP`처럼 기능과 의미를 드러낸다. 같은 화면·같은 의미의 기존 키가 있으면 우선 재사용한다.
3. 동일한 표면 문구라도 문맥·문법·인자가 다르면 별도 키를 사용한다. 한국어 조사나 영어 단어 조각을 코드에서 이어 붙이지 않고 문장 전체를 번역한다.
4. 중복 행이 모든 언어에서 같으면 하나로 합치고 이관 기록을 남긴다. 다르면 기존 첫 번째 행의 실제 표시를 기준으로 비교하여 채택 문구를 기록한다. 마지막 행으로 자동 덮어쓰지 않는다.
5. 모든 언어가 빈 53행은 사용처를 조사한다. 사용 중이면 기본 언어 문구를 작성하고, 미사용이 확인되면 보관한다. 동적 키·외부 차트 참조가 미확인인 행은 조사 대상으로 남긴다.
6. 참조 프로젝트 문구는 의미가 맞는 경우만 활용한다. BB_Indi의 선수·장비·스킬·RTTS 문구와 참조 프로젝트의 콘텐츠를 통째로 교체하지 않는다.
7. 이름, 닉네임, 팀명 등 사용자 입력은 그대로 표시한다. 차트의 `name_id`/`desc_id`는 조회 키이며, 숫자 ID·게임 계산값·보상 값은 번역하지 않는다.
8. `##`는 참조 프로젝트의 이름/설명 분리 관례다. BB_Indi가 이미 `name_id`/`desc_id`를 분리해서 사용하는 데이터에는 새로 강제하지 않는다.

### 3.3 JSON 계약

출력 파일은 현재 사용 중인 `Assets/Resources/Localization/LocalizationItem.json` 하나로 한다. 루트 이름 `LocalizationItem`과 기존 필드명을 유지한다.

```json
{
  "LocalizationItem": [
    {
      "key": "UI_SETTINGS_LANGUAGE_TITLE",
      "Eng": "Language",
      "Kor": "언어",
      "Jpn": "言語",
      "ChnT": "語言",
      "Esp": "Idioma",
      "ChnS": "",
      "Fra": "",
      "Deu": "",
      "Idn": "",
      "Ita": "",
      "Prt": "",
      "Rus": "",
      "Tha": "",
      "Tur": "",
      "Vnm": ""
    }
  ]
}
```

이 행은 규격 예시다. 실제 번역 검수 완료를 의미하지 않는다. Category/Status/Context/Source는 JSON에 내보내지 않는다. JSON은 생성물로 관리하고 문구 수정은 엑셀에서 수행한다.

### 3.4 익스포터 요구사항

신규 파일: `Table/export_localization.py`, `Table/export_localization.bat`, `Table/requirements.txt`, `Table/README.md`.

- Python에서 xlsx를 읽어 JSON을 작성한다. 엑셀 읽기 의존성은 requirements에 버전을 고정하며, 매크로·Excel 설치·외부 Google Sheets 연결 없이 실행 가능해야 한다.
- 기본 입력은 스크립트 폴더의 `Localization_master_v2.xlsx`, 기본 시트는 `Public`, 출력은 프로젝트의 기존 Resources 경로로 고정한다. 현재 작업 디렉터리나 사용자별 절대 경로에 의존하지 않는다.
- 열은 인덱스가 아니라 헤더 이름으로 찾는다. 필수 헤더 누락·중복, 미등록 언어 열, 중복 키, 키 앞뒤 공백, 문자열이 아닌 키, 데이터 셀 수식·Excel 오류는 실패 처리한다. 수식의 캐시값으로 성공 처리하지 않는다.
- 완전히 빈 행만 건너뛴다. 번역/관리 정보가 있는데 키가 빈 행은 오류로 보고한다.
- 번역문의 의도적인 공백·따옴표·아포스트로피를 보존한다. 참조 스크립트의 일괄 `strip()` 및 첫 아포스트로피 제거를 그대로 사용하지 않는다. 줄바꿈은 LF로 통일한다.
- 신규 엑셀 문구의 줄바꿈은 실제 셀 줄바꿈을 사용한다. 기존 문자열의 문자 그대로인 `\n`은 최초 이관 때 실제 개행으로 변환하고 전후 결과를 비교한다. 런타임과 익스포터가 중복으로 이스케이프 변환하지 않도록 한다.
- 데이터는 키 순서로 정렬하고 UTF-8·고정 들여쓰기로 출력한다. 같은 입력에서 JSON 내용이 동일해야 한다.
- 먼저 전체 데이터를 검증하고 임시 출력까지 성공한 후 기존 JSON을 교체한다. 실패하면 기존 JSON을 그대로 보존하며, 기존 `.meta`도 보존한다.
- `--check`는 검증만 실행하고 런타임 파일을 쓰지 않는다. `--check-generated`는 기대 출력과 현재 JSON의 일치를 검사한다.
- 기본 언어 Eng/Kor 누락은 정상 운영 익스포트의 오류다. 도입 중 누락은 검사 보고서로 모으고 데이터를 정리한 뒤 출력한다. Jpn/ChnT/Esp 누락은 초기에는 경고로 집계하고, 해당 언어를 활성화하기 전에는 오류 없이 통과시킨다.
- 치환 인자의 문법 오류·잘못된 괄호는 오류다. 언어 간 인자 집합 차이는 사용처와 함께 보고하고, 의도된 생략인지 오류인지 판정한다. 승인된 예외는 근거를 남기고 검사에서 구분한다. 출시 범위에 미판정 차이를 남기지 않는다.
- 오류 메시지에 파일·시트·행·키·언어·원인을 출력하고 실패 종료 코드를 반환한다. `.bat`도 실패 종료 코드를 유지한다.

구현 후 제공할 명령 계약은 다음과 같다. 현재는 아직 존재하지 않는 도구이므로 이 문서 작성 단계에서 실행하지 않는다.

```powershell
Set-Location 'D:\PROJECTS\Git\BB_Indi'
python -m pip install -r .\Table\requirements.txt
python .\Table\export_localization.py --check
python .\Table\export_localization.py
python .\Table\export_localization.py --check-generated
```

`export_localization.bat`를 더블클릭해도 같은 기본 파일을 내보내도록 한다. Python 또는 의존성이 없으면 설치 안내와 함께 종료하고 도구가 임의로 설치하지 않는다.

## 4. 런타임 적용 규격

### 4.1 공통 매니저와 언어 상태

`Assets/Scripts_New/Manager/LocalizationManager.cs`를 개선한다. 별도 매니저를 추가하지 않고 `KOBManager.Localization`에서 다음 책임을 제공한다.

| 제안 API/책임 | 동작 |
|---|---|
| `GetLocalizedText(key)` | 현재 언어로 조회 |
| `GetLocalizedText(key, language)` | 지정 언어로 조회 |
| `Format(key, params object[] args)` | 조회 후 치환. 언어에 맞는 숫자 표시 규칙 적용 |
| `SetLanguage(language)` | 활성 언어 검증, 현재 상태·저장값·폰트 상태 갱신 후 이벤트 발생 |
| `OnLanguageChanged` | 표시 중 UI 갱신. 같은 언어 재선택 시 불필요한 재갱신 방지 |
| 초기화/진단 | JSON 1회 로드, 중복 검증, Ordinal 키 인덱스, 누락 키 진단 |

기존 `GetUILocalizedValue/GetUILocalizedValue2/GetLocalizedValue/GetLocalizedValue2`는 사용처를 확인하며 공통 조회로 위임한다. `UnityEngine.UI.Text`와 TMP 대상 폰트 적용 책임은 분리하되 문자열 선택 규칙은 하나로 만든다. 문자열을 조회할 때마다 폰트나 JSON을 다시 로드하지 않는다.

언어 식별은 현재 `GameDefine.eLanguage`를 기준으로 한다. 참조 프로젝트의 `Language` enum 숫자를 복사하지 않는다. 예를 들어 참조의 Kor는 2지만 현재 Korea는 8이다. 기존 enum 값·직렬화 데이터를 바꾸지 않고 다음처럼 명시적으로 매핑한다.

| 엑셀/JSON | 현재 enum | locale |
|---|---|---|
| Eng | English | en-US |
| Kor | Korea | ko-KR |
| Jpn | Japan | ja-JP |
| ChnT | China_Traditional | zh-TW |
| Esp | Spain | es-ES |
| ChnS | China_Simplified | zh-CN |

활성 언어·자국어 표기·locale은 신규 `Assets/Resources/Localization/LanguageConfig.json` 한 곳에서 관리한다. enum 이름 또는 언어 코드로 저장하여 참조 프로젝트의 숫자 매핑을 가져오지 않는다. 폰트 자산은 별도의 Unity 설정 자산으로 연결한다.

초기화는 **유효한 `RegistLanguae` 저장값 → 지원되는 기기 언어 → 영어** 순서다. `Enum.Parse` 대신 손상된 저장값과 `MAX`를 안전하게 처리한다. 저장 키의 기존 오탈자는 이번 작업에서 그대로 유지하여 저장값을 잃지 않도록 한다. `GameConfig`와 새 매니저가 서로 초기화를 호출하는 순환을 만들지 않는다.

`PlayerPrefs("Language")`는 과거 CSV 경로의 값이다. 실제 사용과 저장값 형식을 확인한 뒤, 필요한 경우 기존 저장값이 없을 때 한 번만 변환한다. 최종 활성 경로에는 언어 상태를 두 벌로 유지하지 않는다.

누락 문구는 **요청 언어 → Eng → Kor → 키 자체** 순서로 반환한다. 개발 환경에서는 누락을 찾기 쉽게 표시하고 키·언어별 로그를 중복 없이 기록한다. 리소스 누락·JSON 파싱 실패는 명확히 진단하며 NullReferenceException이나 `String Empty` 화면 노출로 이어지지 않게 한다. 대체 표시가 검수 완료를 의미하지는 않는다.

참조 매니저의 Naninovel·`MainManager.JsonData` 의존성은 BB_Indi에 가져오지 않는다. Unity Localization 패키지·Addressables·서버 번역 서비스 도입도 이번 기본 설계의 필수 조건이 아니다.

### 4.2 고정 문구와 동적 문구

**고정 문구:** 신규 `Assets/Scripts_New/UIComponent/Localization/LocalizedText.cs`를 두고 Inspector에 키와 대상 TMP/Text를 지정한다. 활성화될 때 현재 언어를 적용하고 이벤트를 구독한다. 비활성화·파괴 시 구독을 해제하며, 재활성화 시 다시 적용한다. 프리팹에 저장된 색·정렬·크기·줄 간격 등은 유지한다.

**동적 문구:** 수량, 스탯, 일정, 결과 등은 기존 화면 컨트롤러가 원래 데이터로 `Format`을 호출한다. 언어 변경 시 이미 번역된 문자열을 재가공하지 않고 원본 데이터로 다시 그린다. 같은 Text를 고정 바인더와 동적 컨트롤러가 동시에 덮어쓰지 않게 소유자를 하나로 정한다.

화면 갱신을 위해 `OpenWindow()`나 전체 초기화를 다시 호출하지 않는다. 예를 들어 `UI_RTTS.OpenWindow()`는 저장·선택·탭 초기화 등 동작을 포함한다. `RefreshLocalizedTexts()`처럼 표시만 갱신하는 메서드를 분리하여 스크롤, 선택 선수, 경기 상태, 재화 요청을 유지한다.

`{0}`, `{1}`, `{0:N0}` 등 인자의 의미는 Context와 호출부에서 일치시킨다. `Popup_SkillSetting`/`Popup_GearSetting`의 값·퍼센트 전달 순서를 특히 확인한다. `{{`/`}}` 같은 리터럴 괄호, 개행, `<color>`/`<size>`/`<sprite>` 등 기존 태그도 검사한다. 단순 정규식으로 모든 중괄호를 같은 인자로 취급하지 않는다.

영어의 `st/nd/rd/th`, `W/D/L`, 한국어 조사, 이닝의 초/말은 각 언어의 전체 문구로 구성한다. 야구 기록 소수점, 순위, 날짜, 수량 표시는 의미에 따라 유지 또는 locale 적용을 명시한다. 사용자 입력을 format 문자열이나 번역 키로 사용하지 않는다.

### 4.3 경기 UI와 CSV 처리

대상은 `Assets/MainGame/Script/UI/UGUI/Integrated/GameUILocalize.cs`와 실제 경기 컨트롤러다.

- 현재 실행 경로에서 사용하는 GameUILocalize 및 CSV 키를 먼저 식별한다. 미사용 레거시 자산까지 모두 현행 화면의 전환 실적으로 집계하지 않는다.
- 텍스트 사용처는 공통 JSON 조회로 전환하고 `GameUIElement.text`에 반영한다. `graphic.text`만 직접 바꾸면 GameUIElement의 내부 값과 어긋나 재적용될 수 있으므로 표시 API를 사용한다.
- CSV 파서·직접 PlayerPrefs 조회는 해당 사용처의 전환과 직렬화 참조 확인 후 제거한다. 사용 중인 CSV 경로가 남아 있으면 완료로 처리하지 않는다. 자산 파일 삭제는 모든 참조를 확인한 후 수행한다.
- `ElementKind.Sprite`는 번역문 대신 스프라이트 이름을 설정하는 분기다. 실제 사용이 있으면 언어별 아트 매핑을 따로 정의하고, 일반 번역문을 spriteName으로 주입하지 않는다.
- 텍스처·아틀라스에 구워진 글자는 별도 조사한다. 가능한 것은 텍스트로 분리하고, 유지할 아트는 언어별 자산 목록과 기본 언어 대체 규칙을 기록한다.
- 로비와 경기 진입 시 언어가 일치해야 한다. 경기 중 표시는 폴링하지 않고 언어 이벤트 또는 데이터 갱신 시 반영한다.

### 4.4 폰트와 설정 화면

폰트 작업은 화면 전체 전환보다 먼저 작은 화면에서 검증한다. 현재의 영문 중심 디자인을 유지하면서 한글·가나·한자·스페인어 특수문자에 필요한 TMP fallback 또는 언어별 폰트를 연결한다. 한국어·일본어·중국어의 글리프 모양도 구분해서 확인한다.

보유한 `NanumSquareEB.ttf`, `NotoSansCJKkr-Black.otf` 등의 실제 문자 지원을 확인한다. 파일명만으로 모든 목표 언어가 지원된다고 판단하지 않는다. 추가 폰트가 필요하면 프로젝트 배포에 사용할 수 있는 자산으로 확보하고 출처를 기록한다. 일반 Text, TMP, 경기 bitmap/TMP 폰트 각각을 확인하며 새 폰트 때문에 outline·그림자·재질이 사라지지 않도록 한다.

`Popup_Setting_Language.cs`와 기존 프리팹에는 활성 언어의 자국어 이름, 현재 선택 표시, 선택 이벤트를 연결한다. 선택 후 저장과 즉시 갱신이 이루어져야 하며, 닫기·뒤로가기·재진입·앱 재시작 후에도 선택 상태가 맞아야 한다. 언어 팝업의 자체 제목과 버튼도 번역 대상으로 포함한다.

## 5. 구현 순서와 단계별 완료 조건

### 0단계 — 실제 사용처와 이관 목록 확정

작업: 빌드 씬과 `_Test_Local` 등 개발 진입 경로에서 로비 → RTTS → MainLoading → BallPlay → 결과 → 로비 흐름을 추적한다. Scripts_New, MainGame, 실제 참조되는 Scripts_Old/Resources/ResourcesBundle, 씬·프리팹·차트·튜토리얼을 함께 조사한다. 비활성·풀링 객체도 포함하고 외부 패키지 데모는 분리한다.

산출물: `Docs/Localization/LOCALIZATION_INVENTORY.md`. 화면/사용처, 기존 키 또는 원문, 고정·동적·차트·아트 구분, 실제 사용/미사용 확인/확인 보류, 처리 단계, 검증 상태를 기록한다. JSON 중복 7종, CSV 중복 1종, 빈 53행, 동적 키 조합 및 차트 참조 목록을 포함한다.

완료 조건: 현재 사용 경로의 작업 분모가 정해지고 각 미확인 항목의 조사 위치가 기록됨. 문자열 리터럴 검색만으로 전체 키 검증이 완료되었다고 보고하지 않음.

### 1단계 — 엑셀 원본과 익스포터 구축

작업: 현재 JSON의 키와 15개 언어 값을 이관하고 중복을 정리한다. 필요한 CSV·하드코딩 문구를 추가하고, 빈 기본 언어 및 치환 인자를 정리한다. 위 규격의 익스포터·검사·실행 안내를 작성한다.

완료 조건: xlsx → JSON 생성 성공, 중복·필수값·형식 오류 0, 동일 입력 재실행 결과 동일. 기존 대비 추가/삭제/변경 키와 번역 값의 차이를 설명할 수 있음. 실패 시 기존 JSON 보존 확인. 검토 없이 키·번역 값이 사라지지 않음.

### 2단계 — 매니저·언어 저장·폰트·설정 팝업

작업: 단일 조회 경로, 인덱스, 누락 처리, Format, 이벤트, 언어 설정, 폰트 매핑을 구현한다. 기존 조회 API 사용처가 계속 컴파일되게 연결하고 설정 팝업에서 영어 ↔ 한국어를 전환한다.

완료 조건: 로비의 고정 문구 1개, 동적 수량 문구 1개, 캐시된 팝업 1개가 즉시 변경됨. 화면 재개방·씬 이동·재시작 후 언어 유지. 누락 글리프와 이벤트 중복 구독이 없음.

### 3단계 — 로비·RTTS·선수·팝업 적용

| 묶음 | 우선 확인 파일/화면 | 처리 내용 |
|---|---|---|
| 설정·공통 | Popup_Setting, Popup_Setting_Language, Popup_Name, 오류/확인/보상 팝업 | 제목·버튼·안내·입력 placeholder |
| 로비·메뉴 | UI_LobbyRe, 로비 프리팹, UI_Shop, UI_Inventory, UI_TrophyRoad | 고정 라벨·상태 문구 |
| 선수·성장 | UI_BallersList, UI_Ballers, BallerInfoComponent2, BallerAchieveComponent, Popup_StatUpgrade, Popup_Promotion | 이름·설명·필터·성장 수치·직접 키 표시 제거 |
| 장비·스킬 | Popup_GearSetting, Popup_SkillSetting, Popup_Equip | 이름·설명·레벨·수치/퍼센트 인자 |
| RTTS | UI_RTTS, UI_RTTSResult, ScheduleComponent, RttsResult 하위 컴포넌트 | 일정·순위·시즌 결과·승무패·리그 안내 |
| 공통 문자열 유틸 | KOBTextUtil, KOBUtil | 기록명·순위 접미사·좌우타·등급·승패 표현 |
| 보상·업적 | TR_RewardComp, BallerAchieveComponent | name_id/desc_id를 문자열로 직접 노출하는 경로 수정 |

실제 예: `ScheduleComponent`의 `Today Match`, `ResultMyTeamChamp`의 우승 문장, `KOBTextUtil`의 `MY TEAM`/`RANK`/`WIN`/`LOSS`를 키 기반으로 바꾼다. 화면이 열려 있는 동안 언어를 바꿔도 선택·수치·탭·스크롤을 유지한다.

완료 조건: 해당 묶음의 실제 사용자 문구 전환 및 검증 상태가 목록에 기록됨. 원문 유지 항목은 선수 닉네임, 공용 약어 등의 구체적인 이유가 있음.

### 4단계 — 경기·튜토리얼·동적 생성 UI 적용

작업: MainLoading, 점수판, 이닝/판정 안내, 투타 컨트롤, 일시정지·교체, Quick/자동 진행, 스킬 안내, 경기 결과를 전환한다. `Assets/MainGame/Script/util/Util.cs`의 타격 결과·포지션·구장명 생성도 조사한다. `FrontUI_Tutorial` 파생 클래스 및 `TutorialManager`가 공급하는 실제 대사·안내를 전환한다.

NGUI→UGUI 전환이 이미 수행된 GameUIElement·Canvas·클리핑·입력 구조를 유지한다. 화면을 재생성하는 방식의 로컬라이징을 도입하지 않는다. 풀링 셀·동적 팝업·비활성 후 재사용 UI가 최신 언어를 가져오게 한다.

완료 조건: 로비 → RTTS → 수동 경기 → 자동 진행/복귀 → 일시정지·선수 교체 → 결과 → 로비 흐름에서 언어가 일치하고 문구가 갱신됨. 실제 입력 검사와 테스트 API 호출 검사를 구분하여 기록함.

### 5단계 — 추가 언어와 배포 검증

작업: Jpn/ChnT/Esp 번역 누락·치환 인자·글리프·줄바꿈을 정리하고 해당 언어를 활성화한다. 대표 화면의 긴 문구, 작은 버튼, 세로 목록, 숫자/기호를 확인한다. 빌드에 JSON과 폰트가 포함되는지 확인한다.

완료 조건: 활성 언어마다 실사용 키 누락 및 미판정 인자 차이 0. 언어별 화면 검토 완료. Unity 컴파일, 관련 EditMode/PlayMode 검사와 대상 플레이어 빌드 검증 완료. 외부 서버·PVP·실기기 등 미검증 경로는 별도로 표시하고 완료 범위에 섞지 않음.

## 6. 수정·신규 파일 묶음

다음 경로는 BB_Indi 루트 기준이다. 신규 경로는 구현 시 생성한다.

| 묶음 | 파일/폴더 | 구분 |
|---|---|---|
| 번역 원본·도구 | `Table/Localization_master_v2.xlsx`, `export_localization.py`, `export_localization.bat`, `requirements.txt`, `README.md` | 신규 |
| 생성 데이터 | `Assets/Resources/Localization/LocalizationItem.json` | 기존 파일 갱신, meta 보존 |
| 언어 설정 | `Assets/Resources/Localization/LanguageConfig.json` | 신규 |
| 조회·저장 | `Assets/Scripts_New/Manager/LocalizationManager.cs`, `Assets/Scripts_Old/Common/GameConfig.cs` | 수정 |
| 기존 모델·enum | `Assets/Scripts_Old/DataRecords/LocalizationDataRecord.cs`, `Assets/Scripts_Old/Common/GameDefine.cs` | 기존 계약 유지, 필요한 수정만 |
| UI 바인더 | `Assets/Scripts_New/UIComponent/Localization/LocalizedText.cs` | 신규 |
| 폰트 | Localization용 폰트 설정 자산·TMP fallback, 필요 시 TMP Settings | 신규/수정, 기존 디자인 보존 |
| 언어 선택 | `Assets/Scripts_New/UIPopup/Popup_Setting_Language.cs`, `Assets/Resources/UI/Popup/Popup_Setting_Language.prefab` | 수정 |
| 경기 연결 | `Assets/MainGame/Script/UI/UGUI/Integrated/GameUILocalize.cs` 및 실제 UI 컨트롤러 | 수정 |
| 편집 편의 | `Assets/Editor/Localization/`의 키 선택·검색·미리보기 도구 | 필요 기능 신규 |
| 화면 적용 | 단계별 Scripts_New/MainGame 코드, 실제 사용 프리팹·씬 | 목록 기준 수정 |
| 검증 기록 | `Docs/Localization/LOCALIZATION_INVENTORY.md`, `VALIDATION.md` | 신규 |

신규 Unity 자산의 `.meta`를 함께 관리한다. `Library`, `Temp`, 생성 `.csproj`는 산출물에 포함하지 않는다. 참조 프로젝트는 읽기 자료로만 사용한다. 초기 작업에 서버/차트 스키마 수정이나 온라인 차트 배포는 필요하지 않다. 기존 차트 키가 잘못된 경우에는 번역 누락과 구분하여 기록하고 해당 차트 사용처를 확인한 뒤 처리한다.

## 7. 검증 항목

| 검증 | 합격 기준 |
|---|---|
| 엑셀 → JSON | 모든 출력 행·필드·문구가 규칙과 일치하고 관리 열이 런타임에 섞이지 않음 |
| 익스포터 실패 | 중복 키, 잘못된 헤더/행, 수식, format 문법 오류에서 실패하고 기존 JSON 보존 |
| 재현성 | 동일 입력 2회 결과 일치, `--check-generated` 통과 |
| 언어 선택 | 첫 실행·저장값·손상된 값·미지원 기기 언어·동일 언어 재선택이 규칙대로 동작 |
| 조회 | 정상 키, 빈 번역, 없는 키, 빈 키, JSON 누락의 동작과 진단 확인 |
| 포맷 | 복수 인자, 재정렬, 숫자 형식, 리터럴 괄호, 리치 텍스트, 개행, 의도된 인자 생략 검증 |
| 사용 키 검사 | C# 리터럴 + 직렬화된 키 + 동적 조합 + 차트 name_id/desc_id를 합친 실제 사용 목록에서 누락 0 |
| UI 갱신 | 현재 화면, 비활성/캐시 팝업, 풀링 셀, 새로 생성한 경기 UI가 최신 언어 표시 |
| 게임 상태 보존 | 언어 변경으로 저장·보상·네트워크 요청·경기 재시작이 중복 실행되지 않음 |
| 시각 검토 | 활성 언어에서 글리프 누락, 잘림, 버튼 영역 초과, 줄바꿈 오류, outline/그림자 손상 없음 |
| 경기 회귀 | 수동/자동/교체/결과/로비 복귀에서 기존 입력과 GameUIElement 표시 유지 |
| 배포 | 실제 대상 빌드에서 번역 JSON·폰트 로드 및 언어 저장 확인 |

익스포터 검사는 임시 xlsx/출력 파일로 수행하고, 매니저 검사는 공개 조회·언어 변경 동작을 중심으로 작성한다. 로컬라이징 확인을 위해 내부 변수명이나 프리팹 fileID를 고정하는 테스트는 만들지 않는다. 긴 문구와 폰트는 실제 화면 검토를 병행한다.

소스상 문자열 변경 수만으로 완료를 판단하지 않는다. `VALIDATION.md`에는 검사 환경, 언어, 화면/흐름, 실행 방법, 결과, 미검증 항목을 기록한다. 기존 UI 감사 문서에 적힌 과거 검증을 이번 로컬라이징 검증으로 재사용하지 않는다.

## 8. 첫 구현 작업에 바로 전달할 지시

> 0~2단계를 먼저 수행한다. 현재 JSON/CSV/차트·프리팹 사용처를 조사하고, 기존 키와 번역을 보존한 `Table/Localization_master_v2.xlsx` 및 검증 가능한 JSON 익스포터를 만든다. `KOBManager.Localization`의 조회·언어 저장·이벤트를 통합하고, 영어·한국어 폰트와 언어 팝업을 연결한다. 로비 고정 문구, 동적 수량 문구, 캐시 팝업으로 전환 동작을 검증한다. 이후 3~5단계는 확정한 사용처 목록의 화면 묶음 순서로 진행한다. 각 단계에서 데이터 차이와 검증 결과를 남긴다.

## 9. 조사 근거와 이번 문서의 한계

- 참조 마스터 및 4개 분류별 xlsx의 시트/헤더/수식/저장값을 읽기 전용으로 확인했다. 외부 Google Sheets를 갱신하거나 현재 값으로 재계산하지 않았다.
- 참조 `Table/export_localization.py`, `export_localization.bat`, PSDataTool 사용 안내·설정, `Assets/Scripts/Manager/LocalizationManager.cs`, `JsonManager.cs`, `TextMeshInitializer.cs`, 키 편집 도구를 확인했다. 참조 익스포터를 실행하지 않았다.
- 현재 JSON/CSV의 행 수·중복·언어별 빈 값을 다시 집계했다. 매니저·언어 저장·설정 팝업·문자열 유틸·대표 UI 호출부·빌드 씬·폰트 파일·TMP 설정을 확인했다.
- 기존 [UI 감사](../UIAudit/README.md), [문자열 감사 데이터](../UIAudit/localization-data.json), [실행 경로 범위](../UIAudit/RUNTIME_SCOPE.md), [경기 UGUI 전환 기록](../UIAudit/Integrated/PROGRESS.md)을 참고했다.
- 이번 문서 작성에서 전체 씬/프리팹 사용처 감사, Unity 실행, 플레이어 빌드, 다국어 화면 검증은 수행하지 않았다. 해당 작업은 위 단계별 구현·검증에 포함한다.
