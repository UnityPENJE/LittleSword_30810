using UnityEngine;
using LittleSword.Player;
using LittleSword.InputSystem;
using Unity.Netcode;
using Unity.Netcode.Components;
using Logger = LittleSword.Common.Logger;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private BasePlayer baseplayer;
    [SerializeField] private NetworkTransform networkTransform;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private InputHandler inputHandler;

    private void Awake()
    {
        baseplayer = GetComponent<BasePlayer>();
        networkTransform = GetComponent<NetworkTransform>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        inputHandler = GetComponent<InputHandler>();
    }

    public override void OnNetworkSpawn()
    {
        //Logger.Log($"플레이어 접속 : {IsOwner}, IsServer:{IsServer}, IsClient: {IsClient}, OwnerClientId:{OwnerClientId}");
        if (IsOwner)
        {
            inputHandler.enabled = true;
            baseplayer.enabled = true;
        }
        else
        {
            inputHandler.enabled = false;
            baseplayer.enabled = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        Logger.Log("플레이어 접속 종료");
    }
}
