using UnityEngine;

namespace LittleSword
{
    public class SetactiveController : MonoBehaviour
    {
        [SerializeField] private GameObject target;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                target?.SetActive(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                target?.SetActive(false);
        }
    }
}
