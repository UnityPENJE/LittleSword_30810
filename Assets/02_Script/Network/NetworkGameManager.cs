using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Logger = LittleSword.Common.Logger;
using Random = UnityEngine.Random;

namespace LittleSword.Network
{
    /// <summary>
    /// 서버 시작 시 적을 스폰하고 네트워크 오브젝트로 등록하는 간단한 게임 매니저 클래스입니다.
    /// Host/Server 환경에서만 적을 생성합니다.
    /// </summary>
    public class NetworkGameManager : MonoBehaviour
    {
        /// <summary>
        /// 스폰할 적 프리팹 (씬에 드래그하여 할당).
        /// 이 프리팹은 NetworkObject 컴포넌트를 포함해야 합니다.
        /// </summary>
        [SerializeField] private GameObject enemyPrefab;

        /// <summary>
        /// 적을 스폰할 위치들의 배열 (씬에서 Transform들을 할당).
        /// </summary>
        [SerializeField] private Transform[] spawnPoints;

        // 플레이어 프리팹 저장용
        [SerializeField] private GameObject playerPrefab;

        // 세션 나가기 버튼 (씬에서 할당)
        [SerializeField] private Button leaveSessionButton;

        #region 유니티 이벤트

        private void Start()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                // 서버(또는 호스트)로 시작된 경우, 적과 플레이어를 스폰합니다.
                SpawnEnemies();

                // 플레이어 스폰은 클라이언트가 연결될 때마다 개별적으로 처리하는 것이 일반적입니다.
                SpawnPlayers();

                // 새로 연결된 클라이언트가 있을 때마다 플레이어를 스폰하도록 이벤트를 구독합니다.
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            }

            // NetworkManager의 서버 시작 이벤트에 SpawnEnemies를 구독합니다.
            // 서버가 시작될 때(Host 포함) SpawnEnemies가 호출됩니다.
            // NetworkManager.Singleton.OnServerStarted += SpawnEnemies;
        }

        /// <summary>
        /// MonoBehaviour OnEnable 이벤트
        /// - UI 버튼 이벤트를 등록합니다.
        /// </summary>
        private void OnEnable()
        {
            leaveSessionButton.onClick.AddListener(LeaveSession);
        }

        /// <summary>
        /// MonoBehaviour OnDisable 이벤트
        /// - UI 버튼 이벤트를 해제합니다.
        /// - 메모리 누수 방지
        /// </summary>
        private void OnDisable()
        {
            leaveSessionButton.onClick.RemoveAllListeners();
        }

        private void OnDestroy()
        {
            // NetworkManager 인스턴스가 이미 파괴되었거나 null인 경우 이벤트 해제를 시도하지 않습니다.
            if (NetworkManager.Singleton == null) return;

            // OnClientConnectedCallback에서 등록한 델리게이트를 제거하여
            // 중복 호출 및 메모리 누수를 방지합니다.
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }

        // 클라이언트가 서버에 연결될 때마다 호출되는 콜백 메서드입니다.
        private void OnClientConnected(ulong playerId)
        {
            Logger.Log($"클라이언트 접속 : {playerId}");
            SpawnPlayers(playerId);
        }

        // 서버 시작 시 모든 연결된 클라이언트에 플레이어를 스폰하는 메서드입니다.
        private void SpawnPlayers()
        {
            Logger.Log($"접속 플레이어 수 :  {NetworkManager.Singleton.ConnectedClients.Count}");
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                // 각 클라이언트마다 플레이어를 스폰합니다.
                // 랜덤 위치를 사용합니다.
                var spawnPosition = new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0f);

                // 씬 상에서 위치와 회전 정보를 이용하여 플레이어를 생성합니다.
                var player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

                // 생성한 플레이어를 네트워크 오브젝트로 등록하여 해당 클라이언트에만 동기화되도록 합니다.
                player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            }
        }

        // 개별 클라이언트가 연결될 때마다 해당 클라이언트에 플레이어를 스폰하는 메서드입니다.
        private void SpawnPlayers(ulong playerId)
        {
            Logger.Log($"접속 플레이어 수 :  {NetworkManager.Singleton.ConnectedClients.Count}");
                
            // 각 클라이언트마다 플레이어를 스폰합니다.
            // 랜덤 위치를 사용합니다.
            var spawnPosition = new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0f);

            // 씬 상에서 위치와 회전 정보를 이용하여 플레이어를 생성합니다.
            var player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

            // 생성한 플레이어를 네트워크 오브젝트로 등록하여 해당 클라이언트에만 동기화되도록 합니다.
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(playerId);
        }

        /// <summary>
        /// 등록된 스폰 포인트마다 적을 인스턴스화하고 네트워크에 스폰합니다.
        /// 서버(또는 호스트)가 아닌 경우 동작하지 않습니다.
        /// </summary>
        private void SpawnEnemies()
        {
            // 로컬 인스턴스가 서버인지 확인합니다. 클라이언트에서는 적을 직접 생성하지 않습니다.
            if (!NetworkManager.Singleton.IsServer) return;

            foreach (var point in spawnPoints)
            {
                // 씬 상에서 위치와 회전 정보를 이용하여 적을 생성합니다.
                var enemy = Instantiate(enemyPrefab, point.position, point.rotation);

                // 생성한 적을 네트워크 오브젝트로 등록하여 모든 클라이언트에 동기화되도록 합니다.
                // 적 프리팹은 NetworkObject 컴포넌트를 포함해야 합니다.
                enemy.GetComponent<NetworkObject>().Spawn();
            }
        }

        #endregion

        #region 버튼 콜백

        /// <summary>
        /// 세션 나가기 버튼 클릭 핸들러
        /// - MultiplayerSessionManager를 통해 세션에서 나갑니다.
        /// - Lobby와 Relay 연결을 정리하고 로비 씬으로 돌아갑니다.
        /// </summary>
        private void LeaveSession()
        {
            MultiplayerSessionManager.Instance.LeaveSession();
        }

        #endregion
    }
}




















