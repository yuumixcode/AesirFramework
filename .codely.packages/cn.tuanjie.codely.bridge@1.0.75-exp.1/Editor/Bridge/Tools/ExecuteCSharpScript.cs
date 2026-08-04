using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Codely.Newtonsoft.Json.Linq;
using Codely.Microsoft.CodeAnalysis;
using Codely.Microsoft.CodeAnalysis.Emit;
using Codely.Microsoft.CodeAnalysis.CSharp;
using Codely.Microsoft.CodeAnalysis.CSharp.Scripting;
using Codely.Microsoft.CodeAnalysis.CSharp.Syntax;
using Codely.Microsoft.CodeAnalysis.Scripting;
using UnityEngine;
using UnityEditor;
using UnityTcp.Editor.Helpers;

namespace UnityTcp.Editor.Tools
{
    public static class ExecuteCSharpScript
    {
        static readonly List<string> s_CapturedLogs = new List<string>();
        static bool s_IsCapturingLogs;

        static readonly string s_ShadowCopyDir = Path.Combine(
            Application.temporaryCachePath,
            "CodelyScriptRefs"
        );

        static readonly string[] s_ShadowCopyAssemblyNames =
        {
            "Assembly-CSharp",
            "Assembly-CSharp-Editor"
        };

        static readonly List<ScriptFixProvider> s_FixProviders = new List<ScriptFixProvider>
        {
            new FixMissingImports(),
            new FixMissingAssemblyReference(),
            new FixUnqualifiedUnityStaticMethod(),
            new FixMissingParenthesis(),
            new FixMissingBrace(),
            new FixMissingSquareBracket(),
            new FixMissingSemicolon(),
            new FixAmbiguousReference()
        };

        const int k_MaxFixIterations = 50;
        const string k_TextMeshProUGUIKeyword = "TextMeshProUGUI";
        const string k_TmpSettingsAssetPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

        public static object HandleCommand(JObject @params)
        {
            string script = @params["script"]?.ToString();
            string scriptPath = @params["script_path"]?.ToString();
            string description = @params["description"]?.ToString();

            // At least one of script or script_path must be provided
            if (string.IsNullOrEmpty(script) && string.IsNullOrEmpty(scriptPath))
                return Response.Error("'script' parameter is required.");

            // Enforce the caller's declared execution mode against Unity's actual play state.
            // 'play' requires play mode, 'editor' requires edit mode; omitted means either is fine.
            string executionMode = @params["execution_mode"]?.ToString();
            if (!string.IsNullOrEmpty(executionMode))
            {
                bool isPlaying = UnityEditor.EditorApplication.isPlaying;
                // These branches refuse execution: the script never runs, so they
                // must return an error rather than a misleading success response.
                switch (executionMode)
                {
                    case "play":
                        if (!isPlaying)
                            return Response.Error(
                                "execution_mode is 'play' but Unity is not in play mode. " +
                                "Enter play mode before running this script.");
                        break;
                    case "editor":
                        if (isPlaying)
                            return Response.Error(
                                "execution_mode is 'editor' but Unity is in play mode. " +
                                "Exit play mode before running this script.");
                        break;
                    default:
                        return Response.Error(
                            $"Invalid execution_mode '{executionMode}'. Expected 'play', 'editor', or omitted.");
                }
            }

            if (!string.IsNullOrEmpty(description))
                CodelyLogger.Log($"[ExecuteCSharpScript] Description: {description}");

            // If script_path is provided (legacy support), read the file content
            if (!string.IsNullOrEmpty(scriptPath))
            {
                try
                {
                    if (!File.Exists(scriptPath))
                    {
                        // A U+FFFD in the path means the bytes were already decoded as UTF-8 with
                        // replacement at the TCP layer — the client sent a non-UTF-8 path and the
                        // original bytes are unrecoverable here. Diagnose it instead of a vague 404.
                        if (scriptPath.IndexOf((char)0xFFFD) >= 0)
                            return Response.Error(
                                $"Script file not found, and the path contains replacement characters (U+FFFD): '{scriptPath}'. " +
                                "The path was likely sent in a non-UTF-8 encoding (JSON must be UTF-8). " +
                                "Fix the client to send the path as UTF-8 — the original path cannot be recovered on this side.");
                        return Response.Error($"Script file not found: {scriptPath}");
                    }

                    script = ReadScriptFileSmart(scriptPath);
                    CodelyLogger.Log($"[ExecuteCSharpScript] Loaded script from file: {scriptPath} ({script.Length} chars)");

                    if (string.IsNullOrWhiteSpace(script))
                        return Response.Error($"Script file is empty: {scriptPath}");
                }
                catch (IOException ioEx)
                {
                    return Response.Error($"Failed to read script file: {ioEx.Message}");
                }
                catch (UnauthorizedAccessException uaEx)
                {
                    return Response.Error($"Access denied to script file: {uaEx.Message}");
                }
            }
            // Auto-detect if script parameter is a file path
            // Heuristic: single line, ends with .cs, and file exists
            else if (!string.IsNullOrEmpty(script))
            {
                var trimmedScript = script.Trim();
                bool looksLikePath = !trimmedScript.Contains("\n") &&
                                     trimmedScript.EndsWith(".cs", StringComparison.OrdinalIgnoreCase);

                if (looksLikePath && File.Exists(trimmedScript))
                {
                    try
                    {
                        script = ReadScriptFileSmart(trimmedScript);
                        CodelyLogger.Log($"[ExecuteCSharpScript] Auto-detected and loaded script from path: {trimmedScript} ({script.Length} chars)");

                        if (string.IsNullOrWhiteSpace(script))
                            return Response.Error($"Script file is empty: {trimmedScript}");
                    }
                    catch (IOException ioEx)
                    {
                        return Response.Error($"Failed to read script file: {ioEx.Message}");
                    }
                    catch (UnauthorizedAccessException uaEx)
                    {
                        return Response.Error($"Access denied to script file: {uaEx.Message}");
                    }
                }
            }

            ScheduleTmpEssentialsImportIfNeeded(script);

            bool captureLogs = @params["capture_logs"]?.ToObject<bool>() ?? true;
            string[] imports = @params["imports"]?.ToObject<string[]>() ?? new[]
            {
                "System",
                "System.Linq",
                "System.Collections.Generic",
                "UnityEngine",
                "UnityEditor",
                "UnityEditor.SceneManagement",
                "UnityEngine.SceneManagement"
            };

            try
            {
                CodelyLogger.Log($"[ExecuteCSharpScript] Executing script ({script.Length} chars, {imports.Length} imports)");

                // Returns a fully-built response for the synchronous case (a value, or the script's
                // OWN runtime failure, both with captured logs), or a JobContext for the async case
                // (a runner job was scheduled and will enqueue the response when it finishes). Log
                // capture is owned by ExecuteFromCompilation, scoped tightly around the invoke.
                var result = ExecuteScriptInternal(script, imports, captureLogs);

                if (result is JobContext ctx)
                    return ctx;

                CodelyLogger.Log($"[ExecuteCSharpScript] Response: {Codely.Newtonsoft.Json.JsonConvert.SerializeObject(result)}");
                return result;
            }
            catch (BlockingCallException bce)
            {
                // The script was rejected before any user code ran because it contains a call that
                // would block the Unity main thread. Report it as a plain, actionable message.
                CodelyLogger.LogWarning($"[ExecuteCSharpScript] Rejected script: {bce.Message}");
                return Response.Error(bce.Message);
            }
            catch (NoTopLevelStatementsException ne)
            {
                // The script compiled but had nothing to run. Report it as an error (not a success)
                // so the caller fixes the missing call instead of trusting a no-op result.
                CodelyLogger.LogWarning($"[ExecuteCSharpScript] Rejected script: {ne.Message}");
                return Response.Error(ne.Message);
            }
            catch (Exception e)
            {
                // Compilation / setup failure before the script ran — no captured logs to attach.
                var errorResponse = BuildScriptFailureResponse(e, new List<string>());
                CodelyLogger.LogError(
                    $"[ExecuteCSharpScript] Script execution failed: {e?.Message}\n{SafeGetStackTrace(e)}");
                CodelyLogger.Log($"[ExecuteCSharpScript] Error Response: {Codely.Newtonsoft.Json.JsonConvert.SerializeObject(errorResponse)}");
                return errorResponse;
            }
        }

        // Reading Exception.StackTrace can itself throw on Mono/Unity: when the exception's call
        // stack passes through Roslyn's async scripting state machine (Script`1+<RunSubmissionsAsync>),
        // Mono's StackTrace.ConvertAsyncStateMachineMethod tries to decode custom attributes on a type
        // it cannot fully load and raises a TypeLoadException. Access it defensively so surfacing the
        // error never crashes the handler; fall back to the async-safe frame list when it does.
        static string SafeGetStackTrace(Exception e)
        {
            if (e == null)
                return string.Empty;

            try
            {
                return e.StackTrace ?? string.Empty;
            }
            catch (Exception traceEx)
            {
                CodelyLogger.LogWarning(
                    $"[ExecuteCSharpScript] Could not format exception stack trace ({traceEx.GetType().Name}): {traceEx.Message}");

                // Best-effort manual walk that avoids ToString()/attribute decoding on async frames.
                // needFileInfo:true resolves line numbers from the emitted PDB when available.
                try
                {
                    var trace = new System.Diagnostics.StackTrace(e, true);
                    var sb = new StringBuilder();
                    foreach (var frame in trace.GetFrames() ?? Array.Empty<System.Diagnostics.StackFrame>())
                    {
                        var method = frame.GetMethod();
                        if (method == null)
                            continue;
                        sb.Append($"  at {method.DeclaringType?.FullName}.{method.Name}");
                        var line = frame.GetFileLineNumber();
                        if (line > 0)
                            sb.Append($" (in {Path.GetFileName(frame.GetFileName())}:{line})");
                        sb.AppendLine();
                    }
                    return sb.Length > 0 ? sb.ToString() : "(stack trace unavailable)";
                }
                catch
                {
                    return "(stack trace unavailable)";
                }
            }
        }

        static void ScheduleTmpEssentialsImportIfNeeded(string script)
        {
            if (string.IsNullOrEmpty(script))
                return;

            if (script.IndexOf(k_TextMeshProUGUIKeyword, StringComparison.Ordinal) < 0)
                return;

            if (File.Exists(k_TmpSettingsAssetPath))
                return;

            TmpEssentialsAutoImporter.ScheduleImport();
            CodelyLogger.Log("[ExecuteCSharpScript] Scheduled TMP essential resources import because script references TextMeshProUGUI.");
        }

        // Thrown when a script contains a call that would block the Unity main thread.
        class BlockingCallException : Exception
        {
            public BlockingCallException(string message) : base(message) { }
        }

        // Thrown when a script declares types but has no top-level statements, so running it is a no-op.
        internal class NoTopLevelStatementsException : Exception
        {
            public NoTopLevelStatementsException(string message) : base(message) { }
        }

        // A Roslyn submission that contains only declarations (types, methods, usings) compiles and
        // runs cleanly while doing nothing at all — the submission body is empty. Reporting that as a
        // success is actively misleading: the caller sees no error, no logs, and no effect, and is left
        // debugging the declared code instead of the missing call. Reject it instead.
        internal static void CheckForTopLevelStatements(Compilation compilation)
        {
            var root = compilation.SyntaxTrees.First().GetRoot();

            // Global statements only ever appear at the root of a script compilation unit, so a
            // shallow scan is enough — and it never mistakes a method body for an executable statement.
            if (root.ChildNodes().OfType<GlobalStatementSyntax>().Any())
                return;

            throw new NoTopLevelStatementsException(
                "Script contains only declarations and no top-level statements — nothing was executed. " +
                "This is a Roslyn script submission: it requires at least one top-level statement, so " +
                "add a call to the declared code at the end, or write the body as top-level statements.");
        }

        // Scans the (already compiled) script for calls that synchronously block the main thread and
        // throws BlockingCallException if any are found. Detection is SEMANTIC, not name-based: each
        // call is bound to its real symbol and matched against the exact declaring type, so innocent
        // look-alikes (string.Join, Enumerable.Join, Path.Join, a user type's own .Result/.Wait(),
        // SpinWait.SpinOnce, etc.) are never flagged. Symbols that cannot be resolved are skipped.
        static void CheckForBlockingCalls(string script, List<string> imports, List<MetadataReference> references,
            Compilation compilation)
        {
            if (string.IsNullOrEmpty(script))
                return;

            // Reuse the compilation produced by CompileAndAutoFix when it matches the final script.
            // Only fall back to a fresh (expensive) compilation when one wasn't provided.
            if (compilation == null)
            {
                var options = ScriptOptions.Default
                    .WithReferences(references)
                    .WithImports(imports);
                compilation = CSharpScript.Create(script, options).GetCompilation();
            }
            var tree = compilation.SyntaxTrees.First();
            var model = compilation.GetSemanticModel(tree);

            var violations = new List<string>();

            foreach (var node in tree.GetRoot().DescendantNodes())
            {
                if (node is InvocationExpressionSyntax invocation)
                {
                    if (model.GetSymbolInfo(invocation).Symbol is IMethodSymbol method && IsBlockingMethod(method))
                        violations.Add($"  - Line {GetLine(invocation)}: {method.ContainingType.Name}.{method.Name}(...)");
                }
                else if (node is MemberAccessExpressionSyntax member && !(member.Parent is InvocationExpressionSyntax))
                {
                    if (model.GetSymbolInfo(member).Symbol is IPropertySymbol property && IsBlockingProperty(property))
                        violations.Add($"  - Line {GetLine(member)}: {property.ContainingType.Name}.{property.Name}");
                }
            }

            if (violations.Count == 0)
                return;

            // De-duplicate identical lines while preserving order.
            var seen = new HashSet<string>();
            var unique = violations.Where(v => seen.Add(v)).ToList();

            throw new BlockingCallException(
                "Blocking calls are not allowed in execute_csharp_script — the script runs on the Unity " +
                "main thread and these would freeze the editor:\n" +
                string.Join("\n", unique) + "\n" +
                "Disallowed: Thread.Sleep, Thread.SpinWait, Thread.Join, Task.Wait/WaitAll/WaitAny, " +
                "Task/ValueTask.Result, Monitor.Wait, WaitHandle.WaitOne, and GetAwaiter().GetResult(). " +
                "Use EditorApplication.update or EditorApplication.delayCall instead.");
        }

        // True only for methods declared on the specific blocking types below — never for a same-named
        // method on any other type.
        static bool IsBlockingMethod(IMethodSymbol method)
        {
            var declaringType = method.ContainingType?.OriginalDefinition?.ToDisplayString();
            switch (method.Name)
            {
                case "Sleep":
                case "SpinWait":
                case "Join":
                    return declaringType == "System.Threading.Thread";
                case "Wait":
                    return declaringType == "System.Threading.Tasks.Task"
                        || declaringType == "System.Threading.Monitor"
                        || declaringType == "System.Threading.SemaphoreSlim"
                        || declaringType == "System.Threading.ManualResetEventSlim"
                        || declaringType == "System.Threading.CountdownEvent";
                case "WaitAll":
                case "WaitAny":
                    return declaringType == "System.Threading.Tasks.Task"
                        || declaringType == "System.Threading.WaitHandle";
                case "WaitOne":
                    return declaringType == "System.Threading.WaitHandle";
                case "GetResult":
                    // task.GetAwaiter().GetResult() — the awaiter types live in this namespace and
                    // all end in "Awaiter" (TaskAwaiter, TaskAwaiter<T>, ConfiguredTaskAwaiter, ...).
                    return method.ContainingType?.ContainingNamespace?.ToDisplayString()
                            == "System.Runtime.CompilerServices"
                        && method.ContainingType.Name.EndsWith("Awaiter", StringComparison.Ordinal);
                default:
                    return false;
            }
        }

        // True only for Task<T>.Result / ValueTask<T>.Result — the blocking synchronous accessors.
        static bool IsBlockingProperty(IPropertySymbol property)
        {
            if (property.Name != "Result")
                return false;
            var declaringType = property.ContainingType?.OriginalDefinition?.ToDisplayString();
            return declaringType == "System.Threading.Tasks.Task<TResult>"
                || declaringType == "System.Threading.Tasks.ValueTask<TResult>";
        }

        static int GetLine(SyntaxNode node) =>
            node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

        static object ExecuteScriptInternal(string script, string[] imports, bool captureLogs)
        {
            SaveScriptToTemp(script);

            try
            {
                // Build minimal base references — no pre-loaded optional modules
                var references = BuildBaseReferences();
                var fixedImports = new List<string>(imports);
                var fixedScript = script;

                // Compile and auto-fix before execution. Returns the compilation for the final
                // script (or null if it couldn't be matched) so the blocking-call scan can reuse it.
                var compilation = CompileAndAutoFix(ref fixedScript, fixedImports, references);

                // Reject scripts that would block the Unity main thread. Even with the async/coroutine
                // runners, a synchronous blocking wait (Thread.Sleep, Task.Wait/.Result,
                // GetAwaiter().GetResult(), …) still freezes the whole editor (and this bridge) before
                // any runner can tick. The scan only flags those blocking calls — never `await` — so
                // await-based async scripts and coroutines pass through untouched.
                CheckForBlockingCalls(fixedScript, fixedImports, references, compilation);

                var options = ScriptOptions.Default
                    .WithReferences(references)
                    .WithImports(fixedImports)
                    // Emit a PDB so runtime exceptions carry line numbers that map back to the
                    // submission source — without this the stack trace only shows
                    // "Submission#0+<<Initialize>>d__0.MoveNext" with no position.
                    .WithEmitDebugInformation(true)
                    .WithFilePath("CodelyScript.csx")
                    .WithFileEncoding(Encoding.UTF8);

                // Compile, load, invoke, and — for scripts that run across frames — schedule the
                // async work on a runner. Returns a fully-built response when the script ran
                // synchronously, or a JobContext when it went async.
                return ExecuteFromCompilation(fixedScript, options, captureLogs);
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException != null)
                    throw ae.InnerException;
                throw;
            }
        }

        // Compiles, emits to in-memory PE+PDB, loads via Assembly.Load(byte[]), and invokes the
        // submission entry point via reflection — bypassing InteractiveAssemblyLoader entirely.
        // On Mono with ACP=936, the loader's RegisterDependency calls Assembly.GetName() ->
        // get_code_base(), which throws EILSEQ when the project path contains non-ASCII
        // characters (e.g. Chinese) that require code page conversion.
        //
        // Roslyn's entry point always returns Task<object>. We must NOT call .Wait() — that blocks
        // the Unity main thread (and this bridge). When the script ran synchronously (e.g. "1 + 2")
        // we build and return its response directly; when it is still running (awaited) we hand the
        // Task to the AsyncTaskRunner, and when it returned a coroutine/Task to run across frames we
        // hand that to the CoroutineRunner/AsyncTaskRunner — returning a JobContext so the reply is
        // deferred until the runner job finishes. (We also avoid CSharpScript.EvaluateAsync, whose
        // InteractiveAssemblyLoader hits the same EILSEQ path described above.)
        //
        // Returns a fully-built response (sync) or a JobContext (async).
        static object ExecuteFromCompilation(string script, ScriptOptions options, bool captureLogs)
        {
            var compilation = CSharpScript.Create(script, options).GetCompilation();

            byte[] pe, pdb;
            using (var peStream = new MemoryStream())
            using (var pdbStream = new MemoryStream())
            {
                var emitResult = compilation.Emit(
                    peStream, pdbStream, null, null, null,
                    new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb),
                    CancellationToken.None);

                if (!emitResult.Success)
                    throw new Exception("Script compilation failed during emit:\n" +
                        string.Join("\n", emitResult.Diagnostics
                            .Where(d => d.Severity == DiagnosticSeverity.Error)
                            .Select(d => $"  {d.Id}: {d.GetMessage()}")));

                pe = peStream.ToArray();
                pdb = pdbStream.ToArray();
            }

            // Only once the script is known to compile: reject a declaration-only submission, which
            // would "succeed" while doing nothing. Checking this before emit would mask the real
            // diagnostics of a script that both fails to compile and lacks an entry call — including
            // errors that are themselves the reason it has none (a namespace declaration, illegal in
            // script code, makes every statement inside it a non-global one).
            CheckForTopLevelStatements(compilation);

            var assembly = Assembly.Load(pe, pdb);
            var entryPoint = compilation.GetEntryPoint(CancellationToken.None);

            var type = assembly.GetType(entryPoint.ContainingType.MetadataName, throwOnError: false)
                ?? throw new Exception(
                    $"Submission type '{entryPoint.ContainingType.MetadataName}' not found.");
            var method = type.GetMethod(entryPoint.Name,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                ?? throw new Exception($"Entry point '{entryPoint.Name}' not found on '{type.FullName}'.");

            // Capture logs around the invoke and any scheduled async/coroutine work. Sync paths
            // stop capture before returning; ScheduleTask/ScheduleCoroutine stop it when the job
            // finishes so logs from awaited continuations / coroutine frames are included.
            object raw;
            StartLogCapture(captureLogs);
            try
            {
                // Roslyn submission entry points take a single object[] whose [0]=globals,
                // [1]=previous submission result (both null for standalone scripts). A
                // 1-element array throws IndexOutOfRangeException at runtime.
                raw = method.Invoke(null, new object[] { new object[2] });
            }
            catch (TargetInvocationException tie)
            {
                // The script threw during its synchronous execution — report it (with logs) as the
                // synchronous paths do; a script's own failure is output, not a bridge error.
                return BuildScriptFailureResponse(tie.InnerException ?? tie,
                    captureLogs ? StopLogCapture() : new List<string>());
            }

            if (raw is Task<object> scriptTask)
            {
                // Completed synchronously (e.g. "1 + 2" or a script that returned before awaiting).
                if (scriptTask.IsCompleted)
                {
                    if (scriptTask.IsFaulted)
                        return BuildScriptFailureResponse(
                            scriptTask.Exception?.InnerException ?? scriptTask.Exception,
                            captureLogs ? StopLogCapture() : new List<string>());

                    var userResult = scriptTask.Result;

                    // The script returned a Task to run across frames → AsyncTaskRunner.
                    if (userResult is Task innerTask)
                        return ScheduleTask(innerTask, captureLogs);

                    // The script returned a coroutine to run across frames → CoroutineRunner.
                    if (userResult is IEnumerator routine)
                        return ScheduleCoroutine(routine, captureLogs);

                    // Plain value — fully synchronous. Return the response with captured logs.
                    return BuildScriptSuccessResponse(userResult,
                        captureLogs ? StopLogCapture() : new List<string>());
                }

                return ScheduleTask(scriptTask, captureLogs);
            }

            return BuildScriptSuccessResponse(raw, captureLogs ? StopLogCapture() : new List<string>());
        }

        static List<MetadataReference> BuildBaseReferences()
        {
            var references = new List<MetadataReference>();
            var addedLocations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1. Core + Unity assemblies (Unity types are added inside AddCoreAssemblyReferences)
            AddCoreAssemblyReferences(references);

            foreach (var r in references.OfType<PortableExecutableReference>())
                if (r.FilePath != null) addedLocations.Add(r.FilePath);

            // 2. Reference all loaded non-dynamic assemblies so scripts can use any
            //    runtime type (package assemblies, third-party DLLs, etc.)
            //    This fixes CS0246/CS0311 when referencing types from packages like Codely.Utilities.
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.IsDynamic) continue;
                var loc = asm.Location;
                if (string.IsNullOrEmpty(loc)) continue;
                if (!addedLocations.Add(loc)) continue;

                // Assembly-CSharp / -Editor are handled via shadow copy below
                if (s_ShadowCopyAssemblyNames.Contains(GetAssemblySimpleName(asm))) continue;

                try { references.Add(MetadataReference.CreateFromFile(loc)); }
                catch { /* skip unreadable assemblies */ }
            }

            // 3. Assembly-CSharp / -Editor via shadow copy (avoids domain reload file locks)
            foreach (var assemblyName in s_ShadowCopyAssemblyNames)
            {
                var asm = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => GetAssemblySimpleName(a) == assemblyName);
                if (asm == null || string.IsNullOrEmpty(asm.Location))
                    continue;

                var shadowPath = CreateShadowCopy(asm.Location);
                if (addedLocations.Add(shadowPath))
                    references.Add(MetadataReference.CreateFromFile(shadowPath));
            }

            return references;
        }

        // Iteratively compiles the script using the same scripting engine as execution,
        // then applies auto-fixes until clean or exhausted.
        // Returns the Compilation matching the final `script` on the clean exit paths (so callers can
        // reuse it), or null when the script was mutated after its last compilation (loop exhaustion).
        static Compilation CompileAndAutoFix(ref string script, List<string> imports, List<MetadataReference> references)
        {
            HoistUsingDirectives(ref script, imports);

            var addedLocations = new HashSet<string>(references
                .OfType<PortableExecutableReference>()
                .Select(r => r.FilePath ?? ""));

            var context = new ScriptFixContext(imports, references, addedLocations);

            for (int iteration = 0; iteration < k_MaxFixIterations; iteration++)
            {
                var scriptOptions = ScriptOptions.Default
                    .WithReferences(references)
                    .WithImports(imports);
                var scriptObj = CSharpScript.Create(script, scriptOptions);
                var compilation = scriptObj.GetCompilation();
                var tree = compilation.SyntaxTrees.First();

                var errors = compilation.GetDiagnostics()
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .ToList();

                if (!errors.Any())
                {
                    CodelyLogger.Log($"[ExecuteCSharpScript] Compilation check passed (iteration {iteration})");
                    return compilation;
                }

                int errorCountBefore = errors.Count;
                bool anyFixed = false;
                var updatedTree = tree;

                foreach (var diagnostic in errors)
                {
                    foreach (var fix in s_FixProviders)
                    {
                        if (!fix.CanFix(diagnostic))
                            continue;

                        var treeBeforeFix = updatedTree;
                        if (fix.ApplyFix(ref updatedTree, diagnostic, context))
                        {
                            anyFixed = true;
                            CodelyLogger.Log($"[ExecuteCSharpScript] {fix.GetType().Name} applied for {diagnostic.Id}");

                            // If the tree was modified, remaining diagnostic spans are stale.
                            // Break out and let the outer loop recompile with fresh diagnostics.
                            if (!ReferenceEquals(updatedTree, treeBeforeFix))
                                goto fixesApplied;
                        }
                    }
                }

                fixesApplied:
                if (!anyFixed)
                {
                    CodelyLogger.LogWarning("[ExecuteCSharpScript] Auto-fix could not resolve remaining errors:\n" +
                        string.Join("\n", errors.Select(e => $"  {e.Id}: {e.GetMessage()}")));
                    // `script` was not modified this iteration, so `compilation` still matches it.
                    return compilation;
                }

                if (!ReferenceEquals(updatedTree, tree))
                {
                    var candidate = updatedTree.GetText().ToString();

                    // Verify the fix reduced errors; if it made things worse, skip this fix
                    var checkOptions = ScriptOptions.Default
                        .WithReferences(references)
                        .WithImports(imports);
                    var checkErrors = CSharpScript.Create(candidate, checkOptions)
                        .GetCompilation().GetDiagnostics()
                        .Count(d => d.Severity == DiagnosticSeverity.Error);

                    if (checkErrors > errorCountBefore)
                    {
                        CodelyLogger.LogWarning($"[ExecuteCSharpScript] Auto-fix increased errors ({errorCountBefore} → {checkErrors}), reverting");
                        continue;
                    }

                    script = candidate;
                }
            }

            // Loop exhausted after applying a fix: `script` was mutated past its last compilation,
            // so no reusable compilation matches it. Signal the caller to compile fresh.
            return null;
        }

        // Parses top-level `using` directives out of the script, merges them into `imports`,
        // and returns the script body with those directives removed.
        static void HoistUsingDirectives(ref string script, List<string> imports)
        {
            var root = SyntaxFactory.ParseSyntaxTree(script).GetCompilationUnitRoot();
            if (root.Usings.Count == 0)
                return;

            foreach (var usingDirective in root.Usings)
            {
                var namespaceName = usingDirective.Name.ToString();
                if (!imports.Contains(namespaceName))
                    imports.Add(namespaceName);
            }

            // Remove the using directives from the script body
            var stripped = root.RemoveNodes(root.Usings, SyntaxRemoveOptions.KeepNoTrivia);
            script = stripped?.GetText().ToString().TrimStart() ?? script;
        }

        static void AddCoreAssemblyReferences(List<MetadataReference> references)
        {
            var coreTypes = new[]
            {
                typeof(object),
                typeof(System.Linq.Enumerable),
                typeof(System.Collections.Generic.List<>),
                typeof(System.Collections.ArrayList),
                typeof(System.Threading.Tasks.Task),
                typeof(System.Text.StringBuilder),
                typeof(System.IO.File),
                typeof(System.Text.RegularExpressions.Regex),
                typeof(System.Math),
                // Unity
                typeof(UnityEngine.Debug),
                typeof(UnityEngine.GameObject),
                typeof(UnityEngine.Transform),
                typeof(UnityEngine.Component),
                typeof(UnityEngine.MonoBehaviour),
                typeof(UnityEngine.Object),
                typeof(UnityEngine.UI.Button),
                typeof(UnityEngine.UI.Image),
                typeof(UnityEngine.UI.Text),
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.BaseEventData),
                // UnityEditor
                typeof(UnityEditor.EditorApplication),
                typeof(UnityEditor.EditorUtility),
                typeof(UnityEditor.AssetDatabase),
                typeof(UnityEditor.Selection),
                typeof(UnityEditor.SceneManagement.EditorSceneManager),
            };

            var addedLocations = new HashSet<string>();
            foreach (var type in coreTypes)
            {
                var location = type.Assembly.Location;
                if (!string.IsNullOrEmpty(location) && addedLocations.Add(location))
                    references.Add(MetadataReference.CreateFromFile(location));
            }

            foreach (var name in new[] { "netstandard", "System.Runtime", "System.Core" })
            {
                var asm = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => GetAssemblySimpleName(a) == name);
                if (asm != null && !string.IsNullOrEmpty(asm.Location) && addedLocations.Add(asm.Location))
                    references.Add(MetadataReference.CreateFromFile(asm.Location));
            }
        }

        static string CreateShadowCopy(string sourcePath)
        {
            var sourceTime = File.GetLastWriteTimeUtc(sourcePath).Ticks;
            var fileName = Path.GetFileNameWithoutExtension(sourcePath);
            var ext = Path.GetExtension(sourcePath);
            var versionedName = $"{fileName}_{sourceTime}{ext}";
            var destPath = Path.Combine(s_ShadowCopyDir, versionedName);

            Directory.CreateDirectory(s_ShadowCopyDir);

            if (File.Exists(destPath))
            {
                CodelyLogger.Log($"[ExecuteCSharpScript] Shadow copy exists: {versionedName}");
            }
            else
            {
                CleanupOldShadowCopies(fileName, sourceTime);
                File.Copy(sourcePath, destPath, overwrite: false);
                CodelyLogger.Log($"[ExecuteCSharpScript] Shadow copy created: {versionedName}");
            }

            var pdbSource = Path.ChangeExtension(sourcePath, ".pdb");
            var pdbDest = Path.ChangeExtension(destPath, ".pdb");
            if (File.Exists(pdbSource) && !File.Exists(pdbDest))
            {
                try { File.Copy(pdbSource, pdbDest, overwrite: false); }
                catch (IOException)
                {
                    CodelyLogger.LogWarning($"[ExecuteCSharpScript] Could not copy PDB for {fileName}");
                }
            }

            return destPath;
        }

        static void CleanupOldShadowCopies(string assemblyName, long currentTimestamp)
        {
            try
            {
                if (!Directory.Exists(s_ShadowCopyDir))
                    return;

                foreach (var file in Directory.GetFiles(s_ShadowCopyDir, $"{assemblyName}_*"))
                {
                    var nameNoExt = Path.GetFileNameWithoutExtension(file);
                    var lastUnderscore = nameNoExt.LastIndexOf('_');
                    if (lastUnderscore <= 0)
                        continue;

                    if (long.TryParse(nameNoExt.Substring(lastUnderscore + 1), out var fileTimestamp)
                        && fileTimestamp < currentTimestamp)
                    {
                        try { File.Delete(file); }
                        catch (IOException)
                        {
                            CodelyLogger.LogWarning($"[ExecuteCSharpScript] Could not delete old shadow copy: {Path.GetFileName(file)}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CodelyLogger.LogWarning($"[ExecuteCSharpScript] Shadow copy cleanup failed: {e.Message}");
            }
        }

        // Reads a script file with encoding auto-detection. File.ReadAllText assumes UTF-8 when
        // there is no BOM, which corrupts files saved in a local ANSI code page (e.g. GBK/936 on
        // zh-CN Windows) — Chinese identifiers/strings then arrive as '�' and Roslyn reports
        // "error CS1056: Unexpected character". We honor any BOM, validate the bytes as UTF-8
        // ourselves (Mono's UTF8Encoding.throwOnInvalidBytes is unreliable and silently substitutes
        // '�' instead of throwing), and only fall back to a local code page when they are not UTF-8.
        static string ReadScriptFileSmart(string path)
        {
            var bytes = File.ReadAllBytes(path);

            // 1) Honor an explicit BOM.
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                return new UTF8Encoding(false).GetString(bytes, 3, bytes.Length - 3);
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
                return Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
                return Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);

            // 2) No BOM: if the bytes are valid UTF-8 (the common case), decode as UTF-8.
            if (IsValidUtf8(bytes))
                return new UTF8Encoding(false).GetString(bytes);

            // 3) Not UTF-8 — if the bytes are structurally valid GBK/936 (the dominant non-UTF-8
            //    case for these scripts: zh-CN Windows), decode as GBK.
            if (IsValidGbk(bytes))
            {
                CodelyLogger.LogWarning(
                    $"[ExecuteCSharpScript] '{Path.GetFileName(path)}' is not valid UTF-8; " +
                    "decoded as GBK/936. Save the file as UTF-8 to avoid encoding issues.");
                return Encoding.GetEncoding(936).GetString(bytes);
            }

            // 4) Neither UTF-8 nor GBK — encoding cannot be detected confidently (e.g. Shift-JIS,
            //    Big5, or a single-byte ANSI page). Decode with the system default as a last resort
            //    and warn LOUDLY so the result is not silently trusted.
            var fallback = Encoding.Default;
            if (fallback.CodePage == 65001) // system is itself UTF-8 — useless for non-UTF-8 bytes
                fallback = Encoding.GetEncoding(936);
            CodelyLogger.LogWarning(
                $"[ExecuteCSharpScript] Could not confidently detect the encoding of " +
                $"'{Path.GetFileName(path)}' (not UTF-8, not GBK). Decoding with {fallback.WebName} " +
                $"(cp {fallback.CodePage}) as a last resort — output may be garbled. " +
                "Save the file as UTF-8 to fix this.");
            return fallback.GetString(bytes);
        }

        // Manual UTF-8 validation — does not rely on UTF8Encoding throwing (Mono does not).
        static bool IsValidUtf8(byte[] bytes)
        {
            int i = 0, n = bytes.Length;
            while (i < n)
            {
                byte b = bytes[i];
                if (b <= 0x7F) { i++; continue; }

                int extra;
                if ((b & 0xE0) == 0xC0) { extra = 1; if (b < 0xC2) return false; }      // 2-byte, reject overlong
                else if ((b & 0xF0) == 0xE0) { extra = 2; }                              // 3-byte
                else if ((b & 0xF8) == 0xF0) { extra = 3; if (b > 0xF4) return false; }  // 4-byte, reject > U+10FFFF
                else return false;                                                       // lone continuation / invalid lead

                if (i + extra >= n) return false;                                        // truncated sequence
                for (int j = 1; j <= extra; j++)
                    if ((bytes[i + j] & 0xC0) != 0x80) return false;                     // bad continuation byte

                i += extra + 1;
            }
            return true;
        }

        // Manual GBK/936 structural validation — does not rely on the decoder throwing (Mono does not).
        // GBK: single bytes 0x00-0x7F (ASCII) and 0x80 (euro in cp936); double bytes have a lead
        // byte 0x81-0xFE followed by a trailing byte 0x40-0x7E or 0x80-0xFE.
        static bool IsValidGbk(byte[] bytes)
        {
            int i = 0, n = bytes.Length;
            while (i < n)
            {
                byte b = bytes[i];
                if (b <= 0x7F || b == 0x80) { i++; continue; }  // ASCII / euro
                if (b == 0xFF) return false;                    // not a valid lead byte

                if (i + 1 >= n) return false;                   // dangling lead byte
                byte t = bytes[i + 1];
                bool validTrail = (t >= 0x40 && t <= 0x7E) || (t >= 0x80 && t <= 0xFE);
                if (!validTrail) return false;

                i += 2;
            }
            return true;
        }

        static void SaveScriptToTemp(string script)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("HHmmss");
                var tempPath = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "ExecutedCSharpScripts");
                Directory.CreateDirectory(tempPath);
                var filePath = Path.Combine(tempPath, $"script_{timestamp}_{script.Length}.cs");
                File.WriteAllText(filePath, script);
                CodelyLogger.Log($"[ExecuteCSharpScript] Script saved: {filePath}");
            }
            catch (Exception e)
            {
                CodelyLogger.LogWarning($"[ExecuteCSharpScript] Failed to save script to temp: {e.Message}");
            }
        }

        static void StartLogCapture(bool enabled)
        {
            if (!enabled)
            {
                s_IsCapturingLogs = false;
                return;
            }
            s_CapturedLogs.Clear();
            s_IsCapturingLogs = true;
            // Remove first so a nested/repeated start cannot subscribe twice (duplicate log lines).
            Application.logMessageReceived -= OnLogMessageReceived;
            Application.logMessageReceived += OnLogMessageReceived;
        }

        static List<string> StopLogCapture()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
            s_IsCapturingLogs = false;
            var logs = new List<string>(s_CapturedLogs);
            s_CapturedLogs.Clear();
            return logs;
        }

        static void OnLogMessageReceived(string logString, string stackTrace, LogType type)
        {
            if (!s_IsCapturingLogs)
                return;

            // Suppress this tool's own internal trace logs from the captured output —
            // the caller wants their script's logs, not our scaffolding.
            if (!string.IsNullOrEmpty(logString) && logString.StartsWith("[ExecuteCSharpScript]"))
                return;

            var entry = new StringBuilder();
            entry.Append($"[{type}] {logString}");
            if ((type == LogType.Error || type == LogType.Exception) && !string.IsNullOrEmpty(stackTrace))
                entry.Append($"\n{stackTrace}");

            s_CapturedLogs.Add(entry.ToString());
        }

        // Assembly.GetName() calls get_code_base() which throws EILSEQ on Mono when the project
        // path contains non-ASCII characters (ACP=936). Fall back to parsing FullName — it holds
        // the simple name before the first comma and never touches the code base.
        static string GetAssemblySimpleName(Assembly asm)
        {
            try
            {
                return asm.GetName().Name;
            }
            catch
            {
                var fullName = asm.FullName ?? "";
                var comma = fullName.IndexOf(',');
                return comma >= 0 ? fullName.Substring(0, comma) : fullName;
            }
        }

        // Builds the response returned for a synchronously executed script (a plain value).
        static object BuildScriptSuccessResponse(object result, List<string> logs) =>
            Response.Success(
                "C# script executed successfully.",
                new { result = result?.ToString(), logs, log_count = logs.Count });

        // Builds the response returned when a script throws (synchronously, or from an awaited Task /
        // coroutine driven by a runner). A plain Response.Error with the whole failure in `message`
        // and NO `data` payload: the client renders a message-only error as a FAILED tool call (❌),
        // but a response carrying `data` as a success (it shows the data and ignores success:false).
        // So a data-bearing failure would look like a pass — keep failures data-free. Any captured
        // logs are appended to the message (the dropped data was their only other channel).
        static object BuildScriptFailureResponse(Exception e, List<string> logs)
        {
            var message = $"C# script execution failed: {e?.Message}\n{SafeGetStackTrace(e)}";
            if (logs != null && logs.Count > 0)
                message += "\n\nLogs:\n" + string.Join("\n", logs);
            return Response.Error(message);
        }

        // timeoutSeconds: 0 keeps long-running async scripts (e.g. awaiting a bake)
        // from being cut short.
        //
        // Drives a Task the script returned or is still running on the AsyncTaskRunner.
        // Wrapper returns a Response; AsyncTaskRunner delivers it (SetError for a success:false
        // failure Response, SetResult otherwise).
        static JobContext ScheduleTask(Task task, bool captureLogs)
        {
            var ctx = AsyncTaskRunner.CreateJob(CommandContext.RequestId, CommandContext.CommandType);

            async Task<object> Wrapper()
            {
                try
                {
                    await task;
                    // Surface the script's actual return value (the sync and completed-task paths
                    // already do). For a Task<T> that is the awaited T; a non-generic Task has none.
                    var userResult = GetTaskResult(task);
                    var logs = captureLogs ? StopLogCapture() : new List<string>();
                    return BuildScriptSuccessResponse(userResult, logs);
                }
                catch (Exception e)
                {
                    var logs = captureLogs ? StopLogCapture() : new List<string>();
                    return BuildScriptFailureResponse(e, logs);
                }
            }

            AsyncTaskRunner.RunJob(ctx, Wrapper(), timeoutSeconds: 0);
            return ctx;
        }

        // Extracts Task<T>.Result via reflection (T is unknown at compile time). A non-generic Task
        // has no result → null. Only called after the Task has completed successfully.
        static object GetTaskResult(Task task)
        {
            var type = task.GetType();
            if (!type.IsGenericType)
                return null;

            var result = type.GetProperty("Result")?.GetValue(task);

            // A non-generic `async Task` is really a Task<VoidTaskResult> at runtime, so Result is
            // the internal VoidTaskResult sentinel — not a user value. Report it as an empty result
            // (compare by name; the type is internal and can't be referenced directly).
            if (result != null && result.GetType().FullName == "System.Threading.Tasks.VoidTaskResult")
                return "";

            return result;
        }

        // Drives a coroutine the script returned on the CoroutineRunner (one MoveNext per frame).
        // CoroutineRunner nests yielded IEnumerators, so `yield return routine` runs it to completion.
        static JobContext ScheduleCoroutine(IEnumerator routine, bool captureLogs)
        {
            var ctx = CoroutineRunner.CreateJob(CommandContext.RequestId, CommandContext.CommandType);

            IEnumerator Wrapper()
            {
                try
                {
                    yield return routine;
                    var logs = captureLogs ? StopLogCapture() : new List<string>();
                    ctx.SetResult(BuildScriptSuccessResponse(null, logs));
                }
                finally
                {
                    // If the nested routine throws, JobRunnerBase SetError's and this frame never
                    // reaches the success path — still drop the log subscription.
                    if (captureLogs && s_IsCapturingLogs)
                        StopLogCapture();
                }
            }

            CoroutineRunner.RunJob(ctx, Wrapper(), timeoutSeconds: 0);
            return ctx;
        }

    }
}
