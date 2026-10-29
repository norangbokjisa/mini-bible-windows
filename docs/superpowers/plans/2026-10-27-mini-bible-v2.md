# Windows 초소형 성경 뷰어 V2 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox syntax for tracking.

**Goal:** 권한 있는 CSV를 가져와 3~5절씩 읽을 수 있고 크기·테마·위치를 저장하는 Windows x64 WinForms 휴대용 앱을 만든다.

**Architecture:** .NET 9 WinForms UI와 테스트 가능한 Core 라이브러리를 분리한다. Core가 CSV, 구절 탐색, 설정 정규화·저장을 담당하고 UI는 이를 조합한다. 외부 NuGet 패키지 없이 표준 라이브러리만 사용해 오프라인 빌드와 자체 포함형 배포를 가능하게 한다.

**Tech Stack:** C# 13, .NET 9, WinForms, System.Text.Json, Microsoft.VisualBasic.FileIO.TextFieldParser, 자체 테스트 러너

**Spec:** docs/superpowers/specs/2026-10-27-mini-bible-v2-design.md

## Global Constraints

- 실제 개역개정 본문은 배포 산출물에 포함하지 않는다.
- 기본 창은 340×240px, 표시 절 수는 기본 5이고 허용 범위는 3~5다.
- Windows x64에서 .NET 설치 없이 실행 가능한 ZIP을 만든다.
- CSV 영어 헤더 book,chapter,verse,text와 한글 헤더 권,장,절,본문을 지원한다.
- 설정은 LocalApplicationData에 저장하고 손상 시 기본값으로 복구한다.
- 첫 실행에는 CSV 가져오기 안내가 보여야 한다.

## Review Focus

- 쉼표·따옴표·줄바꿈이 있는 CSV 필드는 Task 1에서 검사한다.
- 중복 또는 불량 CSV가 기존 정상 파일을 덮어쓰지 않는지는 Tasks 1, 4에서 검사한다.
- 다음 묶음이 장과 권 경계를 넘는지는 Task 2에서 검사한다.
- 손상된 설정과 범위 밖 값 정규화는 Task 3에서 검사한다.
- 화면 밖 창 좌표 복구는 Task 4에서 검사한다.

---

### Task 1: 솔루션과 CSV 도메인

**Files:** Create src/MiniBible.Core의 프로젝트와 Verse, BibleData, CsvBibleImporter; tests/MiniBible.Tests의 프로젝트, 러너, CSV 테스트.

**Interfaces:** Verse(string Book, int Chapter, int Number, string Text), BibleData(IReadOnlyList<Verse>), CsvBibleImporter.Parse(TextReader).

- [ ] 영어·한글 헤더, BOM, 인용된 쉼표·따옴표·줄바꿈, 필수 열, 양수 장절, 빈 본문, 중복 참조의 실패 테스트를 작성한다.
- [ ] dotnet run --project tests/MiniBible.Tests를 실행해 생산 타입 부재로 예상 실패를 확인한다.
- [ ] 불변 구절 저장, 최초 등장 권 순서, 엄격한 검증, 한국어 BibleImportException을 구현한다.
- [ ] 전체 테스트 러너가 exit code 0인지 확인하고 체크포인트를 기록한다.

### Task 2: 구절 탐색과 묶음 이동

**Files:** Create src/MiniBible.Core/BibleNavigator.cs, tests/MiniBible.Tests/BibleNavigatorTests.cs.

**Interfaces:** GetBooks(), GetChapters(string), GetVerses(string,int), GetWindow(VerseReference,int), MoveWindow(VerseReference,int,int).

- [ ] 3·5절 묶음, 마지막 짧은 묶음, 이전·다음, 장·권 경계, 잘못된 참조의 실패 테스트를 작성한다.
- [ ] 테스트를 실행해 BibleNavigator 부재로 예상 실패를 확인한다.
- [ ] 정규 순서 목록 기반 탐색과 존재하는 참조로의 안전한 보정을 구현한다.
- [ ] 전체 테스트 통과를 확인하고 체크포인트를 기록한다.

### Task 3: 설정 모델과 영속성

**Files:** Create src/MiniBible.Core/AppSettings.cs, SettingsStore.cs, tests/MiniBible.Tests/SettingsTests.cs.

**Interfaces:** ThemeMode(System, Light, Dark), AppSettings.Normalize(), SettingsStore.Load(string), Save(string, AppSettings).

- [ ] 기본 340×240·5절·System, 크기·글꼴 경계, JSON 왕복, 파일 없음, 손상 JSON, 원자적 교체의 실패 테스트를 작성한다.
- [ ] 테스트를 실행해 설정 타입 부재로 예상 실패를 확인한다.
- [ ] 설정 정규화와 임시 파일 후 교체 저장, 안전한 기본값 복구를 구현한다.
- [ ] 전체 테스트 통과를 확인하고 체크포인트를 기록한다.

### Task 4: WinForms UI와 가져오기

**Files:** Create src/MiniBible.App 프로젝트, Program, MainForm, SettingsForm, ThemePalette, AppPaths, ImportService; 관련 서비스 테스트.

**Interfaces:** 첫 실행 가져오기, 주 탐색, 항상 위, 설정 창, 테마, 상태 저장, ImportService.ValidateAndInstall(source,destination).

- [ ] 정상 CSV만 설치되고 불량 CSV는 기존 파일을 보존하며 화면 밖 위치는 주 화면으로 복구되는 실패 테스트를 작성한다.
- [ ] 테스트를 실행해 도우미 부재로 예상 실패를 확인한다.
- [ ] 가져오기와 창 위치 도우미를 구현하고 테스트를 통과시킨다.
- [ ] 반응형 상단 선택기, 본문, 이전·다음, 범위, 항상 위, 설정 버튼을 구현한다.
- [ ] CSV·크기·3~5절·글꼴·System/Light/Dark·기본값·저장/취소 설정 창을 구현한다.
- [ ] 시작 로드, 첫 실행 안내, 종료 저장, DPI, 모니터 복구, 한국어 오류를 연결한다.
- [ ] Release 빌드가 0 warnings, 0 errors이고 전체 테스트가 통과하는지 확인한다.

### Task 5: 안내, 배포, 실사용 검증

**Files:** Create distribution/README.txt, fictional sample_bible.csv, scripts/package.ps1, tests/package-smoke.ps1, outputs/mini_bible_windows_v2.zip.

**Interfaces:** Release 앱을 받아 실행 파일, 안내, 가상 예제만 든 자체 포함형 Windows x64 ZIP을 만든다.

- [ ] 비성경 가상 5절 샘플과 CSV 형식·가져오기·설정·저작권 안내를 작성한다.
- [ ] ZIP 필수 파일, 새 폴더 압축 해제, smoke-test 실행, 저작권 본문 미포함을 검사하는 실패 테스트를 작성·실행한다.
- [ ] dotnet publish -c Release -r win-x64 --self-contained true와 Compress-Archive 기반 package.ps1을 구현한다.
- [ ] 전체 테스트, Release 빌드, 패키징, 압축 해제 smoke test를 차례로 통과시킨다.
- [ ] 추출한 실행 파일에서 340×240, CSV 가져오기, 3/5절, 경계 이동, 항상 위, 크기·글꼴·세 테마, 재시작 복원을 확인한다.
- [ ] ZIP 내용과 SHA-256을 확인하고 outputs/mini_bible_windows_v2.zip만 전달한다.

## Self-review

- Spec coverage: 목표, 제외, 데이터, 저장, 테마, 크기, ZIP 검증이 Tasks 1~5에 연결된다.
- Step scan: 각 기능은 실패 테스트, 예상 실패 확인, 최소 구현, 전체 검증 순서다.
- Type consistency: BibleData, BibleNavigator, AppSettings, SettingsStore, ImportService는 한 번 정의해 하위 작업이 소비한다.
- Review focus: 다섯 위험 모두 자동 테스트에 포함된다.
- Proportion: UI는 행동 기준만 정하고 구현 본문은 실행 단계에 맡긴다.
