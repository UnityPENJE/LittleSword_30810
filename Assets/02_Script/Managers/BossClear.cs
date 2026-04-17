using UnityEngine;

namespace LittleSword
{
    public class BossClear : MonoBehaviour
    {
        [SerializeField] private GameObject clearUI;

        private void OnDestroy()
        {
            clearUI?.SetActive(true);
        }
    }
}