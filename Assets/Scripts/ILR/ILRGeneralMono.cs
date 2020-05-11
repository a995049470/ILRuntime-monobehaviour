using UnityEngine;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;

public class ILRGeneralMono : MonoBehaviour
{
    protected object m_Self;
    protected IType m_Type;
    protected IMethod m_Awake;
    protected IMethod m_Start;
    protected IMethod m_Update;
    protected IMethod m_OnEnable;
    protected IMethod m_OnDisable;
    protected IMethod m_OnDestroy;
    protected bool bIsGetMethod;
    protected AppDomain m_AppDomain;
    protected bool bIsSuccessLoad;
    protected static object[] param1 = new object[1];
    protected static object[] param2 = new object[2];
    protected static object[] param3 = new object[3];
    //protected Dictionary<string, IMethod> m_MethodDic = new Dictionary<string, IMethod>();

    public object CreateInstance(System.Type type)
    {
        string typeName = type.ToString();
        param1[0] = this;
        m_Type = m_AppDomain.GetType(typeName);
        m_Self = m_AppDomain.Instantiate(typeName, null);
        var m = m_Type.GetMethod("SetMono", 1);
        InvokeMethod(m, param1);
        GetMothodOnInstantiate();
        InvokeMethod(m_Awake, null);
        InvokeMethod(m_Start, null);
        return m_Self;
    }

    protected  void GetMothodOnInstantiate()
    {
        if(bIsGetMethod)
        {
            return;
        }
        bIsGetMethod = true;
        m_Start = m_Type.GetMethod("Start", 0);
        m_Update = m_Type.GetMethod("Update", 0);
        m_OnEnable = m_Type.GetMethod("OnEnable", 0);
        m_OnDisable = m_Type.GetMethod("OnDisable", 0);
        m_OnDestroy = m_Type.GetMethod("OnDestroy", 0);
    }
	
	
	
    protected bool SetValue(string vname, object value)
    {
        var type = m_Type.ReflectionType;
        var p = type.GetField(vname, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if(p == null)
        {
            return false;
        }
        p.SetValue(m_Self, value);
        return true;
    }

    public void Test()
    {
        var type = m_Self.GetType();
        var ps = type.GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        foreach (var p in ps)
        {
            Debug.Log(p);
        }
    }

    // public void InvokeAction(string _name, bool bIsCache = false)
    // {
    //     string key = _name;
    //     IMethod m;
    //     if (!bIsCache)
    //     {
    //         m = m_Type.GetMethod(key, 0);
    //     }
    //     else
    //     {
    //         if (!m_MethodDic.ContainsKey(key))
    //         {
    //             m_MethodDic[key] = m_Type.GetMethod(key, 0);
    //         }
    //         m = m_MethodDic[key];
    //     }
    //     InvokeMethod(m, null);
    // }

    private object InvokeMethod(IMethod m, object[] objs = null)
    {
        object res = null;
        if (m == null)
        {
            return res;
        }
        m_AppDomain.Invoke(m, m_Self, objs);
        return res;
    }
    private void Update()
    {
        InvokeMethod(m_Update, null);
    }

    private void OnEnable()
    {
       InvokeMethod(m_OnEnable, null);
    }

    private void OnDisable()
    {
        InvokeMethod(m_OnDisable, null);
    }

    private void OnDestroy()
    {
        InvokeMethod(m_OnDestroy, null);
    }

}
