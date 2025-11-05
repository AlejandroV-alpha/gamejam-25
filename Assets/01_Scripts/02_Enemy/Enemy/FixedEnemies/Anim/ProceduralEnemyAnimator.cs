using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralEnemyAnimator : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] Transform target;                 
    [SerializeField] bool affectChildrenSprites = true;

    [Header("Emerge/Hide")]
    [SerializeField] float emergeTime = 0.25f;
    [SerializeField] float hideTime = 0.20f;
    [SerializeField] float emergeOvershoot = 1.08f;

    [Header("Shoot Recoil")]
    [SerializeField] float recoilDistance = 0.15f;
    [SerializeField] float recoilTime = 0.07f;
    [SerializeField] float recoverTime = 0.10f;

    [Header("Death")]
    [SerializeField] float deathTime = 0.6f;
    [SerializeField] float deathSink = 0.2f;           // cuánto “se hunde” al morir (local Y-)
    [SerializeField] float deathTwist = 18f;           // grados de giro (Z) al morir

    [Header("Aim Nudge")]
    [SerializeField] float aimNudgeThresholdDeg = 8f;  // si gira más que esto, hace “nudge”
    [SerializeField] float aimNudgeAngle = 3.5f;       // grados de inclinación local Z
    [SerializeField] float aimNudgeTime = 0.07f;       // ida
    [SerializeField] float aimNudgeRecover = 0.10f;    // vuelta

    Vector3 baseLocalPos, baseLocalScale;
    Quaternion baseLocalRot;
    bool isRecoiling;
    bool isNudging;
    float lastYawDeg;                                  // rotación global previa (Z en 2D)

    List<SpriteRenderer> sprites = new();

    void Awake()
    {
        if (!target) target = transform;
        baseLocalPos = target.localPosition;
        baseLocalScale = target.localScale;
        baseLocalRot = target.localRotation;

        if (affectChildrenSprites)
            sprites.AddRange(GetComponentsInChildren<SpriteRenderer>(true));
        else
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr) sprites.Add(sr);
        }

        lastYawDeg = GetWorldYawDeg();
    }

    void Update()
    {
        // Detectar cambio de rotación para el “nudge”
        float yaw = GetWorldYawDeg();
        float delta = Mathf.DeltaAngle(lastYawDeg, yaw);
        if (Mathf.Abs(delta) >= aimNudgeThresholdDeg)
        {
            // Sentido del nudge según giro (izq/dcha)
            float sign = Mathf.Sign(delta);
            PlayAimNudge(sign);
        }
        lastYawDeg = yaw;
    }

    float GetWorldYawDeg()
    {
        // Top-down 2D: usamos el eje Z de la rotación mundial
        return transform.rotation.eulerAngles.z;
    }

    public void PlayEmerge()
    {
        StopAllCoroutines();
        StartCoroutine(CoEmerge());
    }

    public void PlayHide(System.Action onHidden = null)
    {
        StopAllCoroutines();
        StartCoroutine(CoHide(onHidden));
    }

    public void PlayShootRecoil(Vector3 localBackDir)
    {
        if (!isRecoiling)
            StartCoroutine(CoRecoil(localBackDir));
    }

    public void PlayDeath(System.Action onFinish = null)
    {
        StopAllCoroutines();
        StartCoroutine(CoDeath(onFinish));
    }

    public void PlayAimNudge(float directionSign)
    {
        if (!isNudging)
            StartCoroutine(CoAimNudge(directionSign));
    }

    IEnumerator CoEmerge()
    {
        // Aparece desde 0  overshoot  1 y sube alpha
        float t = 0f;
        float mid = emergeTime * 0.6f;
        SetAlphaAll(0f);
        target.localScale = baseLocalScale * 0.7f;

        while (t < mid)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / mid);
            target.localScale = Vector3.Lerp(baseLocalScale * 0.7f, baseLocalScale * emergeOvershoot, k);
            SetAlphaAll(k);
            yield return null;
        }

        t = 0f;
        float rest = emergeTime - mid;
        while (t < rest)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / rest);
            target.localScale = Vector3.Lerp(baseLocalScale * emergeOvershoot, baseLocalScale, k);
            yield return null;
        }

        target.localScale = baseLocalScale;
        SetAlphaAll(1f);
    }

    IEnumerator CoHide(System.Action onHidden)
    {
        float t = 0f;
        Vector3 startS = target.localScale;
        while (t < hideTime)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / hideTime);
            target.localScale = Vector3.Lerp(startS, baseLocalScale * 0.0f, k);
            SetAlphaAll(1f - k);
            yield return null;
        }
        target.localScale = baseLocalScale * 0f;
        SetAlphaAll(0f);
        onHidden?.Invoke();
    }

    IEnumerator CoRecoil(Vector3 localBackDir)
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

    IEnumerator CoAimNudge(float sign)
    {
        isNudging = true;
        Quaternion start = baseLocalRot;
        Quaternion peak = Quaternion.Euler(0, 0, aimNudgeAngle * sign) * baseLocalRot;

        float t = 0f;
        while (t < aimNudgeTime)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / aimNudgeTime);
            target.localRotation = Quaternion.Slerp(start, peak, k);
            yield return null;
        }

        t = 0f;
        while (t < aimNudgeRecover)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / aimNudgeRecover);
            target.localRotation = Quaternion.Slerp(peak, baseLocalRot, k);
            yield return null;
        }

        target.localRotation = baseLocalRot;
        isNudging = false;
    }

    IEnumerator CoDeath(System.Action onFinish)
    {
        float t = 0f;
        Vector3 startPos = target.localPosition;
        Quaternion startRot = target.localRotation;
        Vector3 startScale = target.localScale;

        while (t < deathTime)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / deathTime);
            target.localScale = Vector3.Lerp(startScale, Vector3.zero, k);
            target.localPosition = Vector3.Lerp(startPos, startPos + Vector3.down * deathSink, k);
            target.localRotation = Quaternion.Slerp(startRot, Quaternion.Euler(0, 0, deathTwist), k);
            SetAlphaAll(1f - k);
            yield return null;
        }

        target.localScale = Vector3.zero;
        SetAlphaAll(0f);
        onFinish?.Invoke();
    }

    // ---------- utilidades ----------
    void SetAlphaAll(float a)
    {
        for (int i = 0; i < sprites.Count; i++)
        {
            var sr = sprites[i];
            if (!sr) continue;
            var c = sr.color;
            c.a = a;
            sr.color = c;
        }
    }
}
