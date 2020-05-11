using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[ILRMonoBehaviour]
public class BaseMono
{
    protected MonoBehaviour m_Self;
    public MonoBehaviour Self
    {
        get { return m_Self; }
    }
    public Transform transform { get { return Self.transform; } }
    public GameObject gameObject { get { return Self.gameObject; } }
    public bool enabled
    {
        get { return Self.enabled; }
        set { Self.enabled = value; }
    }

    public void SetMono(MonoBehaviour mono)
    {
        m_Self = mono;
    }

    public void StartCoroutine(IEnumerator ie)
    {
        Self.StartCoroutine(ie);
    }

    public void StopCoroutine(IEnumerator ie)
    {
        Self.StopCoroutine(ie);
    }

    public void Destroy(Object obj)
    {
        UnityEngine.Object.Destroy(obj);
    }

    public void Destroy(Object obj, float t)
    {
        UnityEngine.Object.Destroy(obj, t);
    }

    public T Instantiate<T>(T prefab) where T : Object
    {
        return UnityEngine.Object.Instantiate(prefab);

    }
    public T Instantiate<T>(T org, Vector3 pos, Quaternion rot) where T : UnityEngine.Object
    {
        Debug.Log(org);
        return Object.Instantiate<T>(org, pos, rot);
    }


    public void DestroyImmediate(Object obj)
    {
        UnityEngine.Object.DestroyImmediate(obj);
    }

    public bool CompareTag(string tag)
    {
        return Self.CompareTag(tag);

    }

}