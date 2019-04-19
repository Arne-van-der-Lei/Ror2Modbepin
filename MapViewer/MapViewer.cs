using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Newtonsoft.Json;
using R2API;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace be.vanderlei.arne.MapViewer
{
    [BepInDependency("com.bepis.r2api")] 
    [BepInPlugin("be.vanderlei.arne.MapViewer", "MapViewer", "1.0")]
    public class ExamplePlugin : BaseUnityPlugin
    {
        public static List<Type> types;
        public IEnumerator Start()
        {
            string outpath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Maps/";

            types = new List<Type>();
            Directory.CreateDirectory(outpath);
            for (int i = 0; i <= 16; i++)
            {

                AsyncOperation ope =  SceneManager.LoadSceneAsync(i,LoadSceneMode.Single);

                yield return new WaitUntil(() => ope.isDone);

                List<jsonGameObject> objjson = new List<jsonGameObject>();
                GameObject[] objects = SceneManager.GetActiveScene().GetRootGameObjects();

                foreach (GameObject child in objects)
                {
                    Component[] comp = child.GetComponents<Component>();

                    jsonGameObject go = new jsonGameObject
                    {
                        Name = child.name,
                        componts = OutputComponents(comp),
                        children = GetChildren(child.transform)
                    };
                    objjson.Add(go);
                }

                File.WriteAllText(outpath + "map" + i + ".json", JsonConvert.SerializeObject(objjson,Formatting.Indented));
            }
            yield return null;

            File.WriteAllText(outpath + "types.json", JsonConvert.SerializeObject(types, Formatting.Indented));
            SceneManager.LoadScene(0, LoadSceneMode.Single);

            Directory.CreateDirectory(outpath + "classes/");
            foreach (Type type in types)
            {
                if (type.FullName.StartsWith("RoR2"))
                {
                    if (type.Name.EndsWith("]")) continue;
                    string output = "";
                    output += "using UnityEngine;\n\n";
                    output += "namespace " + type.Namespace + "{\n\n";
                    output += "    public class " + type.Name + " : MonoBehaviour {\n";
                    foreach (FieldInfo field in type.GetFields())
                    {
                        output += "        public " + GetCSharpName(field.FieldType) + " " + field.Name + ";\n";
                    }
                    output += "    }\n";
                    output += "}\n";
                    File.WriteAllText(outpath + "classes/" + type.Name + ".cs", output);
                }
            }
        } 

        public static List<jsonComponent> OutputComponents(Component[] components)
        {
            List<jsonComponent> componentsjson = new List<jsonComponent>();

            for (int i = 0; i < components.Length; i++)
            {

                Type type = components[i].GetType();
                
                Dictionary<string, object> properties = new Dictionary<string, object>();

                if(type.FullName == "UnityEngine.Transform")
                {
                    Transform trans = (Transform)components[i];
                    properties.Add("Pos", trans.position.ToString());
                    properties.Add("rot", trans.eulerAngles.ToString());
                    properties.Add("scl", trans.localScale.ToString());
                }
                else
                { 
                    foreach (FieldInfo field in type.GetFields())
                    {
                        object check = field.GetValue(components[i]);
                        if (check != null)
                            properties.Add(field.Name, field.GetValue(components[i]).ToString());
                        else
                            properties.Add(field.Name, "null");


                        if (!types.Contains(field.FieldType))
                        {
                            types.Add(field.FieldType);
                        }
                    }
                }

                if (!types.Contains(components[i].GetType()))
                {
                    types.Add(components[i].GetType());
                }

                jsonComponent definition = new jsonComponent
                {
                    Name = type.ToString(),
                    properties = properties 
                };
                componentsjson.Add(definition);
            }
            return componentsjson;
        }
        
        public static List<jsonGameObject> GetChildren(Transform transform)
        {
            List<jsonGameObject> gameObjects = new List<jsonGameObject>();
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                Component[] comp = child.GetComponents<Component>();

                jsonGameObject go = new jsonGameObject
                {
                    Name = child.name,
                    componts = OutputComponents(comp),
                    children = GetChildren(transform.GetChild(i))
                };
                gameObjects.Add(go);
            }
            return gameObjects;
        }

        public static string GetCSharpName(Type type)
        {
            string result;
            if (primitiveTypes.TryGetValue(type, out result))
                return result;
            else
                result = type.FullName.Replace('+', '.');

            if (!type.IsGenericType)
                return result;
            else if (type.IsNested && type.DeclaringType.IsGenericType)
                throw new NotImplementedException();

            result = result.Substring(0, result.IndexOf("`"));
            return result + "<" + string.Join(", ", type.GetGenericArguments().Select(GetCSharpName)) + ">";
        }

        static Dictionary<Type, string> primitiveTypes = new Dictionary<Type, string>
        {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(string), "string" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(ushort), "ushort" },
        };
    }

    public struct jsonGameObject
    {
        public string Name;
        public List<jsonComponent> componts;
        public List<jsonGameObject> children;
    }

    public struct jsonComponent
    {
        public string Name;
        public Dictionary<string,object> properties;
    }
}