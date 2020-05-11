using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ILRSerializable]
public class T2
{
    public int i;
    public string str;
}


public class T1 : BaseMono
{
    public T2 t2;
    public GameObject go;
    [ILRMonoMethod]
    void Test(string str)
    {
        Debug.Log(str);
    }

    void Awake()
    {
        Debug.Log("Awake");
    }
    
    void Start()
    {
        Debug.Log("Start");
        Debug.Log(t2.str + "   " + t2.i);
        T1 t1 = this.transform.GetILRComponent<T1>();
        Debug.Log(t1.t2.i + "   " + t1.t2.str);
        Debug.Log("go name : " + go?.name);
    }

    void Update()
    {
        
    }
}
