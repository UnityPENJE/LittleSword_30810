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

        if (IsOwner)
        {
            LocalPlayer = this;

            cmCamera.Follow = transform;
            inputHandler.enabled = true;
            baseplayer.enabled = true;

            // HP바 자동 연결
            var hpBar = FindFirstObjectByType<PlayerHPBarUI>();
            hpBar?.SetPlayer(baseplayer);

            // 골드 UI에 스폰 알림
            OnLocalPlayerSpawned?.Invoke(this);
            OnGoldChanged?.Invoke(networkGold.Value);
        }
        else
        {
            spriteRenderer.flipX = !networkIsFacingRight.Value;

            inputHandler.enabled = false;
            baseplayer.enabled = false;
        }
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

    // 클라이언트가 구매 요청 → 서버에서 차감
    [ServerRpc]
    public void SpendGoldServerRpc(int amount)
    {
        if (networkGold.Value < amount) return;
        networkGold.Value -= amount;
    }
}
