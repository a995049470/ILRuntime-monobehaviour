using UnityEngine;
using System.Collections;
using System.IO;
using ILRuntime.Runtime.Enviorment;

public class ILRuntimeHandler
{
    //AppDomain是ILRuntime的入口，最好是在一个单例类中保存，整个游戏全局就一个，这里为了示例方便，每个例子里面都单独做了一个
    //大家在正式项目中请全局只创建一个AppDomain
    AppDomain appdomain;
    System.IO.MemoryStream fs;
    System.IO.MemoryStream p;
    public AppDomain MyAppdomain { get { return appdomain; } }
    private static ILRuntimeHandler s_Instance;
    public static ILRuntimeHandler Instance
    {
        get
        {
            if(s_Instance == null)
            {
                s_Instance = new ILRuntimeHandler();
            }
            return s_Instance;
        }
    }

    public ILRuntimeHandler()
    {
        LoadAssembly();
    }

    private static string AssemblyPath = Application.dataPath.Replace("Assets", "Library/ScriptAssemblies") + "/HotFix.dll";


    private void LoadAssembly()
    {
        byte[] dll = File.ReadAllBytes(AssemblyPath);
        fs = new MemoryStream(dll);
        //p = new MemoryStream(null);
        appdomain = new AppDomain();
        appdomain.LoadAssembly(fs, null, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
    }

   


}
