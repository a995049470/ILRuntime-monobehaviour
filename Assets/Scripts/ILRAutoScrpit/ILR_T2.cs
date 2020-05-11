using UnityEngine;
using System.Collections;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using System.Collections.Generic;
#if ILRuntime
using System.Reflection;
#else

#endif

[System.Serializable]
public struct Data_ILR_T2
{
	public System.Int32 i;
	public System.String str;

}

[System.Serializable]
public class ILR_T2
{
	[SerializeField]
	protected Data_ILR_T2 m_Data;
    protected static string m_TypeName = "T2";
    protected object m_Self;
	public object Self { get { return m_Self; } } 
    protected List<IType> m_ParamList;
    protected IType m_Type;
    protected System.Type _Type;
    protected AppDomain m_AppDomain { get { return ILRuntimeHandler.Instance?.MyAppdomain; } }
    protected static object[] param1 = new object[1];
    protected static object[] param2 = new object[2];
    protected static object[] param3 = new object[3];
    
    public void Init()
    {
        if(m_Self != null)
        {
            return;
        }
        m_Self = m_AppDomain.Instantiate(m_TypeName, null);
#if ILRuntime
        _Type = m_Self.GetType();
#else
        m_Type = m_AppDomain.GetType(m_TypeName);
#endif      
        SetValueOnInstantiate();
    }

	protected  void SetValueOnInstantiate()
    {
		SetValue("i", m_Data.i);
		SetValue("str", m_Data.str);

    }


    protected bool SetValue(string vname, object value)
    {
        System.Type type = null;
#if ILRuntime
        type = _Type;
#else
        type = m_Type.ReflectionType;
#endif
        var p = type.GetField(vname, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if(p == null)
        {
            return false;
        }
        p.SetValue(m_Self, value);
        return true;
    }

    
}
