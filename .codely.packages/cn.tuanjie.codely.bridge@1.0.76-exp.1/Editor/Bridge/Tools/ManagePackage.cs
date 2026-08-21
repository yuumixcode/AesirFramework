using System;
using Codely.Newtonsoft.Json.Linq;
using UnityTcp.Editor.Helpers;
using UnityTcp.Editor.Tools.Jobs;

namespace UnityTcp.Editor.Tools
{
    /// <summary>
    /// [EXPERIMENTAL] Handles Unity Package Manager (UPM) operations.
    /// Supports installing, removing, and querying packages.
    /// Compatible with Unity 2022.3 LTS.
    ///
    /// Every action runs as a <see cref="StepJob"/> and answers only when the operation has
    /// actually finished — see <see cref="UpmJob"/> for why installing and removing cannot be
    /// tracked any other way (UPM reloads the domain, which used to destroy the in-flight request
    /// and its update callback and strand the job as Pending).
    /// </summary>
    public static class ManagePackage
    {
        // UPM resolution reloads the domain, so the whole operation runs as a StepJob and answers
        // once it has actually finished. 300s covers a registry fetch plus the reload it triggers.
        private const int DefaultTimeoutSeconds = 300;

        private static object Run(StepJob job, JObject @params)
            => StepJobRunner.Start(
                CommandContext.RequestId, CommandContext.CommandType, job,
                @params["timeoutSeconds"]?.ToObject<int?>() ?? DefaultTimeoutSeconds);

        public static object HandleCommand(JObject @params)
        {
            string action = @params["action"]?.ToString().ToLower();
            if (string.IsNullOrEmpty(action))
            {
                return Response.Error("Action parameter is required.");
            }

            try
            {
                switch (action)
                {
                    case "install_package":
                    {
                        string idOrUrl = @params["id_or_url"]?.ToString();
                        string version = @params["version"]?.ToString();

                        // "com.unity.package" + "1.2.3" -> "com.unity.package@1.2.3"
                        if (!string.IsNullOrEmpty(version) &&
                            !string.IsNullOrEmpty(idOrUrl) &&
                            !idOrUrl.Contains("@") &&
                            !idOrUrl.StartsWith("http"))
                        {
                            idOrUrl = $"{idOrUrl}@{version}";
                        }

                        return Run(new UpmJob
                        {
                            Remove = false,
                            Target = idOrUrl,
                            PackageName = UpmJob.RegistryNameOf(idOrUrl),
                        }, @params);
                    }
                    case "remove_package":
                    {
                        string packageName = @params["package_name"]?.ToString();
                        return Run(new UpmJob
                        {
                            Remove = true,
                            Target = packageName,
                            PackageName = packageName,
                        }, @params);
                    }
                    case "wait_for_upm":
                        return Run(
                            new WaitForEditorSettledJob { What = "package operation" }, @params);
                    case "list_packages":
                        return Run(new ListPackagesJob(), @params);
                    default:
                        return Response.Error(
                            $"Unknown action: '{action}'. Valid actions: install_package, remove_package, wait_for_upm, list_packages."
                        );
                }
            }
            catch (Exception e)
            {
                CodelyLogger.LogError($"[ManagePackage] Action '{action}' failed: {e}");
                return Response.Error($"[EXPERIMENTAL] Package operation failed: {e.Message}");
            }
        }
    }
}
