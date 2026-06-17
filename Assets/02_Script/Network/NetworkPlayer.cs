using UnityEngine;
using LittleSword.Player;
using LittleSword.InputSystem;
using LittleSword.UI;
using Unity.Netcode;
using Unity.Netcode.Components;
using Logger = LittleSword.Common.Logger;
using Unity.Cinemachine;

[RequireComponent(typeof(NetworkObject), typeof(NetworkTransform))]
[RequireComponent(typeof(NetworkRigidbody2D), typeof(OwnerNetworkAnimator))]
public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private BasePlayer baseplayer;
    [SerializeField] private NetworkTransform networkTransform;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private InputHandler inputHandler;
    private CinemachineCamera cmCamera;

    // 이 클라이언트가 소유한 플레이어 (UI 등에서 참조)
    public static NetworkPlayer LocalPlayer { get; private set; }

    // 플레이어별 골드 (서버만 쓰기 가능, 모두 읽기 가능)
    private NetworkVariable<int> networkGold = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public int Gold => networkGold.Value;
    public event System.Action<int> OnGoldChanged;

    // 플레이어 HP (서버 권위적, 전체 동기화)
    private NetworkVariable<int> networkHP = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public int HP => networkHP.Value;
    public int MaxHP => baseplayer != null ? baseplayer.playerStats.maxHP : 0;
    public event System.Action<int, int> OnHPChanged; // current, max

    // 로컬 플레이어 스폰 시 UI가 구독할 수 있도록 알림
    public static event System.Action<NetworkPlayer> OnLocalPlayerSpawned;

    private void Awake()
    {
        baseplayer = GetComponent<BasePlayer>();
        networkTransform = GetComponent<NetworkTransform>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        inputHandler = GetComponent<InputHandler>();
        cmCamera = FindFirstObjectByType<CinemachineCamera>();
    }

    private void Start()
    {
        if (IsOwner)
        {
            inputHandler.OnMove += HandleMove;
        }
    }

    private void HandleMove(Vector2 ctx)
    {
        bool currentFacingRight = !spriteRenderer.flipX;

        if (networkIsFacingRight.Value != currentFacingRight)
        {
            networkIsFacingRight.Value = currentFacingRight;
        }
    }

    private NetworkVariable<bool> networkIsFacingRight = new NetworkVariable<bool>
        (
            true,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

    public override void OnNetworkSpawn()
    {
        networkIsFacingRight.OnValueChanged += OnFacingRighChanged;
        networkGold.OnValueChanged += (_, newVal) => OnGoldChanged?.Invoke(newVal);
        networkHP.OnValueChanged += OnNetworkHPChanged;

        // 서버가 HP 초기값 설정
        if (IsServer)
            networkHP.Value = baseplayer.playerStats.maxHP;

        if (IsOwner)
        {
            LocalPlayer = this;

            cmCamera.Follow = transform;
            inputHandler.enabled = true;
            baseplayer.enabled = true;

            // HP바는 PlayerHPBarUI가 GetComponentInParent로 직접 이 NetworkPlayer를 찾아 구독함

            // 골드 UI에 스폰 알림
            OnLocalPlayerSpawned?.Invoke(this);
            OnGoldChanged?.Invoke(networkGold.Value);

            // 현재 HP 동기화 및 UI 초기화
            baseplayer.CurrentHP = networkHP.Value;
            baseplayer.SetHPFill((float)networkHP.Value / baseplayer.playerStats.maxHP);
            OnHPChanged?.Invoke(networkHP.Value, baseplayer.playerStats.maxHP);
        }
        else
        {
            spriteRenderer.flipX = !networkIsFacingRight.Value;

            inputHandler.enabled = false;
            baseplayer.enabled = false;
        }
    }

    // networkHP 변경 시 모든 클라이언트에서 호출됨
    private void OnNetworkHPChanged(int prev, int current)
    {
        baseplayer.CurrentHP = current;
        baseplayer.SetHPFill((float)current / baseplayer.playerStats.maxHP);
        baseplayer.OnHPChanged?.Invoke(current, baseplayer.playerStats.maxHP);
        // PlayerHPBarUI가 직접 구독하는 이벤트
        OnHPChanged?.Invoke(current, baseplayer.playerStats.maxHP);

        if (current <= 0 && prev > 0)
        {
            // 씬 리로드는 해당 플레이어 본인 클라이언트에서만 실행
            if (IsOwner)
                baseplayer.TriggerDie();
        }
        else if (current < prev)
        {
            baseplayer.PlayHitAnimation();
        }
    }

    // 서버에서만 호출 가능 - 적이 플레이어에게 데미지 입힐 때 사용
    public void ApplyDamage(int damage)
    {
        if (!IsServer) return;
        if (baseplayer.IsInvincible)
        {
            baseplayer.HandleParry();
            return;
        }
        networkHP.Value = Mathf.Max(0, networkHP.Value - damage);
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner) LocalPlayer = null;
        Logger.Log("플레이어 네트워크 해제");
    }

    private void OnFacingRighChanged(bool previousValue, bool newValue)
    {
        if (!IsOwner)
        {
            spriteRenderer.flipX = !newValue;
        }
    }

    // 서버에서만 호출 (Enemy.Die() 등)
    public void AddGold(int amount)
    {
        if (!IsServer) return;
        networkGold.Value += amount;
    }

    // 패링 등 무적 상태를 서버에 동기화 (오너 클라이언트 → 서버)
    [ServerRpc(RequireOwnership = true)]
    public void SetInvincibleServerRpc(bool value)
    {
        baseplayer.IsInvincible = value;
    }

    // 클라이언트가 구매 요청 → 서버에서 차감
    [ServerRpc]
    public void SpendGoldServerRpc(int amount)
    {
        if (networkGold.Value < amount) return;
        networkGold.Value -= amount;
    }
}
