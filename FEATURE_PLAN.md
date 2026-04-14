# LittleSword 신규 기능 구현 계획

## Context
사용자가 Unity 6 기반 2D 탑다운 멀티플레이 액션 RPG에 3가지 핵심 기능을 추가 요청.
기존 코드에 `OnSkill` 이벤트 훅, FSM 기반 적 AI, ScriptableObject 스탯 구조가 이미 마련되어 있어 자연스럽게 확장 가능.

### 코드 스타일 규칙 (기존 코드와 동일하게 유지)
- 네임스페이스: `LittleSword.기능명`
- 주석: 한국어, 교육적 설명 스타일
- 이벤트 구독: `OnEnable()` / `OnDisable()` 패턴 (Start/Destroy 아님)
- Inspector 필드 그룹: `[Header("설명")]`
- 외부 읽기 전용 접근: `public T Property => field;` 패턴
- 범위 시각화: `OnDrawGizmos()` 필수
- Null 안전 호출: `?.` 연산자 활용

---

## 기능 1: 직업별 특수 스킬 (Q키)

### 수정 파일: `Assets/02_Script/Player/BasePlayer.cs`

**추가 위치 - 필드 (nextAttackTime 아래):**
```csharp
// 다음 스킬 사용이 가능한 시간 (쿨다운 계산용)
// protected: 자식 직업 클래스에서 nextSkillTime 직접 갱신 가능
[SerializeField] protected float skillCooldown = 8f;
protected float nextSkillTime = 0f;
```

**수정 위치 - OnEnable() / OnDisable():**
```csharp
protected void OnEnable()
{
    inputHandler.OnMove += Move;
    inputHandler.OnAttack += Attack;
    inputHandler.OnDash += OnDash;
    inputHandler.OnSkill += UseSkill; // 추가
}

protected void OnDisable()
{
    inputHandler.OnMove -= Move;
    inputHandler.OnAttack -= Attack;
    inputHandler.OnDash -= OnDash;
    inputHandler.OnSkill -= UseSkill; // 추가
}
```

**추가 함수 (Attack() 아래):**
```csharp
// 스킬 사용 - 자식 클래스에서 override해서 직업별 스킬 구현
// virtual + 빈 구현: 직업이 스킬을 구현하지 않아도 에러 없음
protected virtual void UseSkill() { }
```

---

### 수정 파일: `Assets/02_Script/Player/Warrior.cs`

```csharp
[Header("스킬 설정 - 회오리 베기")]
[SerializeField] private float skillRadius = 2.0f;        // 360도 공격 범위 반경
[SerializeField] private int skillDamageMultiplier = 2;   // 공격력 배수

// 스킬: 회오리 베기 - 주변 360도 범위 적 전체에게 데미지
protected override void UseSkill()
{
    if (Time.time < nextSkillTime) return;
    nextSkillTime = Time.time + skillCooldown;

    // OverlapCircleAll: 원형 범위 안의 모든 적 감지
    Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, skillRadius, enemyLayer);
    foreach (var col in colliders)
    {
        col.GetComponent<IDamageable>()?.TakeDamage(playerStats.attackDamage * skillDamageMultiplier);
    }
}

// OnDrawGizmos에 스킬 범위 추가 (보라색 원)
// Gizmos.color = new Color(0.8f, 0f, 1f, 0.2f);
// Gizmos.DrawSphere(transform.position, skillRadius);
```

---

### 수정 파일: `Assets/02_Script/Player/Archer.cs`

```csharp
[Header("스킬 설정 - 3연사")]
[SerializeField] private float spreadAngle = 15f; // 화살 퍼짐 각도

// 스킬: 3연사 - 15도 간격으로 화살 3발 동시 발사
protected override void UseSkill()
{
    if (Time.time < nextSkillTime) return;
    nextSkillTime = Time.time + skillCooldown;

    // -spread, 0, +spread 3방향으로 화살 발사
    FireArrowAtAngle(-spreadAngle);
    FireArrowAtAngle(0f);
    FireArrowAtAngle(spreadAngle);
}

// angle 오프셋을 적용해서 화살 발사 (기존 FireArrow() 패턴 참고)
private void FireArrowAtAngle(float angleOffset)
{
    // 기존 FireArrow() 로직을 각도 오프셋과 함께 호출
    // (Archer.cs의 기존 firePoint, arrowPrefab, fireForce 변수 재사용)
}
```

---

### 수정 파일: `Assets/02_Script/Player/Lancer.cs`

```csharp
[Header("스킬 설정 - 돌진 찌르기")]
[SerializeField] private float dashForce = 18f;        // 돌진 속도
[SerializeField] private float dashDuration = 0.2f;    // 돌진 지속 시간
[SerializeField] private Vector2 thrustSize = new Vector2(3.0f, 0.6f); // 관통 판정 크기

// 스킬: 돌진 찌르기 - 전방으로 빠르게 이동하며 관통 피해
protected override void UseSkill()
{
    if (Time.time < nextSkillTime) return;
    nextSkillTime = Time.time + skillCooldown;

    StartCoroutine(DashThrustCoroutine());
}

private IEnumerator DashThrustCoroutine()
{
    Vector2 direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;

    // 돌진 이동
    rigidBody.linearVelocity = direction * dashForce;

    yield return new WaitForSeconds(dashDuration);

    // 도착 후 관통 판정
    Vector2 center = (Vector2)transform.position + direction * 1.5f;
    Collider2D[] colliders = Physics2D.OverlapBoxAll(center, thrustSize, 0, enemyLayer);
    foreach (var col in colliders)
    {
        col.GetComponent<IDamageable>()?.TakeDamage(playerStats.attackDamage * 3);
    }

    rigidBody.linearVelocity = Vector2.zero;
}
```

---

### 수정 파일: `Assets/02_Script/Player/Healer.cs`

```csharp
[Header("스킬 설정 - 치유 폭발")]
[SerializeField] private float skillHealRadius = 4.0f;  // 치유 폭발 범위
[SerializeField] private int skillHealAmount = 50;       // 즉시 회복량

// 스킬: 치유 폭발 - 넓은 범위의 아군을 즉시 대량 회복
protected override void UseSkill()
{
    if (Time.time < nextSkillTime) return;
    nextSkillTime = Time.time + skillCooldown;

    Collider2D[] allies = Physics2D.OverlapCircleAll(transform.position, skillHealRadius, playerLayer);
    foreach (var ally in allies)
    {
        ally.GetComponent<BasePlayer>()?.Heal(skillHealAmount);
    }
}

// OnDrawGizmos에 스킬 범위 추가 (하늘색)
// Gizmos.color = new Color(0f, 1f, 1f, 0.15f);
// Gizmos.DrawSphere(transform.position, skillHealRadius);
```

---

## 기능 2: 웨이브 스폰 시스템

### 수정 파일: `Assets/02_Script/Player/Enemy/Enemy.cs`

**추가 (필드):**
```csharp
// 적이 사망했을 때 발생하는 이벤트 (WaveManager에서 구독해서 카운트 관리)
public event Action<Enemy> OnDeath;
```

**수정 (Die() 함수 내 ChangeState 호출 전):**
```csharp
public void Die()
{
    OnDeath?.Invoke(this); // 사망 이벤트 발동 (WaveManager가 구독)
    ChangeState<DieState>();
}
```

---

### 신규 파일: `Assets/02_Script/Wave/WaveManager.cs`

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LittleSword.Enemy;

namespace LittleSword.Wave
{
    // ============================================================
    // WaveManager: 웨이브 기반 몬스터 스폰을 관리하는 클래스
    // ============================================================
    // 웨이브 흐름:
    //   1. StartWave() 호출 → 적 스폰
    //   2. 모든 적 처치 시 OnEnemyDeath() 감지
    //   3. waveCooldown 대기 후 다음 웨이브 시작
    //   4. 5의 배수 웨이브마다 보스 스폰
    // ============================================================
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        [Header("스폰 설정")]
        [SerializeField] private GameObject[] enemyPrefabs;   // 일반 적 프리팹 배열
        [SerializeField] private GameObject bossPrefab;        // 보스 프리팹
        [SerializeField] private Transform[] spawnPoints;      // 스폰 위치 배열

        [Header("웨이브 밸런스")]
        [SerializeField] private int baseEnemyCount = 5;       // 1웨이브 기준 적 수
        [SerializeField] private int enemyCountIncrement = 3;  // 웨이브당 추가 적 수
        [SerializeField] private float waveCooldown = 5f;      // 웨이브 사이 대기 시간(초)

        // 현재 웨이브 번호 (외부에서 읽기 전용)
        public int CurrentWave { get; private set; } = 0;

        // 현재 살아있는 적 목록
        private HashSet<Enemy> activeEnemies = new HashSet<Enemy>();

        // 웨이브 변경 이벤트 (WaveUI에서 구독)
        public event Action<int> OnWaveChanged;

        private void Awake()
        {
            // 싱글톤 설정
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            StartWave(1);
        }

        // 웨이브 시작
        public void StartWave(int wave)
        {
            CurrentWave = wave;
            OnWaveChanged?.Invoke(CurrentWave);

            // 5의 배수 웨이브 = 보스 스폰
            if (wave % 5 == 0 && bossPrefab != null)
            {
                SpawnBoss();
                return;
            }

            // 일반 웨이브: 적 수 = 기본 + (웨이브 × 증가량)
            int spawnCount = baseEnemyCount + (wave * enemyCountIncrement);
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnEnemy();
            }
        }

        // 랜덤 위치에 일반 적 스폰
        private void SpawnEnemy()
        {
            if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0) return;

            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            GameObject obj = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            RegisterEnemy(obj);
        }

        // 보스 스폰 (첫 번째 spawnPoint 사용)
        private void SpawnBoss()
        {
            Transform spawnPoint = spawnPoints[0];
            GameObject obj = Instantiate(bossPrefab, spawnPoint.position, Quaternion.identity);
            RegisterEnemy(obj);
        }

        // 스폰된 적을 activeEnemies에 등록하고 사망 이벤트 구독
        private void RegisterEnemy(GameObject obj)
        {
            Enemy enemy = obj.GetComponent<Enemy>();
            if (enemy == null) return;

            activeEnemies.Add(enemy);
            enemy.OnDeath += OnEnemyDeath;
        }

        // 적 사망 시 호출 - 전멸 시 다음 웨이브 시작
        private void OnEnemyDeath(Enemy enemy)
        {
            enemy.OnDeath -= OnEnemyDeath; // 이벤트 구독 해제 (메모리 누수 방지)
            activeEnemies.Remove(enemy);

            if (activeEnemies.Count == 0)
            {
                StartCoroutine(NextWaveCountdown());
            }
        }

        // waveCooldown 대기 후 다음 웨이브 시작
        private IEnumerator NextWaveCountdown()
        {
            yield return new WaitForSeconds(waveCooldown);
            StartWave(CurrentWave + 1);
        }
    }
}
```

---

### 신규 파일: `Assets/02_Script/UI/WaveUI.cs`

```csharp
using TMPro;
using UnityEngine;
using LittleSword.Wave;

namespace LittleSword.UI
{
    // ============================================================
    // WaveUI: 현재 웨이브 번호를 화면에 표시하는 UI 클래스
    // ============================================================
    public class WaveUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI waveText; // "WAVE N" 텍스트

        private void OnEnable()
        {
            if (WaveManager.Instance != null)
                WaveManager.Instance.OnWaveChanged += UpdateWaveText;
        }

        private void OnDisable()
        {
            if (WaveManager.Instance != null)
                WaveManager.Instance.OnWaveChanged -= UpdateWaveText;
        }

        private void UpdateWaveText(int wave)
        {
            waveText.text = $"WAVE {wave}";
        }
    }
}
```

---

## 기능 3: 보스 몬스터

### 신규 파일: `Assets/02_Script/Player/Enemy/BossEnemy.cs`

```csharp
using System;
using System.Collections;
using UnityEngine;
using LittleSword.Enemy.FSM;
using LittleSword.Interfaces;
using LittleSword.UI;
using LittleSword.Effects;

namespace LittleSword.Enemy
{
    // ============================================================
    // BossEnemy: 보스 몬스터 클래스 (Enemy 상속)
    // ============================================================
    // 일반 적과 다른 점:
    //   - HP 임계치마다 페이즈 전환 (강화)
    //   - 특수 공격: 광역 충격파 (Phase 2 이상)
    //   - 분노 상태: 미니언 소환 (HP 20% 이하)
    //
    // 페이즈 구조:
    //   Phase 1: HP > 50% → 기본 AI (Chase + Attack)
    //   Phase 2: HP 50~20% → 충격파 추가, 이동속도 1.5배
    //   Enrage:  HP < 20% → 미니언 소환, 공격 쿨다운 반감
    // ============================================================
    public class BossEnemy : Enemy
    {
        [Header("보스 페이즈 설정")]
        [SerializeField] private float phase2HPRatio = 0.5f;   // Phase 2 진입 HP 비율 (50%)
        [SerializeField] private float enrageHPRatio = 0.2f;   // 분노 상태 진입 HP 비율 (20%)

        [Header("충격파 설정 (Phase 2)")]
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private float shockwaveRadius = 3.5f;    // 충격파 범위
        [SerializeField] private int shockwaveDamage = 20;         // 충격파 데미지
        [SerializeField] private float shockwaveCooldown = 4f;     // 충격파 발동 주기
        [SerializeField] private float knockbackForce = 8f;        // 넉백 강도

        [Header("분노 상태 (Enrage)")]
        [SerializeField] private int minionSpawnCount = 3;         // 소환할 미니언 수

        // 현재 페이즈 (외부 읽기 전용)
        public int CurrentPhase { get; private set; } = 1;

        // 페이즈 전환 이벤트 (BossHPBarUI에서 구독)
        public event Action<int> OnPhaseChanged;

        private float nextShockwaveTime = 0f;
        private bool phase2Triggered = false;
        private bool enrageTriggered = false;

        // Update: 부모 클래스 Update (FSM) + 페이즈별 특수 공격
        private new void Update() // new: Enemy.Update() 숨기고 재정의
        {
            base.Update(); // FSM 실행

            // Phase 2 이상: 주기적으로 충격파 발동
            if (CurrentPhase >= 2 && Time.time >= nextShockwaveTime)
            {
                StartCoroutine(ShockwaveAttack());
                nextShockwaveTime = Time.time + shockwaveCooldown;
            }
        }

        // IDamageable 구현 override: 피해를 받을 때 페이즈 체크
        public new void TakeDamage(int damage)
        {
            base.TakeDamage(damage); // 기본 피해 처리 (HP 감소, 이벤트 발동)
            CheckPhaseTransition();
        }

        // HP 비율에 따라 페이즈 전환 체크
        private void CheckPhaseTransition()
        {
            float hpRatio = (float)CurrentHP / MaxHP;

            // Phase 2 진입 (한 번만)
            if (!phase2Triggered && hpRatio <= phase2HPRatio)
            {
                phase2Triggered = true;
                ChangePhase(2);
            }

            // 분노 상태 진입 (한 번만)
            if (!enrageTriggered && hpRatio <= enrageHPRatio)
            {
                enrageTriggered = true;
                ChangePhase(3);
            }
        }

        // 페이즈 전환 처리
        private void ChangePhase(int phase)
        {
            CurrentPhase = phase;
            OnPhaseChanged?.Invoke(phase);

            if (phase == 2)
            {
                // 이동 속도 1.5배 증가
                // EnemyStats는 ScriptableObject라 직접 수정 불가 → rigidbody 속도 배율로 처리
            }
            else if (phase == 3)
            {
                // 미니언 소환
                StartCoroutine(SummonMinions());
            }
        }

        // 충격파 공격: 범위 내 플레이어에게 데미지 + 넉백
        private IEnumerator ShockwaveAttack()
        {
            // TODO: 충격파 시작 애니메이션 트리거

            yield return new WaitForSeconds(0.3f); // 애니메이션 선딜

            Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, shockwaveRadius, playerLayer);
            foreach (var target in targets)
            {
                // 데미지 적용
                target.GetComponent<IDamageable>()?.TakeDamage(shockwaveDamage);

                // 넉백: 보스 기준으로 바깥 방향으로 밀어냄
                Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 knockbackDir = (target.transform.position - transform.position).normalized;
                    rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
                }
            }
        }

        // 미니언 소환: WaveManager에 즉시 소환 요청
        private IEnumerator SummonMinions()
        {
            // TODO: 소환 애니메이션
            yield return new WaitForSeconds(0.5f);
            // WaveManager.Instance?.SpawnMinions(minionSpawnCount);
            // (WaveManager에 SpawnMinions(int count) 공개 함수 추가 필요)
        }

        // Scene 뷰에서 충격파 범위 시각화 (빨간 원)
        private new void OnDrawGizmos()
        {
            base.OnDrawGizmos(); // Enemy Gizmos (감지/공격 범위)

            // 충격파 범위 (빨간색)
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawSphere(transform.position, shockwaveRadius);
        }
    }
}
```

---

### 신규 파일: `Assets/02_Script/UI/BossHPBarUI.cs`

```csharp
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LittleSword.Enemy;

namespace LittleSword.UI
{
    // ============================================================
    // BossHPBarUI: 화면 하단에 보스 체력을 표시하는 UI
    // ============================================================
    // 보스가 스폰되면 활성화, 사망하면 비활성화
    // HP 감소를 Lerp로 부드럽게 표현 (기존 HPBar.cs 패턴 동일)
    // ============================================================
    public class BossHPBarUI : MonoBehaviour
    {
        [SerializeField] private Image hpFillImage;       // HP 바 이미지 (fillAmount 조절)
        [SerializeField] private TextMeshProUGUI bossNameText; // 보스 이름 텍스트
        [SerializeField] private float lerpSpeed = 5f;    // HP 감소 애니메이션 속도

        private float targetFillAmount = 1f; // 목표 fillAmount (실제 HP 비율)
        private BossEnemy trackedBoss;       // 현재 추적 중인 보스

        private void Awake()
        {
            gameObject.SetActive(false); // 시작 시 숨김
        }

        private void Update()
        {
            // HP 바를 부드럽게 목표값으로 이동 (기존 HPBar.cs 패턴 동일)
            hpFillImage.fillAmount = Mathf.Lerp(hpFillImage.fillAmount, targetFillAmount, Time.deltaTime * lerpSpeed);
        }

        // 보스 추적 시작 (BossEnemy 스폰 시 호출)
        public void TrackBoss(BossEnemy boss, string bossName)
        {
            trackedBoss = boss;
            bossNameText.text = bossName;
            gameObject.SetActive(true);

            boss.OnHPChanged += UpdateHP;
            boss.OnDeath += OnBossDeath;
        }

        private void UpdateHP(int current, int max)
        {
            targetFillAmount = (float)current / max;
        }

        private void OnBossDeath(Enemy boss)
        {
            boss.OnHPChanged -= UpdateHP;
            boss.OnDeath -= OnBossDeath;
            gameObject.SetActive(false);
        }
    }
}
```

---

## 파일 변경 요약

| 파일 | 변경 유형 | 주요 변경 내용 |
|------|----------|--------------|
| `Player/BasePlayer.cs` | 수정 | `UseSkill()` virtual 추가, OnSkill 이벤트 구독 |
| `Player/Warrior.cs` | 수정 | 회오리 베기 스킬 override |
| `Player/Archer.cs` | 수정 | 3연사 스킬 override |
| `Player/Lancer.cs` | 수정 | 돌진 찌르기 스킬 override |
| `Player/Healer.cs` | 수정 | 치유 폭발 스킬 override |
| `Player/Enemy/Enemy.cs` | 수정 | `OnDeath` 이벤트 추가 |
| `Wave/WaveManager.cs` | 신규 | 웨이브 스폰, 사망 감지, 다음 웨이브 전환 |
| `UI/WaveUI.cs` | 신규 | 현재 웨이브 번호 텍스트 |
| `Player/Enemy/BossEnemy.cs` | 신규 | 페이즈 시스템, 충격파, 미니언 소환 |
| `UI/BossHPBarUI.cs` | 신규 | 보스 HP 바 (화면 하단) |

## 전체 10개 기능 구현 순서 (UI/편의성 우선)

| 순서 | 기능 | 분류 | 상태 |
|------|------|------|------|
| 1 | 게임 설정 메뉴 (ESC 일시정지 + 볼륨/로비복귀) | UI | 상세 설계 필요 |
| 2 | 사운드 & 음악 시스템 (SoundManager) | 편의성 | 상세 설계 필요 |
| 3 | 게임 오버 & 결과 화면 (GameManager + UI) | UI | 상세 설계 필요 |
| 4 | 레벨업 스탯 선택 시스템 (카드 3장) | UI | 상세 설계 필요 |
| 5 | HP바 구현 시스템 (HUD 고정형 + 보스/적 통합) | UI | 상세 설계 필요 |
| 6 | 직업별 특수 스킬 (Q키) | 게임플레이 | ✅ 본 문서 "기능 1" 참고 |
| 7 | 웨이브 스폰 시스템 (WaveManager) | 게임플레이 | ✅ 본 문서 "기능 2" 참고 |
| 8 | 보스 몬스터 (페이즈 + 충격파) | 게임플레이 | ✅ 본 문서 "기능 3" 참고 |
| 9 | 적 특수 능력 확장 (GoblinTNT 자폭) | 게임플레이 | 상세 설계 필요 |
| 10 | 엘리트 적 시스템 (스탯 강화 + 색상) | 게임플레이 | 상세 설계 필요 |

> 본 문서의 "기능 1/2/3" 섹션은 위 순서표의 6/7/8번에 해당합니다.
> 1~5번 및 9~10번은 추후 상세 코드 설계를 추가할 예정입니다.
> ※ 코인 상점 시스템은 HP바 구현 시스템으로 대체되었습니다.

## 검증 방법

- **일시정지**: ESC → 게임 정지, 볼륨 슬라이더 동작, 로비 복귀 버튼 동작
- **사운드**: 공격/피격/레벨업 시 효과음 재생, BGM 자동 전환
- **게임 오버**: 전원 사망 → 게임 오버 화면, 재시작 버튼 동작
- **스탯 선택**: 레벨업 시 카드 3장 표시, 선택 후 스탯 반영
- **HP바**: 화면 좌상단 HUD HP바 표시, 피격 시 Lerp 감소, 적/보스 HP바 통합 동작
- **스킬**: Q키 → 각 직업 스킬 발동, Gizmos로 범위 확인
- **웨이브**: 적 전멸 → 5초 후 다음 웨이브 시작, WaveUI 갱신
- **보스**: 5웨이브 → 보스 스폰, HP 50%/20%에서 페이즈 전환
- **GoblinTNT**: 플레이어 접근 → 자폭 딜레이 후 범위 폭발
- **엘리트**: 고웨이브에서 빨간 색상 적 스폰 + 강화된 스탯
