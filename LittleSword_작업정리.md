# LittleSword 작업 정리

## 추가/수정한 기능 요약

| # | 기능 | 분류 | 상태 |
|---|------|------|------|
| 1 | 카메라 흔들림 (CameraShake) | 효과 | ✅ |
| 2 | 카메라 추적 (CameraFollow) | 카메라 | ✅ |
| 3 | 타격감 - HitStop (시간 정지) | 효과 | ✅ |
| 4 | 타격감 - Knockback (밀림) | 효과 | ✅ |
| 5 | 타격감 통합 헬퍼 (HitFeedback) | 효과 | ✅ |
| 6 | 스테이지 적 스폰 (StageSpawner) | 게임플레이 | ✅ |
| 7 | 플레이어 HP 바 (BasePlayer 직접 통합) | UI | ✅ |
| 8 | 적 HP 바 (EnemyHPBar) | UI | ✅ |

---

## 1. CameraShake.cs

**위치:** `Assets/02_Script/Effects/CameraShake.cs`
**어태치:** Main Camera (또는 카메라 리그 자식 카메라)

```csharp
using System.Collections;
using UnityEngine;

namespace LittleSword.Effects
{
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        [SerializeField] private float defaultDuration = 0.15f;
        [SerializeField] private float defaultMagnitude = 0.1f;

        private Vector3 originalLocalPosition;
        private Coroutine shakeCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Shake() => Shake(defaultDuration, defaultMagnitude);

        public void Shake(float duration, float magnitude)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                transform.localPosition = originalLocalPosition;
            }
            shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
        }

        private IEnumerator ShakeCoroutine(float duration, float magnitude)
        {
            originalLocalPosition = transform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                Vector2 offset = Random.insideUnitCircle * magnitude;
                transform.localPosition = originalLocalPosition + new Vector3(offset.x, offset.y, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = originalLocalPosition;
            shakeCoroutine = null;
        }
    }
}
```

---

## 2. CameraFollow.cs

**위치:** `Assets/02_Script/Effects/CameraFollow.cs`
**어태치:** CameraRig (빈 GameObject) — Main Camera는 자식으로

```csharp
using UnityEngine;

namespace LittleSword.Effects
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("추적 설정")]
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime = 0.05f;
        [SerializeField] private Vector2 offset = Vector2.zero;

        [Header("Z축 고정 (2D)")]
        [SerializeField] private float fixedZ = 0f;

        private Vector3 velocity = Vector3.zero;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = new Vector3(
                target.position.x + offset.x,
                target.position.y + offset.y,
                fixedZ
            );

            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        }

        public void SetTarget(Transform newTarget) => target = newTarget;
    }
}
```

### 카메라 리그 구조

```
CameraRig (CameraFollow 붙음, Position Z=0)
  └─ Main Camera (자식, Position Z=-10, CameraShake 붙음)
```

### 잔상 방지

- Player의 **Rigidbody 2D → Interpolate**를 `Interpolate`로 변경

---

## 3. HitStop.cs

**위치:** `Assets/02_Script/Effects/HitStop.cs`
**어태치:** Main Camera (또는 아무 매니저 오브젝트)

```csharp
using System.Collections;
using UnityEngine;

namespace LittleSword.Effects
{
    public class HitStop : MonoBehaviour
    {
        public static HitStop Instance { get; private set; }

        [SerializeField] private float defaultDuration = 0.06f;
        private Coroutine stopCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Stop() => Stop(defaultDuration);

        public void Stop(float duration)
        {
            if (stopCoroutine != null) StopCoroutine(stopCoroutine);
            stopCoroutine = StartCoroutine(StopCoroutine(duration));
        }

        private IEnumerator StopCoroutine(float duration)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
            stopCoroutine = null;
        }
    }
}
```

---

## 4. Knockback.cs

**위치:** `Assets/02_Script/Effects/Knockback.cs`
**어태치:** Player 프리팹, Enemy 프리팹 각각 (Rigidbody2D 자동 요구)

```csharp
using System.Collections;
using UnityEngine;

namespace LittleSword.Effects
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Knockback : MonoBehaviour
    {
        [SerializeField] private float defaultForce = 6f;
        [SerializeField] private float duration = 0.12f;

        private Rigidbody2D rb;
        private Coroutine knockbackCoroutine;

        public bool IsKnockedBack { get; private set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void ApplyFrom(Vector2 sourcePos) => ApplyFrom(sourcePos, defaultForce);

        public void ApplyFrom(Vector2 sourcePos, float force)
        {
            if (knockbackCoroutine != null) StopCoroutine(knockbackCoroutine);

            Vector2 dir = ((Vector2)transform.position - sourcePos).normalized;
            if (dir == Vector2.zero) dir = Vector2.up;

            knockbackCoroutine = StartCoroutine(KnockbackCoroutine(dir * force));
        }

        private IEnumerator KnockbackCoroutine(Vector2 velocity)
        {
            IsKnockedBack = true;
            rb.linearVelocity = velocity;
            yield return new WaitForSeconds(duration);
            rb.linearVelocity = Vector2.zero;
            IsKnockedBack = false;
            knockbackCoroutine = null;
        }
    }
}
```

---

## 5. HitFeedback.cs

**위치:** `Assets/02_Script/Effects/HitFeedback.cs`
**어태치:** ❌ static 클래스 (어태치 안 함)

```csharp
using UnityEngine;

namespace LittleSword.Effects
{
    public static class HitFeedback
    {
        public static void Apply(Collider2D victim, Vector2 attackerPos)
        {
            CameraShake.Instance?.Shake();
            HitStop.Instance?.Stop();
            victim?.GetComponent<Knockback>()?.ApplyFrom(attackerPos);
        }

        public static void Apply(Collider2D victim, Vector2 attackerPos,
                                 float shakeMagnitude, float stopDuration, float knockbackForce)
        {
            CameraShake.Instance?.Shake(0.15f, shakeMagnitude);
            HitStop.Instance?.Stop(stopDuration);
            victim?.GetComponent<Knockback>()?.ApplyFrom(attackerPos, knockbackForce);
        }
    }
}
```

### 호출 위치 (한 줄씩 추가)

| 파일 | 위치 | 추가 |
|------|------|------|
| `Warrior.cs` | OnWarriorAttack foreach | `HitFeedback.Apply(col, transform.position);` |
| `Lancer.cs` | OnLancerAttackEvent foreach | `HitFeedback.Apply(col, transform.position);` |
| `Healer.cs` | OnHealerAttackEvent foreach | `HitFeedback.Apply(col, transform.position);` |
| `Arrow.cs` | OnTriggerEnter2D | `HitFeedback.Apply(other, transform.position);` |
| `Enemy.cs` | OnAttackAnimationEvent | `HitFeedback.Apply(targetCol, transform.position);` |

각 파일에 `using LittleSword.Effects;` 추가 필요

---

## 6. StageSpawner.cs

**위치:** `Assets/02_Script/Spawn/StageSpawner.cs`
**어태치:** 빈 GameObject (스테이지마다 1개)

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace LittleSword.Spawn
{
    public class StageSpawner : MonoBehaviour
    {
        [Header("스폰 설정")]
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private int totalEnemyCount = 5;

        [Header("동작 옵션")]
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField] private bool respawnOnDeath = false;
        [SerializeField] private float spawnInterval = 0f;

        private List<GameObject> aliveEnemies = new List<GameObject>();

        private void Start()
        {
            if (spawnOnStart) SpawnAll();
        }

        public void SpawnAll()
        {
            aliveEnemies.RemoveAll(e => e == null);
            int needed = totalEnemyCount - aliveEnemies.Count;
            if (spawnInterval > 0f)
                StartCoroutine(SpawnSequence(needed));
            else
                for (int i = 0; i < needed; i++) SpawnOne();
        }

        private System.Collections.IEnumerator SpawnSequence(int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnOne();
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        private void SpawnOne()
        {
            if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0) return;
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemy = Instantiate(prefab, point.position, Quaternion.identity);
            aliveEnemies.Add(enemy);
        }

        private void Update()
        {
            if (!respawnOnDeath) return;
            aliveEnemies.RemoveAll(e => e == null);
            if (aliveEnemies.Count < totalEnemyCount) SpawnOne();
        }

        private void OnDrawGizmos()
        {
            if (spawnPoints == null) return;
            Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
            foreach (var p in spawnPoints)
                if (p != null) Gizmos.DrawWireSphere(p.position, 0.4f);
        }
    }
}
```

---

## 7. BasePlayer HP 바 통합

**위치:** `Assets/02_Script/Player/BasePlayer.cs` (수정)

`BasePlayer`에 HP 바 UI를 직접 통합 (별도 UI 스크립트 불필요).

추가된 부분:
- `using UnityEngine.UI;`
- `[SerializeField] private Image hpFillImage;`
- `[SerializeField] private float hpLerpSpeed = 5f;`
- `private float targetHpFill = 1f;`
- `Update()`에서 `Mathf.Lerp`로 부드럽게 갱신
- `TakeDamage()`, `Heal()`, `InitComponents()`에서 `targetHpFill` 갱신

### Canvas 셋업

```
Canvas (Screen Space - Overlay)
  ├─ HPBar_BG (회색 Image)
  └─ HPBar_Fill (빨강 Image, Type=Filled, Method=Horizontal, Origin=Left)
```

### 인스펙터 연결

Warrior(플레이어) GameObject → `Hp Fill Image`에 `HPBar_Fill` 드래그

---

## 8. EnemyHPBar.cs

**위치:** `Assets/02_Script/UI/EnemyHPBar.cs`
**어태치:** 적 프리팹 자식 Canvas (World Space)

```csharp
using UnityEngine;
using UnityEngine.UI;

namespace LittleSword.UI
{
    public class EnemyHPBar : MonoBehaviour
    {
        [SerializeField] private Image hpFillImage;
        [SerializeField] private float lerpSpeed = 8f;
        [SerializeField] private float hideDelay = 2f;
        [SerializeField] private bool hideWhenFull = true;

        private LittleSword.Enemy.Enemy enemy;
        private float targetFill = 1f;
        private float hideTime = 0f;

        private void Awake()
        {
            enemy = GetComponentInParent<LittleSword.Enemy.Enemy>();
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (enemy != null) enemy.OnHPChanged += UpdateHP;
        }

        private void OnDisable()
        {
            if (enemy != null) enemy.OnHPChanged -= UpdateHP;
        }

        private void Update()
        {
            hpFillImage.fillAmount = Mathf.Lerp(hpFillImage.fillAmount, targetFill, Time.deltaTime * lerpSpeed);
            if (Time.time >= hideTime) gameObject.SetActive(false);
        }

        private void UpdateHP(int current, int max)
        {
            targetFill = (float)current / max;
            if (hideWhenFull && current >= max) return;
            gameObject.SetActive(true);
            hideTime = Time.time + hideDelay;
        }
    }
}
```

### 적 프리팹 구조

```
Enemy (Enemy 스크립트)
  └─ Canvas (World Space, Scale 0.01, Position Y 0.8)
      ├─ HPBar_BG
      └─ HPBar_Fill (EnemyHPBar 스크립트는 Canvas에 붙임)
```

---

## 어태치 요약표

| 컴포넌트 | 붙이는 곳 | 개수 |
|---------|---------|------|
| `CameraShake` | Main Camera | 씬에 1개 |
| `CameraFollow` | CameraRig (빈 GameObject) | 씬에 1개 |
| `HitStop` | Main Camera | 씬에 1개 |
| `Knockback` | Player + Enemy 프리팹 | 각각 |
| `HitFeedback` | ❌ 어태치 안 함 (static) | — |
| `StageSpawner` | 빈 GameObject | 스테이지마다 1개 |
| `EnemyHPBar` | Enemy 프리팹 자식 Canvas | 적마다 1개 |
| HP 바 (Player) | `BasePlayer`에 통합됨, Canvas만 셋업 | 씬에 1개 Canvas |

---

## 알려진 이슈 / 팁

- **카메라 흔들림이 안 보임** → CameraShake가 transform.position을 매 프레임 덮어쓰는 CameraFollow와 충돌. CameraRig 구조로 분리 (CameraFollow는 Rig에, CameraShake는 자식 Camera에).
- **캐릭터 잔상/뚝뚝 끊김** → Player의 Rigidbody2D → Interpolate를 `Interpolate`로 변경. 카메라 SmoothTime은 0.05 정도가 적당.
- **체력바 안 줄어듦** → 인스펙터에서 `Hp Fill Image` 슬롯에 Fill 이미지가 드래그 됐는지 확인. Image Type=Filled, Fill Method=Horizontal 필수.
- **`Enemy is namespace but used like type`** → `LittleSword.Enemy.Enemy` 전체 경로로 명시.

---

## 9. 멀티플레이어 UI 분리 (플레이어별 독립 HP바·골드·상점)

### 변경 목적
2인 멀티(Unity Netcode for GameObjects) 환경에서 HP바, 골드 UI, 상점 구매를 플레이어별로 완전히 분리.

### 변경 파일 및 내용

| 파일 | 변경 내용 |
|------|-----------|
| `Network/NetworkPlayer.cs` | `LocalPlayer` 정적 참조 추가, `networkGold` NetworkVariable, `AddGold()` / `SpendGoldServerRpc()`, 스폰 시 HP바·골드UI 자동 연결 |
| `Player/UI/PlayerHPBarUI.cs` | `SetPlayer(BasePlayer)` 메서드 추가 → NetworkPlayer가 스폰 시 자동 연결 |
| `Player/BasePlayer.cs` | `TakeDamage()` / `Heal()`에서 `OnHPChanged` 이벤트 발화 추가 |
| `UI/GoldUI.cs` | `CurrencyManager` 싱글톤 대신 `NetworkPlayer.LocalPlayer` 골드 구독 |
| `UI/ShopUI.cs` | 동일 — 로컬 플레이어 골드 표시 |
| `UI/ShopSlotUI.cs` | 로컬 플레이어 골드로 구매, PlayerPrefs 키를 `{clientId}_{itemKey}` 형태로 분리 |
| `Player/Enemy/Enemy.cs` | `Die()`에서 `CurrencyManager` 대신 타겟 `NetworkPlayer.AddGold()` 호출 |

### 동작 방식

```
플레이어 스폰 (OnNetworkSpawn)
  └─ IsOwner인 경우
       ├─ NetworkPlayer.LocalPlayer = this
       ├─ PlayerHPBarUI.SetPlayer(baseplayer) 자동 연결
       └─ OnLocalPlayerSpawned 이벤트 발행
            ├─ GoldUI 구독 → 내 골드만 표시
            └─ ShopUI 구독 → 내 골드만 표시

적 처치 (Enemy.Die)
  └─ IsServer && target != null
       └─ target의 NetworkPlayer.AddGold(goldReward) 호출
            └─ NetworkVariable<int> networkGold 서버에서 차감/증가
                 └─ 각 클라이언트 UI 자동 동기화

상점 구매 (ShopSlotUI.OnBuy)
  ├─ NetworkPlayer.LocalPlayer.Gold >= price 로컬 검증
  ├─ SpendGoldServerRpc(price) 서버 차감 요청
  └─ PlayerPrefs 키: "{clientId}_{itemKey}" (같은 PC 테스트도 분리)
```

### 골드 NetworkVariable 구조

```csharp
// NetworkPlayer.cs
NetworkVariable<int> networkGold = new NetworkVariable<int>(
    0,
    NetworkVariableReadPermission.Everyone,   // 모두 읽기 가능
    NetworkVariableWritePermission.Server     // 서버만 쓰기 가능
);
```

### 어태치 변경사항

| 항목 | 이전 | 이후 |
|------|------|------|
| GoldUI 골드 참조 | `CurrencyManager.Instance` (전역) | `NetworkPlayer.LocalPlayer` (로컬) |
| ShopUI 골드 참조 | `CurrencyManager.Instance` (전역) | `NetworkPlayer.LocalPlayer` (로컬) |
| PlayerHPBarUI 플레이어 참조 | 인스펙터 직접 드래그 | 런타임 자동 연결 (`SetPlayer`) |
| 적 처치 골드 | 전역 싱글톤 CurrencyManager | 타겟 플레이어 NetworkPlayer |
