# Changelog

ChoDogyu Data Framework & Editor 패키지의 주요 변경 사항을 기록합니다.

## [1.0.0] - 2026-09-15

첫 정식 배포 버전입니다.

### Added

#### Runtime Data Framework

- `IDataEntry` 데이터 항목 계약 추가
- 문자열 기반 ID 사용
- 대소문자를 구분하는 ID 비교 정책 적용
- null, 빈 문자열, 공백 문자열, 앞뒤 공백 ID 검증
- `DataTable<T>` 읽기 전용 데이터 테이블 추가
- 입력 데이터 순서 보존
- 중복 ID 검증
- `Contains()` 데이터 존재 여부 조회
- `TryGet()` bool 기반 데이터 조회
- `Get()` Result 기반 데이터 조회
- 잘못된 ID와 존재하지 않는 ID 오류 구분

#### DataTableAsset

- `DataTableAsset<T>` ScriptableObject 기반 데이터 저장 구조 추가
- `Count` 및 읽기 전용 `Entries` 제공
- `Validate()` 데이터 검증 기능 추가
- `Build()` Runtime `DataTable<T>` 생성 기능 추가
- 검증 실패 시 Runtime Table 생성 차단
- 검증 후 전체 Entries를 교체하는 구조 추가

#### Validation

- `DataTableValidator`
- `DataValidationReport`
- `DataValidationIssue`
- null Entry 검증
- 잘못된 ID 검증
- 중복 ID 검증
- Validation 과정에서 원본 데이터 비수정

#### Error Codes

- `DATA_INVALID_ID`
- `DATA_NOT_FOUND`
- `DATA_VALIDATION_FAILED`
- `DATA_IMPORT_FAILED`

#### CSV Import

- Editor 전용 CSV Import
- Header 기반 자동 Mapping
- LF / CRLF / CR 개행 지원
- 따옴표 처리
- 따옴표 내부 쉼표 및 개행 지원
- Escaped Quote 처리
- 잘못된 CSV 검증
- Header 중복 검증
- Header 대소문자 구분
- Unity 직렬화 필드 이름 기반 Mapping

#### CSV 값 변환

- `string`
- `bool`
- `enum`
- `sbyte`
- `byte`
- `short`
- `ushort`
- `int`
- `uint`
- `long`
- `ulong`
- `float`
- `double`
- `InvariantCulture` 기반 숫자 변환
- 숫자 및 bool 값의 불필요한 앞뒤 공백 차단
- 문자열 값의 앞뒤 공백 보존
- NaN 및 Infinity 차단
- 정수 Overflow 검증
- 지원하지 않는 CSV 필드 타입 검증

#### JSON Import

- Editor 전용 JSON Import
- Root Array 형식 지원
- JSON Object를 Entry로 Mapping
- Nested Serializable 데이터 지원
- Serializable Struct 지원
- Array 지원
- Escaped String 및 Unicode 처리
- 누락된 직렬화 필드의 Unity 기본값 유지
- 잘못된 JSON 검증

#### Unity Serialization Mapping

- public instance field
- `[SerializeField]` private field
- `[SerializeReference]` field
- 상속된 private `[SerializeField]`
- Base Type부터 Derived Type 순서로 Schema 구성
- `static`, `const`, `readonly`, `[NonSerialized]` 제외
- 일반 private field 제외
- 필드 이름 대소문자 구분
- 상속 계층 중복 필드 이름 차단
- Open Generic Entry 차단
- Abstract Entry 차단
- UnityEngine.Object 기반 Entry 차단
- `[Serializable]`이 아닌 Entry 차단

#### Import Candidate

- `DataImportCandidate<T>` 추가
- Candidate Collection Snapshot 보관
- 외부에서 항목 추가, 제거 및 교체 차단
- 원본 Import 순서 유지

#### Diff

- 현재 Asset과 Candidate의 ID 기반 Diff
- `Added`
- `Removed`
- `Modified`
- `Unchanged`
- Candidate 순서 유지
- Removed 항목의 기존 Asset 순서 유지
- 대소문자가 다른 ID를 서로 다른 데이터로 처리
- 단순 순서 변경 시 내용이 동일한 Entry는 Unchanged 처리
- 대량 데이터 Diff 검증

#### Preview

- 실제 Asset 변경 전 Preview 생성
- Validation 결과 표시
- Diff 결과 표시
- Preview와 Apply 분리
- 유효하지 않은 Candidate의 Apply 차단

#### Source of Truth

- Candidate 전체를 새로운 Source of Truth로 적용
- Merge 방식 미사용
- Candidate에 없는 기존 데이터 제거
- Candidate 순서를 최종 Asset 순서로 적용
- 빈 Candidate 적용 지원

#### State Snapshot

- Preview 생성 시 Target 상태 저장
- Target Asset Instance 추적
- Target 직렬화 상태 추적
- Candidate Count 및 순서 추적
- Candidate Entry 상태 추적
- Preview 이후 Target 변경 감지
- Preview 이후 Candidate 변경 감지
- 다른 Target에 기존 Preview 적용 차단

#### Diff 안정성

- Diff 계산 전 Snapshot 생성
- Diff 계산 후 Snapshot 생성
- Diff 계산 중 Target 변경 감지
- Diff 계산 중 Candidate 변경 감지
- 상태 변경이 발생한 Diff 결과 차단

#### Safe Apply

- Preview Validation 재확인
- Diff 존재 여부 확인
- Snapshot 존재 여부 확인
- Candidate 재검증
- Target 및 Candidate Snapshot 일치 확인
- 모든 검증 후 Entries 교체
- 실패 시 기존 Target 보존
- `EditorUtility.SetDirty()` 처리

#### Undo / Redo

- Apply 직전 Unity Undo 등록
- Undo 지원
- Redo 지원
- 전체 데이터 교체 상태 복원 검증

#### EditorWindow

- `Tools > ChoDogyu > Data Framework`
- Target `DataTableAsset` 선택
- CSV / JSON Format 선택
- Source File 선택
- Preview 생성
- Validation 표시
- Diff 표시
- Added / Removed / Modified / Unchanged 개수 표시
- Apply 가능 상태 표시
- 명시적 Apply
- 최종 확인 Dialog
- Preview 폐기
- EditorWindow Session 상태 관리

#### Sample

- Package Manager용 `Basic Import` Sample
- `SampleItemData`
- `SampleItemTableAsset`
- `SampleItemLookupExample`
- CSV Sample
- JSON Sample
- `Build()` 예제
- `Contains()` 예제
- `TryGet()` 예제
- `Get()` 예제
- Sample README

#### Tests

- Runtime Test 96개 통과
- Editor Test 263개 통과
- ID 규칙 검증
- DataTable 및 DataTableAsset 검증
- Validation 검증
- CSV Parser 및 Mapping 검증
- CSV Scalar 변환 경계 검증
- JSON Parser 및 Mapping 검증
- Unity Serialization Schema 검증
- Diff 검증
- Preview 검증
- State Snapshot 검증
- Safe Apply 검증
- Undo / Redo 검증
- CSV / JSON End-to-End 검증
- 대량 데이터 및 순서 안정성 검증

#### UPM Verification

- 새 Unity 6.3 프로젝트에서 Git UPM 설치 검증
- ChoDogyu Core v1.0.0 설치 후 Data Framework 설치 검증
- EditorWindow 실행 검증
- Basic Import Sample Import 검증
- Sample Script 컴파일 검증
- CSV Preview / Apply 검증
- Runtime Build / 조회 검증
- 존재하지 않는 ID 조회 검증
- JSON Source of Truth 전체 교체 검증
- Undo / Redo 검증

#### Documentation

- 저장소 README 추가
- 패키지 README 추가
- 상세 Documentation 추가
- Basic Import Sample README 추가
- Runtime / Editor 책임 범위 문서화
- Data / Save 책임 분리 문서화
- ID 규칙 문서화
- CSV / JSON Import 규칙 문서화
- Source of Truth 정책 문서화
- Safe Apply 및 State Snapshot 정책 문서화