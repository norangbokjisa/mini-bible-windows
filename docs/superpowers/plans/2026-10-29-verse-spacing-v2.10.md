# Verse Spacing V2.10 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 같은 장의 절 사이에 글꼴 크기에 비례한 약 반 줄 간격을 적용한 V2.10 Windows 배포본을 만든다.

**Architecture:** `MainForm.Render`는 같은 장의 절 사이에 단일 줄바꿈만 넣고, WinForms `RichTextBox` 문단 서식으로 절 문단 뒤 간격을 지정한다. 장 경계의 제목 구분은 기존 이중 줄바꿈을 유지한다.

**Tech Stack:** C# 13, .NET 9 WinForms, RichEdit paragraph formatting, 자체 테스트 러너

**Spec:** `docs/superpowers/specs/2026-10-29-verse-spacing-design.md`

## Global Constraints

- 절 문단 간격은 현재 본문 글꼴 크기의 약 50%여야 한다.
- 새 장의 굵은 장 제목과 기존 탐색 동작은 유지한다.
- 버전과 배포 파일명은 V2.10으로 올린다.
- 저작권 보호 성경 본문은 포함하지 않는다.

## Review Focus

- 같은 장의 연속 절 사이에 빈 문단이 없어야 한다.
- 기본 12pt 글꼴에서 절 문단 뒤 간격은 120 twips여야 한다.
- 장이 바뀌는 지점의 제목은 계속 굵게 표시되어야 한다.
- 글꼴 크기 변경 후 다시 렌더링하면 간격도 비례해야 한다.
- 마지막 절 뒤의 불필요한 여백이 본문 탐색을 방해하지 않아야 한다.

---

### Task 1: 절 문단 간격 렌더링

**Files:**
- Modify: `tests/MiniBible.Tests/UiLayoutTests.cs`
- Modify: `src/MiniBible.App/MainForm.cs`

**Interfaces:**
- Produces: 같은 장의 절 사이 단일 줄바꿈과 `fontSize * 10` twips의 문단 뒤 간격

- [ ] 같은 장의 두 절에 빈 문단이 없고 기본 글꼴에서 RTF 문단 간격이 120 twips인지 검사하는 실패 테스트를 작성한다.
- [ ] 전체 테스트를 실행해 새 검사가 현재 이중 줄바꿈 때문에 실패하는지 확인한다.
- [ ] `MainForm.Render`와 RichEdit 문단 서식 도우미에 최소 구현을 추가한다.
- [ ] 전체 테스트 32개가 통과하는지 확인한다.

### Task 2: V2.10 버전 및 배포

**Files:**
- Modify: `src/MiniBible.App/MainForm.cs`
- Modify: `src/MiniBible.App/MiniBible.App.csproj`
- Modify: `distribution/README.txt`
- Modify: `distribution/CHANGELOG.txt`
- Modify: `scripts/package.ps1`
- Modify: `tests/package-smoke.ps1`
- Modify: `latest.json`

**Interfaces:**
- Produces: `outputs/mini_bible_windows_v2.10.zip`

- [ ] 제품·어셈블리·안내·패키지 파일명을 V2.10/2.10.0으로 갱신한다.
- [ ] 전체 테스트, 패키징, 압축 해제 실행 검사를 수행한다.
- [ ] ZIP SHA-256과 저작권 본문 미포함을 확인한다.

