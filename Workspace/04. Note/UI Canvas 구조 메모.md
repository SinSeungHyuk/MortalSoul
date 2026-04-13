# UI Canvas 구조 메모

## HUD 프리팹에 Canvas가 필요한가?

**불필요.** ViewCanvas에 이미 Canvas + CanvasScaler가 있으므로 HUD 프리팹은 RectTransform만으로 충분.

## 구조

```
ViewCanvas (Canvas + CanvasScaler)
└─ BattleHUD (빈 RectTransform, Stretch-Stretch)
   ├─ HPBar
   ├─ SkillSlots
   ├─ SoulSwitchButton
   └─ ...
```

- BattleHUD를 컨테이너로 두고 자식에 각 UI 요소 배치
- BattleHUD 하나만 활성화/비활성화하면 전투 UI 전체 제어 가능

## Canvas 중첩이 필요한 경우

특정 UI 영역의 드로우콜 분리가 필요할 때만 (예: 자주 갱신되는 HP바를 별도 Canvas로 묶어 리빌드 범위 축소). 최적화 단계에서 판단.
