# 로비 시스템 구현 계획 (Risk of Rain 2 스타일)

## Context
현재 네트워크 연결(Host/Client)이 게임 씬(Level01)에서 이루어지고 있음.
로비 씬에서 접속 → 직업 선택 → 준비 완료 → 게임 시작 흐름으로 변경.

## 게임 흐름
```
[Lobby 씬]                              [Level01 씬]
접속 UI → Host/Client 연결              게임 플레이
  ↓                                       ↑
직업 선택 (Warrior / Archer / Lancer / Healer)
  ↓                                       |
Ready 버튼                                |
  ↓                                       |
전원 Ready → 자동 시작 ────────────────→ 씬 전환 → 직업별 프리팹 스폰
```

## 직업 구성 (총 4개)
| 직업 | 컨셉 | 공격 방식 |
|------|------|-----------|
| Warrior | 근접 전사 | OverlapBox 범위 공격 (기존) |
| Archer | 원거리 궁수 | 화살 발사 (기존) |
| **Lancer** (신규) | 창병 | 전방 길고 좁은 범위 찌르기 공격 |
| **Healer** (신규) | 힐러 | 주변 아군 HP 회복 + 약한 근접 공격 |

## 구현할 파일 (코드로 작성 가능한 것만)

### 새로 만들 파일 - 네트워크/로비
| 파일 | 역할 |
|------|------|
| `Assets/02_Script/Network/PlayerClassType.cs` | 직업 enum (Warrior, Archer, Lancer, Healer) |
| `Assets/02_Script/Network/LobbyManager.cs` | 로비 흐름 총괄: 접속→직업선택→Ready→게임시작 |
| `Assets/02_Script/Network/LobbyPlayerState.cs` | 접속한 플레이어별 직업/Ready 상태 (NetworkBehaviour) |

### 새로 만들 파일 - 신규 직업
| 파일 | 역할 |
|------|------|
| `Assets/02_Script/Player/Lancer.cs` | BasePlayer 상속, 전방 찌르기 범위 공격 |
| `Assets/02_Script/Player/Healer.cs` | BasePlayer 상속, 주변 아군 회복 + 약한 공격 |

### 수정할 파일
| 파일 | 변경 내용 |
|------|-----------|
| `ConnectManager.cs` | 접속 후 LobbyManager에 알려서 직업 선택 UI 표시 |
| `NetworkPlayer.cs` | 주석 추가 (이미 구현되어 있음) |

### 유니티 에디터에서 직접 해야 할 작업 (코드로 불가)
1. **Lobby 씬 설정**: NetworkManager를 Level01에서 Lobby로 이동
2. **NetworkManager 설정**: `PlayerPrefab = null`, `AutoSpawnPlayerPrefab = false`
3. **Archer 프리팹**: NetworkObject, NetworkTransform, OwnerNetworkAnimator 추가
4. **Lancer 프리팹 생성**: Warrior처럼 스프라이트 + Animator + NetworkObject/Transform/Animator + Lancer 스크립트
5. **Healer 프리팹 생성**: 마찬가지로 네트워크 컴포넌트 + Healer 스크립트
6. **DefaultNetworkPrefabs**: Archer, Lancer, Healer, LobbyPlayer 프리팹 등록
7. **Build Settings**: Lobby를 0번, Level01을 1번으로 설정
8. **LobbyPlayer 프리팹**: LobbyPlayerState 스크립트가 붙은 빈 프리팹 생성
9. **Level01 씬**: Connection Canvas 제거, PlayerSpawnPoint 빈 오브젝트 배치

## 핵심 구현 로직

### LobbyManager.cs
- `[SerializeField] int maxPlayers` (기본값 4, 인스펙터에서 자유 조절)
- UI 참조: 접속 패널, 직업 선택 패널, 플레이어 목록, Ready 버튼
- 접속 시: 접속 패널 숨기고 직업 선택 패널 표시
- 서버: 클라이언트 접속 시 LobbyPlayerState 프리팹을 SpawnAsPlayerObject로 스폰
- **전원 Ready 시 자동 시작**: 모든 LobbyPlayerState의 IsReady가 true이면 카운트다운 후 `NetworkManager.SceneManager.LoadScene("Level01")`
- `OnLoadEventCompleted` 콜백에서 각 플레이어의 직업에 맞는 프리팹 스폰

### LobbyPlayerState.cs (NetworkBehaviour)
- `NetworkVariable<PlayerClassType> SelectedClass`
- `NetworkVariable<bool> IsReady`
- `SelectClassServerRpc()` / `SetReadyServerRpc()`
- NetworkVariable 변경 시 UI 갱신 콜백

### ConnectManager.cs 수정
- 접속 성공 후 LobbyManager.OnConnected() 호출
- 종료 시 LobbyManager.OnDisconnected() 호출

## 신규 직업 설계

### Lancer.cs
- BasePlayer 상속
- 공격: 전방으로 길고 좁은 OverlapBox (Warrior보다 좁지만 길게)
- size = (2.5f, 0.5f), offset = 1.0f (전방으로 멀리)
- Animation Event에서 `OnLancerAttackEvent()` 호출
- 데미지 판정은 Warrior와 동일한 패턴 (OverlapBoxAll + IDamageable)

### Healer.cs
- BasePlayer 상속
- 공격: 약한 근접 공격 (Warrior 패턴 재사용, 작은 범위)
- 회복: Attack 쿨다운과 별도로, 주기적으로(또는 특수키로) 주변 아군 HP 회복
  - OverlapCircleAll로 playerLayer 감지 → BasePlayer.CurrentHP 회복
  - 자기 자신 제외 가능
- healAmount, healCooldown, healRadius 스탯 추가 (PlayerStats에 또는 직접 SerializeField)

## 주의사항
- NetworkManager는 DontDestroyOnLoad 자동 처리됨 (Netcode 내장)
- LobbyPlayerState는 SpawnAsPlayerObject로 스폰되어 씬 전환 시 자동 유지
- OnLoadEventCompleted 콜백은 씬 전환 전에 등록해야 함
- 스폰 로직은 NetworkManager 이벤트 기반으로 구현 (씬 로컬 MonoBehaviour에 의존 X)
