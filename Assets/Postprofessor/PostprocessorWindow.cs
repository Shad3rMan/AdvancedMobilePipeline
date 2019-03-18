using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using UnityEditor;
using UnityEngine;
using MiniJSON;

namespace Postprofessor
{
    public class PostprocessorWindow : EditorWindow
    {
        private const string SettingsFilePath = "Assets/PostprofessorSettings.txt";

        private static Type[] _types;
        private static string[] _names;
        private static PostprocessorSettings _sets = new PostprocessorSettings();

        static PostprocessorWindow()
        {
            _types = GetProcessorTypes();
            _names = GetProcessorNames();
            if (File.Exists(SettingsFilePath))
            {
                //_sets = JsonConvert.DeserializeObject<PostprocessorSettings>(File.ReadAllText(SettingsFilePath));
                _sets = (PostprocessorSettings)Json.Deserialize(File.ReadAllText(SettingsFilePath));
            }
            else
            {
                _sets = new PostprocessorSettings();
            }
        }

        [MenuItem("Postprofesstor/Test")]
        public static void ShowWindow()
        {
            _types = GetProcessorTypes();
            _names = GetProcessorNames();
            if (File.Exists(SettingsFilePath))
            {
                //_sets = JsonConvert.DeserializeObject<PostprocessorSettings>(File.ReadAllText(SettingsFilePath));
                _sets = (PostprocessorSettings)Json.Deserialize(File.ReadAllText(SettingsFilePath));
            }
            else
            {
                _sets = new PostprocessorSettings();
            }

            Debug.Log(JsonUtility.ToJson((int)0));
            Debug.Log(JsonUtility.ToJson((long)0));
            Debug.Log(JsonUtility.ToJson("asd"));
            Debug.Log(JsonUtility.ToJson(true));
            GetWindow<PostprocessorWindow>();
        }

        private int index;

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            index = EditorGUILayout.Popup("Select", index, _names);
            if (GUILayout.Button("Add"))
            {
                var parameters = _types[index].GetConstructors()[0].GetParameters();
                var d = new ProcessorData();
                d.Type = _types[index];
                d.Params = new List<ConstructorParameter>(parameters.Length);
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameterInfo = parameters[i];
                    d.Params.Add(new ConstructorParameter()
                    {
                        Type = parameterInfo.ParameterType,
                        Value = parameterInfo.ParameterType.IsValueType
                            ? Activator.CreateInstance(parameterInfo.ParameterType)
                            : null,
                        Name = parameterInfo.Name
                    });
                }

                _sets.AddProcessorData(d);
            }

            GUILayout.EndHorizontal();


            foreach (var processorData in _sets.Data)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                DisplayProcessor(processorData);
                GUILayout.EndVertical();
            }

            if (GUILayout.Button("Save"))
            {
                //var settings = new JsonSerializerSettings()
                //{
                //    TypeNameHandling = TypeNameHandling.All,
                //    TypeNameAssemblyFormat = FormatterAssemblyStyle.Full,
                //    Formatting = Formatting.Indented
                //};
//
                //File.WriteAllText(SettingsFilePath, JsonConvert.SerializeObject(_sets, settings));
                //AssetDatabase.ImportAsset(SettingsFilePath);
                
                File.WriteAllText(SettingsFilePath, Json.Serialize(_sets));
                AssetDatabase.ImportAsset(SettingsFilePath);
            }

//            if (GUILayout.Button("Execute"))
//            {
//                ProcessorData d = _sets.Data[0];
//                var p = ProcessorFactory.GetProcessor(d);
//                p.Process();
//            }
        }

        private static void DisplayProcessor(ProcessorData data)
        {
            var type = _types.First(a => a == data.Type);
            var parameters = type.GetConstructors()[0].GetParameters();

            for (var i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];

                if (i < data.Params.Count)
                {
                    var param = data.Params[i];
                    param.Value = DrawUniField(data.Params[i].Name, data.Params[i].Type, param.Value);
                    data.Params[i] = param;
                }
                else
                {
//                    data.Params.Add(p.ParameterType.IsValueType
//                        ? Activator.CreateInstance(p.ParameterType)
//                        : null);
                }
            }
        }

        private static string[] GetProcessorNames()
        {
            var type = typeof(IProcessor);
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && p.IsClass)
                .Select(p => p.Name).ToArray();
        }

        private static Type[] GetProcessorTypes()
        {
            var type = typeof(IProcessor);
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && p.IsClass)
                .ToArray();
        }

        private static object DrawUniField(string name, Type type, object val)
        {
            if (type == typeof(bool))
            {
                return EditorGUILayout.Toggle(name, (bool) val);
            }

            if (type == typeof(string))
            {
                return EditorGUILayout.TextField(name, (string) val);
            }

            if (type == typeof(long))
            {
                return (long) EditorGUILayout.IntField(name, (int) (long) val);
            }

            if (type == typeof(int))
            {
                return EditorGUILayout.IntField(name, (int) (long) val);
            }

            return null;
        }
    }
}