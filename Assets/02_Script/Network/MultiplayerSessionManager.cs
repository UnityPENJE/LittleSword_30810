using System;
using System.Collections.Generic;
using LittleSword.Common;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine.SceneManagement;
using Logger = LittleSword.Common.Logger;

namespace LittleSword.Network
{
    /// <summary>
    /// Unity Multiplayer Services SDK를 사용한 세션 관리자
    /// - Lobby, Relay, Matchmaking을 통합 관리하는 고수준 API를 제공합니다.
    /// - 세션 생성, 빠른 참가, 나가기 기능을 제공합니다.
    /// - 씬 전환 시에도 유지되는 싱글턴 패턴을 사용합니다.
    /// </summary>
    public class MultiplayerSessionManager : Singleton<MultiplayerSessionManager>
    {
        // ===== 상태 =====

        /// <summary>
        /// 현재 활성화된 세션
        /// - ISession 인터페이스로 세션(같은 룸)의 모든 정보와 이벤트에 접근 가능
        /// - Name, Code, MaxPlayers, Players 등의 정보 포함
        /// - PlayerJoined, PlayerLeft 등의 이벤트 포함
        /// - null이면 세션에 참가하지 않은 상태입니다.
        /// </summary>
        private ISession ActiveSession { get; set; }

        // ===== 상수 =====

        /// <summary>
        /// 배틀 씬 이름 (게임 플레이 씬)
        /// </summary>
        private const string BATTLE_SCENE_NAME = "Level01";

        /// <summary>
        /// 로비 씬 이름 (메인 메뉴 씬)
        /// </summary>
        private const string LOBBY_SCENE_NAME = "Lobby";

        // ===== 이벤트 =====

        /// <summary>
        /// 세션 정보 업데이트 이벤트
        /// - 세션 생성/참가 시 발생합니다.
        /// - UI에서 구독하여 세션 이름과 코드를 표시할 수 있습니다.
        /// </summary>
        /// <param name="sessionName">세션 이름</param>
        /// <param name="sessionCode">세션 코드</param>
        public event Action<string, string> OnUpdateSessionInfo;

        #region 유니티 이벤트

        /// <summary>
        /// MonoBehaviour Start 이벤트 (비동기)
        /// - Unity Services를 초기화합니다.
        /// - 익명 인증으로 자동 로그인합니다.
        /// </summary>
        /// <remarks>
        /// Unity Services 초기화:
        /// 1. UnityServices.InitializeAsync() - 모든 Unity Services 초기화
        /// 2. AuthenticationService.SignInAnonymouslyAsync() - 익명 로그인
        ///    - 자동으로 고유한 PlayerId 부여
        ///    - Multiplayer, Lobby, Relay 서비스 이용 가능
        ///
        /// Singleton Start:
        /// - DontDestroyOnLoad로 씬 전환 시에도 유지됨
        /// - 한 번만 초기화됨
        /// </remarks>
        private async void Start()
        {
            try
            {
                // Unity Services 초기화
                await UnityServices.InitializeAsync();

                // 익명 로그인
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                Logger.Log($"익명 로그인 성공 : Player ID : {AuthenticationService.Instance.PlayerId}");
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }
        }

        #endregion

        #region Session 관련 메서드

        /// <summary>
        /// 세션 생성 (Host로 시작)
        /// - SessionOptions를 설정하여 새 세션을 생성합니다.
        /// - Relay 네트워크를 자동으로 설정합니다. (WithRelayNetwork)
        /// - 생성된 세션 정보를 UI 이벤트로 전달합니다.
        /// </summary>
        /// <remarks>
        /// SessionOptions:
        /// - Name: 세션 이름 (UI에 표시됨)
        /// - IsLocked: 세션 잠금 여부 (true면 참가 불가)
        /// - IsPrivate: 비공개 세션 여부 (true면 코드로만 참가 가능)
        /// - MaxPlayers: 최대 플레이어 수
        ///
        /// WithRelayNetwork():
        /// - Relay 네트워크를 자동으로 설정하는 확장 메서드
        /// - LobbyManager에서 수동으로 했던 Relay 할당/JoinCode 생성을 자동화
        /// - 내부적으로 RelayService.CreateAllocationAsync() 등을 호출
        ///
        /// CreateSessionAsync:
        /// - 세션을 생성하고 ISession 인터페이스를 반환
        /// - 자동으로 Host로 시작됨
        /// - Lobby + Relay가 통합되어 생성됨
        /// </remarks>
        /// <param name="sessionName">생성할 세션의 이름</param>
        public async void CreateSessionAsync(string sessionName)
        {
            try
            {
                // Session Options 설정
                var options = new SessionOptions
                {
                    Name = sessionName,         // 세션 이름
                    IsLocked = false,           // 잠금 해제 (참가 가능)
                    IsPrivate = false,          // 공개 세션 (빠른 참가 가능)
                    MaxPlayers = 4              // 최대 4명
                }.WithRelayNetwork();           // Relay 네트워크 자동 설정

                // 세션 생성 (자동으로 Host 시작)
                ActiveSession = await MultiplayerService.Instance.CreateSessionAsync(options);

                // UI 이벤트 발생 (세션 이름, 코드 전달)
                OnUpdateSessionInfo?.Invoke(ActiveSession.Name, ActiveSession.Code);

                Logger.Log($"세션 생성 완료 {ActiveSession.Name} - {ActiveSession.Code}");
            }
            catch (Exception e)
            {
                Logger.logError(e.Message);
            }
        }

        public async void QuickJoinSessionAsync()
        {
            // Join Options 설정
            var joinOptions = new QuickJoinOptions
            {
                Filters = new List<FilterOption>
                {
                    new(FilterField.AvailableSlots, "1", FilterOperation.GreaterOrEqual),
                    // new(FilterField.Name, "Red", FilterOperation.Contains)
                },
                Timeout = TimeSpan.FromSeconds(5),
                CreateSession = true
            };

            // Session Options 설정
            var sessionOptions = new SessionOptions
            {
                IsLocked = false,
                IsPrivate = false,
                MaxPlayers = 4
            }.WithRelayNetwork();

            // 퀵 조인 시도
            ActiveSession = await MultiplayerService.Instance.MatchmakeSessionAsync(joinOptions, sessionOptions);
            OnUpdateSessionInfo?.Invoke(ActiveSession.Name, ActiveSession.Code);
            Logger.Log($"세션 조인 완료: {ActiveSession.Name} - {ActiveSession.Code}");
        }

        /// <summary>
        /// 현재 참가 중인 세션에서 나갑니다.
        /// - 내부적으로 ISession.LeaveAsync()를 호출하여 세션을 떠납니다.
        /// - 나간 후 로비 씬(LOBBY_SCENE_NAME)으로 씬 전환합니다.
        /// </summary>
        public async void LeaveSession()
        {
            // ActiveSession이 null일 경우 예외가 발생할 수 있으므로 호출 전 확인이 필요합니다.
            // (여기서는 호출자가 유효한 세션 참여 상태임을 보장한다고 가정합니다.)
            await ActiveSession.LeaveAsync(); // 세션에서 비동기적으로 탈퇴
            SceneManager.LoadScene(LOBBY_SCENE_NAME, LoadSceneMode.Single); // 로비 씬으로 전환
        }

        public void StartSession()
        {
            NetworkManager.Singleton.SceneManager.LoadScene(BATTLE_SCENE_NAME, LoadSceneMode.Single);
        }

        #endregion
    }
}














