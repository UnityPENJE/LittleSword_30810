using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LittleSword.Network;

/// <summary>
/// 세션 생성/참여 UI를 제어하는 뷰 컴포넌트입니다.
/// 버튼 클릭 시 MultiplayerSessionManager의 비동기 작업을 호출하고,
/// 세션 정보 업데이트 이벤트를 받아 UI를 갱신합니다.
/// </summary>
public class SessionUIView : MonoBehaviour
{
    // UI 컴포넌트 연결 (인스펙터에서 할당)
    [SerializeField] private TMP_InputField sessionNameInput;
    [SerializeField] private TMP_InputField sessionCodeInput;
    [SerializeField] private Button createSessionButton;
    [SerializeField] private Button quickJoinSessionButton;
    [SerializeField] private Button startSessionButton;

    /// <summary>
    /// MultiplayerSessionManager 싱글턴 인스턴스에 대한 편의 접근자.
    /// </summary>
    private MultiplayerSessionManager MPSessionManager => MultiplayerSessionManager.Instance;

    /// <summary>
    /// 오브젝트가 활성화될 때 호출됩니다.
    /// 이벤트 구독 및 UI 버튼의 클릭 리스너를 추가합니다.
    /// </summary>
    private void OnEnable()
    {
        // 세션 정보 변경 시 UI를 갱신하도록 이벤트 구독
        MPSessionManager.OnUpdateSessionInfo += UpdateSessionInfo;

        // 버튼 클릭 시 각 동작을 호출
        createSessionButton.onClick.AddListener(() => MPSessionManager.CreateSessionAsync(sessionNameInput.text));
        quickJoinSessionButton.onClick.AddListener(() => MPSessionManager.QuickJoinSessionAsync());
        startSessionButton.onClick.AddListener(() => MPSessionManager.StartSession());
    }

    /// <summary>
    /// 오브젝트가 비활성화될 때 호출됩니다.
    /// 등록한 이벤트/리스너를 정리하여 메모리 누수 및 중복 호출을 방지합니다.
    /// </summary>
    private void OnDisable()
    {
        // 세션 정보 이벤트 구독 해제
        MPSessionManager.OnUpdateSessionInfo -= UpdateSessionInfo;

        // 버튼에 등록된 모든 리스너 제거
        createSessionButton.onClick.RemoveAllListeners();
        quickJoinSessionButton.onClick.RemoveAllListeners();
        startSessionButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// 세션 이름과 코드를 받아 UI 입력 필드를 갱신합니다.
    /// 다른 쓰레드에서 호출될 수 있다면 메인 스레드에서 UI 갱신하도록 고려해야 합니다.
    /// </summary>
    /// <param name="name">세션 이름</param>
    /// <param name="code">세션 코드</param>
    private void UpdateSessionInfo(string name, string code)
    {
        sessionNameInput.text = name;
        sessionCodeInput.text = code;
    }
}
















