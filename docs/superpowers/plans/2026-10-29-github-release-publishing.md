# GitHub Release Publishing Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 공개 GitHub 저장소에 미니 성경 소스와 V2.9 배포본을 안전하게 게시한다.

**Architecture:** 빌드 산출물과 사용자 성경 데이터는 `.gitignore`로 제외하고 소스·테스트·배포 스크립트만 `main`에 게시한다. 전체 Windows ZIP은 `v2.9.0` GitHub Release 자산으로 올리며 `latest.json`이 최신 버전과 다운로드 URL 및 SHA-256을 제공한다.

**Tech Stack:** Git, GitHub, .NET 9, GitHub Releases

**Spec:** 사용자 승인: 공개 저장소, 성경 본문 제외, Release 기반 업데이트 배포

## Global Constraints

- 저장소 이름은 `norangbokjisa/mini-bible-windows`이다.
- `bible.json`, 사용자 설정, `bin`, `obj`, `work`, `outputs`는 커밋하지 않는다.
- Release 자산은 검증된 `mini_bible_windows_v2.9.zip`이다.
- 공개 전 전체 테스트와 패키지 스모크 테스트가 통과해야 한다.

## Review Focus

- 사용자 성경 본문이 추적 파일에 포함되지 않는지 확인한다.
- `latest.json`의 버전, URL, SHA-256이 Release와 일치하는지 확인한다.
- 공개 ZIP이 실행 스모크 테스트를 통과하는지 확인한다.

### Task 1: 공개 저장소용 로컬 Git 구성

- [ ] `.gitignore`, `README.md`, `latest.json`을 추가한다.
- [ ] 추적 파일 목록에 사용자 성경과 빌드 산출물이 없는지 검사한다.
- [ ] 전체 테스트와 패키지 스모크 테스트를 실행한다.
- [ ] 초기 커밋을 만든다.

### Task 2: GitHub 저장소와 Release 게시

- [ ] 공개 `mini-bible-windows` 저장소를 만든다.
- [ ] `main` 소스를 게시한다.
- [ ] `v2.9.0` Release를 만들고 Windows ZIP을 첨부한다.
- [ ] 저장소와 Release 다운로드 링크를 읽기 전용으로 재검증한다.

