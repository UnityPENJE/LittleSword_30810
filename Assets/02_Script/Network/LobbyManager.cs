using UnityEngine;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using Logger = LittleSword.Common.Logger;
using System;

namespace LittleSword.Network.LobbyUI
{
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField lobbyNameInput;
        [SerializeField] private TMP_InputField lobbyCodeInput;
        [SerializeField] private Button createLobbyButton;
        [SerializeField] private Button joinLobbyButton;
        [SerializeField] private Button quizJoinLobbyButton;
        [SerializeField] private Button leaveLobbyButton;


        private Lobby CurrentLobby;

        private async void Awake()
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Logger.Log($"익명 로그인 성공 : Player ID : {AuthenticationService.Instance.PlayerId}");
        }

        private void OnEnable()
        {
            createLobbyButton.onClick.AddListener(() => CreateLobbyAsync(lobbyNameInput.text));
            joinLobbyButton.onClick.AddListener(() => JoinLobbyAsync(lobbyCodeInput.text));

        }

        private void OnDisable()
        {
            createLobbyButton.onClick.RemoveAllListeners();
            joinLobbyButton.onClick.RemoveAllListeners();
        }

        private async void CreateLobbyAsync(string lobbyName = "Lobby", int maxPlayer = 4)
        {
            try
            {
                CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayer);
                DisplayCurrentLobby();
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

        private void DisplayCurrentLobby()
        {
            lobbyNameInput.text = CurrentLobby.Name;
            lobbyCodeInput.text = CurrentLobby.LobbyCode;

            Logger.Log($"로비 생성 완료 : {CurrentLobby.Name}, 코드 : {CurrentLobby.LobbyCode}");
        }
    }
}


