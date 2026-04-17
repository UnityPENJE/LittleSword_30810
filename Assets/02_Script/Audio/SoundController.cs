using UnityEngine;
using UnityEngine.UI;

namespace LittleSword.UI
{
    public class SoundController : MonoBehaviour
    {
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private AudioSource bgmSource; // ← BGM AudioSource 직접 지정

        private void Start()
        {
            float saved = PlayerPrefs.GetFloat("BGMVolume", 1f);
            bgmSlider.value = saved;
            bgmSource.volume = saved;

            bgmSlider.onValueChanged.AddListener(ApplyBGM);
        }

        private void ApplyBGM(float value)
        {
            bgmSource.volume = value;
            PlayerPrefs.SetFloat("BGMVolume", value);
        }
    }
}