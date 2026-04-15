using UnityEngine;
using UnityEngine.SceneManagement;

namespace LittleSword
{
    public class DataReset : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1)) Reset();
        }

        private void Reset()
        {
            PlayerPrefs.DeleteAll();
            CurrencyManager.Instance?.Add(-CurrencyManager.Instance.Gold);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Debug.Log("데이터 초기화 완료");
        }
    }
}