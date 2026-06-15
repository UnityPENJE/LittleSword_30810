using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Logger = LittleSword.Common.Logger;

namespace LittleSword.Network.LobbyUI
{
    // 로비 생성 / 참가 관련 UI 입력과 Unity Services Lobby API 호출을 관리하는 컴포넌트입니다.
    // - 익명 로그인(Authentication) 및 Unity Services 초기화를 수행합니다.
    // - 인스펙터에 연결된 UI 요소(TMP 입력 필드, 버튼)를 통해 로비 생성 및 향후 조인 기능을 제공하도록 설계되어 있습니다.
    public class LobbyManager : MonoBehaviour
    {
        #region 인스펙터 연결 필드
        // 로비의 커스텀 데이터에서 Relay Join Code를 저장하는 키
        private const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";

        // 게임 씬 이름 상수: 게임이 시작될 때 로드할 씬의 이름을 정의합니다.
        private const string BATTLE_SCENE_NAME = "Level01";

        // 로비 생성 시 사용할 로비 이름을 입력받는 텍스트 필드 (인스펙터 연결).
        [SerializeField] private TMP_InputField lobbyNameInput;

        // 생성된 또는 조인할 로비의 코드(초대 코드)를 표시/입력하는 텍스트 필드 (인스펙터 연결).
        [SerializeField] private TMP_InputField lobbyCodeInput;

        // 로비 생성 버튼 (인스펙터 연결). 클릭 시 CreateLobbyAsync가 호출됩니다.
        [SerializeField] private Button createLobbyButton;

        // 로비 참가 버튼 (인스펙터 연결). 향후 구현 시 이 버튼에 이벤트를 연결합니다.
        [SerializeField] private Button joinLobbyButton;

        // 빠른 참가(Quick Join) 버튼 (인스펙터 연결). 향후 구현 시 이 버튼에 이벤트를 연결합니다.
        [SerializeField] private Button quickJoinLobbyButton;

        // 현재 가입한 로비를 떠나는 버튼 (인스펙터 연결). 향후 구현 시 이 버튼에 이벤트를 연결합니다.
        [SerializeField] private Button leaveLobbyButton;

        // 게임 시작 버튼 (인스펙터 연결). 향후 구현 시 이 버튼에 이벤트를 연결합니다.
        [SerializeField] private Button startGameButton;
        #endregion

        // 현재 플레이어가 속한 로비의 상태를 보관하는 객체입니다.
        // - null이면 현재 어떤 로비에도 속해있지 않음을 의미합니다.
        private Lobby CurrentLobby;

        // 현재 플레이어가 현재 로비의 호스트인지 여부를 반환합니다.
        // - CurrentLobby가 null이면 false.
        // - 로비의 HostId와 AuthenticationService.Instance.PlayerId를 비교하여 결정합니다.
        private bool Ishost => CurrentLobby != null && CurrentLobby.HostId == AuthenticationService.Instance.PlayerId;

        #region 유니티 이벤트

        // Awake에서 Unity Services 초기화 및 익명 로그인(Authentication)을 수행합니다.
        // 비동기 초기화이므로 예외가 발생할 경우 로깅합니다.
        private async void Awake()
        {
            // Unity Services 초기화: Lobby / Authentication 사용을 위해 필요합니다.
            await UnityServices.InitializeAsync();

            // 익명 로그인: AuthenticationService를 통해 PlayerId를 발급받습니다.
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Logger.Log($"익명 로그인 성공 : Player ID : {AuthenticationService.Instance.PlayerId}");
        }

        private void Start()
        {
            // 호스트가 아닌 경우 게임 시작 버튼을 비활성화하여 다른 플레이어가 시작할 수 없도록 합니다.
            startGameButton.interactable = false;
        }

        // OnEnable에서 버튼 클릭 리스너를 연결합니다.
        // - 리스너는 OnDisable에서 반드시 제거해야 메모리 누수/중복 호출을 방지할 수 있습니다.
        private void OnEnable()
        {
            // createLobbyButton 클릭 시 입력된 텍스트를 사용하여 로비 생성 호출
            createLobbyButton.onClick.AddListener(() => CreateLobbyAsync(lobbyNameInput.text));

            // joinLobbyButton 클릭 시 입력된 텍스트를 사용하여 로비 조인 호출
            joinLobbyButton.onClick.AddListener(() => JoinLobbyAsync(lobbyCodeInput.text));

            // quickJoinLobbyButton 클릭 시 빠른 참가 호출
            quickJoinLobbyButton.onClick.AddListener(() => QuickJoinLobbyAsync());

            // leaveLobbyButton 클릭 시 로비 나가기 호출
            leaveLobbyButton.onClick.AddListener(() => LeaveLobbyAsync());

            // startGameButton 클릭 시 게임 시작 호출
            startGameButton.onClick.AddListener(() => GameStart());
        }

        // OnDisable에서 등록한 모든 버튼 리스너를 제거합니다.
        private void OnDisable()
        {
            // 안전을 위해 createLobbyButton의 모든 리스너를 제거합니다.
            createLobbyButton.onClick.RemoveAllListeners();

            // 안전을 위해 joinLobbyButton의 모든 리스너를 제거합니다.
            joinLobbyButton.onClick.RemoveAllListeners();

            // 안전을 위해 quickJoinLobbyButton의 모든 리스너를 제거합니다.
            quickJoinLobbyButton.onClick.RemoveAllListeners();

            // 안전을 위해 leaveLobbyButton의 모든 리스너를 제거합니다.
            leaveLobbyButton.onClick.RemoveAllListeners();

            // 안전을 위해 startGameButton의 모든 리스너를 제거합니다.
            startGameButton.onClick.RemoveAllListeners();

            CancelInvoke(nameof(SendHeartbeatAsync)); // 로비를 떠날 때 Heartbeat 전송 중지
            CancelInvoke(nameof(PollingLobbyAsync)); // 로비를 떠날 때 로비 정보 갱신 중지
        }

        #endregion

        #region 로비 관련 메서드

        // 새로운 로비를 생성합니다.
        // - 성공 시 CurrentLobby를 갱신하고 UI의 입력 필드에 로비 이름과 로비 코드를 설정합니다.
        // - 예외가 발생하면 Logger.LogError로 메시지를 출력합니다.
        // - Unity Services의 비동기 API를 사용하므로 호출자는 예외 및 상태를 고려해야 합니다.

        // #1 Relay 서버 할당
        private async void CreateLobbyAsync(string lobbyName = "Lobby", int maxPlayers = 4)
        {
            try
            {
                // #1 Relay 서버 할당: Unity Relay 서비스를 통해 게임 세션을 호스팅할 서버를 할당받습니다.
                // 호스트 제외한 최대 플레이어 수
                var relayAlloc = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);

                // #2 Relay Join Code 생성: 할당받은 Relay 서버에 참가할 수 있는 초대 코드를 생성합니다.
                var relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(relayAlloc.AllocationId);

                Logger.Log($"Relay 서버 할당 성공 : {relayJoinCode}");

                // #3 통신 방식 설정
                // 보안 프로토콜 : dtls(UDP 기반) 또는 tls(TCP 기반) 선택 가능
                var relayServerData = relayAlloc.ToRelayServerData("dtls");

                // Unity Netcode의 UnityTransport 컴포넌트에 Relay 서버 정보 설정
                var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();

                // Unity Netcode의 UnityTransport 컴포넌트에 Relay 서버 정보를 설정하여
                // 네트워크 통신이 Relay 서버를 통해 이루어지도록 합니다.
                utp.SetRelayServerData(relayServerData);

                // #4 로비 옵션 생성: Unity Lobby 서비스를 통해 새로운 로비를 생성합니다.
                var lobbyOptions = new CreateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        // 로비의 커스텀 데이터에 Relay Join Code를 저장하여 참가자들이
                        // 이 정보를 통해 Relay 서버에 접속할 수 있도록 합니다.
                        [KEY_RELAY_JOIN_CODE] = new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode)
                    }
                };

                // #5 로비 생성 With Options: LobbyService를 통해 새로운 로비를 생성합니다.
                // LobbyService를 통해 로비 생성 요청
                CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, lobbyOptions);
                DisplayCurrentLobby();  // 반복 되는 UI 업데이트 로직을 별도 메서드로 분리하여 가독성 향상

                BindingLobbyCallbacks(); // 로비 이벤트(플레이어 입장/퇴장 등)를 수신하기 위한 콜백 등록

                // 로비를 생성한 플레이어는 호스트이므로 주기적으로 Heartbeat를 보내 로비가 유효함을 유지하고,
                NetworkManager.Singleton.StartHost(); // Unity Netcode의 호스트로 네트워크 세션 시작

                if (Ishost)
                {
                    // 호스트만 게임을 시작할 수 있도록 게임 시작 버튼을 활성화합니다.
                    startGameButton.interactable = true;

                    //InvokeRepeating(nameof(SendHeartbeatAsync), 15f, 15f); // 로비 호스트인 경우 15초마다 Heartbeat 전송 시작
                    InvokeRepeating(nameof(SendHeartbeatAsync), 5f, 5f); // 로비 호스트인 경우 5초마다 Heartbeat 전송 시작

                    //InvokeRepeating(nameof(PollingLobbyAsync), 15f, 15f); // 로비 호스트인 경우 15초마다 로비 정보 갱신 시작
                    InvokeRepeating(nameof(PollingLobbyAsync), 3f, 3f); // 로비 호스트인 경우 3초마다 로비 정보 갱신 시작    
                }
            }
            catch (Exception e)
            {
                // 네트워크 문제, 인증 문제 등 다양한 예외가 발생할 수 있으므로 로깅을 남깁니다.
                Logger.logError(e.Message);
            }
        }

        private void DisplayCurrentLobby()
        {
            // 생성된 로비 정보를 UI에 반영
            lobbyNameInput.text = CurrentLobby.Name;
            lobbyCodeInput.text = CurrentLobby.LobbyCode;

            Logger.Log($"로비 생성 완료 : {CurrentLobby.Name} , 코드 : {CurrentLobby.LobbyCode}");
        }

        // 로비 조인
        private async void JoinLobbyAsync(string lobbyCode)
        {
            // 네트워크 호출 중 발생할 수 있는 예외를 처리하기 위한 try 블록
            try
            {
                // 초대 코드로 로비 참가를 비동기 호출하고 결과를 CurrentLobby에 저장
                CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
                // 참가 성공 시 UI 및 내부 상태를 갱신
                DisplayCurrentLobby();

                // Relay 서버 정보 설정: 참가한 로비의 커스텀 데이터에서
                // Relay Join Code를 추출하여 Relay 서버에 접속하기 위한 정보를 설정합니다.
                await RelaySetup(CurrentLobby);

                // 클라이언트 가동 - Unity Netcode의 클라이언트로 네트워크 세션 시작
                NetworkManager.Singleton.StartClient();
            }
            catch (Exception e)
            {
                // 예외 발생 시 로깅(네트워크 오류, 인증 오류 등)
                Logger.logError(e.Message);
            }
        }

        // 로비 퀵 조인
        private async void QuickJoinLobbyAsync()
        {
            // 네트워크 호출 중 발생할 수 있는 예외를 잡기 위한 try 블록
            try
            {
                // Unity LobbyService의 QuickJoin API를 호출하여 가능한 로비에 자동으로 참가 시도
                // 성공하면 CurrentLobby에 참가한 로비 정보가 할당됩니다.
                CurrentLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

                DisplayCurrentLobby();

                // Relay 서버 정보 설정: 참가한 로비의 커스텀 데이터에서
                // Relay Join Code를 추출하여 Relay 서버에 접속하기 위한 정보를 설정합니다.
                await RelaySetup(CurrentLobby);

                // 클라이언트 가동 - Unity Netcode의 클라이언트로 네트워크 세션 시작
                NetworkManager.Singleton.StartClient();
            }
            catch (Exception e)
            {
                // 예외 발생 시 로깅: 네트워크 오류, 인증 오류, 조건 불일치 등
                Logger.logError(e.Message);
            }
        }

        // 로비 나가기
        private async void LeaveLobbyAsync()
        {
            // 네트워크 호출 중 발생할 수 있는 예외를 처리하기 위한 try 블록
            try
            {
                // 현재 로비에서 나가기 위해 LobbyService의 LeaveLobbyAsync 메서드를 호출
                await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, AuthenticationService.Instance.PlayerId);

                // 나가기 성공 시 CurrentLobby를 null로 초기화하여 로비 상태를 리셋
                ClearCurrentLobby();

                // (선택) UI 갱신 또는 추가 처리 필요 시 여기에 호출 추가
                // 예: ClearLobbyUI();
            }
            catch (Exception e)
            {
                // 예외 발생 시 로깅: 네트워크 오류, 인증 오류 등
                Logger.logError(e.Message);
            }
        }

        private void ClearCurrentLobby()
        {
            // 로비 정보를 초기화하여 UI를 기본 상태로 되돌립니다.
            lobbyNameInput.text = "";
            lobbyCodeInput.text = "";
        }

        #endregion

        #region 로비 유지관리 메서드
        // Heartbeat 전송
        // - 서버(로비 서비스)에 주기적으로 핑을 보내 로비가 유효함을 유지하도록 합니다.
        // - 주로 로비가 장시간 비활성화되어 자동으로 삭제되는 것을 방지하기 위해 사용됩니다.
        private async void SendHeartbeatAsync()
        {
            // 현재 어떤 로비에도 속해있지 않다면 아무 동작도 하지 않습니다.
            if (CurrentLobby == null) return;

            // LobbyService에 Heartbeat 핑을 비동기로 전송합니다. 실패시 예외가 발생할 수 있습니다.
            await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
            // 전송 성공 로그를 남깁니다.
            Logger.Log("Heartbeat 전송 성공");
        }

        // 로비 정보 갱신(폴링)
        // - 서버에서 최신 로비 정보를 주기적으로 받아와 CurrentLobby를 갱신합니다.
        // - 참가자 수, 플레이어 목록, 속성 등의 변경사항을 반영하기 위해 사용됩니다.
        private async void PollingLobbyAsync()
        {
            // 현재 어떤 로비에도 속해있지 않다면 갱신을 수행하지 않습니다.
            if (CurrentLobby == null) return;

            // 서버에서 최신 로비 정보를 가져와 CurrentLobby를 덮어씁니다.
            CurrentLobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
            // 갱신된 로비 정보를 로그로 남깁니다(로비 이름 및 접속자 수 표시).
            Logger.Log($"로비 정보 갱신 : {CurrentLobby.Name}, 접속자 수 : {CurrentLobby.Players.Count}");
        }
        #endregion

        #region 로비 콜백
        // 로비 이벤트(플레이어 입장/퇴장 등)를 바인딩하는 메서드
        private void BindingLobbyCallbacks()
        {
            // 로비 이벤트를 전달받을 콜백 객체 생성
            LobbyEventCallbacks callbacks = new LobbyEventCallbacks();

            // 플레이어가 로비에 들어올 때 호출될 이벤트 핸들러 등록
            callbacks.PlayerJoined += OnPlayerJoined;

            // 플레이어가 로비에서 나갈 때 호출될 이벤트 핸들러 등록
            callbacks.PlayerLeft += OnPlayerLeft;

            // 생성한 콜백 객체를 LobbyService에 등록하여 이벤트 수신 시작
            // 주의: CurrentLobby가 null이면 예외가 발생할 수 있으므로 호출 전 유효성 검사 권장
            LobbyService.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, callbacks);
        }

        // 플레이어가 로비에서 떠났을 때 호출되는 콜백 핸들러
        // - 매개변수 obj는 떠난 플레이어 ID의 리스트로 가정
        private void OnPlayerLeft(List<int> obj)
        {
            // 떠난 각 플레이어 ID에 대해 로그 기록
            foreach (var player in obj)
            {
                Logger.Log($"플레이어 ID : {player} 접속 종료");
            }
        }

        // 플레이어가 로비에 접속했을 때 호출되는 콜백 핸들러
        // - 매개변수 players는 접속한 플레이어 정보 리스트(LobbyPlayerJoined 타입)
        private void OnPlayerJoined(List<LobbyPlayerJoined> players)
        {
            // 접속한 각 플레이어의 정보를 순회하며 로그 기록
            foreach (var player in players)
            {
                Logger.Log($"플레이어 접속 : {player.Player.Id}");
            }
        }

        #endregion

        #region Relay 관련 메서드
        private async Task RelaySetup(Lobby currentLobby)
        {
            try
            {
                // #1  - Relay Join Code 추출: 로비의 커스텀 데이터에서 Relay Join Code를 가져옵니다.
                string joincode = currentLobby.Data[KEY_RELAY_JOIN_CODE].Value;

                // #2  - Relay Alloc 정보 생성: Relay Join Code를 사용하여 Relay 서버에 접속하기 위한 정보를 생성합니다.
                JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(joincode);

                // #3  - UnityTransport 설정: Unity Netcode의 UnityTransport 컴포넌트에 Relay 서버 정보를 설정하여
                //                            네트워크 통신이 Relay 서버를 통해 이루어지도록 합니다.
                var relayServerData = joinAlloc.ToRelayServerData("dtls");
                var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
                utp.SetRelayServerData(relayServerData);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }
        }

        private async void GameStart()
        {
            try
            {
                // 호스트가 호출하면 NetworkManager의 씬 매니저를 통해 모든 클라이언트에 씬 전환을 동기화합니다.
                // LoadSceneMode.Single은 현재 씬을 대체하여 단일 씬으로 로드합니다.
                NetworkManager.Singleton.SceneManager.LoadScene(BATTLE_SCENE_NAME, LoadSceneMode.Single);
            }
            catch (Exception e)
            {
                Logger.logError(e.Message);
            }
        }
        #endregion
    }
}












