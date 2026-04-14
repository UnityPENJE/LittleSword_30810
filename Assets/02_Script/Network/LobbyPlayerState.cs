using Unity.Netcode;
using UnityEngine;

namespace LittleSword.Network
{
    // ============================================================
    // LobbyPlayerState: 로비에 접속한 플레이어 한 명의 상태를 관리
    // ============================================================
    // NetworkBehaviour를 상속받아서 네트워크 동기화를 지원해.
    // 직업 선택, Ready 상태가 서버-클라이언트 간에 자동으로 동기화돼.
    //
    // SpawnAsPlayerObject로 스폰되기 때문에 씬 전환 시 자동으로 유지됨.
    //
    // NetworkVariable = 서버에서 값이 바뀌면 모든 클라이언트에게 자동 전달
    // ServerRpc       = 클라이언트 → 서버 방향으로 함수를 원격 호출
    // ============================================================
    public class LobbyPlayerState : NetworkBehaviour
    {
        // 선택된 직업 - 서버가 권한을 가짐, 모든 클라이언트가 읽기 가능
        public NetworkVariable<PlayerClassType> SelectedClass = new NetworkVariable<PlayerClassType>(
            PlayerClassType.Warrior,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        // 준비 완료 여부 - 서버가 권한을 가짐, 모든 클라이언트가 읽기 가능
        public NetworkVariable<bool> IsReady = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        // 네트워크에서 스폰될 때 호출 (Awake/Start 대신 이걸 써야 네트워크가 준비된 상태)
        public override void OnNetworkSpawn()
        {
            // NetworkVariable 값이 바뀔 때 UI 갱신 콜백 등록
            SelectedClass.OnValueChanged += OnClassChanged;
            IsReady.OnValueChanged += OnReadyChanged;

            // 스폰 직후 현재 값을 LobbyManager에 알려서 UI 초기화
            LobbyManager.Instance?.OnPlayerStateSpawned(this);
        }

        public override void OnNetworkDespawn()
        {
            SelectedClass.OnValueChanged -= OnClassChanged;
            IsReady.OnValueChanged -= OnReadyChanged;

            LobbyManager.Instance?.OnPlayerStateDespawned(this);
        }

        // ─── ServerRpc: 클라이언트가 서버에게 직업 변경을 요청 ──────────
        // [ServerRpc(RequireOwnership = true)]: 이 오브젝트의 소유자(Owner)만 호출 가능
        [ServerRpc(RequireOwnership = true)]
        public void SelectClassServerRpc(PlayerClassType classType)
        {
            // 이미 Ready 상태면 직업 변경 불가
            if (IsReady.Value) return;
            SelectedClass.Value = classType;
        }

        // ─── ServerRpc: 클라이언트가 서버에게 Ready 상태 토글을 요청 ────
        [ServerRpc(RequireOwnership = true)]
        public void SetReadyServerRpc(bool ready)
        {
            IsReady.Value = ready;

            // Ready 상태가 바뀔 때마다 LobbyManager에서 전원 Ready 여부 체크
            LobbyManager.Instance?.CheckAllReady();
        }

        // ─── NetworkVariable 변경 콜백 ────────────────────────────────
        // 직업이 바뀌었을 때 (서버 포함 모든 클라이언트에서 호출)
        private void OnClassChanged(PlayerClassType previousValue, PlayerClassType newValue)
        {
            LobbyManager.Instance?.RefreshPlayerList();
        }

        // Ready 상태가 바뀌었을 때 (서버 포함 모든 클라이언트에서 호출)
        private void OnReadyChanged(bool previousValue, bool newValue)
        {
            LobbyManager.Instance?.RefreshPlayerList();
        }
    }
}
