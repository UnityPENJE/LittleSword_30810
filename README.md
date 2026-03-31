# LittleSword

Unity로 개발 중인 2D 액션 게임입니다.

## 개발 환경

| 항목 | 버전 |
|------|------|
| Unity | 6000.2.7f2 (Unity 6) |
| 렌더 파이프라인 | URP (Universal Render Pipeline) |
| 언어 | C# |

## 프로젝트 구조

```
Assets/
├── 01_Scenes/        # 게임 씬
├── 02_Script/        # C# 스크립트
│   ├── Common/       # 공통 유틸리티
│   ├── InputSystem/  # 입력 처리
│   ├── Interfaces/   # 인터페이스 (IDamageable 등)
│   └── Player/
│       ├── Controller/   # 애니메이션, 이동 컨트롤러
│       ├── Editor/       # 커스텀 에디터
│       ├── Enemy/        # 적 AI (FSM 기반)
│       │   └── FSM/      # 상태 머신 (Idle, Chase, Attack)
│       └── Stats/        # 플레이어 스탯
├── 03_Prefabs/       # 프리팹 (Warrior, Goblin, Archer 등)
└── 04_Sprites/       # 스프라이트 리소스
```

## 주요 기능

- FSM(유한 상태 머신) 기반 적 AI
- 플레이어 감지 및 추적 시스템
- 애니메이션 컨트롤러
- ScriptableObject 기반 스탯 관리
