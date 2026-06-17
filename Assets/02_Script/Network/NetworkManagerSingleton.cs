using LittleSword.Common;

/// <summary>
/// NetworkManager를 싱글턴 패턴으로 관리하는 래퍼 클래스
/// - Singleton<T> 베이스 클래스를 상속받아 단일 인스턴스를 보장합니다.
/// - 씬 전환 시에도 파괴되지 않습니다. (DontDestroyOnLoad)
/// - Unity Netcode의 NetworkManager와 함께 사용하여 네트워크 설정을 관리합니다.
/// </summary>
/// <remarks>
/// 사용 방법:
/// 1. NetworkManager 게임오브젝트에 이 컴포넌트를 추가합니다.
/// 2. NetworkManagerSingleton.Instance로 어디서든 접근 가능합니다.
/// 3. 씬 전환 시에도 NetworkManager가 유지됩니다.
/// </remarks>
public class NetworkManagerSingleton : Singleton<NetworkManagerSingleton> { }



















