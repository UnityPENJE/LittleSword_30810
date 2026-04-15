using UnityEngine;

namespace LittleSword
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;

        private void Start()
        {
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}