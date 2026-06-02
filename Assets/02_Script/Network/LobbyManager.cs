using UnityEngine;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using Logger = LittleSword.Common.Logger;
using System;
using System.Security.Cryptography;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

namespace LittleSword.Network.LobbyUI
{
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField lobbyNameInput;
        [SerializeField] private TMP_InputField lobbyCodeInput;
        [SerializeField] private Button createLobbyButton;
        [SerializeField] private Button joinLobbyButton;
        [SerializeField] private Button quickJoinLobbyButton;
        [SerializeField] private Button leaveLobbyButton;
        [SerializeField] private Button startGameButton;


        private Lobby CurrentLobby;
        private bool Ishost => CurrentLobby != null && CurrentLobby.HostId == AuthenticationService.Instance.PlayerId;

        private async void Awake()
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Logger.Log($"익명 로그인 성공 : Player ID : {AuthenticationService.Instance.PlayerId}");
        }

        private void Start()
        {
            startGameButton.interactable = false;
        }

        private void OnEnable()
        {
            createLobbyButton.onClick.AddListener(() => CreateLobbyAsync(lobbyNameInput.text));
            joinLobbyButton.onClick.AddListener(() => JoinLobbyAsync(lobbyCodeInput.text));
            quickJoinLobbyButton.onClick.AddListener(() => QuickJoinLobbyAsync());
            leaveLobbyButton.onClick.AddListener(() => LeaveLobbyAsync());

        }

        private void OnDisable()
        {
            createLobbyButton.onClick.RemoveAllListeners();
            joinLobbyButton.onClick.RemoveAllListeners();
            quickJoinLobbyButton.onClick.RemoveAllListeners();
            leaveLobbyButton.onClick.RemoveAllListeners();

            CancelInvoke(nameof(SendHeartbeatAsync));
            CancelInvoke(nameof(PollingLobbyAsync));
        }

        private async void CreateLobbyAsync(string lobbyName = "Lobby", int maxPlayer = 4)
        {
            try
            {
                CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayer);
                DisplayCurrentLobby();

                if (Ishost)
                {
                    InvokeRepeating(nameof(SendHeartbeatAsync), 5f, 5f);
                    InvokeRepeating(nameof(PollingLobbyAsync), 3f, 3f);
                }

                BindingLobbyCallbacks();
            }
            catch (Exception e)
            {
                Logger.logError(e.Message);
            }
        }

        private async void JoinLobbyAsync(string lobbyCode)
        {
            try
            {
                CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
                DisplayCurrentLobby();
            }
            catch (Exception e)
            {
                Logger.logError(e.Message);
            }
        }

        private async void QuickJoinLobbyAsync()
        {
            try
            {
                CurrentLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
                DisplayCurrentLobby();
            }
            catch (Exception e)
            {
                Logger.logError(e.Message);
            }
        }

        private async void LeaveLobbyAsync()
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, AuthenticationService.Instance.PlayerId);
                ClearCurrentLobby();
            }
            catch (Exception e)
            {
                Logger.logError(e.Message);
            }
        }

        private void ClearCurrentLobby()
        {
            lobbyNameInput.text = "";
            lobbyCodeInput.text = "";
        }

        private void DisplayCurrentLobby()
        {
            lobbyNameInput.text = CurrentLobby.Name;
            lobbyCodeInput.text = CurrentLobby.LobbyCode;

            Logger.Log($"로비 생성 완료 : {CurrentLobby.Name}, 코드 : {CurrentLobby.LobbyCode}");
        }

        private async void SendHeartbeatAsync()
        {
            if (CurrentLobby == null) return;

            await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
            Logger.Log("Hearbeat 전송 성공");
        }

        private async void PollingLobbyAsync()
        {
            if (CurrentLobby == null) return;
            CurrentLobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
            Logger.Log($"로비 정보 갱신 : {CurrentLobby.Name}, 접속자 수 : {CurrentLobby.Players.Count}");
        }

        private void BindingLobbyCallbacks()
        {
            LobbyEventCallbacks callbacks = new LobbyEventCallbacks();
            callbacks.PlayerJoined += OnPlayerJoined;
            callbacks.PlayerLeft += OnPlayerLeft;
            LobbyService.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, callbacks);

        }

        private void OnPlayerLeft(List<int> obj)
        {
            foreach(var player in obj)
            {
                Logger.Log($"플레이어 ID : {player} 접속 종료");
            }
        }

        private void OnPlayerJoined(List<LobbyPlayerJoined> players)
        {
            foreach(var player in players)
            {
                Logger.Log($"플레이어 접속 : {player.Player.Id}");
            }
        }
    }
}


