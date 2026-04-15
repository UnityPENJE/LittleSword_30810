using UnityEngine;

namespace LittleSword.UI
{
    public class DamageTextSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject damageTextPrefab;
        [SerializeField] private Vector3 offset = new Vector3(0, 0.5f, 0);

        public void Spawn(int damage)
        {
            if (damageTextPrefab == null) return;
            GameObject obj = Instantiate(damageTextPrefab, transform.position + offset, Quaternion.identity);
            obj.GetComponent<DamageText>()?.Show(damage);
        }
    }
}