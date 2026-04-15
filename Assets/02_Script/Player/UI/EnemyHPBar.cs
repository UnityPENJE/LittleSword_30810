using UnityEngine;
using UnityEngine.UI;

public class EnemyHPBar : MonoBehaviour
{
    [SerializeField] private Image hpFillImage;
    [SerializeField] private float lerpSpeed = 8f;

    private float targetFill = 1f;

    public void Show(int current, int max)
    {
        targetFill = (float)current / max;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        hpFillImage.fillAmount = Mathf.Lerp(hpFillImage.fillAmount, targetFill, Time.deltaTime * lerpSpeed);
    }
}