using UnityEngine;
using LittleSword.Player;
using LittleSword.InputSystem;
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

        //Logger.Log($"플레이어 접속 : {IsOwner}, IsServer:{IsServer}, IsClient: {IsClient}, OwnerClientId:{OwnerClientId}");
        if (IsOwner)
        {
            cmCamera.Follow = transform;
            inputHandler.enabled = true;
            baseplayer.enabled = true;
        }
        else
        {
            spriteRenderer.flipX = !networkIsFacingRight.Value;

            inputHandler.enabled = false;
            baseplayer.enabled = false;
        }
    }

    private void OnFacingRighChanged(bool previousValue, bool newValue)
    {
        if (!IsOwner)
        {
            spriteRenderer.flipX = !newValue;
        }
        return;
    }

    public override void OnNetworkDespawn()
    {
        Logger.Log("플레이어 접속 종료");
    }
}
