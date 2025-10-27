using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarUI : MonoBehaviour
{
    [Header("Refs (auto si los dejas vacíos)")]
    [SerializeField] private PlayerHealth playerHealth; 
    [SerializeField] private Image fillImage;          

    [Header("Visual")]
    [SerializeField] private float smooth = 8f;         // 0 = instantáneo

    private void Awake()
    {
        if (!playerHealth)
        {
            var p = GameObject.FindWithTag("Player");
            if (p) playerHealth = p.GetComponent<PlayerHealth>();
        }
        if (!fillImage)
        {
            var t = transform.Find("HealthFill");
            if (t) fillImage = t.GetComponent<Image>();
        }
    }

    private void Update()
    {
        if (!playerHealth || !fillImage) return;

        float max = Mathf.Max(0.0001f, playerHealth.GetMaxEnergy());
        float t = Mathf.Clamp01(playerHealth.GetCurrentEnergy() / max);

        fillImage.fillAmount = (smooth > 0f)
            ? Mathf.Lerp(fillImage.fillAmount, t, smooth * Time.deltaTime)
            : t;
    }
}
