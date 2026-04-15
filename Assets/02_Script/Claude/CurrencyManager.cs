using UnityEngine;

namespace LittleSword
{
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }
        public int Gold { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Gold = PlayerPrefs.GetInt("Gold", 0);
        }

        public void Add(int amount)
        {
            Gold += amount;
            PlayerPrefs.SetInt("Gold", Gold);
            OnGoldChanged?.Invoke(Gold);
        }

        public bool Spend(int amount)
        {
            if (Gold < amount) return false;
            Gold -= amount;
            PlayerPrefs.SetInt("Gold", Gold);
            OnGoldChanged?.Invoke(Gold);
            return true;
        }

        public event System.Action<int> OnGoldChanged;
    }
}