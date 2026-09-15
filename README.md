\# ChoDogyu Data Framework \& Editor



Unity 프로젝트에서 정적 데이터와 설정 데이터를 정의, 검증, 가져오기 및 조회하기 위한 범용 Data Framework입니다.



Runtime에서는 `IDataEntry`, `DataTable<T>`, `DataTableAsset<T>`를 중심으로 읽기 전용 데이터 조회를 제공하고, Editor에서는 CSV / JSON 데이터를 Preview, Validation, Diff 과정을 거쳐 안전하게 Unity Asset에 적용할 수 있습니다.



특정 게임이나 장르에 종속되지 않도록 구성했으며 Unity Package Manager를 통해 독립적으로 설치할 수 있습니다.



\---



\## 주요 기능



\### Runtime



\- `IDataEntry` 기반 데이터 정의

\- 문자열 ID 및 ID Validation

\- 대소문자를 구분하는 ID 조회

\- `DataTable<T>`

\- `DataTableAsset<T>`

\- `Contains()`

\- `TryGet()`

\- `Get()`

\- Validation Report

\- 읽기 전용 Runtime DataTable 생성



\### Editor



\- CSV Import

\- JSON Import

\- Unity 직렬화 필드 기반 자동 Mapping

\- Import Candidate Validation

\- Added / Removed / Modified / Unchanged Diff

\- Preview

\- Source of Truth 전체 교체

\- State Snapshot

\- 오래된 Preview 적용 차단

\- Safe Apply

\- Undo / Redo

\- Data Framework EditorWindow



\### Sample



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



\---



\## 저장소 구조



```text

ChoDogyuData/

├─ DataDevelopment/

│  └─ 패키지 개발 및 검증용 Unity 프로젝트

│

├─ com.chodogyu.data/

│  ├─ Runtime/

│  ├─ Editor/

│  ├─ Tests/

│  ├─ Samples\~/

│  ├─ Documentation\~/

│  ├─ package.json

│  ├─ README.md

│  └─ CHANGELOG.md

│

├─ .gitignore

└─ README.md

```



\### DataDevelopment



Data Framework의 개발, 테스트 및 통합 검증을 위한 Unity 프로젝트입니다.



실제 UPM 배포 대상에는 포함되지 않습니다.



\### com.chodogyu.data



실제 배포하는 Unity Package Manager 패키지입니다.



다른 Unity 프로젝트에서는 이 폴더를 Git UPM 패키지로 설치하여 사용할 수 있습니다.



\---



\## 요구 사항



\- Unity 6.3 이상

\- ChoDogyu Core 1.0.0



개발 및 검증 환경:



```text

Unity 6.3 LTS

6000.3.9f1

```



Data Framework는 `CDG.Core.Results`의 Result 계열 타입을 사용합니다.



따라서 ChoDogyu Core를 먼저 설치해야 합니다.



\---



\## 설치



\### 1. ChoDogyu Core 설치



Unity Package Manager에서 다음 Git URL을 설치합니다.



```text

https://github.com/ChoDoGyu/ChoDogyuCore.git?path=/com.chodogyu.core#v1.0.0

```



\### 2. ChoDogyu Data Framework 설치



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



Core를 먼저 설치한 뒤 Data Framework URL을 입력합니다.



\---



\## 기본 구조



데이터 Entry는 `IDataEntry`를 구현합니다.



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

&#x20;   private string displayName;



&#x20;   \[SerializeField]

&#x20;   private int attack;



&#x20;   public string Id => id;

&#x20;   public string DisplayName => displayName;

&#x20;   public int Attack => attack;

}

```



Unity Asset으로 저장하려면 `DataTableAsset<T>`를 상속합니다.



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



\---



\## Runtime 사용



Asset 데이터를 Runtime Table로 생성합니다.



```csharp

Result<DataTable<ItemData>> result =

&#x20;   itemTableAsset.Build();

```



성공한 경우:



```csharp

DataTable<ItemData> table =

&#x20;   result.Value;

```



ID 존재 여부 확인:



```csharp

bool exists =

&#x20;   table.Contains("sword\_iron");

```



bool 기반 조회:



```csharp

if (table.TryGet(

&#x20;   "sword\_iron",

&#x20;   out ItemData item))

{

&#x20;   // item 사용

}

```



Result 기반 조회:



```csharp

Result<ItemData> getResult =

&#x20;   table.Get("sword\_iron");

```



\---



\## Editor Import



Unity 상단 메뉴:



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



Preview 단계에서는 Target Asset을 수정하지 않습니다.



\---



\## Import 정책



Data Framework의 Import는 Merge가 아닙니다.



Candidate 전체를 새로운 Source of Truth로 사용합니다.



예:



```text

현재 Asset



A

B

C

```



외부 Source:



```text

B

D

```



Apply 결과:



```text

B

D

```



Candidate에 없는 기존 데이터는 제거됩니다.



\---



\## Safe Apply



Preview 생성 시 Target Asset과 Candidate 상태를 Snapshot으로 저장합니다.



Apply 직전에 현재 상태와 비교하여 다음과 같은 상황을 차단합니다.



```text

Preview 이후 Target 변경

Preview 이후 Candidate 변경

다른 Target에 Preview 적용

Diff 계산 중 상태 변경

```



모든 검증에 성공한 경우에만 Candidate 전체를 Target에 적용합니다.



Apply 작업은 Unity Undo / Redo를 지원합니다.



\---



\## ID 정책



ID는 문자열을 사용하며 대소문자를 구분합니다.



```text

item\_sword

Item\_Sword

```



위 두 ID는 서로 다른 ID입니다.



다음 ID는 허용하지 않습니다.



```text

null

""

"   "

" item\_001"

"item\_001 "

```



ID를 자동으로 Trim하지 않습니다.



\---



\## Error Codes



```text

DATA\_INVALID\_ID

DATA\_NOT\_FOUND

DATA\_VALIDATION\_FAILED

DATA\_IMPORT\_FAILED

```



`DATA\_INVALID\_ID`



\- 잘못된 ID



`DATA\_NOT\_FOUND`



\- 유효한 ID지만 데이터가 존재하지 않음



`DATA\_VALIDATION\_FAILED`



\- 데이터 구조 또는 항목 검증 실패



`DATA\_IMPORT\_FAILED`



\- CSV / JSON Import, Mapping, Preview 또는 Apply 실패



\---



\## 테스트



Unity Test Framework를 사용하여 Runtime 및 Editor 기능을 검증했습니다.



v1.0 기준:



```text

Runtime

96 Passed



Editor

263 Passed



Failed

0

```



검증 범위에는 다음이 포함됩니다.



\- ID 규칙

\- DataTable

\- DataTableAsset

\- Validation

\- CSV Parsing

\- CSV 자동 Mapping

\- CSV 값 변환

\- JSON Parsing

\- JSON 자동 Mapping

\- Unity Serialization Schema

\- Diff

\- Preview

\- State Snapshot

\- Safe Apply

\- Undo / Redo

\- End-to-End Import

\- 대량 데이터 및 순서 안정성



\---



\## UPM 설치 검증



완전히 새로운 Unity 6.3 프로젝트에서 실제 Git UPM 설치를 검증했습니다.



```text

Core v1.0.0 설치

→ 성공



Data Framework 설치

→ 성공



EditorWindow 실행

→ 성공



Basic Import Sample Import

→ 성공



CSV Preview / Apply

→ 성공



Runtime Build / Lookup

→ 성공



JSON Source of Truth 교체

→ 성공



Undo / Redo

→ 성공

```



개발 프로젝트의 로컬 경로에 의존하지 않고 Git UPM 패키지로 사용할 수 있음을 확인했습니다.



\---



\## v1.0 범위



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



플레이 중 변경되는 데이터는 이후 별도의 Save 시스템이 담당하는 것을 전제로 합니다.



\---



\## 버전



현재 패키지 버전:



```text

v1.0.0

```



주요 변경 사항:



```text

com.chodogyu.data/CHANGELOG.md

```



패키지 기본 사용법:



```text

com.chodogyu.data/README.md

```



세부 설계 및 사용 규칙:



```text

com.chodogyu.data/Documentation\~/index.md

```



\---



\## License



별도의 라이선스 정책이 지정되지 않은 경우 저장소의 라이선스 정책을 따릅니다.

