using System;
using System.Collections.Generic;
using System.IO;
using Runestone.AesirArchitecture;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif

namespace Runestone.AesirModules
{
    /// <summary>
    /// Object Binder 核心组件。配置自动绑定信息并生成对应的脚本代码。
    /// </summary>
    [DisallowMultipleComponent]
    public class BinderAssistant : AesirMonoBehaviour
    {
        [PropertyOrder(-5)]
        [HorizontalGroup("Bool")]
        [ToggleLeft]
        [LabelText("开启自动检查")]
        public bool OpenAutoValidate = true;

        [LabelText("命名空间: ")]
        [LabelWidth(100)]
        [InlineButton("DefaultNamespace", "默认命名空间")]
        public string TargetNamespace;

        [LabelText("脚本名: ")]
        [LabelWidth(100)]
        [InlineButton("DefaultScriptName", "默认脚本名")]
        public string ScriptName;

        [ValueDropdown(nameof(GetBaseTypes))]
        [LabelText("基类: ")]
        [LabelWidth(100)]
        public string BaseType;

        [FolderPath(RequireExistingPath = true)]
        [LabelText("目标文件夹路径: ")]
        [LabelWidth(100)]
        public string FolderPath;

        [Title("自动绑定列表")]
        public List<BinderInfo> Units;

        [TitleGroup("自定义命名空间", "示例: UnityEngine.UI")]
        public List<string> CustomNamespaces;

        [PropertyOrder(-3)]
        [HorizontalGroup("Bool")]
        [ToggleLeft]
        [LabelText("当前绑定信息有错误")]
        [ShowInInspector]
        public bool HasError { get; private set; }

        /// <summary>
        /// 当前物体在场景层级中的绝对路径
        /// </summary>
        public string HierarchyPath => BinderHierarchyUtility.GetAbsolutePath(transform);

        void Reset()
        {
            DefaultNamespace();
            DefaultScriptName();
            BaseType = nameof(MonoBehaviour);
        }

        void DefaultNamespace()
        {
            TargetNamespace = "Game";
        }

        void DefaultScriptName()
        {
            ScriptName = gameObject.name + "Presenter";
        }

        /// <summary>
        /// 可选基类下拉列表
        /// </summary>
        public ValueDropdownList<string> GetBaseTypes()
        {
            var typeStrings = new ValueDropdownList<string>
            {
                new ValueDropdownItem<string>(nameof(MonoBehaviour), typeof(MonoBehaviour).FullName)
            };
            return typeStrings;
        }

        [TitleGroup("按钮操作")]
        [Button("构建绑定单元")]
        void CreateUnits()
        {
            var labels = transform.GetComponentsInChildren<BinderTag>(true);

            foreach (var label in labels)
            {
                var number = label.ComponentNumber;
                for (var i = 0; i < Units.Count; i++)
                {
                    var unit = Units[i];
                    if (unit.LabelObj != label.SelfObj)
                    {
                        continue;
                    }

                    unit.UpdatePath(this);
                    number--;
                }

                while (number > 0)
                {
                    Units.Add(new BinderInfo(this, label));
                    number--;
                }
            }
        }

        [TitleGroup("按钮操作")]
        [Button("生成文件夹")]
        void CreateFolder()
        {
#if UNITY_EDITOR
            if (!AssetDatabase.IsValidFolder(FolderPath))
            {
                AesirModulesDebug.Log(AesirModulesDebug.ObjectBinderTag, "不存在该路径");
                var guid = AssetDatabase.CreateFolder("Assets", FolderPath.Replace("Assets/", ""));
                var newFolderPath = AssetDatabase.GUIDToAssetPath(guid);
                AesirModulesDebug.Log(AesirModulesDebug.ObjectBinderTag, newFolderPath);
            }
#endif
        }

        [TitleGroup("按钮操作")]
        [Button("生成脚本")]
        void GenerateCode()
        {
            var generatedPath = Path.Combine(FolderPath, ScriptName + ".generated.cs");
            var controllerPath = Path.Combine(FolderPath, ScriptName + ".cs");
            try
            {
                WriteGeneratedScript(generatedPath);
                WriteControllerScript(controllerPath);
            }
            catch (Exception ex)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.ObjectBinderTag, $"生成脚本失败: {ex.Message}");
            }
#if UNITY_EDITOR

            AssetDatabase.ImportAsset(controllerPath);
            AssetDatabase.ImportAsset(generatedPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }

        void WriteControllerScript(string controllerPath)
        {
            using (var writer = new StreamWriter(controllerPath))
            {
                writer.WriteLine("// * ---------------------------------------------");
                writer.WriteLine("// * Controller 脚本仅由 Object Binder 生成一次");
                writer.WriteLine("// * 生成时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                writer.WriteLine("// * ---------------------------------------------");
                writer.WriteLine("using UnityEngine;");
                foreach (var item in CustomNamespaces)
                {
                    item.TrimEnd(';');
                    writer.WriteLine("using " + item + ";");
                }

                writer.WriteLine();
                writer.WriteLine("namespace " + TargetNamespace);
                writer.WriteLine("{");
                writer.WriteLine("    public partial class " + ScriptName);
                writer.WriteLine("    {");
                writer.WriteLine();
                writer.WriteLine("    }");
                writer.WriteLine("}");
                writer.Flush();
                writer.Close();
            }
        }

        void WriteGeneratedScript(string generatedPath)
        {
            using (var writer = new StreamWriter(generatedPath))
            {
                writer.WriteLine("// * ---------------------------------------------");
                writer.WriteLine("// * Generated 脚本由 Object Binder 自动生成，手动修改将会被覆盖");
                writer.WriteLine("// * 生成时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                writer.WriteLine("// * ---------------------------------------------");
                writer.WriteLine("using UnityEngine;");
                writer.WriteLine("using Sirenix.OdinInspector;");
                foreach (var item in CustomNamespaces)
                {
                    item.TrimEnd(';');
                    writer.WriteLine("using " + item + ";");
                }

                writer.WriteLine();
                writer.WriteLine("namespace " + TargetNamespace);
                writer.WriteLine("{");
                writer.WriteLine("    public partial class " + ScriptName + " : " + BaseType + ", " +
                                 typeof(IComponentBinder).FullName);
                writer.WriteLine("    {");
                foreach (var unit in Units)
                {
                    writer.WriteLine("        [PropertyOrder(-1000)]");
                    writer.WriteLine("        [TitleGroup(\"自动绑定变量\")]");
                    writer.WriteLine("        [BoxGroup(\"自动绑定变量/Box\",ShowLabel = false)]");
                    writer.WriteLine("        [SerializeField]");
                    writer.WriteLine("        " + unit.ComponentFullName + " " + unit.FieldName + ";");
                }

                writer.WriteLine();
                writer.WriteLine("        public void BindReferences()");
                writer.WriteLine("        {");
                foreach (var unit in Units)
                {
                    if (unit.ComponentFullName == typeof(GameObject).FullName)
                    {
                        writer.WriteLine("            " + unit.FieldName + " = transform.Find(" + "\"" +
                                         unit.HierarchyPath + "\"" + ").gameObject;");
                    }
                    else
                    {
                        writer.WriteLine("            " + unit.FieldName + " = transform.Find(" + "\"" +
                                         unit.HierarchyPath + "\"" + ").GetComponent<" +
                                         unit.ComponentFullName + ">();");
                    }
                }

                writer.WriteLine("        }");
                writer.WriteLine();
                writer.WriteLine("        [ContextMenu(\"绑定引用\", false)]");
                writer.WriteLine("        public void BindCommand()");
                writer.WriteLine("        {");
                writer.WriteLine("            BindReferences();");
                writer.WriteLine("        }");
                writer.WriteLine("    }");
                writer.WriteLine("}");
                writer.Flush();
                writer.Close();
            }
#if UNITY_EDITOR
            EditorPrefs.SetInt("即将绑定脚本的物体 Id", gameObject.GetInstanceID());
            EditorPrefs.SetString("即将绑定的脚本类型",
                TargetNamespace + "." + ScriptName +
                ", Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
#endif

            AesirModulesDebug.Log(AesirModulesDebug.ObjectBinderTag, $"成功生成脚本: {generatedPath}");
        }

#if UNITY_EDITOR
        [DidReloadScripts]
        static void CheckBinderUnit()
        {
            var assistants =
                FindObjectsByType<BinderAssistant>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var assistant in assistants)
            {
                assistant.HasError = false;
                if (!assistant.OpenAutoValidate)
                {
                    continue;
                }

                foreach (var unit in assistant.Units)
                {
                    if (!unit.LabelObj)
                    {
                        assistant.HasError = true;
                    }

                    if (unit.HierarchyPath != BinderHierarchyUtility.GetRelativePath(assistant.HierarchyPath,
                            unit.LabelObj.GetComponent<BinderTag>().HierarchyPath))
                    {
                        assistant.HasError = true;
                    }
                }

                if (assistant.HasError)
                {
                    AesirModulesDebug.LogWarning(AesirModulesDebug.ObjectBinderTag,
                        assistant.name + " Binder Assistant 发现绑定错误，请重新生成绑定信息单元");
                }
            }
        }

        [DidReloadScripts]
        static void AttachToGameObject()
        {
            if (!EditorPrefs.HasKey("即将绑定脚本的物体 Id"))
            {
                return;
            }

            var targetObj =
                EditorUtility.InstanceIDToObject(EditorPrefs.GetInt("即将绑定脚本的物体 Id")) as GameObject;
            if (!targetObj)
            {
                EditorPrefs.DeleteKey("即将绑定脚本的物体 Id");
                EditorPrefs.DeleteKey("即将绑定的脚本类型");
                return;
            }

            var scriptType = Type.GetType(EditorPrefs.GetString("即将绑定的脚本类型"));
            if (scriptType == null)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.ObjectBinderTag, "即将绑定的脚本类型为空");
                return;
            }

            if (!targetObj.GetComponent(scriptType))
            {
                targetObj.AddComponent(scriptType);
            }

            var script = targetObj.GetComponent(scriptType);
            if (script is IComponentBinder bindReferences)
            {
                bindReferences.BindComponents();
            }

            Selection.activeObject = targetObj;
            EditorPrefs.DeleteKey("即将绑定脚本的物体 Id");
            EditorPrefs.DeleteKey("即将绑定的脚本类型");
        }
#endif
    }
}
