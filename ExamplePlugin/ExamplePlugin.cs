using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace be.vanderlei.arne.Prefaboutput
{ 
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("be.vanderlei.arne.Prefaboutput", "PrefabOutput", "1.0")]
    public class ExamplePlugin : BaseUnityPlugin
    {

        public static List<Type> types;

        public void Start()
        {
            string outpath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Bodys/";
            string output = "";
            string logOut = "";
            Logger.LogInfo(outpath);
            logOut += outpath + "\n";
            types = new List<Type>();
            try
            {
                logOut += "create Directory\n";
                Directory.CreateDirectory(outpath);
                logOut += "for each\n";
                foreach (GameObject obj in Resources.LoadAll<GameObject>("Prefabs/CharacterBodies/"))
                {
                    Component[] comps = obj.GetComponents<Component>();

                    output = "";
                    output += obj.name + "\n";
                    output += OutputComponents(comps, ">"); 
                    output += GetChildren(obj.transform, ">");
                    File.WriteAllText(outpath + obj.name + ".txt", output);
                }

                logOut += "types\n";
                output = "";
                output += "\n\ntypes\n\n";
                for (int j = 0; j < types.Count; j++)
                {
                    MemberInfo[] meminfo = types[j].GetMembers();
                    output = output + types[j].Name + "\n";
                    for (int k = 0; k < meminfo.Length; k++)
                    {
                        if (!(meminfo[k].DeclaringType.Name == "MonoBehaviour") && !(meminfo[k].DeclaringType.Name == "Component") && !(meminfo[k].DeclaringType.Name == "Behaviour") && !(meminfo[k].DeclaringType.Name == "Object"))
                        {
                            output += "> " + meminfo[k].MemberType.ToString() + ": " + meminfo[k].Name + "\n";
                        }
                    }
                }
                File.WriteAllText(outpath + "types.txt", output);
            }
            catch (Exception e)
            {
                logOut += e.ToString();
            }
            File.WriteAllText(outpath + "log.txt", logOut);
        }
        
        // Token: 0x0600284F RID: 10319
        public static string OutputComponents(Component[] components, string delimi)
        {
            string output = "";
            for (int i = 0; i < components.Length; i++)
            {
                string extra = "";
                switch (components[i].GetType().FullName) {
                    case "UnityEngine.Transform":
                        Transform trans = (Transform)components[i];
                        extra = string.Concat(new string[]
                        {
                            "transform = p:",
                            trans.localPosition.ToString(),
                            " r:",
                            trans.eulerAngles.ToString(),
                            " s:",
                            trans.localScale.ToString(),
                            "\n"
                        });
                    break;
                    default:
                        Type type = components[i].GetType();
                        extra = type.FullName + "\n";
                        foreach (FieldInfo field in type.GetFields())
                        {
                            extra += delimi + "v " + field.Name + " = " + field.GetValue(components[i]) + "\n";
                        }
                        break;
                    }
                    
                    output += "\n" + delimi + " " + extra;
                    
                    if (!types.Contains(components[i].GetType()))
                    {
                        types.Add(components[i].GetType());
                    }
                }
            return output;
        }

        // Token: 0x06002863 RID: 10339
        public static string GetChildren(Transform transform, string delimi)
        {
            string output = "";
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                output = string.Concat(new string[]
                {
                    output,
                    delimi, 
                    "c ",
                    child.name,
                    "\n"
                });
                Component[] comp = child.GetComponents<Component>();
                output += OutputComponents(comp, delimi + ">");
                output += GetChildren(transform.GetChild(i), delimi + ">");
            }
            return output;
        }
    }
}