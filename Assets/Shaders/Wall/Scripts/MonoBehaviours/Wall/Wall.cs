using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Wall : MonoBehaviour
{
    public bool useTag = true;

    //[Sirenix.OdinInspector.ShowIf("useTag")]
    [Tag]
    public string targetTag;

    //[Sirenix.OdinInspector.HideIf("useTag")]
    public LayerMask tagetLayer;

    [Range(0.1f, 5)]
    public float time = 1;
    public Renderer _renderer;

#if UNITY_EDITOR
    private void OnValidate()
    {
        _renderer = GetComponent<Renderer>();
    }
#endif

    private void OnCollisionEnter(Collision collision)
    {
        if (useTag)
        {
            if (collision.gameObject.CompareTag(targetTag))
            {
                CollisionEnter(collision);
            }
        }
        else
        {
            if (((1 << collision.gameObject.layer) & tagetLayer) != 0)
            {
                CollisionEnter(collision);
            }
        }
    }

    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Mathf.Clamp01(Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB));
    }

    private void OnDrawGizmos()
    {
        Bounds b = _renderer.bounds;
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(b.center, b.size);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(CalculateMaxBottom(b), Vector3.one);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(CalculateMinBottom(b), Vector3.one);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(CalculateMaxTop(b), Vector3.one);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(CalculateMinTop(b), Vector3.one);
    }

    Vector3 CalculateMinBottom(Bounds b)
    {
        Vector3 min = b.max;
        min.y = min.y - b.size.y;
        return min;
    }

    Vector3 CalculateMaxBottom(Bounds b)
    {
        return b.min;
    }

    Vector3 CalculateMaxTop(Bounds b)
    {
        Vector3 min = b.min;
        min.y = min.y + b.size.y;
        return min;
    }

    Vector3 CalculateMinTop(Bounds b)
    {
        return b.max;
    }

    void CollisionEnter(Collision collision)
    {
        Vector3 hitPoint = collision.contacts[0].point;
        Bounds b = _renderer.bounds;

        float x = InverseLerp(CalculateMinTop(b), CalculateMinBottom(b), hitPoint);
        float y = InverseLerp(CalculateMinBottom(b), CalculateMaxBottom(b), hitPoint);

        _renderer.material.SetFloat("_UseHit", 1.0f);
        Vector4 v = _renderer.material.GetVector("_FocalPoint");
        v.x = x;
        v.y = y;
        _renderer.material.SetVector("_FocalPoint", v);

        _renderer.material.SetFloat("_Speed", 0);
        _renderer.material.DOKill();

        _renderer.material.DOFloat(1, "_Speed", time).OnComplete(() =>
        {
            _renderer.material.SetFloat("_Speed", 0);

            _renderer.material.SetFloat("_UseHit", 0.0f);
            Vector4 v = _renderer.material.GetVector("_FocalPoint");
            v.y = 0.5f;
            v.x = 0.5f;
            _renderer.material.SetVector("_FocalPoint", v);
        });
    }
}
