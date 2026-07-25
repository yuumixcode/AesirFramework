using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [Summary("包管理器编辑器工具")]
    public static class PackageManagerEditorUtility
    {
        static AddRequest _addRequest;
        static ListRequest _listRequest;
        static RemoveRequest _removeRequest;
        static PackageCollection _currentPackageCollection;

        [Summary("是否正在处理包管理请求")]
        public static bool IsBusy => _listRequest != null || _addRequest != null || _removeRequest != null;

        [Summary("包安装或移除完成时触发的事件，用于通知外部刷新 UI 状态")]
        public static event Action OnPackagesChanged;

        [Summary("列出所有本地包")]
        public static async Task ListAllLocalPackages()
        {
            await ListPackagesAsyncOffline();
            if (_currentPackageCollection == null)
            {
                return;
            }

            foreach (var package in _currentPackageCollection)
            {
                AesirInspectorLogger.Info($"找到 Package: {package.name} @ {package.version}");
            }
        }

        [Summary("从卡片异步安装包")]
        public static void InstallPackageAsyncFromCard(ExtensionPackageCard card)
        {
            if (card == null)
            {
                return;
            }

            InstallPackageFromGitUrl(card.GitUrl);
        }

        [Summary("从卡片异步移除包")]
        public static void RemovePackageAsyncFromCard(ExtensionPackageCard card)
        {
            if (card == null)
            {
                return;
            }

            _ = RemovePackage(card.PackageName);
        }

        [Summary("根据包名移除包（异步）")]
        public static async Task RemovePackage(string packageName)
        {
            if (!IsPackageInstalled(packageName))
            {
                return;
            }

            _removeRequest = Client.Remove(packageName);
            while (!_removeRequest.IsCompleted)
            {
                await Task.Delay(50);
            }

            switch (_removeRequest.Status)
            {
                case StatusCode.Success:
                    AesirInspectorLogger.Info($"成功移除包：{packageName}");
                    break;
                case StatusCode.Failure:
                    AesirInspectorLogger.Error($"移除包失败：{_removeRequest.Error.message}");
                    OnPackagesChanged?.Invoke();
                    break;
            }

            _removeRequest = null;
        }

        [Summary("从 Git URL 安装包")]
        public static void InstallPackageFromGitUrl(string gitUrl)
        {
            if (!string.IsNullOrEmpty(gitUrl) && gitUrl.Contains(".git"))
            {
                _addRequest = Client.Add(gitUrl);
                EditorApplication.update += InstallProgressUpdate;
            }
            else
            {
                AesirInspectorLogger.Error("无效的 Git URL，安装已取消");
            }
        }

        [Summary("检查包是否已安装")]
        public static bool IsPackageInstalled(string packageName)
        {
            return _currentPackageCollection != null &&
                   _currentPackageCollection.Any(p => p.name == packageName);
        }

        [Summary("异步离线列出包")]
        public static async Task ListPackagesAsyncOffline()
        {
            _currentPackageCollection = null;
            _listRequest = Client.List(true, false);
            while (!_listRequest.IsCompleted)
            {
                await Task.Delay(50);
            }

            if (_listRequest.Status == StatusCode.Success)
            {
                AesirInspectorLogger.Info("成功获取当前项目的包列表！");
                _currentPackageCollection = _listRequest.Result;
            }
            else
            {
                AesirInspectorLogger.Error($"获取包列表失败！{_listRequest.Error.message}");
            }

            _listRequest = null;
        }

        #region Internal

        static void InstallProgressUpdate()
        {
            if (_addRequest == null || !_addRequest.IsCompleted)
            {
                return;
            }

            switch (_addRequest.Status)
            {
                case StatusCode.Success:
                    AesirInspectorLogger.Info($"成功安装包：{_addRequest.Result.displayName}");
                    break;
                case StatusCode.Failure:
                    AesirInspectorLogger.Error($"安装包失败：{_addRequest.Error.message}");
                    OnPackagesChanged?.Invoke();
                    break;
            }

            EditorApplication.update -= InstallProgressUpdate;
            _addRequest = null;
        }

        #endregion
    }
}
