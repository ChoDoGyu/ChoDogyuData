\# ChoDogyu Data Framework \& Editor Documentation



\## 1. 개요



ChoDogyu Data Framework \& Editor는 Unity 프로젝트에서 정적 데이터와 설정 데이터를 정의하고 검증하며, Editor에서 외부 CSV / JSON 데이터를 안전하게 가져오고 Runtime에서 ID 기반으로 조회하기 위한 패키지입니다.



핵심 구조는 다음과 같습니다.



```text

외부 데이터

CSV / JSON

&#x20;   ↓

Editor Import

&#x20;   ↓

Candidate

&#x20;   ↓

Validation

&#x20;   ↓

Diff

&#x20;   ↓

Preview

&#x20;   ↓

Safe Apply

&#x20;   ↓

DataTableAsset<T>

&#x20;   ↓

Build()

&#x20;   ↓

DataTable<T>

&#x20;   ↓

Runtime Lookup

```



Runtime과 Editor의 책임을 분리합니다.



Runtime에서는 외부 CSV / JSON 파일을 직접 파싱하지 않습니다.



Editor에서 외부 데이터를 검증하고 Unity Asset으로 확정한 뒤 Runtime에서는 확정된 데이터를 사용합니다.



\---



\## 2. Data와 Save 상태의 구분



이 패키지에서 Data는 정적 정의 데이터를 의미합니다.



예:



```text

아이템 기본 공격력

아이템 이름

몬스터 기본 HP

스킬 기본 쿨타임

상점 상품 정의

퀘스트 정의

스테이지 설정

```



반면 다음과 같은 값은 Save Data에 해당합니다.



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

Data

= 무엇인가를 정의하는 값



Save

= 현재 플레이 상태를 기록하는 값

```



Data Framework는 Save 상태를 관리하지 않습니다.



\---



\## 3. IDataEntry



모든 데이터 Entry는 `IDataEntry`를 구현합니다.



```csharp

public interface IDataEntry

{

&#x20;   string Id { get; }

}

```



예:



```csharp

using System;

using CDG.Data;

using UnityEngine;



\[Serializable]

public sealed class ItemData : IDataEntry

{

&#x20;   \[SerializeField]

&#x20;   private string id;



&#x20;   \[SerializeField]

&#x20;   private int attack;



&#x20;   public string Id => id;

&#x20;   public int Attack => attack;

}

```



Framework는 `Id`를 데이터의 고유 식별자로 사용합니다.



\---



\## 4. ID 규칙



ID는 문자열입니다.



비교 방식:



```text

StringComparer.Ordinal

```



따라서 대소문자를 구분합니다.



```text

item\_001

Item\_001

```



은 서로 다른 ID입니다.



유효하지 않은 예:



```text

null

""

" "

" item\_001"

"item\_001 "

```



Framework는 ID를 자동으로 Trim하지 않습니다.



```text

" item\_001 "

```



을:



```text

"item\_001"

```



로 변경하지 않고 잘못된 ID로 판단합니다.



이 정책은 원본 데이터의 실수를 조용히 수정하지 않고 명확하게 발견하기 위한 것입니다.



\---



\## 5. DataTable<T>



`DataTable<T>`는 Runtime에서 사용하는 읽기 전용 데이터 테이블입니다.



생성:



```csharp

Result<DataTable<ItemData>> result =

&#x20;   DataTable<ItemData>.Create(entries);

```



생성 과정에서 다음을 검사합니다.



```text

source null

Entry null

잘못된 ID

중복 ID

```



정상적으로 생성된 테이블은 데이터 추가 / 제거 API를 제공하지 않습니다.



입력 순서를 그대로 유지합니다.



\---



\## 6. Contains



```csharp

bool exists =

&#x20;   table.Contains("item\_001");

```



ID가 존재하면 `true`입니다.



유효하지 않은 ID 또는 존재하지 않는 ID는 `false`입니다.



\---



\## 7. TryGet



```csharp

if (table.TryGet(

&#x20;   "item\_001",

&#x20;   out ItemData item))

{

&#x20;   // item 사용

}

```



존재 여부를 bool 흐름으로 처리할 때 사용합니다.



유효하지 않거나 존재하지 않는 ID는 `false`입니다.



\---



\## 8. Get



```csharp

Result<ItemData> result =

&#x20;   table.Get("item\_001");

```



실패 이유가 필요한 경우 사용합니다.



잘못된 ID:



```text

DATA\_INVALID\_ID

```



존재하지 않는 ID:



```text

DATA\_NOT\_FOUND

```



두 경우를 구분합니다.



\---



\## 9. DataTableAsset<T>



`DataTableAsset<T>`는 Unity ScriptableObject로 데이터 Entry를 저장하는 기반 클래스입니다.



예:



```csharp

using CDG.Data;

using UnityEngine;



\[CreateAssetMenu(

&#x20;   fileName = "ItemTable",

&#x20;   menuName = "Game Data/Item Table")]

public sealed class ItemTableAsset : DataTableAsset<ItemData>

{

}

```



제공 기능:



```text

Count

Entries

Validate()

Build()

```



`Entries`는 외부에서 항목을 추가하거나 제거할 수 없는 Read-Only View입니다.



\---



\## 10. Validate



```csharp

DataValidationReport report =

&#x20;   asset.Validate();

```



현재 Asset에 저장된 전체 Entry를 검증합니다.



Validation은 Asset을 수정하지 않습니다.



발견된 문제는 `DataValidationIssue` 형태로 Report에 저장됩니다.



\---



\## 11. Build



```csharp

Result<DataTable<ItemData>> result =

&#x20;   asset.Build();

```



Build 흐름:



```text

Asset Entries

→ Validate

→ 실패 시 Result Failure

→ 성공 시 DataTable<T> 생성

```



Runtime에서는 `DataTableAsset<T>`에서 직접 반복 검색하기보다 한 번 Build한 `DataTable<T>`를 사용하여 ID 조회를 수행할 수 있습니다.



\---



\## 12. Editor Import 개요



Data Framework EditorWindow:



```text

Tools

→ ChoDogyu

→ Data Framework

```



Import는 다음 단계로 진행됩니다.



```text

외부 Source 선택

→ Parsing / Mapping

→ Candidate 생성

→ Validation

→ Current Asset과 Diff

→ Preview

→ 사용자 확인

→ Apply

```



Preview 단계에서는 Target Asset을 수정하지 않습니다.



\---



\## 13. CSV Import



CSV 기본 예:



```csv

id,name,attack

item\_001,Sword,10

item\_002,Axe,20

```



첫 번째 행은 Header입니다.



Header 이름은 데이터 타입의 Unity 직렬화 필드 이름과 정확히 일치해야 합니다.



비교는 대소문자를 구분합니다.



지원하는 기본 필드 타입:



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



숫자 파싱은 Culture에 의존하지 않도록 InvariantCulture를 사용합니다.



예:



```text

1.5

```



는 시스템 언어와 관계없이 소수점 `.` 형식을 사용합니다.



숫자나 bool 값에는 불필요한 앞뒤 공백을 허용하지 않습니다.



문자열 값의 앞뒤 공백은 데이터 자체로 유지합니다.



\---



\## 14. CSV와 복합 데이터



CSV 자동 매핑은 기본 Scalar 값을 위한 기능입니다.



다음과 같은 복합 구조:



```text

Nested Serializable Class

Nested Struct

Array

복합 Object

```



는 JSON 사용을 권장합니다.



CSV에서 복잡한 Unity 직렬화 구조를 표현하기 위한 별도 문법은 v1.0에서 제공하지 않습니다.



\---



\## 15. JSON Import



JSON Source는 Root Array여야 합니다.



```json

\[

&#x20; {

&#x20;   "id": "item\_001",

&#x20;   "attack": 10

&#x20; },

&#x20; {

&#x20;   "id": "item\_002",

&#x20;   "attack": 20

&#x20; }

]

```



Root Object 하나만 제공하는 형식은 v1.0 Import 규칙에 포함하지 않습니다.



각 Array 요소는 Entry 하나로 처리됩니다.



\---



\## 16. Unity 직렬화 규칙



자동 Mapping은 Unity 직렬화 대상 필드를 기준으로 합니다.



포함 대상:



```text

public instance field

\[SerializeField] private field

\[SerializeReference] field

상속된 private \[SerializeField]

```



제외 대상:



```text

static

const

readonly

\[NonSerialized]

일반 private field

```



상속 계층에서 동일한 이름의 직렬화 필드가 중복되는 경우 자동 Mapping이 모호해질 수 있으므로 Import 실패로 처리합니다.



\---



\## 17. JSON 중첩 데이터



JSON은 Unity가 직렬화할 수 있는 Nested Serializable 데이터를 사용할 수 있습니다.



예:



```csharp

\[Serializable]

public struct Stats

{

&#x20;   public int attack;

&#x20;   public int defense;

}

```



```csharp

\[SerializeField]

private Stats stats;

```



JSON:



```json

{

&#x20; "id": "item\_001",

&#x20; "stats": {

&#x20;   "attack": 10,

&#x20;   "defense": 5

&#x20; }

}

```



Array 역시 Unity 직렬화 범위 안에서 사용할 수 있습니다.



\---



\## 18. Candidate



Parsing과 Mapping이 성공하면 실제 Asset에 바로 반영하지 않고 `DataImportCandidate<T>`를 생성합니다.



Candidate는 가져오기 데이터의 순서를 유지합니다.



외부에서는 Candidate Collection의 항목 추가 / 제거 / 교체를 수행할 수 없습니다.



단 Entry 자체가 Reference Type인 경우 Entry 내부 값까지 강제로 Immutable하게 만드는 구조는 아닙니다.



이 때문에 Apply 안전성을 위해 별도의 State Snapshot 검증을 사용합니다.



\---



\## 19. Validation



Candidate는 Apply 전에 공통 데이터 규칙을 검증합니다.



주요 검증:



```text

null Entry

잘못된 ID

중복 ID

```



검증 실패 Candidate는 Apply할 수 없습니다.



\---



\## 20. Diff



현재 Asset과 Candidate를 ID 기준으로 비교합니다.



결과 종류:



```text

Added

Removed

Modified

Unchanged

```



Candidate에 존재하는 항목은 Candidate 순서를 유지합니다.



Candidate에 존재하지 않아 제거되는 현재 데이터는 기존 Asset 순서대로 Diff 뒤쪽에 배치됩니다.



\---



\## 21. Reorder



동일한 Entry 내용이 단순히 순서만 변경된 경우 항목 자체는 `Unchanged`로 판단될 수 있습니다.



하지만 Import 정책은 Candidate가 전체 Source of Truth이므로 Apply 시 Candidate 순서가 최종 Asset 순서가 됩니다.



따라서:



```text

Diff.HasChanges == false

```



여도 Source 순서 교체를 위해 Apply 자체는 의미가 있을 수 있습니다.



\---



\## 22. Source of Truth



v1.0 Import는 Merge가 아닙니다.



현재:



```text

A

B

C

```



Incoming:



```text

B

D

```



Apply:



```text

B

D

```



Candidate에 없는 A와 C는 제거됩니다.



이 정책을 통해 외부 CSV / JSON 원본과 Asset의 전체 상태를 명확하게 동기화할 수 있습니다.



\---



\## 23. State Snapshot



Preview 생성 시 Target Asset과 Candidate의 상태 Snapshot을 생성합니다.



Snapshot은 다음을 확인합니다.



```text

Target Asset Instance

Target Serialized State

Candidate Count

Candidate 순서

Candidate 각 Entry Serialized State

```



Apply 직전에 현재 상태와 Snapshot을 다시 비교합니다.



다른 경우 기존 Preview를 적용하지 않습니다.



\---



\## 24. Diff 계산 중 상태 변경



Target 또는 Candidate가 Diff 계산 중 변경되는 상황도 감지합니다.



흐름:



```text

Before Snapshot

→ Diff 계산

→ After Snapshot

→ 두 Snapshot 비교

```



상태가 달라졌다면 정상 Preview를 생성하지 않고 Import Failure로 처리합니다.



이는 Comparer 또는 외부 코드가 Diff 과정에서 예상치 못한 부작용을 발생시키는 경우를 차단합니다.



\---



\## 25. Apply



Apply는 다음 조건을 확인합니다.



```text

Preview Valid

Diff 존재

State Snapshot 존재

Candidate 현재 Validation 통과

현재 Target / Candidate가 Snapshot과 동일

```



모든 조건을 만족한 경우에만 Target Entries 전체를 Candidate로 교체합니다.



검증 실패 시 Target은 변경하지 않습니다.



\---



\## 26. Undo / Redo



Apply 직전에 Unity Undo 시스템에 Target Object 전체 상태를 등록합니다.



따라서:



```text

Apply

→ Undo

→ 이전 상태 복원

→ Redo

→ Applied 상태 복원

```



이 가능합니다.



\---



\## 27. Error Codes



\### DATA\_INVALID\_ID



잘못된 ID입니다.



대표 예:



```text

null

empty

whitespace

leading whitespace

trailing whitespace

```



\### DATA\_NOT\_FOUND



ID 자체는 유효하지만 해당 Entry가 테이블에 없습니다.



\### DATA\_VALIDATION\_FAILED



데이터 규칙 검증에 실패했습니다.



대표 예:



```text

null Entry

duplicate ID

invalid ID

```



\### DATA\_IMPORT\_FAILED



Editor Import 작업을 정상적으로 완료하지 못했습니다.



대표 예:



```text

잘못된 CSV

잘못된 JSON

자동 Mapping 불가

지원하지 않는 CSV 필드 타입

오래된 Preview

Snapshot 불일치

```



\---



\## 28. Sample



Package Manager에서 `Basic Import` Sample을 Import할 수 있습니다.



Sample에는 다음이 포함됩니다.



```text

SampleItemData

SampleItemTableAsset

SampleItemLookupExample

SampleItems.csv

SampleItems.json

README.md

```



Sample 사용 흐름:



```text

Sample Import

→ SampleItemTable 생성

→ CSV Preview / Apply

→ Runtime Build / Lookup

→ JSON Preview / Apply

→ Undo / Redo

```



\---



\## 29. v1.0 범위



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

Basic Sample

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



필요 이상의 기능을 기본 패키지에 포함시키지 않고 정적 데이터의 정의, 검증, Import, 조회에 책임을 제한합니다.



\---



\## 30. 테스트



v1.0 기준:



```text

Runtime Tests

96 Passed



Editor Tests

263 Passed



Failed

0

```



Runtime 테스트에서는 데이터 정의, ID, Validation, DataTable 및 DataTableAsset 동작을 검증합니다.



Editor 테스트에서는 CSV / JSON Import, Mapping, Diff, Preview, Snapshot, Apply, Undo / Redo 및 End-to-End 흐름을 검증합니다.



\---



\## 31. 설치 검증



완전히 새로운 Unity 6.3 프로젝트에서 Git UPM 설치를 검증했습니다.



검증 흐름:



```text

Core v1.0.0 설치

→ Data 설치

→ EditorWindow 확인

→ Basic Import Sample Import

→ Sample Script 컴파일

→ CSV Preview / Apply

→ Runtime Build

→ Contains / TryGet / Get

→ DATA\_NOT\_FOUND

→ JSON 전체 교체

→ Undo / Redo

```



최종 Console Error 없이 동작하는 것을 확인했습니다.



\---



\## 32. 권장 사용 흐름



프로젝트의 정적 데이터 타입을 정의합니다.



```text

IDataEntry 구현

```



Asset 타입을 만듭니다.



```text

DataTableAsset<T> 상속

```



Editor에서 외부 원본을 관리합니다.



```text

CSV / JSON

→ Preview

→ Validation

→ Diff

→ Apply

```



Runtime에서:



```text

DataTableAsset.Build()

→ DataTable<T>

→ ID Lookup

```



을 사용합니다.



플레이 중 변경되는 값은 별도의 Save 시스템으로 분리합니다.

