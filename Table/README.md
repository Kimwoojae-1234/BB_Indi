# BB_Indi 로컬라이징 운영

`Localization_master_v2.xlsx`의 **Public** 시트가 원본이다. 앞 6열은 참조 프로젝트와 같은 `key, Eng, Kor, Jpn, ChnT, Esp`이며 기존 게임의 나머지 10개 언어도 보존했다. 영어가 기본 언어다. 새 영어 번역의 검토 상태는 `Status`, 원문과 사용처는 `Context`, `Source`에 기록한다.

선택 목록은 BeachBaseball과 동일한 영어·일본어·한국어·스페인어·중국어 번체 5개다. 언어 선택 가능 여부가 전체 번역 완료를 의미하지 않는다. 로비·설정의 표시 수정과 나머지 미번역 목록은 `Docs/Localization/LANGUAGE_SWITCH_FIX.md`, `language-coverage.json`에 기록한다.

## 적용 대상

**자동 로컬라이즈 컴포넌트는 고정 UI 라벨에만 붙인다. 코드가 텍스트를 대입하는 대상에는 붙이지 않는다.** 초기 문구가 일반 문장이어도 실행 중 코드가 바꾸는 필드라면 제외한다. 코드에서 색상·활성 상태만 바꾸는 고정 라벨은 연결할 수 있다.

| 표시 내용 | 처리 |
|---|---|
| 고정 버튼·메뉴·설명 라벨 | 의미 있는 키와 `LocalizedText` 또는 기존 `GameUILocalize` 연결 |
| 코드가 표시하는 고정 안내문·문장 형식 | 코드가 `L10n.T/F/SetText`로 필요한 시점에 조회·대입. 컴포넌트와 갱신 콜백은 추가하지 않음 |
| 닉네임, 사용자 입력, 서버에서 받은 이름·공지·우편 본문 | 받은 값을 그대로 표시. 번역표의 샘플 문구로 치환하지 않음 |
| 점수·타이머·스탯·가격·보상 등 코드/외부 데이터 | 해당 컨트롤러가 표시를 결정. 자동 로컬라이즈 제외 |
| 데이터 계약에 명시된 번역 키 | 해당 키만 코드에서 조회. 일반 문자열을 키로 추측하지 않음 |

닉네임 샘플 `grigrigri`는 번역 항목이 아니다. 서버 팀 이름도 팀 인덱스로 만든 번역 키로 대체하지 않는다.

## 키와 문구 편집

- `Common.Action.Confirm`, `Baseball.Stat.BattingAverage`, `Baseball.Result.HomeRun`처럼 영역·역할·의미가 드러나는 키를 사용한다. 해시형 `TXT_` 키는 사용하지 않는다. 문구가 수정돼도 의미가 같으면 키를 유지한다.
- `Eng`에 영어를 작성하고 `{0}`, `{1}`의 번호·서식과 색상·크기 태그를 보존한다. 언어별로 인자 위치를 바꿀 수 있다.
- 셀 안 줄바꿈 또는 `\n`을 사용할 수 있다. 익스포터가 실제 줄바꿈으로 정규화한다.
- `AllowEmpty=TRUE`는 모든 언어가 의도적으로 빈 설명일 때만 사용한다. 기존 53개를 보존했다.
- 중복 키, 빈 영어, 수식, 영어 열의 한글, 잘못된 포맷 문법은 익스포트 오류다. 기존 호환 키 `Ingame.DidNo Swing` 외의 공백 포함 키도 거부한다.
- 번역의 인자 번호가 영어와 다르면 엑셀 원문을 유지하고 해당 번역만 JSON에서 비운다. 기존 한국어 스킬 설명 35건은 영어로 대체되며 `Docs/Localization/export-warnings.txt`에 기록했다.

## 익스포트

프로젝트 루트에서 Python 3.10 이상으로 실행한다.

```powershell
python -m pip install -r Table/requirements.txt
python Table/export_localization.py
python Table/export_localization.py --check-generated
python -m unittest discover -s Table -p "test_*.py" -v
```

Python Launcher가 설치되어 있으면 `Table/export_localization.bat`도 사용할 수 있다. `--input`, `--output`으로 경로를 지정한다. `--check`는 저장하지 않고 검증한다.

출력은 `Assets/Resources/Localization/LocalizationItem.json`이다. 게임에서는 `Resources/Localization/LocalizationItem`을 로드한다. JSON은 익스포트로 생성하므로 문구 수정은 엑셀에서 한다.

## 코드와 고정 UI 연결

```csharp
// 고정 안내문을 코드에서 한 번 조회한다.
string title = L10n.T("Common.Action.Confirm");
string description = L10n.F("achieve_desc_1", 75);
L10n.SetText(label, "UI.TeamNumber", teamNumber);

// 외부 입력은 그대로 표시한다. 이 label에는 로컬라이즈 컴포넌트를 붙이지 않는다.
nicknameLabel.text = player.Nickname;
```

`L10n.SetText`는 텍스트만 대입한다. `AddComponent`나 언어 변경 이벤트 등록을 하지 않는다. 코드 소유 문구는 컨트롤러의 원래 갱신 시점에 다시 조회한다.

고정 UI의 `LocalizedText`에는 `key`, 대상 컴포넌트, 원래 `sourceText`를 지정한다. 경기 UI는 `GameUIElement`를 연결하며 자식 TMP 렌더러나 그림자에 중복 연결하지 않는다. 입력란의 입력 값에는 연결하지 않는다.

기존 `KOBManager.Localization` 호출도 같은 데이터와 언어 상태를 사용한다. 번역이 비면 영어, 기존 한국어, 키 순서로 대체한다. `L10n.SetLanguage` 또는 매니저의 `SetLanguage`로 언어를 바꾸면 고정 UI만 자동 갱신한다. 저장 키 `RegistLanguae`를 유지하고 이전 혼합 언어 빌드에서 처음 실행할 때 한 번 영어 기준으로 전환한다.

## 검사와 유지 관리

- `Tools > Localization > Check Static Text Scope`: `Docs/Localization/static-text-scope.json`의 고정 라벨과 제외 위치를 실제로 로드해 검사한다. 코드·데이터 소유 텍스트에 바인더가 있으면 실패한다.
- `Check Catalog and Bindings`: 조회·대체·포맷, 고정 라벨 갱신, 코드 소유 텍스트와 서버 이름 보존을 검사한다.
- `Audit Game UI`: 비활성 오브젝트를 포함한 실제 UI를 읽어 `Library/Localization/audit.json`에 기록한다.

신규 UI나 계층 경로를 변경할 때 검사 명세도 갱신한다. 고정 라벨을 코드가 쓰는 필드로 변경하면 로컬라이즈 컴포넌트를 제거하고 제외 목록으로 옮긴다. 문구만 보고 컴포넌트를 일괄 추가하던 도구는 폐기했다. `prepare_ui_bindings.py`의 파싱 함수는 원본 보존 검사에만 남아 있다.

키 변경 이력은 `Docs/Localization/key-renames.json`, 이번 수정 기록은 `key-and-ownership-correction.json`에 있다. 이전 일괄 적용 기록은 `Library/Localization/HistoricalReports`에 보관한다. 원본 자산을 Unity에서 일괄 재저장하지 않고 바인더와 키 참조만 수정하여 레이아웃과 기존 직렬화 데이터를 보존했다.

영어 외 언어의 완역·폰트 검수, 이미지에 구워진 글자 교체와 전체 게임 화면의 시각 검수는 별도 작업이다. 플러그인·Asset Store 예제, 로그·리소스 식별자와 참조되지 않는 과거 약관은 게임 번역 대상에서 제외한다.
