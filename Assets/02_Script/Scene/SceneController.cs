using UnityEngine;
using UnityEngine.SceneManagement;

namespace LittleSword.UI
{
    public class SceneController : MonoBehaviour
    {
        public void LoadScene(string sceneName) => SceneFader.Instance.LoadScene(sceneName);
        public void LoadScene(int index) => SceneFader.Instance.LoadScene(index);
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void Show(GameObject obj) => obj.SetActive(true);
        public void Hide(GameObject obj) => obj.SetActive(false);
        public void Toggle(GameObject obj) => obj.SetActive(!obj.activeSelf);

        [SerializeField] private GameObject[] targets; // 인스펙터에서 대상 지정
        public void ShowTarget(int index) => targets[index].SetActive(true);
        public void HideTarget(int index) => targets[index].SetActive(false);
        public void ToggleTarget(int index) => targets[index].SetActive(!targets[index].activeSelf);
    }
}