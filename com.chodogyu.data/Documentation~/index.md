# ChoDogyu Data Framework & Editor Documentation

## 1. 개요

ChoDogyu Data Framework & Editor는 Unity 프로젝트에서 정적 데이터와 설정 데이터를 정의하고 검증하며, Editor에서 CSV / JSON 데이터를 안전하게 가져오고 Runtime에서 ID 기반으로 조회하기 위한 패키지입니다.

전체 흐름은 다음과 같습니다.

```text
CSV / JSON
→ Editor Import
→ Candidate
→ Validation
→ Diff
→ Preview
→ Safe Apply
→ DataTableAsset<T>
→ Build()
→ DataTable<T>
→ Runtime Lookup
```

Runtime에서는 외부 CSV / JSON을 직접 파싱하지 않습니다.

Editor에서 데이터를 검증하여 Unity Asset으로 확정한 후 Runtime에서는 확정된 데이터를 조회합니다.

---

## 2. Data와 Save의 구분

Data Framework가 관리하는 값은 정적 정의 데이터입니다.

```text
아이템 기본 공격력
아이템 이름
몬스터 기본 HP
스킬 기본 쿨타임
상점 상품 정의
퀘스트 정의
스테이지 설정
```

다음 값은 Save 상태에 해당합니다.

```text
현재 플레이어 HP
보유 아이템 개수
현재 골드
퀘스트 진행도
클리어 여부
장착 장비
```

즉:

```text
Data = 무엇인가를 정의하는 값
Save = 현재 플레이 상태를 기록하는 값
```

Data Framework는 플레이 상태를 저장하지 않습니다.

---

## 3. IDataEntry

모든 데이터 Entry는 `IDataEntry`를 구현합니다.

```csharp
public interface IDataEntry
{
    string Id { get; }
}
```

예:

```csharp
using System;
using CDG.Data;
using UnityEngine;

[Serializable]
public sealed class ItemData : IDataEntry
{
    [SerializeField]
    private string id;

    [SerializeField]
    private int attack;

    public string Id => id;
    public int Attack => attack;
}
```

Framework는 `Id`를 데이터의 고유 식별자로 사용합니다.

---

## 4. ID 규칙

ID는 문자열이며 `StringComparer.Ordinal` 기준으로 비교합니다.

따라서 대소문자를 구분합니다.

```text
item_001
Item_001
```

두 값은 서로 다른 ID입니다.

허용하지 않는 값:

```text
null
""
" "
" item_001"
"item_001 "
```

Framework는 ID를 자동으로 `Trim()`하지 않습니다.

원본의 잘못된 값을 조용히 수정하지 않고 Validation 실패로 처리합니다.

---

## 5. DataTable<T>

`DataTable<T>`는 Runtime에서 사용하는 읽기 전용 데이터 테이블입니다.

```csharp
Result<DataTable<ItemData>> result = DataTable<ItemData>.Create(entries);
```

생성 과정에서 다음을 검사합니다.

```text
source null
Entry null
잘못된 ID
중복 ID
```

성공적으로 생성된 테이블은 데이터 추가 / 제거 API를 제공하지 않습니다.

입력 순서를 유지하며 ID 조회를 위한 내부 Dictionary를 사용합니다.

---

## 6. Contains

```csharp
bool exists = table.Contains("item_001");
```

ID가 존재하면 `true`입니다.

유효하지 않은 ID 또는 존재하지 않는 ID는 `false`입니다.

---

## 7. TryGet

```csharp
if (table.TryGet("item_001", out ItemData item))
{
    // item 사용
}
```

조회 성공 여부만 필요한 경우 사용합니다.

유효하지 않거나 존재하지 않는 ID는 `false`입니다.

---

## 8. Get

```csharp
Result<ItemData> result = table.Get("item_001");
```

실패 이유가 필요한 경우 사용합니다.

```text
잘못된 ID
→ DATA_INVALID_ID

존재하지 않는 ID
→ DATA_NOT_FOUND
```

---

## 9. DataTableAsset<T>

`DataTableAsset<T>`는 Unity ScriptableObject로 데이터 Entry를 저장하는 기반 클래스입니다.

```csharp
using CDG.Data;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemTable", menuName = "Game Data/Item Table")]
public sealed class ItemTableAsset : DataTableAsset<ItemData>
{
}
```

주요 API:

```text
Count
Entries
Validate()
Build()
```

`Entries`는 외부에서 항목을 추가하거나 제거할 수 없는 읽기 전용 View입니다.

---

## 10. Validate

```csharp
DataValidationReport report = asset.Validate();
```

현재 Asset의 전체 Entry를 검사합니다.

Validation 자체는 Asset 데이터를 수정하지 않습니다.

문제는 `DataValidationIssue`를 통해 Report에 기록됩니다.

---

## 11. Build

```csharp
Result<DataTable<ItemData>> result = asset.Build();
```

흐름:

```text
Asset Entries
→ Validate
→ 실패 시 Result Failure
→ 성공 시 DataTable<T> 생성
```

Runtime에서는 일반적으로 한 번 Build한 `DataTable<T>`를 보관하고 ID 기반으로 조회합니다.

---

## 12. Editor Import

Data Framework EditorWindow:

```text
Tools
→ ChoDogyu
→ Data Framework
```

Import 흐름:

```text
Source 선택
→ Parsing / Mapping
→ Candidate
→ Validation
→ Diff
→ Preview
→ 사용자 확인
→ Apply
```

Preview 단계에서는 Target Asset이 변경되지 않습니다.

---

## 13. CSV Import

예:

```csv
id,name,attack
item_001,Sword,10
item_002,Axe,20
```

첫 번째 행은 Header입니다.

Header는 C# Property 이름이 아니라 Unity 직렬화 필드 이름과 정확히 일치해야 합니다.

대소문자를 구분합니다.

지원하는 기본 타입:

```text
string
bool
enum
sbyte
byte
short
ushort
int
uint
long
ulong
float
double
```

숫자 변환은 `InvariantCulture`를 사용합니다.

```text
1.5
```

처럼 소수점 `.`을 사용합니다.

숫자와 bool 값의 불필요한 앞뒤 공백은 허용하지 않습니다.

문자열의 공백은 데이터 자체로 유지합니다.

---

## 14. CSV와 복합 데이터

CSV 자동 Mapping은 기본 Scalar 데이터를 위한 기능입니다.

다음 구조는 JSON 사용을 권장합니다.

```text
Nested Serializable Class
Nested Struct
Array
복합 Object
```

v1.0에서는 CSV를 위한 별도의 복합 데이터 표현 문법을 제공하지 않습니다.

---

## 15. JSON Import

JSON Source는 Root Array 형식입니다.

```json
[
  {
    "id": "item_001",
    "attack": 10
  },
  {
    "id": "item_002",
    "attack": 20
  }
]
```

각 배열 요소가 Entry 하나가 됩니다.

Root Object 하나만 사용하는 형식은 v1.0에서 지원하지 않습니다.

---

## 16. Unity Serialization Mapping

자동 Mapping은 Unity 직렬화 대상 필드를 기준으로 합니다.

포함:

```text
public instance field
[SerializeField] private field
[SerializeReference] field
상속된 private [SerializeField]
```

제외:

```text
static
const
readonly
[NonSerialized]
일반 private field
```

상속 계층에 동일한 이름의 직렬화 필드가 중복되면 Mapping을 실패 처리합니다.

---

## 17. JSON 중첩 데이터

JSON에서는 Unity가 직렬화할 수 있는 Nested Serializable 데이터를 사용할 수 있습니다.

```csharp
[Serializable]
public struct Stats
{
    public int attack;
    public int defense;
}
```

```csharp
[SerializeField]
private Stats stats;
```

JSON:

```json
{
  "id": "item_001",
  "stats": {
    "attack": 10,
    "defense": 5
  }
}
```

Array 역시 Unity 직렬화 범위 안에서 사용할 수 있습니다.

---

## 18. Candidate

Parsing과 Mapping이 성공하면 Asset을 즉시 수정하지 않고 `DataImportCandidate<T>`를 생성합니다.

Candidate는 다음 특성을 갖습니다.

```text
Import 순서 유지
외부에서 항목 추가 차단
외부에서 항목 제거 차단
외부에서 항목 교체 차단
```

Entry가 Reference Type인 경우 Entry 객체 내부까지 Immutable하게 만드는 구조는 아닙니다.

이를 보완하기 위해 Apply 과정에서 State Snapshot을 검증합니다.

---

## 19. Validation

Candidate는 Apply 전에 공통 데이터 규칙을 검사합니다.

```text
null Entry
잘못된 ID
중복 ID
```

Validation에 실패한 Candidate는 적용할 수 없습니다.

---

## 20. Diff

현재 Asset과 Candidate를 ID 기준으로 비교합니다.

```text
Added
Removed
Modified
Unchanged
```

Candidate에 존재하는 데이터는 Candidate 순서를 유지합니다.

Candidate에 없어 제거되는 데이터는 기존 Asset 순서대로 Diff 뒤쪽에 배치됩니다.

---

## 21. Reorder

데이터 내용이 같고 순서만 변경된 경우 각 Entry는 `Unchanged`로 판단될 수 있습니다.

하지만 Import는 Candidate 전체를 Source of Truth로 사용하므로 Apply 시 Candidate 순서가 최종 순서가 됩니다.

따라서:

```text
Diff.HasChanges == false
```

이더라도 순서 변경을 적용할 의미가 있을 수 있습니다.

---

## 22. Source of Truth

Import는 Merge 방식이 아닙니다.

현재 데이터:

```text
A
B
C
```

Candidate:

```text
B
D
```

Apply 결과:

```text
B
D
```

Candidate에 없는 `A`, `C`는 제거됩니다.

---

## 23. State Snapshot

Preview 생성 시 Target과 Candidate의 상태를 저장합니다.

주요 추적 대상:

```text
Target Asset Instance
Target Serialized State
Candidate Count
Candidate 순서
Candidate Entry Serialized State
```

Apply 직전에 현재 상태와 Snapshot을 다시 비교합니다.

다르면 기존 Preview를 적용하지 않습니다.

---

## 24. Diff 계산 중 상태 변경

Diff 계산 자체가 진행되는 동안 상태가 변경되는 경우도 감지합니다.

```text
Before Snapshot
→ Diff
→ After Snapshot
→ 상태 비교
```

두 Snapshot이 다르면 정상 Preview 생성을 중단합니다.

---

## 25. Safe Apply

Apply 전에 다음 조건을 확인합니다.

```text
Preview Valid
Diff 존재
State Snapshot 존재
Candidate 현재 Validation 성공
Target / Candidate Snapshot 일치
```

모든 조건을 만족할 때만 Target Entries 전체를 Candidate로 교체합니다.

검증에 실패하면 기존 Target을 유지합니다.

---

## 26. Undo / Redo

Apply 직전에 Unity Undo 시스템에 Target 상태를 등록합니다.

```text
Apply
→ Undo
→ 이전 데이터
→ Redo
→ 적용된 데이터
```

전체 데이터 교체도 Undo / Redo할 수 있습니다.

---

## 27. Error Codes

### DATA_INVALID_ID

잘못된 ID입니다.

```text
null
empty
whitespace
leading whitespace
trailing whitespace
```

### DATA_NOT_FOUND

유효한 ID지만 Entry가 존재하지 않습니다.

### DATA_VALIDATION_FAILED

데이터 규칙 검증에 실패했습니다.

```text
null Entry
duplicate ID
invalid ID
```

### DATA_IMPORT_FAILED

Editor Import 작업을 완료할 수 없는 경우입니다.

```text
잘못된 CSV
잘못된 JSON
자동 Mapping 실패
지원하지 않는 CSV 타입
오래된 Preview
Snapshot 불일치
```

---

## 28. Basic Import Sample

Package Manager에서 `Basic Import` Sample을 Import할 수 있습니다.

포함:

```text
SampleItemData
SampleItemTableAsset
SampleItemLookupExample
SampleItems.csv
SampleItems.json
README.md
```

검증 흐름:

```text
Sample Import
→ SampleItemTable 생성
→ CSV Preview / Apply
→ Runtime Build / Lookup
→ JSON Preview / Apply
→ Undo / Redo
```

---

## 29. v1.0 범위

포함:

```text
IDataEntry
string ID
DataTable<T>
DataTableAsset<T>
Validation
CSV Import
JSON Import
자동 Mapping
Preview
Diff
Safe Apply
Undo / Redo
EditorWindow
Basic Import Sample
```

포함하지 않음:

```text
Runtime CSV Loading
Runtime JSON Loading
Runtime Mutation
Global DataManager
Global Registry
Addressables Integration
Reference 자동 Validation
Hot Reload
Spreadsheet Editor
Code Generation
XLSX
Google Sheets
Auto Fix
Auto Generated ID
```

---

## 30. 테스트

v1.0 기준:

```text
Runtime Tests
96 Passed

Editor Tests
263 Passed

Failed
0
```

Runtime에서는 ID, Validation, DataTable 및 DataTableAsset을 검증합니다.

Editor에서는 CSV / JSON, Mapping, Diff, Preview, Snapshot, Apply, Undo / Redo 및 End-to-End 흐름을 검증합니다.

---

## 31. 설치 검증

새 Unity 6.3 프로젝트에서 Git UPM 설치를 검증했습니다.

```text
Core v1.0.0 설치
→ Data 설치
→ EditorWindow 확인
→ Basic Import Sample Import
→ Sample Script 컴파일
→ CSV Preview / Apply
→ Runtime Build
→ Contains / TryGet / Get
→ DATA_NOT_FOUND
→ JSON 전체 교체
→ Undo / Redo
```

---

## 32. 권장 사용 흐름

```text
IDataEntry 구현
→ DataTableAsset<T> 구현
→ CSV / JSON 준비
→ Preview
→ Validation
→ Diff
→ Apply
→ Runtime Build()
→ DataTable<T> ID Lookup
```

플레이 중 변경되는 데이터는 별도의 Save 시스템으로 분리하는 것을 권장합니다.