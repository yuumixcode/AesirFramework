using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Runestone.AesirModules
{
    /// <summary>
    /// SceneModule 的 UniTask 适配 API——游戏工程包含 UniTask 时（宏 <c>AESIR_MODULES_UNITASK</c> 自动维护），
    /// 提供可 await 的场景加载/卸载流程。本类型位于独立适配程序集
    /// <c>Runestone.AesirModules.UniTask</c>，未包含 UniTask 时整体不编译，公开 API 与
    /// <see cref="SceneModule" /> 静态门面一一对应。
    /// <para>
    /// 失败语义：场景路径无效、引用无效（不在 BuildSettings）、Addressable 场景等失败情形，
    /// await 侧抛出 <see cref="InvalidOperationException" />——具体原因已由 SceneModule 输出到 Console。
    /// </para>
    /// <para>
    /// 取消语义：取消仅中止等待（await 侧抛 <see cref="OperationCanceledException" />），
    /// 底层加载/卸载流程继续完成（Unity 场景操作不支持中途取消）；SceneModule 宿主被销毁时
    /// 内部流程静默中止，等待方同样以取消收场（不会无限悬挂）。
    /// </para>
    /// </summary>
    public static class SceneModuleUniTask
    {
        /// <summary>
        /// 加载场景并等待完成。Single 模式：卸载全部场景、重设激活场景、加载成功后清空叠加追踪（失败时保留）。
        /// </summary>
        /// <param name="scenePath">场景路径（须已登记 BuildSettings）。</param>
        /// <param name="onProgress">逐帧进度回调（0-1，已按 <c>SceneModuleConfigSO.progressCap</c> 归一化，默认 0.9），随等待期间持续报告。</param>
        /// <param name="cancellationToken">取消令牌（仅中止等待，流程本身继续完成）。</param>
        public static UniTask LoadSceneSingleAsync(string scenePath,
            Action<float> onProgress = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var source = new UniTaskCompletionSource();
            SceneModule.LoadSceneSingle(scenePath,
                () => source.TrySetResult(),
                () => source.TrySetException(BuildFailureException("场景加载")),
                onProgress);
            return AwaitWithCancellation(source, cancellationToken);
        }

        /// <summary>
        /// 加载场景并等待完成。Single 模式。通过 <see cref="SceneAssetWrapper" /> 指定场景。
        /// 引用无效（空/不在 BuildSettings）或为 Addressable 场景时抛出 <see cref="InvalidOperationException" />。
        /// </summary>
        public static UniTask LoadSceneSingleAsync(SceneAssetWrapper sceneRef,
            Action<float> onProgress = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var source = new UniTaskCompletionSource();
            SceneModule.LoadSceneSingle(sceneRef,
                () => source.TrySetResult(),
                () => source.TrySetException(BuildFailureException("场景加载")),
                onProgress);
            return AwaitWithCancellation(source, cancellationToken);
        }

        /// <summary>
        /// 加载场景并等待完成。Additive 模式：纯叠加、不改变激活场景（对齐 Unity 原生语义），并记入叠加追踪。
        /// <para>
        /// 约定：请勿对同一路径重复叠加加载——Unity 会加载两个场景实例，而追踪列表按路径粒度只记录一次。
        /// </para>
        /// </summary>
        public static UniTask LoadSceneAdditiveAsync(string scenePath,
            Action<float> onProgress = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var source = new UniTaskCompletionSource();
            SceneModule.LoadSceneAdditive(scenePath,
                () => source.TrySetResult(),
                () => source.TrySetException(BuildFailureException("场景加载")),
                onProgress);
            return AwaitWithCancellation(source, cancellationToken);
        }

        /// <summary>
        /// 加载场景并等待完成。Additive 模式。通过 <see cref="SceneAssetWrapper" /> 指定场景。
        /// 引用无效（空/不在 BuildSettings）或为 Addressable 场景时抛出 <see cref="InvalidOperationException" />。
        /// </summary>
        public static UniTask LoadSceneAdditiveAsync(SceneAssetWrapper sceneRef,
            Action<float> onProgress = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var source = new UniTaskCompletionSource();
            SceneModule.LoadSceneAdditive(sceneRef,
                () => source.TrySetResult(),
                () => source.TrySetException(BuildFailureException("场景加载")),
                onProgress);
            return AwaitWithCancellation(source, cancellationToken);
        }

        /// <summary>
        /// 卸载场景并等待完成。若该场景在叠加追踪列表中则自动移出。
        /// 场景不存在等失败情形抛出 <see cref="InvalidOperationException" />。
        /// </summary>
        public static UniTask UnloadSceneAsync(string scenePath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var source = new UniTaskCompletionSource();
            SceneModule.UnloadScene(scenePath,
                () => source.TrySetResult(),
                () => source.TrySetException(BuildFailureException("场景卸载")));
            return AwaitWithCancellation(source, cancellationToken);
        }

        /// <summary>
        /// 卸载场景并等待完成。通过 <see cref="SceneAssetWrapper" /> 指定场景。
        /// </summary>
        public static UniTask UnloadSceneAsync(SceneAssetWrapper sceneRef, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var source = new UniTaskCompletionSource();
            SceneModule.UnloadScene(sceneRef,
                () => source.TrySetResult(),
                () => source.TrySetException(BuildFailureException("场景卸载")));
            return AwaitWithCancellation(source, cancellationToken);
        }

        /// <summary>
        /// 重新加载当前激活场景并等待完成。异步 Single 模式，加载成功后清空叠加场景追踪。
        /// 编辑器中激活场景尚未保存（无有效路径）时抛出 <see cref="InvalidOperationException" />。
        /// </summary>
        public static UniTask ReloadSceneAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var source = new UniTaskCompletionSource();
            SceneModule.ReloadScene(
                () => source.TrySetResult(),
                () => source.TrySetException(BuildFailureException("场景重载")));
            return AwaitWithCancellation(source, cancellationToken);
        }

        /// <summary>
        /// 卸载所有经本模块叠加加载的场景并等待完成。单个场景卸载失败（场景已被外部卸载）时跳过并告警，不影响其余场景。
        /// </summary>
        public static UniTask UnloadAllAddedScenesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var source = new UniTaskCompletionSource();
            SceneModule.UnloadAllAddedScenes(() => source.TrySetResult());
            return AwaitWithCancellation(source, cancellationToken);
        }

        /// <summary>
        /// 等待完成源，同时链接取消令牌与 SceneModule 宿主的销毁令牌：
        /// 主动取消或宿主被销毁（内部流程静默中止、完成回调不再触发）时，等待方以
        /// <see cref="OperationCanceledException" /> 收场，不会无限悬挂。
        /// </summary>
        static async UniTask AwaitWithCancellation(UniTaskCompletionSource source, CancellationToken cancellationToken)
        {
            var host = SceneModule.Instance;

            using (var linked = CancellationTokenSource.CreateLinkedTokenSource(
                       cancellationToken, host.destroyCancellationToken))
            using (linked.Token.Register(static state => ((UniTaskCompletionSource)state).TrySetCanceled(), source))
            {
                await source.Task;
            }
        }

        /// <summary>
        /// 构造失败异常。失败的具体原因已由 SceneModule 输出错误日志，异常消息仅指引排查方向。
        /// </summary>
        static InvalidOperationException BuildFailureException(string operation)
        {
            return new InvalidOperationException($"{operation}失败（具体原因见 Console 的 SceneModule 错误日志）。");
        }
    }
}
