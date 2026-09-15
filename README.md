# ChoDogyu Data Framework & Editor

Unity 프로젝트에서 정적 데이터와 설정 데이터를 정의, 검증, 가져오기 및 조회하기 위한 범용 Data Framework입니다.

Runtime에서는 `IDataEntry`, `DataTable<T>`, `DataTableAsset<T>`를 중심으로 읽기 전용 데이터 조회를 제공하고, Editor에서는 CSV / JSON 데이터를 Preview, Validation, Diff 과정을 거쳐 안전하게 Unity Asset에 적용할 수 있습니다.

특정 게임이나 장르에 종속되지 않도록 구성했으며 Unity Package Manager를 통해 독립적으로 설치할 수 있습니다.

---

## 주요 기능

### Runtime

- `IDataEntry` 기반 데이터 정의
- 문자열 ID 및 ID Validation
- 대소문자를 구분하는 ID 조회
- `DataTable<T>`
- `DataTableAsset<T>`
- `Contains()`
- `TryGet()`
- `Get()`
- Validation Report
- 읽기 전용 Runtime DataTable 생성

### Editor

- CSV Import
- JSON Import
- Unity 직렬화 필드 기반 자동 Mapping
- Import Candidate Validation
- Added / Removed / Modified / Unchanged Diff
- Preview
- Source of Truth 전체 교체
- State Snapshot
- 오래된 Preview 적용 차단
- Safe Apply
- Undo / Redo
- Data Framework EditorWindow

### Sample

Package Manager에서 `Basic Import` Sample을 Import할 수 있습니다.

Sample에서는 다음 흐름을 확인할 수 있습니다.

```text
DataTableAsset 생성
→ CSV Preview
→ Validation
→ Diff
→ Apply
→ Runtime Build
→ Contains / TryGet / Get
→ JSON 전체 교체
→ Undo / Redo
```

---

## 저장소 구조

```text
ChoDogyuData/
├─ DataDevelopment/
│  └─ 패키지 개발 및 검증용 Unity 프로젝트
│
├─ com.chodogyu.data/
│  ├─ Runtime/
│  ├─ Editor/
│  ├─ Tests/
│  ├─ Samples~/
│  ├─ Documentation~/
│  ├─ package.json
│  ├─ README.md
│  └─ CHANGELOG.md
│
├─ .gitignore
└─ README.md
```

### DataDevelopment

Data Framework의 개발, 테스트 및 통합 검증을 위한 Unity 프로젝트입니다.

실제 UPM 배포 대상에는 포함되지 않습니다.

### com.chodogyu.data

실제 배포하는 Unity Package Manager 패키지입니다.

다른 Unity 프로젝트에서는 이 폴더를 Git UPM 패키지로 설치하여 사용할 수 있습니다.

---

## 요구 사항

- Unity 6.3 이상
- ChoDogyu Core 1.0.0

개발 및 검증 환경:

```text
Unity 6.3 LTS
6000.3.9f1
```

Data Framework는 `CDG.Core.Results`의 Result 계열 타입을 사용합니다.

따라서 ChoDogyu Core를 먼저 설치해야 합니다.

---

## 설치

### 1. ChoDogyu Core 설치

```text
https://github.com/ChoDoGyu/ChoDogyuCore.git?path=/com.chodogyu.core#v1.0.0
```

### 2. ChoDogyu Data Framework 설치

```text
https://github.com/ChoDoGyu/ChoDogyuData.git?path=/com.chodogyu.data#v1.0.0
```

Unity에서:

```text
Window
→ Package Management
→ Package Manager
→ Install package from git URL...
```

Core를 먼저 설치한 뒤 Data Framework를 설치합니다.

---

## 기본 구조

데이터 Entry는 `IDataEntry`를 구현합니다.

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

Unity Asset으로 저장하려면 `DataTableAsset<T>`를 상속합니다.

```csharp
using CDG.Data;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemTable", menuName = "Game Data/Item Table")]
public sealed class ItemTableAsset : DataTableAsset<ItemData>
{
}
```

---

## Runtime 사용

```csharp
Result<DataTable<ItemData>> result = itemTableAsset.Build();

if (result.IsFailure)
{
    Debug.LogError($"{result.Error.Code}: {result.Error.Message}");
    return;
}

DataTable<ItemData> table = result.Value;
```

존재 여부:

```csharp
bool exists = table.Contains("sword_iron");
```

bool 기반 조회:

```csharp
if (table.TryGet("sword_iron", out ItemData item))
{
    Debug.Log(item.DisplayName);
}
```

Result 기반 조회:

```csharp
Result<ItemData> result = table.Get("sword_iron");
```

---

## Editor Import

```text
Tools
→ ChoDogyu
→ Data Framework
```

기본 흐름:

```text
Target Asset 선택
→ CSV / JSON 선택
→ Source File 선택
→ Create Preview
→ Validation 확인
→ Diff 확인
→ Apply Preview
→ 최종 확인
→ Apply
```

Preview만 생성한 상태에서는 Target Asset을 수정하지 않습니다.

---

## Import 정책

Import는 Merge 방식이 아닙니다.

Candidate 전체가 새로운 Source of Truth가 됩니다.

```text
현재 Asset
A
B
C

Candidate
B
D

Apply 결과
B
D
```

Candidate에 없는 기존 데이터는 제거됩니다.

---

## Safe Apply

Preview 생성 시 Target Asset과 Candidate의 상태를 Snapshot으로 보관합니다.

Apply 직전에 상태를 다시 비교하여 다음 상황을 차단합니다.

```text
Preview 이후 Target 변경
Preview 이후 Candidate 변경
다른 Target에 Preview 적용
Diff 계산 중 상태 변경
```

모든 검증에 성공한 경우에만 Candidate를 적용합니다.

Apply는 Unity Undo / Redo를 지원합니다.

---

## ID 정책

ID는 문자열이며 대소문자를 구분합니다.

```text
item_sword
Item_Sword
```

두 값은 서로 다른 ID입니다.

다음 값은 허용하지 않습니다.

```text
null
""
"   "
" item_001"
"item_001 "
```

ID를 자동으로 Trim하지 않습니다.

---

## Error Codes

```text
DATA_INVALID_ID
DATA_NOT_FOUND
DATA_VALIDATION_FAILED
DATA_IMPORT_FAILED
```

- `DATA_INVALID_ID`: 잘못된 ID
- `DATA_NOT_FOUND`: 유효한 ID지만 데이터가 존재하지 않음
- `DATA_VALIDATION_FAILED`: 데이터 검증 실패
- `DATA_IMPORT_FAILED`: Import, Mapping, Preview 또는 Apply 실패

---

## 테스트

Unity Test Framework 기반으로 검증했습니다.

```text
Runtime
96 Passed

Editor
263 Passed

Failed
0
```

---

## UPM 설치 검증

완전히 새로운 Unity 6.3 프로젝트에서 다음 흐름을 검증했습니다.

```text
Core v1.0.0 설치
→ Data Framework 설치
→ EditorWindow 실행
→ Basic Import Sample Import
→ CSV Preview / Apply
→ Runtime Build / Lookup
→ JSON Source of Truth 교체
→ Undo / Redo
```

---

## v1.0 범위

포함:

```text
IDataEntry
DataTable<T>
DataTableAsset<T>
Validation
CSV Import
JSON Import
자동 Mapping
Preview
Diff
State Snapshot
Safe Apply
Undo / Redo
EditorWindow
Basic Import Sample
```

포함하지 않음:

```text
Runtime CSV / JSON Loading
Runtime Mutation
Global DataManager
Global Registry
Addressables Integration
Hot Reload
Spreadsheet Editor
Code Generation
XLSX
Google Sheets
Auto Fix
Auto Generated ID
```

플레이 중 변경되는 데이터는 별도의 Save 시스템이 담당하는 것을 전제로 합니다.

---

## 버전

현재 패키지 버전:

```text
v1.0.0
```

주요 변경 사항:

```text
com.chodogyu.data/CHANGELOG.md
```

패키지 사용법:

```text
com.chodogyu.data/README.md
```

세부 문서:

```text
com.chodogyu.data/Documentation~/index.md
```