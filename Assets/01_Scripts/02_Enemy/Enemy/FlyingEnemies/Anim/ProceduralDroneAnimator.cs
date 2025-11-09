using System.Collections.Generic;
using UnityEngine;
using System;

public class ProceduralDroneAnimator : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] Transform target;
    [SerializeField] List<SpriteRenderer> coreSprites = new();

    [Header("Rotors")]
    [SerializeField] List<Transform> rotors = new();
    [SerializeField] float rotorSpeedDegPerSec = 900f;
    bool rotorsActive = true;

    [Header("Hover")]
    [SerializeField] bool hoverActive = true;
    [SerializeField] float hoverAmplitude = 0.12f;
    [SerializeField] float hoverSpeed = 1.8f;

    [Header("Alert Tilt & Glow")]
    [SerializeField] bool alertActive = false;
    [SerializeField] float alertTiltAngle = 7f;
    [SerializeField] float alertLerp = 8f;
    [SerializeField] Color alertGlowColor = new Color(0.4f, 0.85f, 1f, 1f);
    [SerializeField] float glowLerp = 6f;

    [Header("Shoot Recoil")]
    [SerializeField] float recoilDistance = 0.12f;
    [SerializeField] float recoilTime = 0.06f;
    [SerializeField] float recoverTime = 0.10f;

    [Header("Death")]
    [SerializeField] float deathTime = 0.8f;
    [SerializeField] float deathFallDistance = 0.6f;
    [SerializeField] float deathTiltAngle = 25f;

    Vector3 baseLocalPos, baseLocalScale;
    Quaternion baseLocalRot;
    float hoverT;
    bool isRecoiling, isDying;

    List<Color> coreBaseColors = new();
    bool warnedRotors, warnedCore;

    void Awake()
    {
        if (!target) target = transform;
        baseLocalPos = target.localPosition;
        baseLocalScale = target.localScale;
        baseLocalRot = target.localRotation;

        CacheBaseCoreColors();
        WarnIfMissing();
    }

    void Update()
    {
        if (isDying) return;

        // Hélices
        if (rotorsActive)
        {
            if (rotors == null || rotors.Count == 0)
            {
                if (!warnedRotors) { Debug.LogWarning("[ProceduralDroneAnimator] No hay hélices asignadas en 'Rotors'."); warnedRotors = true; }
            }
            else
            {
                float delta = rotorSpeedDegPerSec * Time.deltaTime;
                for (int i = 0; i < rotors.Count; i++)
                    if (rotors[i]) rotors[i].Rotate(0f, 0f, -delta, Space.Self);
            }
        }

        // Hover
        if (hoverActive)
        {
            hoverT += Time.deltaTime * hoverSpeed;
            float y = Mathf.Sin(hoverT) * hoverAmplitude;
            var p = baseLocalPos; p.y += y;
            target.localPosition = Vector3.Lerp(target.localPosition, p, 0.25f);
        }

        // Tilt & Glow
        Quaternion toRot = alertActive ? Quaternion.Euler(0, 0, -alertTiltAngle) * baseLocalRot : baseLocalRot;
        target.localRotation = Quaternion.Slerp(target.localRotation, toRot, Time.deltaTime * alertLerp);

        if (coreSprites != null && coreSprites.Count > 0)
        {
            for (int i = 0; i < coreSprites.Count; i++)
            {
                var sr = coreSprites[i];
                if (!sr) continue;
                Color baseC = (i < coreBaseColors.Count) ? coreBaseColors[i] : Color.white;
                Color goal = alertActive ? alertGlowColor : baseC;
                sr.color = Color.Lerp(sr.color, goal, Time.deltaTime * glowLerp);
            }
        }
    }

    public void EnableHover(bool v) { hoverActive = v; if (!v) { target.localPosition = baseLocalPos; hoverT = 0f; } }
    public void SetAlert(bool v) { alertActive = v; }
    public void SetRotorsActive(bool v) { rotorsActive = v; }

    public void PlayShootRecoil(Vector3 localBackDir)
    {
        if (!isRecoiling) StartCoroutine(CoRecoil(localBackDir));
    }
    public void PlayDeath(Action onFinish)
    {
        if (!isDying) StartCoroutine(CoDeath(onFinish));
    }

    System.Collections.IEnumerator CoRecoil(Vector3 localBackDir)
    {
        isRecoiling = true;
        Vector3 start = baseLocalPos;
        Vector3 end = baseLocalPos + localBackDir.normalized * recoilDistance;

        float t = 0f;
        while (t < recoilTime)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / recoilTime);
            target.localPosition = Vector3.Lerp(start, end, k);
            yield return null;
        }

        t = 0f;
        while (t < recoverTime)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / recoverTime);
            target.localPosition = Vector3.Lerp(end, start, k);
            yield return null;
        }

        target.localPosition = start;
        isRecoiling = false;
    }

    System.Collections.IEnumerator CoDeath(Action onFinish)
    {
        isDying = true;
        rotorsActive = false;
        alertActive = false;
        hoverActive = false;

        Vector3 startPos = target.localPosition;
        Quaternion startRot = target.localRotation;
        Vector3 goalPos = startPos + Vector3.down * deathFallDistance;
        Quaternion goalRot = Quaternion.Euler(0, 0, deathTiltAngle) * startRot;

        float t = 0f;
        List<Color> begin = new(coreSprites.Count);
        for (int i = 0; i < coreSprites.Count; i++) begin.Add(coreSprites[i] ? coreSprites[i].color : Color.white);

        while (t < deathTime)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / deathTime);
            target.localPosition = Vector3.Lerp(startPos, goalPos, k);
            target.localRotation = Quaternion.Slerp(startRot, goalRot, k);

            for (int i = 0; i < coreSprites.Count; i++)
            {
                var sr = coreSprites[i];
                if (!sr) continue;
                var bc = begin[i];
                var c = Color.Lerp(bc, new Color(bc.r, bc.g, bc.b, 0f), k);
                sr.color = c;
            }
            yield return null;
        }

        onFinish?.Invoke();
    }

    void CacheBaseCoreColors()
    {
        coreBaseColors.Clear();
        if (coreSprites != null)
            foreach (var sr in coreSprites) coreBaseColors.Add(sr ? sr.color : Color.white);
    }

    void WarnIfMissing()
    {
        if ((rotors == null || rotors.Count == 0) && !warnedRotors)
        { Debug.LogWarning("[ProceduralDroneAnimator] Asigna las hélices en 'Rotors' (p. ej., HelicesDron_0 y HelicesDron_1)."); warnedRotors = true; }
        if ((coreSprites == null || coreSprites.Count == 0) && !warnedCore)
        { Debug.LogWarning("[ProceduralDroneAnimator] (Opcional) Asigna 'Core Sprites' para el glow (p. ej., CuerpoDron_0)."); warnedCore = true; }
    }

#if UNITY_EDITOR
void OnValidate()
{
    if (!target) target = transform;

    // Solo hélices: recoge hijos cuyo nombre contenga "Helices"
    var foundRotors = new List<Transform>();
    foreach (var t in GetComponentsInChildren<Transform>(true))
    {
        if (t == transform) continue;
        string n = t.name.ToLower();
        if (n.Contains("helices"))  // <-- esto excluye "paralashelices" si quieres excluirlo, usa igualdad:
            foundRotors.Add(t);
    }
    // Si quieres excluir explícitamente los aros "ParaLasHelices", haz:
    foundRotors.RemoveAll(t => t.name.ToLower().Contains("paralashelices"));

    if (foundRotors.Count > 0) rotors = foundRotors;

    // (Opcional) coreSprites por si quieres glow
    // coreSprites = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>(true));
}
#endif

}
