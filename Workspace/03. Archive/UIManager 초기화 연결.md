
작성일 2026-04-06

### 작업 요청
1. `Main`의 자식 오브젝트로 `ViewCanvas`, `PopupCanvas`, `SystemCanvas`가 씬에 **에디터로 미리 배치**되어 있다는 전제 하에, `UIManager`가 이 세 Canvas의 Transform 참조를 보관하도록 초기화 로직을 연결한다. (자식 오브젝트 배치 자체는 작업 범위 밖 — 사용자가 직접 세팅)
2. `UIManager`는 **일반 클래스**로 유지하고, 생성자가 아닌 별도 초기화 메서드 `InitUIManager(Transform root)`를 통해 참조를 주입받는다. `[SerializeField]` 및 인스펙터 연결은 사용하지 않는다 (프로젝트 규칙).
3. `Main.Awake()`에서 `UIManager`를 `new` 한 직후 `UIManager.InitUIManager(transform)`을 호출하여, `FindChildDeep` 기반으로 세 Canvas를 자동으로 찾아 바인딩한다.
4. `ViewCanvas` / `PopupCanvas` / `SystemCanvas`는 `UIManager` 내부에서 `public Transform ... { get; private set; }` 형태의 프로퍼티로 노출한다. (향후 UI 스폰/팝업 스택 등이 이 세 Transform을 기반으로 동작 예정)
5. 씬에 세 Canvas 중 하나라도 누락된 경우 초기화 시점에 명확한 에러 로그를 남긴다 (UIManager는 Main의 핵심 구성요소이므로 조용히 실패하지 않는다).

### 참고 사항
- `TransformExtensions.FindChildDeep`이 이전 프로젝트 Utils에 존재했으므로, 현재 프로젝트에 해당 확장 메서드가 없다면 함께 포함시켜 이식한다. (위치: `Assets/02. Scripts/Utils/TransformExtensions.cs`)
- 현재 `Main.Awake()`에는 `ObjectPoolManager`가 동적으로 자식 오브젝트를 생성하는 패턴이 이미 있으므로, 초기화 순서(매니저 new → Init 호출)는 그 스타일을 따른다.
- 관련 메모: `MortalSoulDoc/04. Assets/메모.md` §1 "Main과 UIManager 초기화"


### 작업 내용
1. `UIManager.cs`에 `ViewCanvas` / `PopupCanvas` / `SystemCanvas` 세 개의 `public Transform { get; private set; }` 프로퍼티 추가.
2. `InitUIManager(Transform root)` 메서드 구현 — `MS.Utils.TransformExtensions.FindChildDeep`으로 이름 기반 자동 탐색 후 바인딩. `root == null` 방어 처리 포함.
3. `Main.Awake()`에서 `UIManager = new UIManager();` 직후 `UIManager.InitUIManager(transform);` 호출 추가. (`ObjectPoolManager.InitObjectPoolManager` 패턴과 동일한 스타일)
4. `TransformExtensions.FindChildDeep`은 이미 `Assets/02. Scripts/Utils/TransformExtensions.cs`에 존재하여 별도 이식 없이 재사용.

### 특이사항
1. Canvas 누락 에러 로그는 사용자 판단으로 간결화(제거) — `FindChildDeep` 실패 시 null 반환만 됨. 이후 UI 스폰 로직에서 null 체크가 자연스럽게 커버할 예정.
2. 실제 `ViewCanvas` / `PopupCanvas` / `SystemCanvas` GameObject는 본 작업 범위 외이며, 사용자가 씬에 직접 배치해야 정상 동작한다.


---
태그 : #UIManager #Main #초기화 #Canvas #아키텍처
