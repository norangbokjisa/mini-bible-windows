# 미니 성경 Windows

Windows에서 작은 창으로 성경 본문을 읽는 WinForms 뷰어입니다.

## 주요 기능

- 구약·신약 및 성경 권 선택
- 장·절 이동과 첫 장 이동
- 1~10절 표시
- 마우스 휠로 한 절씩 이동
- 글꼴·크기·창 크기 설정
- 라이트·다크 테마
- 항상 위 표시
- 마지막 위치와 설정 저장

## 성경 데이터

이 저장소와 배포 파일에는 개역개정 등 저작권 보호 성경 본문이 포함되지 않습니다.
사용자는 본인이 적법하게 사용할 권한이 있는 JSON 또는 CSV 파일을 프로그램 설정에서 연결해야 합니다.

지원 형식은 [배포 안내](distribution/README.txt)를 참고하세요.

## 다운로드

최신 전체 배포본은 GitHub의 **Releases**에서 받을 수 있습니다. 전체 배포본에는 전용 .NET 런타임이 포함되어 있어 별도 설치 없이 `실행.cmd`로 실행할 수 있습니다.

## 개발 및 검사

```powershell
dotnet run --project tests/MiniBible.Tests/MiniBible.Tests.csproj
powershell -ExecutionPolicy Bypass -File scripts/package.ps1
powershell -ExecutionPolicy Bypass -File tests/package-smoke.ps1
```

## 저작권

프로그램 소스와 샘플 데이터만 공개합니다. 성경 본문에 관한 권리는 각 권리자에게 있습니다.

