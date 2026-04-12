# MortalSoul 전체 코드 리뷰 리포트

**작성일**: 2026-04-11
**작업 완료일**: 2026-04-12
**리뷰 방식**: 4개 도메인 병렬 (`superpowers:code-reviewer` 에이전트)
**리뷰 범위**: `Assets/02. Scripts/` 전체

---

## 작업 완료 기록 (2026-04-12)

리뷰 문서에서 도출된 16개 이슈를 6개 Phase로 나눠 단계별 처리. 각 Phase는 독립 커밋으로 분리하여 리그레션 격리 가능하도록 구성.

### Phase별 커밋 요약

| Phase | 커밋 | 내용 |
|---|---|---|
| 1 | `5c40057` | **전투 데미지 파이프라인 복구 + AttributeSet 통합** (빌드 복구 최우선) |
| 2 | `d012659` | **콤보 레이스 버그 + 벽 끼임 해결** |
| 3 | `8b7bc3f` | **SkillSystem 수명주기/네이밍 정리** |
| 4 | `f1aa220` | **Core 수명주기 정비** (Singleton/Main/DataManager) |
| 5 | `4012788` | **BSC/WSC 대칭 Clear + 구조 정리** |
| 6 | `59824c7` | **Quality/Nit 일괄 정리** |

### Phase 1 상세
- `BSC.InitBSC(owner, attrSet, EWeaponType?)` 단일 시그니처. 플레이어는 3번째 인자 전달(WSC 생성), 몬스터는 생략(WSC = null)
- `BSC.TakeDamage` 구현: 회피 → 치명타 → 약점 → 방어 커브 → 체력 감소 → LifeSteal → OnHit/OnDodged/OnDead 이벤트
- `BattleUtils` 신규 작성 (CalcDefenseStat / CalcEvasionStat / CalcWeaknessAttribute / CalcCriticDamage)
- 크리티컬 공식: `base * CriticMultiple / 100` — CriticMultiple 기본값 150 = 1.5배 (ARPG 표준 컨벤션)
- `OneHandSwordAttack.DoHit` 본체 복구 — `DamageInfo` 생성 후 `target.BSC.TakeDamage` 호출. 크리 굴림은 호출처가 아닌 BSC 내부에서 처리
- `GreatSwordAttack` → `TwoHandSwordAttack` 클래스명 정렬 (enum `EWeaponType.TwoHandSword`와 일치)
- **AttributeSet 통합 리팩터** (보너스 작업): `BaseAttributeSet` / `PlayerAttributeSet` / `MonsterAttributeSet` 3개 클래스를 단일 `AttributeSet`으로 합침. `PlayerAttributeSetSettingData` / `MonsterAttributeSetSettingData`도 단일 `AttributeSetSettingData`로 통합. TakeDamage에서 PlayerAttributeSet 캐스팅 분기 제거, 깔끔한 파이프라인 확보

### Phase 2 상세
- **콤보 WhenAny 레이스 수정**: `WhenAny(combo_ready, Complete)` 호출 전에 두 Task를 변수에 저장 → `combo_ready`로 빠져나온 경우 저장해둔 `completeTask`를 재활용. 기존에는 두 번째 `WaitForAnimCompleteAsync()` 호출이 새 TCS를 만들어, Complete가 그 틈에 이미 지나갔다면 영원히 await가 풀리지 않는 hang 가능성 존재
- **Dash/Jump 상태 전환 레이스 검증**: 현재 구조가 사용자 요구("즉시 공격")와 정합. `OnDashExit`에서 중력 복원, `OnAttackEnter`에서 X 속도 0, Spine `PlayAnimation` 자동 캔슬로 공격 중 대시도 깔끔하게 정리됨 — **수정 불필요**
- **X축 벽 끼임**: 코드 수정이 아닌 `PM_PlayerCharacter.physicsMaterial2D` (Friction=0) 에셋을 플레이어 collider에 적용하는 Unity 표준 방식으로 해결

### Phase 3 상세
- **CancelSkill/CancelAllSkills**가 Cancel 직후 `runningSkillDict`에서도 제거 → 취소 후 동일 프레임 재사용 가능
- **UseSkill.finally 동일성 체크**: `ReferenceEquals(current, cts)` — 새 UseSkill 호출이 같은 키로 이미 새 cts를 꽂았다면 건드리지 않음 (재진입 안전)
- **ClearSSC 이중 Dispose 방어**: Cancel만 트리거, CTS Dispose는 각 `UseSkill.finally`가 전담
- **Pre-use 쿨타임 롤백**: `catch (Exception)` 경로에서 `ResetCooltime()` 호출. `OperationCanceledException`은 롤백 안 함(의도적 캔슬은 쿨타임 유지)
- **BaseSkill 네이밍**: `elapsedCooltime` → `remainCooltime`, `CooltimeRatio` → `CooltimeRemainRatio`. `ResetCooltime()` 추가

### Phase 4 상세
- **MonoSingleton.Awake**: `GetComponent<T>()` → `this as T`. 중복 오브젝트는 `Destroy(gameObject)`로 처리. `OnDestroy`에서 `_instance == this`면 nullify
- **Main.OnDestroy 정리 훅 체인**: `BattleObjectManager.ClearBattleObject` → `ObjectPoolManager.ClearAllPools` → `SoundManager.ClearAllSounds` → `AddressableManager.ReleaseAll` → `DataManager.ReleaseGameData` 순으로 호출
- **DataManager.InitGameData 호출 지점 연결**: 던전 입장 로직이 아직 없으므로 임시로 부팅 직후 호출 + TODO 주석

### Phase 5 상세
- `BSC.ClearBSC` 추가 — SSC/WSC 일괄 정리 + 상태이상 End + 이벤트 구독 해제. `FieldCharacter.OnDestroy`에서 연결
- `WSC.ClearWSC` 추가 — `curAttack` 이벤트 해제 + 리스너 nullify
- `BSC.UpdateStatusEffects` GC 제거 — 매 프레임 `new List<string>()` 할당을 `removeStatusEffectList` 필드 재사용으로 변경
- **PMC ↔ PlayerCharacter 양방향 GetComponent 해소**: PMC가 `GetComponent<PlayerCharacter>()` 제거, `InitController(player, wsc)` 주입 방식으로 단방향화

### Phase 6 상세
- `ObjectPoolManager.Get` Quaternion default 버그 수정 — 오버로드 분리 (명시형 / pos만 / key만)
- `SpineController.SetSkin`: `combinedSkin` 필드 캐시 + `Clear()` 재사용 (매 소울 교체마다 `new Skin()` 할당 제거). 이름도 `SetCombinedSkin` → `SetSkin`으로 간결화
- PMC `isScaleXRight` 로컬 필드 제거, `spineController.IsScaleXRight`로 단일화
- `OnAttackUpdate` dash/jump 체크 중복 → `CheckCurInput()` 재사용
- `TestOneHandAttack`: 불필요한 `async` + `await UniTask.CompletedTask` 제거
- `MathUtils` `/// <summary>` 3개 제거 (CLAUDE.md 규칙 9)
- `Settings.cs` / `MSStateMachine.cs` 인코딩 손상된 한글 주석 정리

### 특이사항

1. **AttributeSet 통합 범위 확대**: 원래 Phase 1 계획은 `BSC.TakeDamage` 내부에서 `as PlayerAttributeSet` 캐스팅으로 플레이어/몬스터 분기할 예정이었으나, 작업 도중 사용자 제안으로 `BaseAttributeSet`/`PlayerAttributeSet`/`MonsterAttributeSet` 3개 클래스를 단일 `AttributeSet`으로 통합하는 쪽으로 확장. SettingData도 `AttributeSetSettingData` 하나로 합치고, JSON에서 필요한 필드만 채우고 나머지는 Newtonsoft 기본값으로 처리. TakeDamage 파이프라인에서 캐스팅 분기가 완전히 제거되어 훨씬 깔끔해짐

2. **BSC 내부 WSC 생성 방식 결정**: 사용자가 "WSC는 BSC 내부 컴포넌트이므로 항상 생성하되 사용할 곳에서만 초기화" vs "몬스터는 WSC=null로 명시" 두 안 중 후자를 선택. CLAUDE.md 규격에 맞게 `InitBSC(owner, attrSet, EWeaponType? = null)` 단일 시그니처 + 옵셔널 weaponType 분기

3. **크리티컬 공식 네이밍 결정**: CriticMultiple=150이 "1.5배"인지 "150% 추가 = 2.5배"인지 논의 후 표준 ARPG 컨벤션(`base * CriticMultiple / 100`)으로 확정. 150 = 1.5배, 100 = 원본 그대로. Diablo/PoE 등과 동일

4. **EWeaponType.TwoHandSword vs GreatSword 네이밍**: 원래 리뷰는 enum이 `TwoHandSword`인데 클래스가 `GreatSwordAttack`이라 리플렉션 실패를 지적. 사용자가 `TwoHandSword`를 정석으로 유지하고 클래스 쪽을 `TwoHandSwordAttack`으로 변경하는 방향 결정. CLAUDE.md 본문의 `GreatSword` 언급은 향후 업데이트 필요

5. **벽 끼임 해결 — 코드 아닌 에디터 설정**: 원래 계획은 `Rigidbody2D.Cast`로 벽 접촉 판정 코드를 추가할 예정이었으나, 사용자 제안으로 `PhysicsMaterial2D`(Friction=0)를 플레이어 collider에 적용하는 Unity 표준 방식으로 전환. 코드 수정 0줄로 해결

6. **Phase 2-2 (Dash/Jump 중 공격 상태 게이트)**: 리뷰는 "상태 게이트 복구"를 권장했으나, 실제 코드 트레이싱 결과 사용자 요구사항("즉시 공격 = Dash/Jump 캔슬")과 정합하며 `OnDashExit`/`OnAttackEnter`/`Spine PlayAnimation` 자동 캔슬로 깔끔하게 정리됨이 확인됨 → **수정 불필요** 판정

7. **스킵된 이슈 2건**:
   - **Phase 4-4 (SettingData 로드 실패 전파 + 주석 dict 복구)**: 사용자 결정으로 스킵. `MonsterSettingData.json` / `SoundSettingData.json` 파일이 아직 준비되지 않아 지금은 실익 없음
   - **Phase 6-2 (SkillSettingData.TryGetValue 버전 추가)**: 사용자 판단 — "휴먼 에러의 영역" (데이터 작성 시점에 확인할 수 있는 것이지 런타임 API 레벨에서 방어할 필요 없음)

8. **`removeStatusEffectList` 네이밍**: 처음 `removeEffectKeyBuffer`로 제안했으나 사용자가 프로젝트 다른 곳(이전 레퍼런스 코드)과의 일관성을 위해 `removeStatusEffectList`로 변경 요청. `readonly`는 제거

9. **SpineController.SetCombinedSkin → SetSkin 이름 단순화**: 6-3 작업 도중 사용자가 메서드명을 `SetSkin`으로 변경. "combined"는 구현 디테일이지 인터페이스 의미가 아님

10. **IDE 컴파일 캐시 지연**: 작업 도중 `AttributeSet.cs` 신규 생성 직후 PlayerCharacter.cs 쪽에 "형식을 찾을 수 없음" 일시 에러. Unity Editor가 C# 프로젝트 파일 재생성을 아직 안 한 시점. `.meta` GUID를 기존 `BaseAttributeSet.cs.meta`와 동일하게 유지해 에셋 참조 호환성 보장

### 태그

#코드리뷰 #전투시스템 #데미지파이프라인 #AttributeSet통합 #BSC #WSC #SSC #콤보레이스 #벽끼임 #PhysicsMaterial2D #MonoSingleton #수명주기 #ClearBSC #SkillSystem #쿨타임롤백 #리팩터 #Nit정리

---

## 📋 통합 요약


4. **`EWeaponType.TwoHandSword` vs `GreatSwordAttack` 네이밍 불일치** — WSC 리플렉션 팩토리(`MS.Battle.{WeaponType}Attack`)가 `TwoHandSwordAttack`을 찾아 실패 → 대검 무기 핸들러 생성 불가.
5. 8. **단일 이벤트 대기 한계** — `SpineController.cs:52-59` `WaitForAnimEventAsync`가 동시 1개 키만 대기. 다단히트 확장 시 이벤트 유실.
6. 12. **`MonoSingleton.Awake` 잘못된 참조 획득** — `Singleton.cs:30-36` `GetComponent<T>()` 사용, `this as T`가 정석.
7. - **BSC에 데미지 파이프라인 일원화** — 현재 각 소비처가 크리/방어커브를 개별 계산하는 위험 구조. `BSC.TakeDamage(DamageInfo)` 하나로 통합.
- **Init 명명/호출 체인 정리** — `Init{Class}` / `Init{Abbr}` 혼재, BSC가 WSC Init을 스킵하는 게 혼란의 직접 원인.
- **`ClearBSC`/정리 훅 대칭 구축** — `ClearSSC`는 있으나 BSC/WSC에는 없음. 이벤트 구독 수명 관리 불명확.
- **MovementController ↔ BSC 입력 게이트 통합** — 알려진 이슈와 직결.
- **인코딩 손상 주석 일괄 수정** — `MSStateMachine.cs`, `Settings.cs`에 cp949/utf-8 혼용 깨짐.
- - **`SkillSystemComponent.cs:82, 96`** — 쿨타임 이중 트리거 가능성. `IsPostUseCooltime=false`인 경우 82라인에서 `SetCooltime()`을 먼저 부르는데, 96라인의 `finally`는 `skillToUse.IsPostUseCooltime`만 체크해서 false 브랜치에서는 재호출되지 않아 안전하지만, 예외가 `ActivateSkillAsync` 실행 전에 터질 수 없는 구조가 아니라는 점 — 예외 발생 시 pre-use 경로는 쿨타임이 이미 들어가 있고, `OnSkillUsed` 이벤트는 발행되지 않는다. 피드백을 받는 UI가 있으면 불일치. 권장: `SetCooltime` 호출 위치를 try 블록 바깥/안에서 명확히 분리하고, pre-use 경로의 예외 롤백 정책을 정하라.

### 🔴 현재 빌드에서 "플레이어 전투가 원천 작동 불가" 상태

체인으로 엮여서 플레이해도 데미지 0이 나오는 구조:

1. **BSC → WSC Init 체인 단절** — `BattleSystemComponent.cs:17-26`이 `_weaponType`을 받지만 `WSC.InitWSC`를 호출하지 않음. WSC 내부 `owner/attrSet/curAttack` 전부 null. 또한 몬스터(`_weaponType==null`) 경우도 WSC를 무조건 생성.
2. **`BSC.TakeDamage` 미구현** — `BattleSystemComponent.cs:28-31` TODO로 비어있음. 데미지 파이프라인 전체 부재.
3. **`DoHit` 본체 주석 처리** — `OneHandSwordAttack.cs:95-111` 히트 판정은 돌지만 데미지 미전달.


### 🔴 플레이어 컨트롤 — 알려진 "공격 상태머신 우회" 이슈 현존

5. **`PlayerMovementController.OnAttack`이 상태 게이트 없이 WSC 직호출** — `PlayerMovementController.cs:336-345`. Dash/Jump 중 공격 입력이 통과되어 `OnAttackStartedCallback`에서 `TransitState(Attack)` → `OnDashExit` 실행 → 대시 중 중력 0 계약 깨짐.
6. **X축 속도 벽 끼임** — `PlayerMovementController.cs:81` FixedUpdate마다 `curVelocityX`로 무조건 덮어씀, 벽 접촉 판정 부재.

### 🔴 Spine/비동기 레이스 — 콤보 Hang 가능

7. **combo_ready/Complete 레이스** — `OneHandSwordAttack.cs:56-71` `WhenAny`로 빠져나온 뒤 두 번째 `WaitForAnimCompleteAsync()`를 한 번 더 await. Complete가 그 사이에 지나간 경우 TCS 재생성 → 영원히 Complete 못 받음.


### 🔴 부팅/수명주기 — 매니저가 "껍데기"

9. **`DataManager.InitGameData/ReleaseGameData` 호출 지점 0건** — Soul/Level/Dungeon/BattleGameData 전부 dead code.
10. **SettingData 로드 실패 전파 누락** — `SettingData.cs:18-38` 실패해도 `IsBootCompleted=true`. `MonsterSettingDict`/`SoundSettingDict` 로드는 주석 처리.
11. **`Main` 정리 훅 부재** — `OnDestroy` 없음. Addressables/ObjectPool/Sound 핸들 전부 누수.


---

## 

## High

- **`SkillSystemComponent.cs:98-102`** — `finally`에서 `runningSkillDict.ContainsKey` 체크 후 제거한다. `CancelAllSkills`/`CancelSkill`이 `Cancel()`만 호출하고 딕셔너리에서 지우지 않기 때문에, 취소 경로에서는 엔트리가 여전히 남아 있다가 여기에서 정리된다. 재진입 문제: 취소 후 동일 프레임에 `UseSkill(_skillKey)`를 다시 호출하면 75라인의 `ContainsKey` 체크가 true를 반환해 새 사용이 차단된다. 권장: `CancelSkill`/`CancelAllSkills`에서 Cancel 후 즉시 딕셔너리에서 remove.

- **`SkillSystemComponent.cs:136-143`** — `ClearSSC`가 먼저 `CancelAllSkills()`로 모든 CTS에 `Cancel()`을 부른 뒤, 바로 이어서 같은 루프로 `Dispose()`를 호출한다. 취소 후 컨티뉴에이션(`finally` 블록)이 비동기로 돌아올 때 그 CTS는 이미 Dispose 된 상태라서 `cts.Token` 접근이나 `cts.Dispose()` 재호출(finally의 101라인)이 `ObjectDisposedException`을 던질 수 있다.

- **`BattleSystemComponent.cs:59-76`** — `UpdateStatusEffects`가 매 프레임 `new List<string>()`을 할당 → GC 가비지. 필드로 `removeEffectKeyBuffer`를 두고 `Clear()` → 재사용.
- **`MonsterAttributeSet.cs`** — `WeaknessAttributeType` setter가 `BaseAttributeSet`에 있는데 몬스터 전용이라면 `MonsterAttributeSet`으로 내려보내는 것이 책임 분리상 깔끔.

## 구조적 제안

4. **`BSC.ClearBSC`가 없음**. BSC가 파괴되는 시점에 `OnHealthChanged`, `OnSkillAdded/Used`, `OnAttackStarted/Ended`가 외부에서 구독된 채로 남아 수명 관리 불명확. 각 시스템에 `Clear*` 대칭 메서드를 두고 BSC에서 일괄 호출.

---

# 2. Weapon/Skill 리뷰 리포트

**대상**: `WeaponAttack/*`, `Skill/*`

## Critical

1. **콤보 플로우 교착 가능성 — `OneHandSwordAttack.cs:56-71`** — 한 번의 루프 반복 안에서 `WaitForAnimEventAsync("combo_ready")`와 `WaitForAnimCompleteAsync()`를 `WhenAny`로 묶은 뒤, `combo_ready`로 빠져나온 경우 `WaitForAnimCompleteAsync()`를 한 번 더 await. 문제는 이 두 번째 await 직전에 이미 Complete 이벤트가 발생해 있었다면(combo_ready가 애니 종료 매우 직전에 배치된 경우), `SpineController`는 Complete TCS가 없는 상태에서 이벤트를 흘려보낸 뒤 새 TCS를 만들게 되어 **영원히 Complete를 못 받습니다**. 회수 동작이 없는 애니메이션, 또는 `combo_ready`가 마지막 프레임 근처인 경우에 현재 콤보가 끝나지 않아 다음 공격 입력이 모두 "예약"으로만 들어가버리는 hang 발생 가능.

2. **`SkillSystemComponent.ClearSSC` 이중 Dispose 가능성** (Battle 코어 리뷰 중복).

3. **`BaseSkill.CooltimeRatio` 의미 혼선 — `BaseSkill.cs:21`** — `elapsedCooltime`이 실제로는 "남은 시간"으로 감소(`-=`)되는데 변수명은 `elapsed`. 같은 값을 `CooltimeRatio = elapsed / cur`로 내놓아서 UI 바인딩 시 "남은 비율"로 해석되어 반대 방향 UI 버그 유발. `remainCooltime` 또는 `CooltimeRemainRatio`로 명시.

## Low / Nit

- **`BaseWeaponAttack.OnAttackStarted/Ended` 구독 해제** — WSC의 `ChangeWeaponType`은 `-=`로 깔끔하게 해제. 다만 WSC가 파괴되는 경로에서 마지막 핸들러를 명시적으로 해제하는 `ClearWSC()`가 없음.
- **`TestOneHandAttack`** — `await UniTask.CompletedTask` 불필요. `return UniTask.CompletedTask;`로 충분.

## 구조적 제안

E. **`BaseSkill`의 쿨타임/캐스팅을 SSC로 뽑을지 재검토**: 현재 `SetCooltime`은 `BaseSkill` 내부에서 `CooltimeAccel`을 직접 읽음. SSC가 한 번 계산하여 주입하는 구조가 테스트/모킹에 유리.

---

# 3. FieldObject 리뷰 리포트

**대상**: `FieldObject`, `BattleObject`, `FieldCharacter`, `SpineController`, `PlayerCharacter`, `PlayerMovementController`, `PlayerSoulController`

**C1. `PlayerMovementController.OnFixedUpdate` — `curVelocityX`가 지면 충돌을 무시하고 벽에 박힘** (`PlayerMovementController.cs:81`)
`rb.linearVelocity = new Vector2(curVelocityX, rb.linearVelocityY)` 매 FixedUpdate마다 X축 속도를 무조건 덮어씁니다. 이동/대시 중 벽과 접촉하면 Rigidbody2D가 자체적으로 속도 보정을 해도 다음 프레임에 다시 `curVelocityX`로 덮어쓰기 때문에, 캐릭터가 벽에 붙은 상태에서 계속 힘을 주게 되어 마찰로 공중에 붙거나 튕기는 벽 끼임(wall stick) 현상이 발생합니다. 특히 `DashSpeed=30`, 지상 대시 시 벽을 만나면 `gravityScale=0` 구간 동안 공중에 매달릴 수 있습니다. (보충설명: 실제로 문제되는 상황은 벽이 앞에 있는 상태에서 벽쪽으로 이동하면서 점프를 하면 벽에 매달려 있음)

**M3. `PlayerMovementController.Awake`에서 `GetComponent<PlayerCharacter>`** (`PlayerMovementController.cs:36-42`)
PlayerCharacter에서도 `GetComponent<PlayerMovementController>`. 양방향 GetComponent 참조. 한쪽만 세팅하고 다른쪽을 `InitController`에서 주입받는 형태가 깔끔.

**M7. `SpineController.SetCombinedSkin`이 매 소울교체마다 `new Skin("combined")`** (`SpineController.cs:84-97`)
GC 할당. Skin 인스턴스를 재사용하는 방식이 더 나음.

**M8. `SpineController.IsScaleXRight`와 `PlayerMovementController.isScaleXRight` 이중 상태** — Source of truth 하나로 통일 권장 (PMC는 `spineController.IsScaleXRight`만 읽으면 됨).

## Low / Nit
- **L3.** `PlayerMovementController.OnFixedUpdate` — `col == null` 체크는 런타임 불필요.
- **L4.** `PlayerMovementController.CheckCurInput`가 Idle/Move에서만 쓰이는데 `OnAttackUpdate`/`OnJumpUpdate`가 동일 로직 중복.


---

# 4. Core/Data/Utils 리뷰 리포트

**대상**: `Core/*`, `Data/*`, `Utils/*`, `Editor/AddressableEditor.cs`

## Critical

**C1. `MonoSingleton<T>.Awake`의 참조 획득이 잘못됨** (`Core/Singleton.cs:30-36`)
```
if (_instance == null)
    _instance = GetComponent<T>();
else
    DestroyImmediate(this);
```
- `GetComponent<T>()`는 T가 서브클래스(`Main`)여야 자신을 잡을 수 있는데, 위험한 코드. 더 안전하게 `_instance = this as T;` 가 맞다.

## High

**H1. `ObjectPoolManager.Get` 오버로드의 "기본 Quaternion" 문제** (`Core/Manager/ObjectPoolManager.cs:67-78`)
```
public GameObject Get(string _key, Vector3 _pos = default, Quaternion _rot = default)
```
- `Quaternion _rot = default` → `default(Quaternion) = (0,0,0,0)` (identity가 아님!). 2D라 현재 문제 미노출이지만 치명적.

**M7. `SkillSettingData.GetValue`가 없는 키 요청 시 0 리턴** (`Data/SettingData/SkillSettingData.cs:17-22`) — "0 데미지"와 "미정의"가 구분 불가. `TryGetValue` 버전 권장.

**M9. `EWeaponType.TwoHandSword` vs `GreatSwordAttack` 네이밍 불일치** (`Data/GlobalDefine.cs:34`)
- WSC의 리플렉션 규약 `MS.Battle.{WeaponType}Attack`으로 `TwoHandSwordAttack`을 찾으려다 실패. **사실상 GreatSword 무기는 현재 동작 불가**. Critical로 승격 가능.

**M10. `Utils/MathUtils.cs`에 `/// <summary>` 3회 존재** (`Utils/MathUtils.cs:7-10, 16-18, 24-26`) — CLAUDE.md 규칙 9 위반. 즉시 제거.
