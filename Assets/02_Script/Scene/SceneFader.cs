using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

namespace LittleSword.UI
{
    public class SceneFader : MonoBehaviour
    {
        public static SceneFader Instance { get; private set; }

        [SerializeField] private Image fadeImage;
        [SerializeField] private float fadeDuration = 0.5f;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
        }

        private void Start() => StartCoroutine(FadeIn());

        public void LoadScene(string sceneName) => StartCoroutine(FadeAndLoad(sceneName));
        public void LoadScene(int index) => StartCoroutine(FadeAndLoad(index));

        private IEnumerator FadeAndLoad(object scene)
        {
            yield return StartCoroutine(FadeOut());
            if (scene is string s) SceneManager.LoadScene(s);
            else if (scene is int i) SceneManager.LoadScene(i);
            yield return StartCoroutine(FadeIn());
        }

        private IEnumerator FadeIn()
        {
            
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeImage.color = new Color(0, 0, 0, 1f - (elapsed / fadeDuration));
                yield return null;
            }
            fadeImage.color = new Color(0, 0, 0, 0);
            fadeImage.gameObject.SetActive(false);

        }

        private IEnumerator FadeOut()
        {
            fadeImage.gameObject.SetActive(true);
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeImage.color = new Color(0, 0, 0, elapsed / fadeDuration);
                yield return null;
            }
            fadeImage.color = new Color(0, 0, 0, 1);
        }
    }
}