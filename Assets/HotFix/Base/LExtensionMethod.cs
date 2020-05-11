using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LExtensionMethod
{


    public static string ApeendColor(this string str, string color)
    {
        if (string.IsNullOrEmpty(color))
        {
            return str;
        }
        return $"[color=#{color}]{str}[/color]";
    }



    public static void SetTargetBit(ref this int n, int i, int v)
    {
        int t = i - 1;
        if (v == 1)
        {
            n = n | (1 << t);
        }
        else if (v == 0)
        {
            n = n & ~(1 << t);
        }
    }

    public static int ChangeBit(this int n, int i, int v)
    {
        int t = i - 1;
        var m = n;
        if (v == 1)
        {
            m = m | (1 << t);
        }
        else if (v == 0)
        {
            m = m & ~(1 << t);
        }
        return m;
    }

    //获取n从右向左数第i位的值是否为1
    public static bool GetTargetBoolean(this int n, int i)
    {
        return n.GetTargetBit(i) == 1;
    }

    //获取n从右向作数第i位的值
    public static int GetTargetBit(this int n, int i)
    {
        int t = i - 1;
        var res = (n & (1 << t)) >> t;
        return res;
    }

    // public static T AddILRComponent<T>(this GameObject g) where T : BaseMono
    // {
    //     T value = default(T);
    //     ILRGeneralMono mono = g.AddComponent<ILRGeneralMono>();
    //     value = mono.CreateInstance(typeof(T)) as T;
    //     return value;
    // }

    public static T GetUnityComponent<T>(this Component t) where T : UnityEngine.Component
    {
        return t.GetComponent<T>();
    }

    public static T GetILRComponent<T>(this Component t) where T : BaseMono
    {
        T value = default(T);
        ILRMono mono = t.GetComponent<ILRMono>();
        value = mono?.Self as T;
        return value;
    }

    public static T GetUnityComponent<T>(this GameObject g) where T : UnityEngine.Component
    {
        return g.GetComponent<T>();
    }

    public static T GetILRComponent<T>(this GameObject g) where T : BaseMono
    {
        if (g == null)
        {
            return null;
        }
        T value = default(T);
        ILRMono mono = g.GetComponent<ILRMono>();
        value = mono?.Self as T;
        return value;
    }

    public static T FindUnityCommpent<T>(this Transform t, string path) where T : Component
    {
        T res = null;
        Transform child = t.Find(path);
        if (child == null)
        {
            return null;
        }
        res = child.GetComponent<T>();
        return res;
    }

    public static T FindCLRCommpent<T>(this Transform t, string path) where T : BaseMono
    {
        T res = null;
        Transform child = t.Find(path);
        if (child == null)
        {
            return null;
        }
        res = child.GetComponent<ILRMono>()?.Self as T;
        return res;
    }

}