using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Logger = LittleSword.Common.Logger;

namespace LittleSword.Network
{
    // ============================================================
    // LobbyManager: 로비 전체 흐름을 관리하는 핵심 클래스
    // ============================================================
    // 게임 흐름:
    //   1. 접속 UI에서 Host/Client 접속
    //   2. 접속 성공 → 직업 선택 패널 표시
    //   3. 직업 선택 후 Ready 버튼 클릭
    //   4. 전원 Ready → 카운트다운 → Level01 씬으로 이동
    //   5. 씬 이동 완료 → 직업별 프리팹 스폰
    //
    // Singleton 패턴: 씬 전체에 하나만 존재하고 어디서든 접근 가능
    // ============================================================
    public class LobbyManager : MonoBehaviour
    {
        // Singleton: LobbyManager.Instance로 어디서든 접근
        public static LobbyManager Instance { get; private set; }

        // ─── 인스펙터 설정 ─────────────────────────────────────────────
        [Header("최대 플레이어 수")]
        [SerializeField] private int maxPlayers = 4;

        [Header("UI 패널")]
        [SerializeField] private GameObject connectPanel;      // 접속 버튼이 있는 패널
        [SerializeField] private GameObject classSelectPanel;  // 직업 선택 패널

        [Header("직업 선택 버튼")]
        [SerializeField] private Button warriorButton;
        [SerializeField] private Button archerButton;
        [SerializeField] private Button lancerButton;
        [SerializeField] private Button healerButton;

        [Header("Ready & 상태")]
        [SerializeField] private Button readyButton;
        [SerializeField] private TextMeshProUGUI readyButtonText;
        [SerializeField] private TextMeshProUGUI countdownText;   // 카운트다운 텍스트

        [Header("플레이어 목록 UI")]
        [SerializeField] private Transform playerListParent;      // 플레이어 목록을 표시할 부모 Transform
        [SerializeField] private GameObject playerEntryPrefab;    // 플레이어 한 명의 UI 프리팹 (TextMeshProUGUI가 붙어 있어야 해)

        [Header("네트워크 프리팹")]
        [SerializeField] private GameObject lobbyPlayerStatePrefab; // LobbyPlayerState가 붙은 프리팹

        [Header("직업별 게임 프리팹")]
        [SerializeField] private GameObject warriorPrefab;
        [SerializeField] private GameObject archerPrefab;
        [SerializeField] private GameObject lancerPrefab;
        [SerializeField] private GameObject healerPrefab;

        [Header("스폰 위치")]
        [SerializeField] private Transform[] spawnPoints;  // Level01 씬에 배치할 스폰 포인트들

        // ─── 내부 상태 ─────────────────────────────────────────────────
        // 접속한 플레이어들의 LobbyPlayerState 목록
        private readonly List<LobbyPlayerState> playerStates = new List<LobbyPlayerState>();

        // 내 LobbyPlayerState (소유 오브젝트)
        private LobbyPlayerState myState;

        // 카운트다운 진행 중 여부
        private bool isCountingDown = false;

        // ─── 유니티 생명주기 ────────────────────────────────────────────
        private void Awake()
        {
            // Singleton 설정: 이미 인스턴스가 있으면 중복 제거
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // 버튼 이벤트 연결
            warriorButton.onClick.AddListener(() => OnClickSelectClass(PlayerClassType.Warrior));
            archerButton.onClick.AddListener(() => OnClickSelectClass(PlayerClassType.Archer));
            lancerButton.onClick.AddListener(() => OnClickSelectClass(PlayerClassType.Lancer));
            healerButton.onClick.AddListener(() => OnClickSelectClass(PlayerClassType.Healer));
            readyButton.onClick.AddListener(OnClickReady);

            // 초기 상태: 접속 패널만 표시
            connectPanel.SetActive(true);
            classSelectPanel.SetActive(false);
            countdownText.gameObject.SetActive(false);

            // 네트워크 콜백 등록
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

                // 씬 이동 완료 콜백: 여기서 직업별 프리팹을 스폰함
                // 반드시 씬 이동 전에 등록해야 해!
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoadCompleted;
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

                if (NetworkManager.Singleton.SceneManager != null)
                    NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoadCompleted;
            }
        }

        // ─── ConnectManager에서 호출하는 접속/종료 콜백 ───────────────
        // 접속 성공 시 ConnectManager가 호출해줌
        public void OnConnected()
        {
            connectPanel.SetActive(false);
            classSelectPanel.SetActive(true);

            // 서버(또는 호스트)라면 클라이언트가 접속할 때 LobbyPlayerState를 스폰해줘야 해
            // → 이미 Start()에서 OnClientConnectedCallback에 등록했으므로 자동 처리됨
        }

        // 연결 종료 시 ConnectManager가 호출해줌
        public void OnDisconnected()
        {
            connectPanel.SetActive(true);
            classSelectPanel.SetActive(false);
            countdownText.gameObject.SetActive(false);
            playerStates.Clear();
            myState = null;
            isCountingDown = false;
            RefreshPlayerList();
        }

        // ─── 네트워크 콜백 ──────────────────────────────────────────────
        // 클라이언트가 서버에 접속했을 때 (서버에서만 실행됨)
        private void OnClientConnected(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer) return;

            // LobbyPlayerState 프리팹을 해당 클라이언트의 PlayerObject로 스폰
            // SpawnAsPlayerObject = 이 오브젝트를 해당 클라이언트의 "플레이어 오브젝트"로 등록
            //                       씬 전환 시 자동으로 유지됨
            GameObject obj = Instantiate(lobbyPlayerStatePrefab);
            obj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, destroyWithScene: false);
        }

        // 클라이언트가 연결 해제됐을 때
        private void OnClientDisconnected(ulong clientId)
        {
            // LobbyPlayerState의 OnNetworkDespawn에서 OnPlayerStateDespawned()를 호출하므로
            // 별도 처리 없이 목록 갱신만 진행
            RefreshPlayerList();
        }

        // ─── LobbyPlayerState에서 호출하는 콜백 ─────────────────────────
        // 새 플레이어 상태 오브젝트가 스폰됐을 때
        public void OnPlayerStateSpawned(LobbyPlayerState state)
        {
            if (!playerStates.Contains(state))
                playerStates.Add(state);

            // 내 소유(Owner)라면 myState에 저장
            if (state.IsOwner)
                myState = state;

            RefreshPlayerList();
        }

        // 플레이어 상태 오브젝트가 디스폰됐을 때
        public void OnPlayerStateDespawned(LobbyPlayerState state)
        {
            playerStates.Remove(state);
            if (myState == state) myState = null;
            RefreshPlayerList();
        }

        // ─── 직업 선택 & Ready ───────────────────────────────────────────
        private void OnClickSelectClass(PlayerClassType classType)
        {
            // 소유한 LobbyPlayerState를 통해 서버에 직업 변경 요청
            myState?.SelectClassServerRpc(classType);
        }

        private void OnClickReady()
        {
            if (myState == null) return;

            // 토글: 현재 Ready이면 해제, 아니면 Ready
            bool newReady = !myState.IsReady.Value;
            myState.SetReadyServerRpc(newReady);

            // 버튼 텍스트 즉시 갱신 (서버 응답 전에도 시각 피드백)
            readyButtonText.text = newReady ? "취소" : "준비 완료";
        }

        // ─── 전원 Ready 체크 ─────────────────────────────────────────────
        // LobbyPlayerState.SetReadyServerRpc()에서 Ready 상태가 바뀔 때마다 호출
        public void CheckAllReady()
        {
            if (!NetworkManager.Singleton.IsServer) return;
            if (playerStates.Count == 0) return;
            if (isCountingDown) return;

            // 최소 2명 이상이어야 시작 (혼자 Ready면 무시)
            // 필요하다면 조건 수정: playerStates.Count >= 2
            foreach (var state in playerStates)
            {
                if (!state.IsReady.Value) return; // 한 명이라도 Ready 아니면 중단
            }

            // 전원 Ready → 카운트다운 시작
            StartCoroutine(CountdownRoutine());
        }

        private IEnumerator CountdownRoutine()
        {
            isCountingDown = true;
            countdownText.gameObject.SetActive(true);

            for (int i = 3; i > 0; i--)
            {
                countdownText.text = $"게임 시작: {i}";
                Logger.Log($"카운트다운: {i}");
                yield return new WaitForSeconds(1f);

                // 카운트다운 도중 누군가 Ready 취소하면 중단
                bool allReady = true;
                foreach (var state in playerStates)
                {
                    if (!state.IsReady.Value) { allReady = false; break; }
                }

                if (!allReady)
                {
                    countdownText.gameObject.SetActive(false);
                    isCountingDown = false;
                    yield break;
                }
            }

            countdownText.text = "게임 시작!";
            yield return new WaitForSeconds(0.5f);

            // 서버가 모든 클라이언트를 Level01 씬으로 이동
            NetworkManager.Singleton.SceneManager.LoadScene("Level01", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        // ─── 씬 이동 완료 → 직업별 프리팹 스폰 ───────────────────────────
        // 씬 이동이 모든 클라이언트에서 완료됐을 때 서버에서 호출됨
        private void OnSceneLoadCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode,
            List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (!NetworkManager.Singleton.IsServer) return;
            if (sceneName != "Level01") return;

            // 스폰 포인트 인덱스 (플레이어 수만큼 순서대로 배치)
            int spawnIndex = 0;

            foreach (var state in playerStates)
            {
                // 직업에 맞는 프리팹 선택
                GameObject prefab = GetPrefabForClass(state.SelectedClass.Value);
                if (prefab == null)
                {
                    Logger.Log($"직업 프리팹을 찾을 수 없음: {state.SelectedClass.Value}");
                    continue;
                }

                // 스폰 위치 결정 (스폰 포인트가 있으면 사용, 없으면 원점)
                Vector3 spawnPos = Vector3.zero;
                if (spawnPoints != null && spawnIndex < spawnPoints.Length)
                    spawnPos = spawnPoints[spawnIndex].position;

                spawnIndex++;

                // 직업 프리팹 인스턴스 생성
                GameObject player = Instantiate(prefab, spawnPos, Quaternion.identity);
                NetworkObject netObj = player.GetComponent<NetworkObject>();

                // 해당 플레이어의 소유권과 함께 스폰
                // OwnerClientId = state 오브젝트의 소유자 클라이언트
                netObj.SpawnWithOwnership(state.OwnerClientId);
            }
        }

        // 직업 타입에 따라 대응하는 프리팹 반환
        private GameObject GetPrefabForClass(PlayerClassType classType)
        {
            return classType switch
            {
                PlayerClassType.Warrior => warriorPrefab,
                PlayerClassType.Archer  => archerPrefab,
                PlayerClassType.Lancer  => lancerPrefab,
                PlayerClassType.Healer  => healerPrefab,
                _                       => warriorPrefab,
            };
        }

        // ─── 플레이어 목록 UI 갱신 ───────────────────────────────────────
        // 직업/Ready 상태가 바뀔 때마다 호출해서 UI를 최신 상태로 유지
        public void RefreshPlayerList()
        {
            if (playerListParent == null || playerEntryPrefab == null) return;

            // 기존 목록 UI 전부 제거
            foreach (Transform child in playerListParent)
                Destroy(child.gameObject);

            // 현재 플레이어 목록으로 다시 생성
            foreach (var state in playerStates)
            {
                GameObject entry = Instantiate(playerEntryPrefab, playerListParent);
                TextMeshProUGUI text = entry.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    string readyMark = state.IsReady.Value ? "[준비완료]" : "[대기중]";
                    string className = state.SelectedClass.Value.ToString();
                    string ownerMark = state.IsOwner ? " (나)" : "";
                    text.text = $"{readyMark} {className}{ownerMark}";
                }
            }
        }
    }
}
