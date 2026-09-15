\# Changelog



ChoDogyu Data Framework \& Editor 패키지의 주요 변경 사항을 기록합니다.



\## \[1.0.0] - 2026-09-15



첫 정식 배포 버전입니다.



\### Added



\#### Runtime Data Framework



\- `IDataEntry` 데이터 항목 계약 추가

\- 문자열 기반 ID 사용

\- 대소문자를 구분하는 ID 비교 정책 적용

\- null, 빈 문자열, 공백 문자열, 앞뒤 공백 ID 검증

\- `DataTable<T>` 읽기 전용 데이터 테이블 추가

\- 입력 데이터 순서 보존

\- 중복 ID 검증

\- `Contains()` 데이터 존재 여부 조회

\- `TryGet()` bool 기반 데이터 조회

\- `Get()` Result 기반 데이터 조회

\- 잘못된 ID와 존재하지 않는 ID 오류 구분



\#### DataTableAsset



\- `DataTableAsset<T>` ScriptableObject 기반 데이터 저장 구조 추가

\- `Count` 제공

\- 읽기 전용 `Entries` 제공

\- `Validate()` 데이터 검증 기능 추가

\- `Build()` Runtime `DataTable<T>` 생성 기능 추가

\- 검증 실패 시 Runtime Table 생성 차단

\- 내부 데이터 전체 교체 시 검증 후 적용하는 원자적 교체 구조 추가



\#### Validation



\- `DataTableValidator` 추가

\- `DataValidationReport` 추가

\- `DataValidationIssue` 추가

\- null Entry 검증

\- 잘못된 ID 검증

\- 중복 ID 검증

\- 검증 과정에서 원본 데이터 비수정 보장



\#### Error Codes



\- `DATA\_INVALID\_ID` 추가

\- `DATA\_NOT\_FOUND` 추가

\- `DATA\_VALIDATION\_FAILED` 추가

\- `DATA\_IMPORT\_FAILED` 추가



\#### CSV Import



\- Editor 전용 CSV Import 기능 추가

\- CSV Header 기반 자동 필드 매핑

\- 쉼표 구분 필드 파싱

\- LF / CRLF / CR 개행 지원

\- 따옴표 처리 지원

\- 따옴표 내부 쉼표 지원

\- 따옴표 내부 개행 지원

\- Escaped Quote 처리

\- 잘못된 CSV 형식 검증

\- Header 중복 검증

\- Header 대소문자 구분

\- Unity 직렬화 필드 이름 기반 Header 매핑



\#### CSV 값 변환



\- `string` 지원

\- `bool` 지원

\- `enum` 지원

\- `sbyte` 지원

\- `byte` 지원

\- `short` 지원

\- `ushort` 지원

\- `int` 지원

\- `uint` 지원

\- `long` 지원

\- `ulong` 지원

\- `float` 지원

\- `double` 지원

\- 숫자 변환에 `InvariantCulture` 적용

\- 숫자 및 bool 값의 불필요한 앞뒤 공백 차단

\- 문자열 값의 앞뒤 공백 보존

\- NaN 및 Infinity 차단

\- 정수 Overflow 검증

\- 지원하지 않는 CSV 필드 타입 검증



\#### JSON Import



\- Editor 전용 JSON Import 기능 추가

\- Root Array 기반 데이터 가져오기

\- 각 JSON Object를 하나의 Entry로 매핑

\- Nested Serializable 데이터 지원

\- Serializable Struct 지원

\- Array 데이터 지원

\- Escaped String 및 Unicode 데이터 처리

\- 누락된 직렬화 필드의 Unity 기본값 유지

\- 잘못된 JSON 형식 검증



\#### Unity Serialization Mapping



\- public instance field 자동 Mapping

\- `\[SerializeField]` private field 자동 Mapping

\- `\[SerializeReference]` field 인식

\- 상속된 private `\[SerializeField]` 인식

\- Base Type에서 Derived Type 순서로 직렬화 Schema 구성

\- `static` field 제외

\- `const` field 제외

\- `readonly` field 제외

\- `\[NonSerialized]` field 제외

\- 일반 private field 제외

\- 직렬화 필드 이름 대소문자 구분

\- 상속 계층의 중복 직렬화 필드 이름 차단

\- Open Generic Entry 차단

\- Abstract Entry 차단

\- UnityEngine.Object 기반 Entry 차단

\- `\[Serializable]`이 아닌 Entry 차단



\#### Import Candidate



\- 외부 데이터 Parsing 이후 `DataImportCandidate<T>` 생성

\- Candidate Collection 스냅샷 보관

\- 외부에서 Candidate 항목 추가, 제거 및 교체 차단

\- 원본 Import 순서 유지



\#### Diff



\- 현재 Asset과 Candidate의 ID 기반 Diff 계산 추가

\- `Added` 상태 추가

\- `Removed` 상태 추가

\- `Modified` 상태 추가

\- `Unchanged` 상태 추가

\- Candidate 순서 우선 유지

\- Removed 항목의 기존 Asset 순서 유지

\- 대소문자가 다른 ID를 서로 다른 항목으로 처리

\- 단순 순서 변경 시 Entry 내용은 Unchanged로 판단

\- 대량 데이터 Diff 동작 검증



\#### Preview



\- Import 결과를 실제 Asset에 반영하기 전 Preview 생성

\- Candidate Validation 결과 표시

\- Diff 결과 표시

\- Preview 생성만으로 Target Asset이 변경되지 않도록 분리

\- 유효하지 않은 Candidate의 Apply 차단



\#### Source of Truth



\- Import Candidate 전체를 새로운 Source of Truth로 사용하는 정책 적용

\- 기존 Asset과 Candidate를 Merge하지 않음

\- Candidate에 존재하지 않는 기존 데이터 제거

\- Candidate 순서를 최종 Asset 순서로 적용

\- 빈 Candidate 적용 시 Target 데이터 전체 제거 지원



\#### State Snapshot



\- Preview 생성 시 Target Asset 상태 Snapshot 저장

\- Target Asset Instance ID 추적

\- Target Asset 직렬화 상태 추적

\- Candidate Count 추적

\- Candidate 순서 추적

\- Candidate 각 Entry의 직렬화 상태 추적

\- Preview 이후 Target 변경 감지

\- Preview 이후 Candidate 변경 감지

\- 다른 Target에 기존 Preview 적용 방지



\#### Diff 상태 안정성



\- Diff 계산 전 State Snapshot 생성

\- Diff 계산 후 State Snapshot 생성

\- 두 Snapshot 비교

\- Diff 계산 중 Target 상태 변경 감지

\- Diff 계산 중 Candidate 상태 변경 감지

\- 부작용이 발생한 Diff 결과의 Preview 생성 차단



\#### Safe Apply



\- Preview Validation 재확인

\- Diff 존재 여부 확인

\- State Snapshot 존재 여부 확인

\- Candidate 현재 상태 재검증

\- Target 및 Candidate Snapshot 일치 여부 확인

\- 모든 조건 통과 후에만 Target Entries 교체

\- Apply 실패 시 기존 Target 데이터 보존

\- `EditorUtility.SetDirty()` 처리



\#### Undo / Redo



\- Apply 직전 Unity Undo 등록

\- Apply 후 Undo 지원

\- Undo 후 Redo 지원

\- 전체 데이터 교체 상태의 복원 검증



\#### EditorWindow



\- `Tools > ChoDogyu > Data Framework` 메뉴 추가

\- Target `DataTableAsset` 선택

\- CSV / JSON Format 선택

\- 외부 Source File 선택

\- Preview 생성

\- Validation 결과 표시

\- Diff 결과 표시

\- Added / Removed / Modified / Unchanged 개수 표시

\- Apply 가능 상태 표시

\- 명시적인 Apply 버튼 제공

\- 최종 확인 Dialog 제공

\- Preview 폐기 기능 제공

\- EditorWindow Session 상태 관리



\#### Sample



\- Package Manager용 `Basic Import` Sample 추가

\- `SampleItemData` 추가

\- `SampleItemTableAsset` 추가

\- `SampleItemLookupExample` 추가

\- CSV Sample 데이터 추가

\- JSON Sample 데이터 추가

\- Runtime `Build()` 사용 예제 추가

\- `Contains()` 사용 예제 추가

\- `TryGet()` 사용 예제 추가

\- `Get()` 사용 예제 추가

\- Sample 사용 README 추가



\#### Tests



\- Unity Test Framework 기반 Runtime Test 추가

\- Unity Test Framework 기반 Editor Test 추가

\- ID 규칙 검증

\- DataTable 생성 및 조회 검증

\- DataTableAsset 검증

\- Validation 검증

\- CSV Parser 검증

\- CSV 자동 Mapping 검증

\- CSV Scalar 값 변환 경계 검증

\- JSON Parser 검증

\- JSON 자동 Mapping 검증

\- Unity Serialization Schema 검증

\- Diff 검증

\- Preview 검증

\- State Snapshot 검증

\- Safe Apply 검증

\- Undo / Redo 검증

\- CSV / JSON Editor End-to-End 검증

\- 대량 데이터 Diff 및 순서 안정성 검증

\- Runtime Test 96개 통과

\- Editor Test 263개 통과



\#### UPM Verification



\- 완전히 새로운 Unity 6.3 프로젝트에서 Git UPM 설치 검증

\- ChoDogyu Core v1.0.0 설치 후 Data Framework 설치 검증

\- EditorWindow 실행 검증

\- Package Manager Sample 노출 검증

\- Basic Import Sample Import 검증

\- Sample Script 컴파일 검증

\- CSV Preview 및 Apply 검증

\- Runtime Build 및 ID 조회 검증

\- 존재하지 않는 ID 조회 검증

\- JSON Source of Truth 전체 교체 검증

\- Undo / Redo 검증



\#### Documentation



\- 패키지 README 추가

\- 세부 설계 및 사용 문서 추가

\- Basic Import Sample README 추가

\- Runtime과 Editor 책임 범위 문서화

\- Data와 Save 상태의 책임 분리 문서화

\- ID 규칙 문서화

\- CSV / JSON Import 규칙 문서화

\- Source of Truth 정책 문서화

\- Safe Apply 및 State Snapshot 정책 문서화

