# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 프로젝트 개요

**MortalSoul**은 Unity 6 (버전 6000.3.10f1)으로 개발 중인 2D 모바일 게임입니다. 플랫폼은 Android이며, Universal Render Pipeline(URP) 기반의 2D 렌더링을 사용합니다.

## Work Flow
** 반드시 이 규칙을 따라 작업을 수행합니다. **
1. MortalSoulDoc\01. WorkReq 폴더의 작업요청 md 문서를 참조할 경우 이 문서의 내용을 바탕으로 작업을 수행한다.
2. 작업을 수행하기 시작하면 사용자 승인 없이 즉시 MortalSoulDoc\02. InProgress 폴더로 해당 md 파일을 옮긴다.
3. 모든 작업이 완료되면 작업 내용을 요약하여 사용자에게 승인을 요청한다.
4. 승인을 받으면 MortalSoulDoc\03. Archive 폴더로 해당 md 파일을 옮기고 작업내용/특이사항/태그를 작성한다. 태그는 적절하게 작성한다.
5. 이후 깃허브에 커밋-푸시까지 진행하여 최종적으로 작업을 마무리한다.

## 코드 아키텍처

### 폴더 구조 규칙

`Assets/` 내부는 번호 접두사로 정렬됩니다:

- `01. Scenes/` — 게임 씬 파일
- `02. Scripts/` — C# 스크립트 (게임 로직의 핵심)
  - `Editor/` — Unity Editor 전용 스크립트
  - `Test/` — 테스트 스크립트
- `03. Resources/` — Prefab 및 ScriptableObject 에셋
  - `Prefabs/` — 재사용 가능한 게임 오브젝트
  - `ScriptableObjects/` — 데이터 컨테이너(설정값, 게임 데이터)
- `04. Settings/` — 게임 설정 관련 파일
- `Settings/` — URP 렌더링 파이핀 설정 (Renderer2D.asset, UniversalRP.asset)

### 핵심 시스템

**입력 시스템**: Unity Input System v1.18.0 사용. 입력 정의는 `Assets/InputSystem_Actions.inputactions`에서 관리됩니다. 현재 정의된 액션 맵:
- Player: Move(Vector2), Look(Vector2), Attack(Button), Interact(Button+Hold), Crouch(Button)

**렌더링**: Universal Render Pipeline(URP) v17.3.0 + 2D Renderer. 모바일 최적화가 목표이므로 Draw Call 최소화와 Sprite Atlas 활용을 고려해야 합니다.

**2D 핵심 패키지**:
- `com.unity.2d.animation` v13.0.4 — 캐릭터 애니메이션
- `com.unity.2d.tilemap` v1.0.0 — 타일맵 기반 레벨
- `com.unity.2d.spriteshape` v13.0.0 — 유기적인 지형 표현

**Visual Scripting**: `com.unity.visualscripting` v1.9.9 포함 (코드와 병행 사용 가능)

### 스크립트 작성 지침

- 새 스크립트는 기능에 따라 `Assets/02. Scripts/` 하위 폴더(예: `Player/`, `Enemy/`, `UI/`, `Manager/`)를 만들어 정리합니다.
- 데이터 중심 설계를 위해 설정값은 `ScriptableObject`로 분리하여 `03. Resources/ScriptableObjects/`에 배치합니다.
- 입력 처리는 반드시 `InputSystem_Actions.inputactions`의 액션을 통해서만 받습니다(레거시 Input 사용 금지).
