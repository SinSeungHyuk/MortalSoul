
작성일 2026-04-04

### 작업 요청
1. 새롭게 개편된 Main 패턴에 따라 기존 Old 스크립트에 있는 매니저들을 적절하게 이 프로젝트의 Core/Manager 폴더로 옮긴다. 
2. Main은 모노 싱글톤 클래스로, 각 매니저 인스턴스를 소유하고 관리하는 중앙 집중형 방식의 핵심 클래스가 된다. 
3. *게임의 내용도 많이 다르고 매니저 클래스 내부의 구현사항도 많이 변하므로 모든 매니저 클래스를 다 한번에 가져올 필요도 없으며 내부 코드도 전부 가져올 필요가 없고 우선 비워둔다. 즉, 최소한 필요한 Manager 몇개만 빈 껍데기를 가져오는 것으로, 기존 네임스페이스 규칙을 생각하여 이제는 Manager라는 네임스페이스가 사라지고 Core로 통일한다.*


### 작업 내용
1. `Assets/02. Scripts/Core/Manager/` 폴더에 빈 껍데기 매니저 7개 생성 (DataManager, AddressableManager, UIManager, SoundManager, ObjectPoolManager, PlayerManager, MonsterManager)
2. 모든 매니저의 네임스페이스를 `Core`로 통일 (기존 `MS.Manager` 제거)
3. 각 매니저는 일반 클래스로 구현 (싱글톤 패턴 제거)
4. `Main.cs`에 7개 매니저 프로퍼티 추가 및 `Awake()`에서 인스턴스 생성하여 중앙 집중 관리 구현


### 특이사항
1. 매니저 내부 구현은 모두 비워둔 상태. 향후 기능별로 채워나갈 예정.
2. 접근 방식: `Main.Instance.XXXManager`를 통해 매니저 사용


---
태그 : #Core #Main패턴 #매니저 #아키텍처개편
