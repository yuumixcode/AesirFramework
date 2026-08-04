using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    public static class PackageManagerEditorUtility
    {
        static AddRequest _addRequest;
        static ListRequest _listRequest;
        static RemoveRequest _removeRequest;
        static PackageCollection _currentPackageCollection;

        public static bool IsBusy => _listRequest != null || _addRequest != null || _removeRequest != null;

        public static event Action OnPackagesChanged;

        public static async Task ListAllLocalPackages()
        {
            await ListPackagesAsyncOffline();
            if (_currentPackageCollection == null)
            {
                return;
            }

            foreach (var package in _currentPackageCollection)
            {
                AesirInspectorDebug.Info($"找到 Package: {package.name} @ {package.version}");
            }
        }

        public static void InstallPackageAsyncFromCard(ExtensionPackageCard card)
        {
            if (card == null)
            {
                return;
            }

            InstallPackageFromGitUrl(card.GitUrl);
        }

        public static void RemovePackageAsyncFromCard(ExtensionPackageCard card)
        {
            if (card == null)
            {
                return;
            }

            _ = RemovePackage(card.PackageName);
        }

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
                    AesirInspectorDebug.Info($"成功移除包：{packageName}");
                    break;
                case StatusCode.Failure:
                    AesirInspectorDebug.Error($"移除包失败：{_removeRequest.Error.message}");
                    OnPackagesChanged?.Invoke();
                    break;
            }

            _removeRequest = null;
        }

        public static void InstallPackageFromGitUrl(string gitUrl)
        {
            if (!string.IsNullOrEmpty(gitUrl) && gitUrl.Contains(".git"))
            {
                _addRequest = Client.Add(gitUrl);
                EditorApplication.update += InstallProgressUpdate;
            }
            else
            {
                AesirInspectorDebug.Error("无效的 Git URL，安装已取消");
            }
        }

        public static bool IsPackageInstalled(string packageName)
        {
            return _currentPackageCollection != null &&
                   _currentPackageCollection.Any(p => p.name == packageName);
        }

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
                AesirInspectorDebug.Info("成功获取当前项目的包列表！");
                _currentPackageCollection = _listRequest.Result;
            }
            else
            {
                AesirInspectorDebug.Error($"获取包列表失败！{_listRequest.Error.message}");
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
                    AesirInspectorDebug.Info($"成功安装包：{_addRequest.Result.displayName}");
                    break;
                case StatusCode.Failure:
                    AesirInspectorDebug.Error($"安装包失败：{_addRequest.Error.message}");
                    OnPackagesChanged?.Invoke();
                    break;
            }

            EditorApplication.update -= InstallProgressUpdate;
            _addRequest = null;
        }

        #endregion
    }
}
