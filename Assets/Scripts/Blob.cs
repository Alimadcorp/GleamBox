using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Blob : MonoBehaviour
{
    public string id = "";
    public Color[] colorsForPowerups = new Color[5];
    public TextMeshPro view;
    public GameObject blot;
    public bool Blowing = false;

    private bool _destroyed = false;

    public void MakePowerup(string _id)
    {
        id = _id;
        int index = idToInt(id);
        Color c = colorsForPowerups[index];

        SafeSetColor(blot?.GetComponent<SpriteRenderer>(), c);
        SafeSetColor(GetComponent<SpriteRenderer>(), c);
        SafeSetColor(GetComponentInChildren<Light2D>(), c);
        if (view != null) view.color = c;
    }

    public static int idToInt(string _id) => _id switch
    {
        "2x" => 0,
        "4x" => 1,
        "slow" => 2,
        "luck" => 3,
        "More" => 4,
        _ => 0
    };

    public static string intToId(int n) => n switch
    {
        0 => "2x",
        1 => "4x",
        2 => "slow",
        3 => "luck",
        4 => "More",
        _ => ""
    };

    private void Start()
    {
        StartCoroutine(spawn());
        Invoke(nameof(despawn), 15f);
    }

    private IEnumerator spawn()
    {
        var light = GetComponentInChildren<Light2D>();
        while (transform.localScale.x < 2)
        {
            transform.localScale += Vector3.one * Time.unscaledDeltaTime * 5f;
            if (light != null)
                light.intensity = Mathf.Lerp(light.intensity, 0.05f, 0.5f);
            if (transform.localScale.x >= 2)
            {
                transform.localScale = Vector3.one * 2;
                if (light != null) light.intensity = 0.05f;
            }
            yield return null;
        }
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        if (Blowing)
        {
            StopAllCoroutines();
            StartCoroutine(Despawn());
        }
    }

    public void Collect()
    {
        if (_destroyed || gameObject == null) return;
        Blowing = true;
        try
        {
            StartCoroutine(collect());
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            HardDestroy();
        }
    }

    public void despawn()
    {
        if (_destroyed) return;
        Blowing = true;
        StartCoroutine(Despawn());
    }

    private IEnumerator Despawn()
    {
        var col = GetComponent<Collider2D>();
        var light = GetComponentInChildren<Light2D>();
        while (transform.localScale.x > 0)
        {
            if (col != null) col.enabled = false;
            transform.localScale -= Vector3.one * Time.unscaledDeltaTime * 3f;
            if (light != null) light.intensity /= 1.08f;
            yield return null;
        }
        transform.localScale = Vector3.zero;
        GameManager.Instance.blobAmt--;
        HardDestroy();
    }

    private IEnumerator collect()
    {
        CancelInvoke();
        StopCoroutine(spawn());

        if (blot != null) blot.transform.SetParent(null, true);
        if (blot != null) blot.transform.localScale = Vector3.one * 2;

        var col = GetComponent<Collider2D>();
        var light = GetComponentInChildren<Light2D>();

        while (transform.localScale.x > 0)
        {
            if (col != null) col.enabled = false;
            transform.localScale -= Vector3.one * Time.unscaledDeltaTime * 3f;
            if (light != null) light.intensity /= 1.08f;

            if (blot != null)
            {
                blot.transform.localScale += Vector3.one * Time.unscaledDeltaTime * 3f;
                var sr = blot.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color -= new Color(0, 0, 0, Time.unscaledDeltaTime * 3f);
                    if (sr.color.a < 0) Destroy(blot);
                }
            }
            yield return null;
        }

        Destroy(blot);
        HardDestroy();
    }

    private void HardDestroy()
    {
        if (_destroyed) return;
        _destroyed = true;
        Destroy(gameObject);
    }

        private void SafeSetColor(Component comp, Color c)
        {
            switch (comp)
            {
                case SpriteRenderer sr: sr.color = c; break;
                case Light2D l: l.color = c; break;
            }
        }}
