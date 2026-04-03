using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Logger = LittleSword.Common.Logger;

namespace LittleSword.Network
{
    public class ConnectManager : MonoBehaviour
    {
        [SerializeField] private Button serverButton;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button clientButton;
        [SerializeField] private Button closeButton;
        private void Start()
        {
            serverButton.onClick.AddListener(OnClickServer);
            hostButton.onClick.AddListener(OnClickHost);
            clientButton.onClick.AddListener(OnClickClient);
            closeButton.onClick.AddListener(OnClickClose);
        }

        private void OnClickServer()
        {
            //NetworkManager.Singleton
        }

        private void OnClickHost()
        {
            //NetworkManager.Singleton
        }

        private void OnClickClient()
        {
           // NetworkManager.Singleton
        }

        private void OnClickClose()
        {
            //NetworkManager.Singleton
        }




    }
}
    


