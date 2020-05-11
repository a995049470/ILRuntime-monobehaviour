using UnityEngine;
using System.Collections;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using System.Collections.Generic;
using System.Reflection;


[System.Serializable]
public struct Data_ILR_BaseMono
{

}

public class ILR_BaseMono : ILRMono
{
	[SerializeField]
	protected Data_ILR_BaseMono m_Data;
    protected static string m_TypeName = "BaseMono";
    protected static bool bIsDataEmpty = true;
    protected static IType m_Type;
    protected static System.Type _Type;
#if ILRuntime
    protected static MethodInfo _Awake;
    protected static MethodInfo _Start;
    protected static MethodInfo _Update;
    protected static MethodInfo _FixedUpdate;
    protected static MethodInfo _LateUpdate;
    protected static MethodInfo _OnEnable;
    protected static MethodInfo _OnDisable;
    protected static MethodInfo _OnDestroy;
    protected static MethodInfo _OnTriggerEnter;
    protected static MethodInfo _OnTriggerStay;
    protected static MethodInfo _OnTriggerExit;
    protected static MethodInfo _OnCollisionEnter;
    protected static MethodInfo _OnCollisionStay;
    protected static MethodInfo _OnCollisionExit;
#else
    protected static IMethod m_Awake;
    protected static IMethod m_Start;
    protected static IMethod m_Update;
    protected static IMethod m_FixedUpdate;
    protected static IMethod m_LateUpdate;
    protected static IMethod m_OnEnable;
    protected static IMethod m_OnDisable;
    protected static IMethod m_OnDestroy;
    protected static IMethod m_OnTriggerEnter;
    protected static IMethod m_OnTriggerStay;
    protected static IMethod m_OnTriggerExit;
    protected static IMethod m_OnCollisionEnter;
    protected static IMethod m_OnCollisionStay;
    protected static IMethod m_OnCollisionExit;
#endif

    protected static bool bIsGetMethod;
    protected static AppDomain m_AppDomain { get { return ILRuntimeHandler.Instance?.MyAppdomain; } }

    protected static Dictionary<string, IMethod> m_MethodDic = new Dictionary<string, IMethod>();
    protected static Dictionary<string, FieldInfo> m_FiledDic = new Dictionary<string, FieldInfo>();
    //protected static Dictionary<string, Data_ILR_BaseMono> m_DataDic = new Dictionary<string, Data_ILR_BaseMono>();
    private void Awake()
    {
        
        //sw.Restart();
        CreateInstance();
        //sw.Stop();
        //Debug.Log($"<color=#ff0000ff>{this.gameObject.name} CreateInstance {sw.ElapsedMilliseconds}ms</color>");
    }

    protected override void CreateInstance()
    {
        if(m_Self != null)
        {
            return;
        }
        // if(m_AppDomain == null)
        // {
        //     Debug.Log($"<color=#ff0000ff>环境未加载</color>");
        //     return;
        // }
        m_Self = m_AppDomain.Instantiate(m_TypeName, null);
#if ILRuntime
        if(_Type == null)
        {
            _Type = m_Self.GetType();
        }
        
#else
        if(m_Type == null)
        {
            m_Type = m_AppDomain.GetType(m_TypeName);
        }
        
#endif
		LoadData();
        GetMothodOnInstantiate();
        SetDataValue();
        SetValue("m_Self", this);
#if ILRuntime
        InvokeMethod(_Awake, param0);      
#else
        InvokeMethod(m_Awake, param0);
#endif
        
    }

	public void SetDataValue()
    {	

    }
#if UNITY_EDITOR
	public string ToJson()
    {
        string jsonStr = JsonUtility.ToJson(m_Data);
        jsonStr = ReplaceInstanceID(jsonStr);
        return jsonStr;
    }
#endif	
	public string GetKey()
    {
        string key = this.gameObject.name.Replace("(Clone)","") + "_" + m_TypeName.Replace(".", "_");
        return key;
    }

    public string ToJson<T>(T obj)
    {
        string str = "";
        if (obj == null || obj.ToString() == "null")
        {
            str = "null\n";
            return str;
        }
        GameObject go = (obj as Component)?.gameObject ?? (obj as GameObject);
        string path = "";
        var temp = go.transform;
        while (temp != this.transform)
        {
            path = (path == "") ? temp.name : temp.name + "/" + path;
            temp = temp.parent;
        }
        str = path + "\n";
        return str;
    }
    
    public Object ToObj<T>(string[] strAry, int i) where T : Object
    {
        string str;
        if(i >= strAry.Length)
        {
            return null;
        }
        str = strAry[i];
        Object value = default(T);
        if(str == "null")
        {
            return null;
        }
        var t = typeof(T);
        if (IsFAS(t, typeof(Component)))
        {
            value = this.transform.Find(str).GetComponent<T>();
        }
        else if (IsFAS(t, typeof(GameObject)))
        {
            value = this.transform.Find(str).gameObject;
        }
        return value;
    }

    public GameObject ToGameObject(string str)
    {
        if(str == "")
        {
            return this.gameObject;
        }
        else
        {
            return this.transform.Find(str).gameObject;
        }
    }

//    public static void LoadPrefabData(string pname)
//    {
//        string key = pname + "_" + m_TypeName;
//        if(m_DataDic.ContainsKey(key))
//        {
//            return;
//        }
//        var jsonStr = ResManager.GetResource<TextAsset>(key)?.text;
//        if(string.IsNullOrEmpty(jsonStr))
//        {
//            return;
//        }
//        jsonStr = RecoverInstanceID(jsonStr);
//        var data = JsonUtility.FromJson<Data_ILR_BaseMono>(jsonStr);
//        m_DataDic[key] = data;
//    }

    public void LoadData()
    {
        if(bIsDataEmpty)
        {
            return;
        }
        string jsonStr = "";
        string key = GetKey();
        string jsonPath;
        string path;
// #if UNITY_EDITOR
//         jsonPath = Application.dataPath + "/Resources/AutoLoad/GameJsonData/[key].txt";
//         path = jsonPath.Replace("[key]", key);

//         if(!System.IO.File.Exists(path))
//         {
// 			Debug.Log("<color=#ff0000ff>无目标数据</color>");
//             return;
//         }
//         jsonStr = System.IO.File.ReadAllText(path);
//         jsonStr = RecoverInstanceID(jsonStr);
//         m_Data = JsonUtility.FromJson<Data_ILR_BaseMono>(jsonStr);
// #else
// #endif
        if(Application.isPlaying)
        {
            jsonStr = Resources.Load<TextAsset>($"AutoLoad/GameJsonData/{key}")?.text;
#if UNITY_EDITOR
            if(string.IsNullOrEmpty(jsonStr))
            {
                jsonPath = Application.dataPath + "/Resources/AutoLoad/GameJsonData/[key].txt";
                path = jsonPath.Replace("[key]", key);
                if (!System.IO.File.Exists(path))
                {
                    Debug.Log($"<color=#ff0000ff>{key} 无目标数据</color>");
                    return;
                }
                jsonStr = System.IO.File.ReadAllText(path);
            }
#endif
        }
        else
        {
            jsonPath = Application.dataPath + "/Resources/AutoLoad/GameJsonData/[key].txt";
            path = jsonPath.Replace("[key]", key);
            if (!System.IO.File.Exists(path))
            {
                Debug.Log("<color=#ff0000ff>无目标数据</color>");
                return;
            }
            jsonStr = System.IO.File.ReadAllText(path);
        }
        if(string.IsNullOrEmpty(jsonStr))
        {
            Debug.Log($"{key} 无对应数据");
            return;
        }
        jsonStr = RecoverInstanceID(jsonStr);
        m_Data = JsonUtility.FromJson<Data_ILR_BaseMono>(jsonStr);
        //m_DataDic[key] = m_Data;
    }	


	
    public static void GetMothodOnInstantiate()
    {
        if(bIsGetMethod)
        {
            return;
        }
        bIsGetMethod = true;
#if ILRuntime
        if(_Type == null)
        {
            return;
        } 
        _Awake = _Type.GetMethod("Awake", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _Start = _Type.GetMethod("Start", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _Update = _Type.GetMethod("Update", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _FixedUpdate = _Type.GetMethod("FixedUpdate", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _LateUpdate = _Type.GetMethod("LateUpdate", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _OnEnable = _Type.GetMethod("OnEnable", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _OnDisable = _Type.GetMethod("OnDisable", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _OnDestroy = _Type.GetMethod("OnDestroy", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _OnTriggerEnter = _Type.GetMethod("OnTriggerEnter", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _OnTriggerStay = _Type.GetMethod("OnTriggerStay", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _OnTriggerExit = _Type.GetMethod("OnTriggerExit", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _OnCollisionEnter = _Type.GetMethod("OnCollisionEnter", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _OnCollisionStay = _Type.GetMethod("OnCollisionStay", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _OnCollisionExit = _Type.GetMethod("OnCollisionExit", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
#else
        m_Type = m_AppDomain.GetType(m_TypeName);
        m_Awake = m_Type.GetMethod("Awake", 0);
        m_Start = m_Type.GetMethod("Start", 0);
        m_Update = m_Type.GetMethod("Update", 0);
        m_FixedUpdate = m_Type.GetMethod("FixedUpdate", 0);
        m_LateUpdate = m_Type.GetMethod("LateUpdate", 0);
        m_OnEnable = m_Type.GetMethod("OnEnable", 0);
        m_OnDisable = m_Type.GetMethod("OnDisable", 0);
        m_OnDestroy = m_Type.GetMethod("OnDestroy", 0);
        m_OnTriggerEnter = m_Type.GetMethod("OnTriggerEnter", 1);
        m_OnTriggerStay = m_Type.GetMethod("OnTriggerStay", 1);
        m_OnTriggerExit = m_Type.GetMethod("OnTriggerExit", 1);
        m_OnCollisionEnter = m_Type.GetMethod("OnCollisionEnter", 1);
        m_OnCollisionStay = m_Type.GetMethod("OnCollisionStay", 1);
        m_OnCollisionExit = m_Type.GetMethod("OnCollisionExit", 1);
#endif
    }

	
    protected bool SetValue(string vname, object value)
    {
        System.Type type = null;
#if ILRuntime
        type = _Type;
#else
        type = m_Type.ReflectionType;
#endif
        
        FieldInfo p;
        if(!m_FiledDic.ContainsKey(vname))
        {
            m_FiledDic[vname] = type.GetField(vname, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        }
        p = m_FiledDic[vname];
        if(p == null)
        {
            return false;
        }
        p.SetValue(m_Self, value);
        return true;
    }

#if ILRuntime
    private void InvokeMethod(MethodInfo m, object[] objs = null)
    {   
        if (m == null || m_Self == null)
        {
            return;
        }
        m.Invoke(m_Self, objs);
    } 
#else
    private void InvokeMethod(IMethod m, object[] objs = null)
    {   
        if (m == null || m_Self == null)
        {
            return;
        }
        m_AppDomain.Invoke(m, m_Self, objs);
    }
#endif

    private void Start()
    {
#if ILRuntime
    InvokeMethod(_Start, param0);
#else
    InvokeMethod(m_Start, param0);
#endif 
    }

    private void Update()
    {
#if ILRuntime
    InvokeMethod(_Update, param0);
#else
    InvokeMethod(m_Update, param0);
#endif     
    }

    private void FixedUpdate()
    {
#if ILRuntime
    InvokeMethod(_FixedUpdate, param0);
#else
    InvokeMethod(m_FixedUpdate, param0);
#endif     
    }

    private void LateUpdate()
    {
#if ILRuntime
    InvokeMethod(_LateUpdate, param0);
#else
    InvokeMethod(m_LateUpdate, param0);
#endif     
    }

    private void OnEnable()
    {
#if ILRuntime
        InvokeMethod(_OnEnable, param0);
#else
        InvokeMethod(m_OnEnable, param0);
#endif
       
       
    }

    private void OnDisable()
    {
#if ILRuntime
        InvokeMethod(_OnDisable, param0);
#else
        InvokeMethod(m_OnDisable, param0);
#endif
    }

    private void OnDestroy()
    {
#if ILRuntime
        InvokeMethod(_OnDestroy, param0);
#else
        InvokeMethod(m_OnDestroy, param0);
#endif
        SetValue("m_Self", null);
    }

    private void OnTriggerEnter(Collider other) 
    {
        param1[0] = other;
#if ILRuntime
        InvokeMethod(_OnTriggerEnter, param1);
#else
        InvokeMethod(m_OnTriggerEnter, param1);
#endif        
    }

    private void OnTriggerStay(Collider other) 
    {

        param1[0] = other;
#if ILRuntime
        InvokeMethod(_OnTriggerStay, param1);
#else
        InvokeMethod(m_OnTriggerStay, param1);
#endif               
    }

    private void OnTriggerExit(Collider other) 
    {
        param1[0] = other;
#if ILRuntime
        InvokeMethod(_OnTriggerExit, param1);
#else
        InvokeMethod(m_OnTriggerExit, param1);
#endif               
    }

    private void OnCollisionEnter(Collision other) 
    {
        param1[0] = other;
#if ILRuntime
        InvokeMethod(_OnCollisionEnter, param1);
#else
        InvokeMethod(m_OnCollisionEnter, param1);
#endif              
    }

    private void OnCollisionStay(Collision other) 
    {
        param1[0] = other;
#if ILRuntime
        InvokeMethod(_OnCollisionStay, param1);
#else
        InvokeMethod(m_OnCollisionStay, param1);
#endif               
    }

    private void OnCollisionExit(Collision other) 
    {
        param1[0] = other;
#if ILRuntime
        InvokeMethod(_OnCollisionExit, param1);
#else
        InvokeMethod(m_OnCollisionExit, param1);
#endif      
    }


}
