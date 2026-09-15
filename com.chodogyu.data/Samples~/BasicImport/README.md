# Basic Import Sample

ChoDogyu Data Framework & Editor의 기본 CSV/JSON Import와 Runtime 조회 흐름을 확인하기 위한 Sample입니다.

## 포함 내용

- `SampleItemData`
  - `IDataEntry` 구현 예제
  - 문자열, 정수, Enum 필드 사용
- `SampleItemTableAsset`
  - `DataTableAsset<T>` 상속 예제
- `SampleItemLookupExample`
  - `Build()`
  - `Contains()`
  - `TryGet()`
  - `Get()`
  - Runtime ID 조회 예제
- `SampleItems.csv`
  - CSV 자동 매핑 예제
- `SampleItems.json`
  - JSON 자동 매핑 예제

## 1. SampleItemTable Asset 생성

Sample을 Import한 뒤 Unity Project 창에서 다음 메뉴를 사용합니다.

`Create > CDG Data Samples > Sample Item Table`

생성된 `SampleItemTable` Asset을 원하는 위치에 저장합니다.

## 2. Data Framework 창 열기

Unity 상단 메뉴에서 다음 창을 엽니다.

`Tools > ChoDogyu > Data Framework`

`Target Asset`에 앞에서 생성한 `SampleItemTable`을 지정합니다.

## 3. CSV Import

Format을 `Csv`로 선택합니다.

Browse를 눌러 Sample에 포함된 다음 파일을 선택합니다.

`Data/SampleItems.csv`

그 후 다음 순서로 진행합니다.

1. `Create Preview`
2. Validation 결과 확인
3. Diff 결과 확인
4. `Apply Preview`
5. 최종 확인 창에서 `Apply`

정상적으로 적용되면 SampleItemTable에 다음 세 데이터가 저장됩니다.

- `sword_iron`
- `sword_knight`
- `sword_dragon`

## 4. JSON Import

Format을 `Json`으로 변경하고 다음 파일을 선택합니다.

`Data/SampleItems.json`

다시 Preview를 생성하면 기존 Sword 데이터와 새 Staff 데이터의 차이를 Diff에서 확인할 수 있습니다.

Apply하면 JSON 데이터 전체가 새로운 Source of Truth로 적용됩니다.

즉 기존 Sword 데이터에 Staff 데이터가 추가되는 Merge 방식이 아니라,
기존 데이터 전체가 JSON Candidate로 교체됩니다.

## 5. Runtime 조회

CSV 데이터를 Apply한 상태에서 Runtime 조회 예제를 실행할 수 있습니다.

Hierarchy에 빈 GameObject를 생성하고 다음 Component를 추가합니다.

`SampleItemLookupExample`

Inspector에서 다음 값을 지정합니다.

`Table Asset`
- 앞에서 생성한 `SampleItemTable`

`Lookup Id`
- `sword_knight`

Play Mode를 실행하면 Console에서 다음 흐름을 확인할 수 있습니다.

`Build()`
- DataTableAsset의 현재 데이터를 검증합니다.
- 검증에 성공하면 Runtime용 읽기 전용 `DataTable<SampleItemData>`를 생성합니다.

`Contains("sword_knight")`
- 지정한 ID가 존재하는지 확인합니다.

`TryGet("sword_knight", out item)`
- 조회 성공 여부를 bool로 확인하면서 데이터를 가져옵니다.

`Get("sword_knight")`
- `Result<SampleItemData>`를 반환합니다.
- 성공 시 데이터를 사용하고 실패 시 오류 코드와 메시지를 확인할 수 있습니다.

CSV Sample을 적용한 경우 정상적으로 다음 데이터가 조회됩니다.

`ID`
- `sword_knight`

`Display Name`
- `Knight Sword`

`Power`
- `30`

`Rarity`
- `Rare`

Lookup Id를 존재하지 않는 값으로 변경하면 `TryGet`은 false를 반환하고,
`Get`은 실패 Result를 반환합니다.

## CSV Header 규칙

CSV Header는 C# 프로퍼티 이름이 아니라 Unity 직렬화 필드 이름과 정확히 일치해야 합니다.

이 Sample에서는 다음 Header를 사용합니다.

`id,displayName,power,rarity`

필드 이름 비교는 대소문자를 구분합니다.

## ID 규칙

ID는 다음 조건을 만족해야 합니다.

- null 또는 빈 문자열 금지
- 공백만 있는 ID 금지
- 앞뒤 공백 금지
- 중복 ID 금지
- 대소문자 구분

잘못된 데이터는 Preview의 Validation 단계에서 확인할 수 있으며,
유효하지 않은 Preview는 Target Asset에 Apply할 수 없습니다.