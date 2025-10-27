using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerAmmoUI : MonoBehaviour
{
    [SerializeField] private PlayerAmmo playerAmmo;
    [SerializeField] private PlayerShooter playerShooter;
    [SerializeField] private TMP_Text ammoText; // TMP
    [SerializeField] private Image ammoIcon;    // opcional iconos por tipo
    [SerializeField] private Sprite[] bulletTypeIcons;

    void Reset()
    {
        var p = GameObject.FindWithTag("Player");
        if (p)
        {
            if (!playerAmmo) playerAmmo = p.GetComponent<PlayerAmmo>();
            if (!playerShooter) playerShooter = p.GetComponent<PlayerShooter>();
        }
        if (!ammoText)
        {
            var t = transform.Find("AmmoText");
            if (t) ammoText = t.GetComponent<TMP_Text>();
        }
        if (!ammoIcon)
        {
            var i = transform.Find("AmmoIcon");
            if (i) ammoIcon = i.GetComponent<Image>();
        }
    }

    void Update()
    {
        if (!playerAmmo || !playerShooter || !ammoText) return;

        int idx = playerShooter.GetCurrentBulletIndex();
        bool infinite = playerAmmo.IsInfinite(idx);

        if (infinite)
        {
            ammoText.text = "?";
        }
        else
        {
            int cur = playerAmmo.GetCurrentAmmo(idx);
            int max = playerAmmo.GetMaxAmmo(idx);
            ammoText.text = max > 0 ? $"{cur}/{max}" : cur.ToString();
        }

        if (ammoIcon && bulletTypeIcons != null && idx >= 0 && idx < bulletTypeIcons.Length)
        {
            ammoIcon.sprite = bulletTypeIcons[idx];
            ammoIcon.enabled = (bulletTypeIcons[idx] != null);
        }
    }
}
