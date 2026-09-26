# NGUI 전체 제거 및 UGUI 전환 결과

2026-09-26 · Unity 6000.3.9f1 · 기준 커밋 `53f75713e7c4d398b8cd13f795b48102e97454b1`

남아 있던 NGUI UI를 UGUI로 전환하고 `Assets/NGUI`를 제거했다. 게임 코드, 프리팹·씬, 애니메이션, 폰트·아틀라스 및 머티리얼의 플러그인 의존성을 함께 정리했다. 기존에 작성한 [제거 전 조사](../NGUIRemaining/README.md)는 이전 상태를 기록한 자료다.

## 전환 범위

| 항목 | 결과 |
|---|---|
| 남은 전환 대상 | 프리팹 232개, 씬 8개, NGUI 컴포넌트 11,432개 |
| 전환 대상에 포함된 데이터 자산 | 아틀라스 39개, 비트맵 폰트 14개 |
| 전체 자산 검사 | 프리팹·씬 1,050개, 네이티브 UI 위젯 14,548개 |
| 기존 컨트롤러 연결 | 해석 가능한 원본 참조 17,430개 보존 |
| 애니메이션 검사 | 바인딩 1,379개, NGUI 타입 잔존 없음 |
| 머티리얼 | 26개를 프로젝트 자체 투명 셰이더로 전환 |
| 제거된 플러그인 참조 | 원본 GUID 224개를 대조, 직렬화 참조 0개 |
| Missing Script | 전환 전·후 모두 980개, 신규 발생 없음 |

범위에는 경기 결과·기록·보상·성장 UI, 동적 기록 행, 기존 로그인·다운로드·타이틀, PVP 테스트 씬, 이전 ResourcesBundle UI와 비활성 하위 오브젝트도 포함된다. 현재 실행 경로에 쓰이지 않는 보관 자산도 플러그인 참조를 제거했다. 원래 컨트롤러가 없거나 더 이상 존재하지 않는 필드 등 원본에서 검증할 수 없는 참조 4,800개는 별도로 집계했으며, 이를 정상 동작으로 판정하지 않았다.

## 구현

- 게임에서 사용하는 UI 접근 API는 `GameUIElement`, `GameUIPanel` 등 프로젝트 코드로 옮겼다. 실제 표시와 입력은 Unity의 Canvas, Image, TextMeshPro, InputField, Slider, Toggle, ScrollRect 및 EventSystem을 사용한다.
- 원본 GameObject·컴포넌트 ID와 컨트롤러 필드, 버튼 콜백, 피벗·앵커·클리핑·깊이·색상·알파·트윈을 보존했다. 오래된 바이너리 프리팹도 원본 데이터를 읽어 전환했다.
- 동적 프리팹이 호스트 카메라와 마스크를 상속하도록 연결했다. 스크롤 목록은 런타임에 추가·제거되는 행의 실제 크기를 콘텐츠 영역에 반영한다.
- 토글의 단일 클릭·라디오 그룹, 슬라이더의 단계·채움·드래그, 한글 입력·제출, 버튼의 상태별 색상·스프라이트를 처리했다.
- 결과 기록창의 표시 순서를 수정하고 9개 행이 모두 보이도록 뷰포트를 맞췄다. 잘못된 원본 ResultUI 아틀라스의 포지션 좌표는 정상 MainGameUI 아틀라스 항목으로 교체했다.
- NGUI 전용 셰이더는 `Game/UI/Transparent Vertex Color`로 대체했다. NGUI를 필요로 하는 일회성 에디터 변환기는 컴파일 경로에서 제외해 [자료로 보관](../LegacyConverterSources/)했고, 이후 검증기는 NGUI 없이 동작한다.

## 검증 근거

| 검사 | 근거 |
|---|---|
| 제거 후 에디터·런타임 스크립트 컴파일, 전체 자산·참조·애니메이션·셰이더 검사 | [applied-checks.txt](applied-checks.txt), [자산별 목록](applied-assets.txt) |
| 삭제된 GUID의 잔존 참조 검사 | [plugin-removal-checks.json](plugin-removal-checks.json), [GUID 목록](removed-plugin-guids.json) |
| 실제 전환된 토글·슬라이더·InputField 및 동적 목록 회귀 검사 | [control-play-checks.txt](control-play-checks.txt) |
| GraphicRaycaster, 마스크·알파·드래그·휠·트윈·재배치 검사 | [connection-regression-checks.txt](connection-regression-checks.txt) |
| 결과 점수판의 12이닝, X·3X, R/H/E 및 숨김 처리 | [result-scoreboard-checks.txt](result-scoreboard-checks.txt) |
| NGUI 제거 후 로컬 9이닝 경기, 결과·기록 탭, 로비 복귀 | [NGUIFinal 실행 기록](Integration/NGUIFinal/integration-trace.txt), [실제 입력 기록](Integration/NGUIFinal/input-checks.txt) |
| 마지막 기록창 보정: 9개 행의 클리핑 좌표 검사와 화면 확인 | [최종 기록창](result-records.png), [캡처 기록](result-records-capture.txt) |

`NGUIFinal`은 `_Test_Local` 비 RTTS 경기다. 수동 모드에서 Auto/Quick으로 전환하여 실제 경기 로직으로 9이닝을 완료했고 결과는 14:1이었다. 경기 진행 중 `setHoldFastForward` 진입·해제를 3회 호출해 빈 스킬 슬롯 2개가 있어도 연출 종료와 배속 복원이 성공했다. 이 검사는 실제 생산 메서드를 호출했으며 OS의 길게 누르기 입력을 자동화한 것은 아니다. 기록 열기·투수/상대팀 탭·닫기·로비 복귀는 실제 EventSystem 경로로 확인했다.

그 후 발견한 동적 목록 크기와 기록창 아이콘·뷰포트 보정은 별도 Play Mode 회귀 검사와 실제 결과 프리팹 캡처로 검증했다. `Integration/NGUIFinal/records-final.png`는 이 마지막 시각 보정 **전** 실행 증거이며 최신 화면은 `result-records.png`다. 초기 `NGUIComplete` 실행에서 경기 종료 후 호출한 skip 검사는 전제조건 오류로 거부되었고, 이후 `NGUIFinal`의 진행 중 경기에서 정상 검증했다.

## 남은 기존 문제와 검증 한계

- 원래 있던 Missing Script 980개는 임의로 삭제하거나 복원하지 않았다. 오래된 일부 UI의 업무 로직까지 복구했다는 의미는 아니다.
- `tk2dCamera.GetGameViewSize`의 Unity 에디터 API 경고와 누락된 Spine 애니메이션(`DEADBALL2`, `PITCHER_INCONVENENCE_01` 등)은 NGUI 제거 후에도 재현되며 별도 문제로 남아 있다.
- 기존 수동/자동 버튼·일시정지 콜라이더 및 일부 콜백 설정, `_Test_Local` 선수 교체에서 카드 데이터가 없는 문제도 이번 UI 의존성 제거와 별개다.
- 전체 1,050개 자산에 대한 구조 검사와 대표 컨트롤·경기 실행을 수행했다. 모든 보관 UI의 업무 흐름, RTTS 서버 보상 제출, Android 기기 빌드·터치 입력을 전수 검증한 것은 아니다.

## 재검증 및 복구 자료

Unity를 연 상태에서 `Library/UGUIMigration/request.txt`에 아래 명령을 한 번에 하나씩 기록한다. `result.txt`의 완료 결과를 확인하고 다음 명령을 실행한다. Play Mode 검사는 해당 결과 문서가 생성되고 원래 편집 씬으로 복귀할 때까지 기다린다.

```text
remaining:check
remaining:play-controls
remaining:play-connections
remaining:capture-records
integration:check-result
remaining:dependencies
```

삭제된 GUID 검사는 프로젝트 루트에서 `python -B Docs/UIAudit/final_ngui_scan.py`로 재실행할 수 있다. 이 검사는 표준 Python만 사용한다. 변환용 Python 도구의 UnityPy/PyYAML 의존성은 로컬 `Library/UGUIMigration/Python`에 격리되어 있다. 완료된 자산에 변환 `prepare/apply`를 다시 실행하면 후속 보정을 덮어쓸 수 있으므로 재검증에는 위 검사 명령을 사용한다.

원본은 Git과 로컬 `Library/UGUIMigration/Complete/BeforeApply`에, 제거된 플러그인은 `Library/UGUIMigration/Complete/Retired/Assets/NGUI`에 보관했다. Library는 Git 대상이 아니다. 압축된 `inventory-with-settings.json.gz`와 `candidate-plan.json.gz`는 전환 당시 상세 자료이고 `apply-manifest.json`은 중간 적용 내역이므로 최종 파일 해시 목록으로 사용하지 않는다. 이 작업은 커밋·푸시하지 않았다.
