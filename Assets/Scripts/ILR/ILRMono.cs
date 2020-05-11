using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ILRMono : MonoBehaviour
{
    protected object m_Self;
    public object Self
    {
        get
        {
            if(m_Self == null)
            {
                CreateInstance();
            }
            return m_Self;
        }
    }

    protected static object[] param0 = new object[0];
    protected static object[] param1 = new object[1];
    protected static object[] param2 = new object[2];
    protected static object[] param3 = new object[3];
    protected static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
    

    protected virtual void CreateInstance()
    {
        
    }

    public static bool IsFAS(System.Type child, System.Type parent)
    {
        var temp = child;
        while (temp != null)
        {
            if(temp == parent)
            {
                return true;
            }
            temp = temp.BaseType;
        }
        return false;
    }

    private static string pat_1 = @"{""instanceID"":(-?\d+)}";
    private static string pat_2 = @"{""instanceID"":([\w\.|_/]+)}";

    public string RecoverInstanceID(string jsonStr)
    {
        var resStr = jsonStr;
        var ms = Regex.Matches(resStr, pat_2);
        for (int i = 0; i < ms.Count; i++)
        {
            var gs = ms[i].Groups;
            if (gs.Count <= 1)
            {
                continue;
            }

            var sp = gs[1].ToString().Split('|');
            var id = ToInstanceID(sp[0], sp[1]);
            string repStr = @"{""instanceID"":{id}}".Replace("{id}", id);
            resStr = resStr.Replace(gs[0].ToString(), repStr);
        }
        return resStr;
    }

    private string ToInstanceID(string path, string t)
    {
        string id = "";
        if(path == "null" && t == "")
        {
            return "0";
        }
        UnityEngine.Object obj;
        if(t == typeof(GameObject).ToString())
        {
            obj = path == "" ? this.gameObject : this.transform.Find(path)?.gameObject;
        }
        else
        {
            obj = this.transform.Find(path).GetComponent(t);
        }
        id = obj?.GetInstanceID().ToString();
        return id;
    }

#if UNITY_EDITOR
    public string ReplaceInstanceID(string jsonStr)
    {
        var resStr = jsonStr;
        
        var ms = Regex.Matches(resStr, pat_1);
        for (int i = 0; i < ms.Count; i++)
        {
            var gs = ms[i].Groups;
            if(gs.Count <= 1)
            {
                continue;
            }
            int id = int.Parse(gs[1].ToString());
            var obj = EditorUtility.InstanceIDToObject(id);
            var path = ToPath(obj);
            string type = "";
            type = obj?.GetType()?.ToString() ?? "";
            string rep = @"{""instanceID"":{path}|{type}}".Replace("{path}", path).Replace("{type}", type);
            resStr = resStr.Replace(gs[0].ToString(), rep);
        }
        
        return resStr;
    }

    private string ToPath(object obj)
    {
        GameObject go = (obj as Component)?.gameObject ?? (obj as GameObject);
        string path = "";
        if(go == null)
        {
            return "null";
        }
        var temp = go.transform;
        while (temp != this.transform)
        {
            path = (path == "") ? temp.name : temp.name + "/" + path;
            temp = temp.parent;
        }
        return path;
    }
#endif


}
