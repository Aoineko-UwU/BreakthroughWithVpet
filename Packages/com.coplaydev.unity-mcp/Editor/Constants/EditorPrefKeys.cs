using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace MCPForUnity.Editor.Constants
{
    /// <summary>
    /// Centralized list of EditorPrefs keys used by the MCP for Unity package.
    /// Keeping them in one place avoids typos and simplifies migrations.
    /// </summary>
    internal static class EditorPrefKeys
    {
        internal const string UseHttpTransport = "MCPForUnity.UseHttpTransport";
        internal const string HttpTransportScope = "MCPForUnity.HttpTransportScope"; // "local" | "remote"
        internal static string LastLocalHttpServerPid => ProjectScoped("MCPForUnity.LocalHttpServer.LastPid");
        internal static string LastLocalHttpServerPort => ProjectScoped("MCPForUnity.LocalHttpServer.LastPort");
        internal static string LastLocalHttpServerStartedUtc => ProjectScoped("MCPForUnity.LocalHttpServer.LastStartedUtc");
        internal static string LastLocalHttpServerPidArgsHash => ProjectScoped("MCPForUnity.LocalHttpServer.LastPidArgsHash");
        internal static string LastLocalHttpServerPidFilePath => ProjectScoped("MCPForUnity.LocalHttpServer.LastPidFilePath");
        internal static string LastLocalHttpServerInstanceToken => ProjectScoped("MCPForUnity.LocalHttpServer.LastInstanceToken");
        internal const string DebugLogs = "MCPForUnity.DebugLogs";
        internal const string ValidationLevel = "MCPForUnity.ValidationLevel";
        internal const string UnitySocketPort = "MCPForUnity.UnitySocketPort";
        internal const string ResumeStdioAfterReload = "MCPForUnity.ResumeStdioAfterReload";

        internal const string UvxPathOverride = "MCPForUnity.UvxPath";
        internal const string ClaudeCliPathOverride = "MCPForUnity.ClaudeCliPath";
        internal const string ClientProjectDirOverride = "MCPForUnity.ClientProjectDir";

        internal static string HttpBaseUrl => ProjectScoped("MCPForUnity.HttpUrl");
        internal const string HttpRemoteBaseUrl = "MCPForUnity.HttpRemoteUrl";
        internal const string SessionId = "MCPForUnity.SessionId";
        internal const string WebSocketUrlOverride = "MCPForUnity.WebSocketUrl";
        internal const string GitUrlOverride = "MCPForUnity.GitUrlOverride";
        internal const string DevModeForceServerRefresh = "MCPForUnity.DevModeForceServerRefresh";
        internal const string ProjectScopedToolsLocalHttp = "MCPForUnity.ProjectScopedTools.LocalHttp";
        internal const string AllowLanHttpBind = "MCPForUnity.Security.AllowLanHttpBind";
        internal const string AllowInsecureRemoteHttp = "MCPForUnity.Security.AllowInsecureRemoteHttp";

        internal const string PackageDeploySourcePath = "MCPForUnity.PackageDeploy.SourcePath";
        internal const string PackageDeployLastBackupPath = "MCPForUnity.PackageDeploy.LastBackupPath";
        internal const string PackageDeployLastTargetPath = "MCPForUnity.PackageDeploy.LastTargetPath";
        internal const string PackageDeployLastSourcePath = "MCPForUnity.PackageDeploy.LastSourcePath";

        internal const string ServerSrc = "MCPForUnity.ServerSrc";
        internal const string UseEmbeddedServer = "MCPForUnity.UseEmbeddedServer";
        internal const string LockCursorConfig = "MCPForUnity.LockCursorConfig";
        internal const string AutoRegisterEnabled = "MCPForUnity.AutoRegisterEnabled";
        internal const string ToolEnabledPrefix = "MCPForUnity.ToolEnabled.";
        internal const string ToolFoldoutStatePrefix = "MCPForUnity.ToolFoldout.";
        internal const string ResourceEnabledPrefix = "MCPForUnity.ResourceEnabled.";
        internal const string ResourceFoldoutStatePrefix = "MCPForUnity.ResourceFoldout.";
        internal const string EditorWindowActivePanel = "MCPForUnity.EditorWindow.ActivePanel";
        internal const string LastSelectedClientId = "MCPForUnity.LastSelectedClientId";
        internal const string ClientDetailsFoldoutOpen = "MCPForUnity.ClientConfig.DetailsFoldoutOpen";

        internal const string SetupCompleted = "MCPForUnity.SetupCompleted";
        internal const string SetupDismissed = "MCPForUnity.SetupDismissed";

        internal const string CustomToolRegistrationEnabled = "MCPForUnity.CustomToolRegistrationEnabled";

        internal const string LastUpdateCheck = "MCPForUnity.LastUpdateCheck";
        internal const string LatestKnownVersion = "MCPForUnity.LatestKnownVersion";
        internal const string LastAssetStoreUpdateCheck = "MCPForUnity.LastAssetStoreUpdateCheck";
        internal const string LatestKnownAssetStoreVersion = "MCPForUnity.LatestKnownAssetStoreVersion";
        internal const string LastStdIoUpgradeVersion = "MCPForUnity.LastStdIoUpgradeVersion";

        internal const string TelemetryDisabled = "MCPForUnity.TelemetryDisabled";
        internal const string CustomerUuid = "MCPForUnity.CustomerUUID";

        internal const string ApiKey = "MCPForUnity.ApiKey";

        internal const string AutoStartOnLoad = "MCPForUnity.AutoStartOnLoad";
        internal const string HttpServerLaunchConfirmed = "MCPForUnity.HttpServerLaunchConfirmed";
        internal const string BatchExecuteMaxCommands = "MCPForUnity.BatchExecute.MaxCommands";
        internal const string LogRecordEnabled = "MCPForUnity.LogRecordEnabled";

        internal const string ExecuteCodeCompiler = "MCPForUnity.ExecuteCode.Compiler";

        // AI Asset Generation — NON-SECRET config only. Provider API keys live in the OS
        // secure store (MCPForUnity.Editor.Security.SecureKeyStore), never in EditorPrefs.
        internal const string AssetGenSelectedModelProvider = "MCPForUnity.AssetGen.ModelProvider";
        internal const string AssetGenSelectedImageProvider = "MCPForUnity.AssetGen.ImageProvider";
        internal const string AssetGenSelectedAudioProvider = "MCPForUnity.AssetGen.AudioProvider";
        // Selected model id per (kind, provider): key = prefix + "<kind>.<provider>". Empty => use
        // the catalog default. Per-provider (not per-type) so e.g. the Tripo and Meshy 3D dropdowns
        // — which have disjoint model lists — never clobber each other's selection.
        internal const string AssetGenSelectedModelPrefix = "MCPForUnity.AssetGen.Model.";
        internal const string AssetGenDefaultFormat = "MCPForUnity.AssetGen.Format";
        internal const string AssetGenOutputRoot = "MCPForUnity.AssetGen.OutputRoot";
        internal const string AssetGenAutoNormalize = "MCPForUnity.AssetGen.AutoNormalize";
        internal const string AssetGenProviderEnabledPrefix = "MCPForUnity.AssetGen.Enabled.";

        private static string _projectScopeSuffix;

        private static string ProjectScoped(string key)
        {
            if (string.IsNullOrEmpty(_projectScopeSuffix))
            {
                string projectPath = Application.dataPath ?? string.Empty;
                try { projectPath = Path.GetFullPath(projectPath); } catch { }
                projectPath = projectPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                if (Path.DirectorySeparatorChar == '\\')
                {
                    projectPath = projectPath.ToUpperInvariant();
                }

                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(projectPath));
                    _projectScopeSuffix = BitConverter.ToString(hash, 0, 8).Replace("-", string.Empty).ToLowerInvariant();
                }
            }

            return key + "." + _projectScopeSuffix;
        }
    }
}
