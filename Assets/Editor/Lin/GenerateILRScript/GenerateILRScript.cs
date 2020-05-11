using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Text;
using System.IO;

public class GenerateILRScript
{
    private static Type SerAttType = typeof(ILRSerializableAttribute);
    private static Type MonoAttType = typeof(ILRMonoBehaviourAttribute);
    private static Type MonoMothodType = typeof(ILRMonoMethodAttribute);
    //dll 位置 
    private static string AssemblyPath = Application.dataPath.Replace("Assets","Library/ScriptAssemblies") + "/Hotfix.dll";
    //生成的脚本的存放位置
    private static string ScriptPath = $"{Application.dataPath}/Scripts/ILRAutoScrpit/{Prefix}[type].cs";
    //生成的脚本的存放位置
    private static string EditorPath = $"{Application.dataPath}/Editor/Lin/ILREditor/[type]Editor.cs";
    private static string Prefix = "ILR_";
    private static string VarDec = "\tpublic {type_var} {name_var};\n";
    private static string VarSet = "\t\tSetValue(\"{name_var_hot}\", {name_var});\n";
    //各种模板txt的位置
    private static string TemplatePath_Mono = Application.dataPath +"/Editor/Lin/GenerateILRScript/Template_Mono.txt";
    private static string TemplatePath_Object = Application.dataPath +"/Editor/Lin/GenerateILRScript/Template_Object.txt";
    private static string TemplatePath_Editor = Application.dataPath +"/Editor/Lin/GenerateILRScript/Template_MonoEditor.txt";
    private static string TemplatePath_Method = Application.dataPath +"/Editor/Lin/GenerateILRScript/Template_Method.txt";
    private static string TemplatePath_MethodBind = Application.dataPath +"/Editor/Lin/GenerateILRScript/Template_ILRMethodBinder.txt";
    private static Type[] Types;

    
    //[MenuItem("Tools/MyTool/脚本改名")]
    private static void Rename()
    {
        string jsonPath = $"{Application.dataPath}/Resources/AutoLoad/GameJsonData";
        DirectoryInfo fdir = new DirectoryInfo(jsonPath);
        FileInfo[] files = fdir.GetFiles();
        for (int i = 0; i < files.Length; i++)
        {
            var f = files[i];
            var p1 = f.FullName;
            if(p1.Contains(".meta"))
            {
                continue;
            }
            var p2 = p1.Replace("LGame.", "LGame_");
            var txt = File.ReadAllText(p1);
            File.WriteAllText(p2, txt);
            f.Delete();
        }
        AssetDatabase.Refresh();
    }
    

    [MenuItem("Tools/MyTool/清除中间热更类脚本")]
    private static void Clear()
    {
        ClearPath($"{Application.dataPath}/Scripts/ILRAutoScrpit");
        ClearPath($"{Application.dataPath}/Editor/Lin/ILREditor");
    }

    private static void ClearPath(string path)
    {
        DirectoryInfo fdir = new DirectoryInfo(path);
        FileInfo[] files = fdir.GetFiles();
        for (int i = 0; i < files.Length; i++)
        {
            var f = files[i];
            f.Delete();
        }
        AssetDatabase.Refresh();
    }
    
    [MenuItem("Tools/MyTool/生成热更类中间脚本")]
    private static void ReadAssembly()
    {
        Assembly ass = Assembly.LoadFile(AssemblyPath);
        if(ass == null)
        {
            return;
        }
        Types = ass.GetTypes();
        string code_bind = "";
        string mbPath = $"{Application.dataPath}/Scripts/ILRAutoScrpit/ILRMethodBinder.cs";
        string mbStr = File.ReadAllText(TemplatePath_MethodBind);
        foreach (var type in Types)
        {

            bool bIsMono = false;
            if(!IsNeedGenerate(type, out bIsMono))
            {
                continue;
            }

            List<FieldInfo> fieldList = new List<FieldInfo>();
            List<MethodInfo> methodList = new List<MethodInfo>();
            var fs = type.GetFields(BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            var ms = type.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var f in fs)
            {
                if(!IsFiledSerializeField(f))
                {
                    continue;
                }
                fieldList.Add(f);
            }

            foreach (var m in ms)
            {
                var count = m.GetCustomAttributes(MonoMothodType, false).Length;
                if(count == 0)
                {
                    continue;
                }
                methodList.Add(m);
            }
            if(bIsMono)
            {
                string className = Prefix + GetTypeName(type);
                code_bind += $"\t\t{className}.GetMothodOnInstantiate();\n";
            }
            
            GenerateScript(type, fieldList, methodList, bIsMono);
        }
        mbStr = mbStr.Replace("{code_bind}", code_bind);
        File.WriteAllText(mbPath, mbStr);
        AssetDatabase.Refresh();
        ass = null;
        Debug.Log("完成");
    }

    private static string MethodToString(MethodInfo m)
    {
        string str = File.ReadAllText(TemplatePath_Method);
        var pars = m.GetParameters();
        int count_var = pars.Length;
        string name_func = m.Name;
        string desc_var = "";
        string set_objs = "";
        string name_objs = "";
        if(count_var >= 1)
        {
            name_objs = "param" + count_var;
        }
        else
        {
            name_objs = "param0";
        }
        
        for (int i = 0; i < count_var; i++)
        {
            var par = pars[i];
            var typename = GetTypeName(par);
            desc_var += $"{typename} arg{i + 1}";
            set_objs += $"{name_objs}[{i}] = arg{i + 1};  ";
            if(i != count_var - 1)
            {
                desc_var += ",";
            }
        }

        str = str.Replace("{name_func}", name_func);
        str = str.Replace("{desc_var}", desc_var);
        str = str.Replace("{set_objs}", set_objs);
        str = str.Replace("{name_objs}", name_objs);
        str = str.Replace("{count_var}", count_var.ToString());
        str += '\n';
        return str;
    }

    private static bool IsFiledSerializeField(FieldInfo f)
    {
        var attCount_SF = f.GetCustomAttributes(typeof(SerializeField), false).Length;
        var attCount_Hide = f.GetCustomAttributes(typeof(HideInInspector), false).Length;
        if(f.IsPublic && !f.Name.Contains("this") && attCount_Hide == 0)
        {
            return true;
        }
        if(attCount_SF > 0)
        {
            return true;
        }
        return false;
    }

    private static bool IsNeedGenerate(Type type, out bool bIsMono)
    {
        //var attCount_Mono = type.GetCustomAttributes(MonoAttType, false).Length;
        var attCount_Ser = type.GetCustomAttributes(SerAttType, false).Length;
        if(attCount_Ser > 0)
        {
            bIsMono = false;
            return true;
        }
        var temp = type;
        while (temp != null)
        {
            var attCount_Mono = temp.GetCustomAttributes(MonoAttType, false).Length;
            if(attCount_Mono > 0)
            {
                bIsMono = true;
                return true;
            }
            temp = temp.BaseType;
        }

        bIsMono = false;
        return false;
    }

    private static void GenerateScript(Type type, List<FieldInfo> fieldList, List<MethodInfo> methodList, bool bIsMono)
    {
        // string saveStr = "\t\tjsonStr += ToJson<{type_var}>(m_Data.{name_var});\n";
        // string loadStr1 = "\t\tm_Data = JsonUtility.FromJson<Data_{name_class}>(res[0]);\n";
        // string loadStr2 = "\t\tm_Data.{name_var} = ({type_var})ToObj<{type_var}>(res, {i});\n";
        //string loadStr = "\t\tm_Data.{name_var} = typeof({type_var}) == typeof(GameObject) ? ToGameObject(res[{i}]) : ToObj<{type_var}>(res[{i}]);\n";

        string full_name = type.ToString();
        string cname = Prefix + GetTypeName(type);
        string str_method = "";
        string path = ScriptPath.Replace("[type]", cname);
        StringBuilder decSB = new StringBuilder();
        StringBuilder setSB = new StringBuilder();
        // StringBuilder code_save = new StringBuilder();
        // StringBuilder code_load = new StringBuilder();
        var tpath = bIsMono ? TemplatePath_Mono : TemplatePath_Object;
        var str = File.ReadAllText(tpath);
        
        //int index = 1;
        //code_load.Append(loadStr1.Replace("{name_class}", cname));
        for (int i = 0; i < fieldList.Count; i++)
        {
            var f = fieldList[i];
            var bIsCLRType = IsCLRType(f.FieldType);
            var t = GetTypeName(f);
            var n = f.Name;
            decSB.Append(VarDec.Replace("{type_var}", t).Replace("{name_var}", n));
            if (bIsCLRType)
            {
                setSB.Append($"\t\tm_Data.{n}.Init();\n");
                setSB.Append(VarSet.Replace("{name_var_hot}", n).Replace("{name_var}", $"m_Data.{n}.Self"));
            }
            else
            {
                setSB.Append(VarSet.Replace("{name_var_hot}", n).Replace("{name_var}", $"m_Data.{n}"));
            }     
            // if(IsFAS(f.FieldType, typeof(GameObject)) || IsFAS(f.FieldType, typeof(Component)))
            // {
            //     code_load.Append(loadStr2.Replace("{type_var}", t).Replace("{name_var}", n).Replace("{i}", index.ToString()));
            //     index++;
            //     code_save.Append(saveStr.Replace("{type_var}", t).Replace("{name_var}", n));
            // }        
            
        }
        for (int i = 0; i < methodList.Count; i++)
        {
            var m = methodList[i];
            str_method += MethodToString(m);
        }
        string str_dataEmpty = fieldList.Count == 0 ? "true" : "false";
        str = str.Replace("{str_dataEmpty}", str_dataEmpty);
        str = str.Replace("{full_name}", full_name);
        str = str.Replace("{area_var}", decSB.ToString());
        str = str.Replace("{are_setvalue}", setSB.ToString());
        str = str.Replace("{name_class}", cname);
        str = str.Replace("{str_method}", str_method);
        // str = str.Replace("{code_save}", code_save.ToString());
        // str = str.Replace("{code_load}", code_load.ToString());
        File.WriteAllText(path, str);
        //Debug.Log(cname + "   " + full_name);
        if(bIsMono && fieldList.Count > 0)
        {
            str = File.ReadAllText(TemplatePath_Editor);
            str = str.Replace("{name_class}", cname);
            path = EditorPath.Replace("[type]", cname);
            File.WriteAllText(path, str);
        }
    }

    private static bool IsFAS(Type child, Type parent)
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

    private static bool IsCLRType(Type t)
    {
        for (int i = 0; i < Types.Length; i++)
        {
            if(t == Types[i])
            {
                return true;
            }
            // if(t.ToString().Contains(Types[i].ToString()))
            // {
            //     return true;
            // }
        }
        // if(t.GetType().ToString().Contains("LGame."))
        // {
        //     return true;
        // }
        return false;
    }    


    private static string GetTypeName(Type type)
    {
        var res = type.ToString().Split('.');
        string str = res[res.Length - 1];
        return str;
    }

    private static string GetTypeName(ParameterInfo info)
    {
        string str = info.ParameterType.ToString().Replace("`1", "");
        if(str.Contains("List"))
        {
            str = str.Replace("[", "<").Replace("]", ">");
        }
        return str;
    }

    private static string GetTypeName(FieldInfo info)
    {
        string str = info.FieldType.ToString().Replace("`1", "");
        if(str.Contains("List"))
        {
            str = str.Replace("[", "<").Replace("]", ">");
        }
        if(IsCLRType(info.FieldType))
        {
            var res = str.Split('.');
            str = res[res.Length - 1];
            str = Prefix + str;
        }
        return str;
    }

   


}
