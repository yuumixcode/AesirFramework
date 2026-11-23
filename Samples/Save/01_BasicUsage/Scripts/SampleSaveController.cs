#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Runestone.AesirModules.Samples.Save.BasicUsage
{
    /// <summary>
    /// Save 模块基础用法示例 —— OnGUI 面板驱动全部公开 API：
    /// 保存 / 加载 / 删除存档槽、自动保存开关、加载状态与来源展示、破坏主存档演示备份回退。
    /// </summary>
    /// <remarks>
    /// 本示例演示运行时自动创建路径：场景中无需预放置 SaveModule，首次调用静态 API 时
    /// 自动挂载到 [Aesir Modules] 宿主下。面板文字使用系统动态中文字体（与 Audio 示例同款）。
    /// 公开方法供编辑器脚本 / Bridge 驱动验证（OnGUI 按钮只是壳）。
    /// </remarks>
    [AddComponentMenu("")]
    public class SampleSaveController : MonoBehaviour
    {
        /// <summary>示例存档数据（纯数据对象：计数器 + 背包）。</summary>
        [Serializable]
        public class DemoSaveData
        {
            public int counter;
            public List<string> inventory = new List<string>();
        }

        const string SlotKey = "demo-slot";

        static readonly string[] FontCandidates =
        {
            "Microsoft YaHei", // Windows 简体中文
            "Microsoft YaHei UI",
            "PingFang SC",      // macOS 简体中文
            "Noto Sans CJK SC", // Linux / 跨平台
            "WenQuanYi Zen Hei" // 部分 Linux 发行版
        };

        DemoSaveData _data = new DemoSaveData();
        string _status = "尚未保存";
        bool _autoSaveEnabled;
        Font _dynamicFont;

        /// <summary>最近一次加载结果的状态文本（Status / Source / Error）。</summary>
        public string StatusText => _status;

        /// <summary>当前内存数据（验证用）。</summary>
        public DemoSaveData Data => _data;

        void Awake()
        {
            foreach (var candidate in FontCandidates)
            {
                _dynamicFont = Font.CreateDynamicFontFromOSFont(candidate, 16);
                if (_dynamicFont != null)
                {
                    break;
                }
            }
        }

        void OnDestroy()
        {
            SaveModule.ClearAutoSave();
        }

        /// <summary>计数器 +1（修改内存数据，尚未落盘）。</summary>
        public void Increment()
        {
            _data.counter++;
            _status = $"内存计数 {_data.counter}（未保存）";
        }

        /// <summary>向背包加一件物品。</summary>
        public void AddItem()
        {
            _data.inventory.Add($"物品{_data.inventory.Count + 1}");
            _status = $"背包 {_data.inventory.Count} 件（未保存）";
        }

        /// <summary>保存当前内存数据到存档槽（事务式写入）。</summary>
        public void SaveDemo()
        {
            var result = SaveModule.Save(SlotKey, _data);
            _status = result.IsSuccess
                ? $"已保存：计数 {_data.counter}、背包 {_data.inventory.Count} 件"
                : $"保存失败：{result.Error}";
        }

        /// <summary>加载存档槽（主存档损坏时自动回退备份，状态文本标明来源）。</summary>
        public void LoadDemo()
        {
            var result = SaveModule.Load<DemoSaveData>(SlotKey);
            switch (result.Status)
            {
                case LoadStatus.Success:
                    _data = result.Value;
                    _status = $"加载成功（来源 {result.Source}）：计数 {_data.counter}、背包 {_data.inventory.Count} 件";
                    break;
                case LoadStatus.NoSave:
                    _status = "无存档（NoSave）";
                    break;
                default:
                    _status = $"加载失败（{result.Status}）：{result.Error}";
                    break;
            }
        }

        /// <summary>删除存档槽。</summary>
        public void DeleteDemo()
        {
            SaveModule.Delete(SlotKey);
            _status = "已删除存档";
        }

        /// <summary>切换自动保存（10 秒周期 + 切后台 / 退出钩子）。</summary>
        public void ToggleAutoSave()
        {
            _autoSaveEnabled = !_autoSaveEnabled;
            if (_autoSaveEnabled)
            {
                SaveModule.SetAutoSave(SaveDemo, 10f);
                _status = "自动保存已开启（10 秒周期）";
            }
            else
            {
                SaveModule.ClearAutoSave();
                _status = "自动保存已关闭";
            }
        }

        /// <summary>
        /// 破坏主存档字节（演示用）：下次加载应校验失败并自动回退备份。
        /// 直接写存储目录的主文件，模拟磁盘损坏 / 位翻转。
        /// </summary>
        public void CorruptPrimarySave()
        {
            var path = Path.Combine(Application.persistentDataPath, "Saves", SlotKey + ".sav");
            if (!File.Exists(path))
            {
                _status = "主存档不存在，先保存一次再破坏";
                return;
            }

            File.WriteAllBytes(path, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
            _status = "主存档已破坏——点「加载」看备份回退";
        }

        void OnGUI()
        {
            if (_dynamicFont != null)
            {
                GUI.skin.label.font = _dynamicFont;
                GUI.skin.button.font = _dynamicFont;
                GUI.skin.toggle.font = _dynamicFont;
            }

            GUILayout.BeginArea(new Rect(20, 20, 420, 400), GUI.skin.box);
            GUILayout.Label("Save 模块基础用法示例", GUI.skin.label);
            GUILayout.Space(6);
            GUILayout.Label($"内存数据：计数 {_data.counter}／背包 {_data.inventory.Count} 件");
            GUILayout.Label($"状态：{_status}");
            GUILayout.Space(6);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("计数 +1")) Increment();
            if (GUILayout.Button("加物品")) AddItem();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("保存")) SaveDemo();
            if (GUILayout.Button("加载")) LoadDemo();
            if (GUILayout.Button("删除")) DeleteDemo();
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            var autoSave = GUILayout.Toggle(_autoSaveEnabled, "自动保存（10 秒周期 + 切后台/退出）");
            if (autoSave != _autoSaveEnabled)
            {
                ToggleAutoSave();
            }

            GUILayout.Space(6);
            if (GUILayout.Button("破坏主存档（演示备份回退）")) CorruptPrimarySave();
            GUILayout.Label("流程：保存 → 计数+1 再保存 → 破坏主存档 → 加载（应显示来源 Backup）");
            GUILayout.EndArea();
        }
    }
}
#endif