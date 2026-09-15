# ChoDogyu Data Framework & Editor

Unity 프로젝트에서 정적 데이터와 설정 데이터를 일관된 방식으로 정의, 검증, 가져오기 및 조회하기 위한 범용 Data Framework입니다.

Runtime에서는 `IDataEntry`, `DataTable<T>`, `DataTableAsset<T>`를 중심으로 읽기 전용 데이터 조회와 검증을 제공하며, Editor에서는 CSV / JSON 데이터를 Preview, Validation, Diff 과정을 거쳐 안전하게 Asset에 적용할 수 있습니다.

특정 게임이나 장르에 종속되지 않으며 독립적인 Unity Package Manager 패키지로 사용할 수 있도록 구성했습니다.

---

## 주요 기능

### Runtime

- `IDataEntry` 기반 데이터 항목 정의
- 문자열 ID 기반 데이터 식별
- ID 대소문자 구분
- ID 유효성 검증
- 중복 ID 검증
- `DataTable<T>` 읽기 전용 Runtime 데이터 테이블
- `Contains()` 존재 여부 조회
- `TryGet()` 안전한 데이터 조회
- `Get()` Result 기반 데이터 조회
- `DataTableAsset<T>` ScriptableObject 기반 데이터 저장
- `Validate()` 데이터 검증
- `Build()` Runtime DataTable 생성
- Validation Report 및 Issue 제공

### Editor

- CSV Import
- JSON Import
- Unity 직렬화 필드 기반 자동 매핑
- Import Candidate 검증
- 현재 Asset과 Candidate의 Diff 생성
- Added / Removed / Modified / Unchanged 표시
- Preview 단계와 실제 Apply 단계 분리
- Source of Truth 전체 교체 방식
- Preview 이후 Target 변경 감지
- Preview 이후 Candidate 변경 감지
- 안전한 Apply
- Unity Undo / Redo 지원
- Data Framework EditorWindow 제공

---

## 요구 사항

- Unity 6.3 이상
- ChoDogyu Core 1.0.0

개발 및 검증 환경:

```text
Unity 6.3 LTS
6000.3.9f1
```

이 패키지는 `CDG.Core.Results`의 `Result`, `Result<T>`, `ResultError`를 사용하므로 ChoDogyu Core가 필요합니다.

---

## 설치

ChoDogyu Core를 먼저 설치한 뒤 Data Framework를 설치합니다.

### 1. Core 설치

Unity Package Manager에서 다음 Git URL을 설치합니다.

```text
https://github.com/ChoDoGyu/ChoDogyuCore.git?path=/com.chodogyu.core#v1.0.0
```

### 2. Data Framework 설치

```text
https://github.com/ChoDoGyu/ChoDogyuData.git?path=/com.chodogyu.data#v1.0.0
```

Unity에서 다음 경로로 이동합니다.

```text
Window
→ Package Management
→ Package Manager
→ Install package from git URL...
```

위 Git URL을 입력하면 패키지를 설치할 수 있습니다.

---

## 기본 데이터 정의

데이터 항목은 `IDataEntry`를 구현합니다.

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
    private string displayName;

    [SerializeField]
    private int attack;

    public string Id => id;
    public string DisplayName => displayName;
    public int Attack => attack;
}
```

`IDataEntry`가 요구하는 값은 다음 하나입니다.

```csharp
string Id { get; }
```

각 데이터 항목은 유효하고 중복되지 않는 ID를 가져야 합니다.

---

## DataTableAsset 정의

Unity Asset으로 데이터를 저장하려면 `DataTableAsset<T>`를 상속합니다.

```csharp
using CDG.Data;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ItemTable",
    menuName = "Game Data/Item Table")]
public sealed class ItemTableAsset : DataTableAsset<ItemData>
{
}
```

Unity에서 다음 메뉴를 통해 Asset을 생성할 수 있습니다.

```text
Create
→ Game Data
→ Item Table
```

---

## Runtime 조회

`DataTableAsset<T>.Build()`를 호출하면 현재 Asset 데이터를 검증한 뒤 Runtime용 읽기 전용 `DataTable<T>`를 생성합니다.

```csharp
using CDG.Core.Results;
using CDG.Data;
using UnityEngine;

public sealed class ItemDatabase : MonoBehaviour
{
    [SerializeField]
    private ItemTableAsset itemTableAsset;

    private DataTable<ItemData> itemTable;

    private void Awake()
    {
        Result<DataTable<ItemData>> result =
            itemTableAsset.Build();

        if (result.IsFailure)
        {
            Debug.LogError(
                $"{result.Error.Code}: {result.Error.Message}");

            return;
        }

        itemTable = result.Value;
    }
}
```

생성된 `DataTable<T>`는 입력 순서를 유지하며 ID 기반 조회를 제공합니다.

---

## Contains

```csharp
bool exists =
    itemTable.Contains("sword_iron");
```

유효하지 않은 ID 또는 존재하지 않는 ID는 `false`를 반환합니다.

---

## TryGet

```csharp
if (itemTable.TryGet(
    "sword_iron",
    out ItemData item))
{
    Debug.Log(item.DisplayName);
}
```

조회 성공 여부를 bool로 처리하고 싶을 때 사용합니다.

---

## Get

```csharp
Result<ItemData> result =
    itemTable.Get("sword_iron");

if (result.IsSuccess)
{
    ItemData item = result.Value;
}
else
{
    Debug.LogError(
        $"{result.Error.Code}: {result.Error.Message}");
}
```

잘못된 ID와 존재하지 않는 ID를 오류 코드로 구분할 수 있습니다.

---

## ID 규칙

Data Framework의 ID는 문자열을 사용합니다.

다음 값은 유효하지 않습니다.

```text
null
""
"   "
" item_001"
"item_001 "
```

규칙:

- null 금지
- 빈 문자열 금지
- 공백만 있는 문자열 금지
- 앞쪽 공백 금지
- 뒤쪽 공백 금지
- 중복 ID 금지

문자열은 자동으로 `Trim()`하지 않습니다.

또한 ID는 대소문자를 구분합니다.

```text
item_sword
Item_Sword
```

위 두 ID는 서로 다른 ID입니다.

---

## 데이터 검증

`DataTableAsset<T>`에서 직접 Validation을 수행할 수 있습니다.

```csharp
DataValidationReport report =
    itemTableAsset.Validate();
```

주요 검증 대상:

- null Entry
- 잘못된 ID
- 중복 ID

유효하지 않은 데이터가 존재하면 `Build()` 역시 실패합니다.

---

## Data Framework EditorWindow

Unity 상단 메뉴에서 다음 경로로 실행합니다.

```text
Tools
→ ChoDogyu
→ Data Framework
```

기본 Import 흐름:

```text
Target Asset 선택
→ Format 선택
→ CSV 또는 JSON 선택
→ Create Preview
→ Validation 확인
→ Diff 확인
→ Apply Preview
→ 최종 확인
→ Apply
```

Preview 생성만으로는 Target Asset이 수정되지 않습니다.

---

## CSV Import

예:

```csv
id,displayName,attack
sword_iron,Iron Sword,15
sword_knight,Knight Sword,30
```

CSV Header는 C# Property 이름이 아니라 **Unity 직렬화 필드 이름**과 일치해야 합니다.

예를 들어:

```csharp
[SerializeField]
private string displayName;

public string DisplayName => displayName;
```

이라면 CSV Header는:

```text
displayName
```

이어야 합니다.

다음 두 이름은 서로 다릅니다.

```text
displayName
DisplayName
```

Header 이름은 대소문자를 구분합니다.

CSV 자동 변환에서 지원하는 기본 타입:

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

복합 데이터는 JSON Import 사용을 권장합니다.

---

## JSON Import

JSON은 Root Array 형식을 사용합니다.

```json
[
  {
    "id": "sword_iron",
    "displayName": "Iron Sword",
    "attack": 15
  },
  {
    "id": "sword_knight",
    "displayName": "Knight Sword",
    "attack": 30
  }
]
```

각 배열 요소는 하나의 데이터 Entry로 매핑됩니다.

Unity 직렬화 규칙을 따르며 중첩된 Serializable 데이터도 사용할 수 있습니다.

JSON에서 누락된 직렬화 필드는 Unity 기본값을 유지합니다.

---

## Preview와 Diff

Import 데이터가 정상적으로 파싱되면 현재 Target과 Candidate를 비교하여 Diff를 생성합니다.

Diff 상태:

```text
Added
Removed
Modified
Unchanged
```

예:

```text
현재 Asset

item_a
item_b

Import Candidate

item_a
item_c
```

결과:

```text
item_a → Unchanged 또는 Modified
item_c → Added
item_b → Removed
```

---

## Source of Truth 정책

Import는 Merge 방식이 아닙니다.

Import Candidate 전체가 새로운 Source of Truth가 됩니다.

예:

```text
현재 Asset

A
B
C
```

Import:

```text
B
D
```

Apply 결과:

```text
B
D
```

가 됩니다.

다음처럼 되지 않습니다.

```text
A
B
C
D
```

데이터 전체 스냅샷을 외부 원본과 동기화하기 위한 정책입니다.

---

## 안전한 Apply

Preview를 만든 뒤 Target Asset 또는 Candidate 상태가 변경되면 오래된 Preview를 그대로 적용하지 않습니다.

기본 흐름:

```text
Preview 생성
→ Target / Candidate 상태 Snapshot 저장
→ 사용자 확인
→ Apply 직전 상태 재검증
→ 동일한 경우에만 Apply
```

Preview 생성 이후 Target이 변경되었다면 기존 Preview를 다시 생성해야 합니다.

---

## Undo / Redo

정상적인 Apply는 Unity Undo 시스템에 등록됩니다.

```text
Import Apply
→ Undo
→ 이전 데이터 복원
→ Redo
→ Import 데이터 재적용
```

이 기능은 Editor Import 작업에 적용됩니다.

---

## Sample

Package Manager에서 다음 Sample을 Import할 수 있습니다.

```text
Basic Import
```

포함 내용:

- `IDataEntry` 구현 예제
- `DataTableAsset<T>` 구현 예제
- CSV 데이터
- JSON 데이터
- Editor Import 예제
- Runtime Build 예제
- Contains / TryGet / Get 조회 예제

---

## 오류 코드

Data Framework에서 사용하는 주요 오류 코드:

```text
DATA_INVALID_ID
DATA_NOT_FOUND
DATA_VALIDATION_FAILED
DATA_IMPORT_FAILED
```

### DATA_INVALID_ID

잘못된 ID가 사용된 경우입니다.

### DATA_NOT_FOUND

유효한 ID지만 해당 데이터가 존재하지 않는 경우입니다.

### DATA_VALIDATION_FAILED

데이터 구조 또는 항목 검증에 실패한 경우입니다.

### DATA_IMPORT_FAILED

CSV / JSON Import, Mapping, Preview 또는 Apply 과정에서 가져오기 작업을 완료할 수 없는 경우입니다.

---

## 책임 범위

Data Framework가 담당하는 범위:

```text
정적 데이터 정의
ID 규칙
데이터 검증
DataTable 생성
읽기 전용 Runtime 조회
ScriptableObject 기반 데이터 저장
CSV Import
JSON Import
Preview
Diff
안전한 Apply
Undo / Redo
```

Data Framework가 담당하지 않는 범위:

```text
플레이어 진행 상태
현재 HP
인벤토리 보유 수량
퀘스트 진행 상태
런타임 Save Data
서버 동기화
Addressables 로딩
Runtime CSV / JSON Hot Reload
Google Sheets 연동
XLSX 직접 Import
```

플레이 중 변경되는 상태는 별도의 Save 시스템에서 관리하는 것을 전제로 합니다.

---

## 테스트

Unity Test Framework 기반으로 Runtime 및 Editor 기능을 검증했습니다.

v1.0 기준:

```text
Runtime
96 Passed

Editor
263 Passed

Failed
0
```

주요 검증 범위:

- ID 유효성
- DataTable 생성
- Contains / TryGet / Get
- 중복 ID
- DataTableAsset
- Validation
- CSV Parser
- CSV 자동 타입 변환
- JSON Parser
- JSON 자동 Mapping
- 상속된 SerializeField
- Nested Serializable Data
- Diff
- Candidate 순서 보존
- 대량 데이터 Diff
- Preview
- 상태 Snapshot
- 오래된 Preview 차단
- 안전한 Apply
- Undo / Redo
- Editor Import End-to-End

---

## UPM 설치 검증

완전히 새로운 Unity 6.3 프로젝트에서 다음 흐름을 검증했습니다.

```text
ChoDogyu Core v1.0.0 Git 설치
→ 성공

ChoDogyu Data Framework Git 설치
→ 성공

EditorWindow 실행
→ 성공

Basic Import Sample Import
→ 성공

CSV Preview / Apply
→ 성공

Runtime Build / 조회
→ 성공

JSON 전체 교체
→ 성공

Undo / Redo
→ 성공
```

---

## 버전

현재 패키지 버전:

```text
v1.0.0
```

변경 사항은 `CHANGELOG.md`에서 확인할 수 있습니다.

더 자세한 설계 및 사용 규칙은 다음 문서에서 확인할 수 있습니다.

```text
Documentation~/index.md
```