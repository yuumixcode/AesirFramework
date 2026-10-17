using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityTcp.Editor.Tools;

namespace TJGenerators.Editor.Tests
{
    /// <summary>
    /// Manual EditMode samples for 3D model window adaptation via the generation Service (non-GUI).
    /// Agent-facing 3D generation has moved to the MCP backend (generate_3d_model + check_task);
    /// Unity-side landing uses import_3d_model_from_url. These samples keep covering the
    /// Editor-window generator path (window UI still submits through TJGeneratorsGenerationService).
    /// One submit per scenario to avoid burning credits.
    /// </summary>
    [Explicit("Manual EditMode sample — select and Run in Test Runner after login")]
    public class CustomTool3DModelSampleTests : CustomToolSampleTestBase
    {
        // ── Rodin (Editor 窗口生成器) ─────────────────────────────────────────

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator Rodin_EditorWindow_Text_WaitUntilCompleted()
        {
            yield return RunEditorGeneratorWaitUntilCompleted(
                "rodin",
                "Rodin_EditorWindow_Text_WaitDone",
                generator => generator.SetTextPrompt(DefaultPrompt));
        }

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator Rodin_EditorWindow_ImageOnly_WaitUntilCompleted()
        {
            string imagePath = null;
            yield return GenerateAiSingleRef(path => imagePath = path);

            yield return RunEditorGeneratorWaitUntilCompleted(
                "rodin",
                "Rodin_EditorWindow_ImageOnly",
                generator => generator.SetImagePath(imagePath));
        }

        // ── Tripo 3D / tripo v3.1 (Editor 窗口生成器) ─────────────────────────

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator TripoV31_Text_WaitUntilCompleted()
        {
            yield return RunEditorGeneratorWaitUntilCompleted(
                "tripo",
                "TripoV31_Text_WaitDone",
                generator => generator.SetTextPrompt(DefaultPrompt));
        }

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator TripoV31_Multiview_AiRefs_WaitUntilCompleted()
        {
            string[] mvPaths = null;
            yield return GenerateAiMultiViewRefs(paths => mvPaths = paths);

            yield return RunEditorGeneratorWaitUntilCompleted(
                "tripo",
                "TripoV31_Multiview_AiRefs",
                generator => generator.SetMultiViewPaths(mvPaths));
        }

    }
}
