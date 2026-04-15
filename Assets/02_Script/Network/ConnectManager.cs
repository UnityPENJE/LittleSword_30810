using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Logger = LittleSword.Common.Logger;

namespace LittleSword.Network
{
    // ============================================================
    // ConnectManager: 멀티플레이어 네트워크 연결을 관리하는 클래스
    // ============================================================
    // Unity Netcode for GameObjects를 사용해서 멀티플레이어를 구현할 때 쓰는 클래스야.
    //
    // 네트워크 역할 3가지:
    //   Server  = 게임 진행을 관리하는 서버 (플레이어 없이 서버만 존재)
    //   Host    = 서버 + 플레이어 둘 다인 사람 (방장 같은 개념)
    //   Client  = 서버에 접속하는 일반 플레이어
    // ============================================================
    public class ConnectManager : MonoBehaviour
    {
        // [SerializeField]: 유니티 인스펙터 창에서 값을 직접 연결할 수 있게 해줌
        // private이지만 인스펙터에서 보이게 하는 트릭!
        [SerializeField] private Button serverButton; // 서버로 시작 버튼
        [SerializeField] private Button hostButton;   // 호스트로 시작 버튼
        [SerializeField] private Button clientButton; // 클라이언트로 접속 버튼
        [SerializeField] private Button closeButton;  // 연결 종료 버튼

        // Start: 게임이 시작될 때 한 번 호출됨
        private void Start()
        {
            // 각 버튼에 클릭 이벤트 함수를 연결
            // onClick.AddListener(함수): 버튼이 클릭됐을 때 해당 함수 실행
            serverButton.onClick.AddListener(OnClickServer);
            hostButton.onClick.AddListener(OnClickHost);
            clientButton.onClick.AddListener(OnClickClient);
            closeButton.onClick.AddListener(OnClickClose);


            // NetworkManager가 이미 존재하면 서버 관련 콜백을 바인딩
            if(NetworkManager.Singleton != null)
            {
                BindingServerCallbacks();
            }
        }

        // OnDisable: 오브젝트가 비활성화될 때 이벤트 리스너 정리 (메모리 누수 방지)
        private void OnDisable()
        {
            // 버튼 클릭 이벤트 해제
            serverButton.onClick.RemoveListener(OnClickServer);
            hostButton.onClick.RemoveListener(OnClickHost);
            clientButton.onClick.RemoveListener(OnClickClient);
            closeButton.onClick.RemoveListener(OnClickClose);


            // NetworkManager가 존재하면 서버 콜백 언바인딩
            if (NetworkManager.Singleton != null)
            {
                UnBindingServerCallbacks();
            }
        }


        // NetworkManager 이벤트에 콜백 함수 연결 (서버/클라이언트 시작/종료 시 알림)
        private void BindingServerCallbacks()
        {
            NetworkManager.Singleton.OnServerStarted += OnServerStartedCallback;
            NetworkManager.Singleton.OnServerStopped += OnServerStoppedCallback;
            NetworkManager.Singleton.OnClientStarted += OnClientStartedCallback;
        }


        // 연결했던 이벤트 콜백을 해제 (OnDisable에서 호출)
        private void UnBindingServerCallbacks()
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStartedCallback;
            NetworkManager.Singleton.OnServerStopped -= OnServerStoppedCallback;
            NetworkManager.Singleton.OnClientStarted -= OnClientStartedCallback;
        }

        // 서버가 시작됐을 때 호출되는 콜백
        private void OnServerStartedCallback()
        {
            Logger.Log("서버 시작");
        }

        // 서버가 종료됐을 때 호출되는 콜백
        private void OnServerStoppedCallback(bool obj)
        {
            Logger.Log("서버 종료");
        }

        // 클라이언트가 시작됐을 때 호출되는 콜백
        private void OnClientStartedCallback()
        {
            Logger.Log("클라이언트 시작");
        }

        // 서버 버튼 클릭 시 호출 - 서버로만 동작 (화면 없이 게임 관리만)
        private void OnClickServer()
        {
            NetworkManager.Singleton.StartServer();
        }

        // 호스트 버튼 클릭 시 호출 - 서버이자 플레이어로 게임 시작
        private void OnClickHost()
        {
            NetworkManager.Singleton.StartHost();
        }

        // 클라이언트 버튼 클릭 시 호출 - 기존 서버/호스트에 접속
        private void OnClickClient()
        {
           NetworkManager.Singleton.StartClient();
        }

        // 종료 버튼 클릭 시 호출 - 네트워크 연결 종료
        private void OnClickClose()
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
