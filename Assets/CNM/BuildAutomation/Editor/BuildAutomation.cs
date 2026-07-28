using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using UnityEditor.Rendering.LookDev;

namespace CNM
{
    public static class BuildAutomation
    {
        [MenuItem("CNM/Build/Build Windows Dev")]
        public static void BuildWindowsDev()
        {
            BuildOptions options = GetDevBuildOptions();
            Build(BuildTarget.StandaloneWindows64, options, ".exe");
        }
        [MenuItem("CNM/Build/Build WebGL Dev")]
        public static void BuildWebGLDev()
        {
            BuildOptions options = GetDevBuildOptions();
            Build(BuildTarget.WebGL, options);
        }
        [MenuItem("CNM/Build/Build Windows Release")]
        public static void BuildWindowsRelease()
        {
            BuildOptions options = GetReleaseBuildOptions();
            Build(BuildTarget.StandaloneWindows64, options, ".exe");
        }
        [MenuItem("CNM/Build/Build WebGL Release")]
        public static void BuildWebGLRelease()
        {
            BuildOptions options = GetReleaseBuildOptions();
            Build(BuildTarget.WebGL, options);
        }

        private static BuildOptions GetDevBuildOptions()
        {
            var options = new BuildOptions();
            options |= BuildOptions.AllowDebugging;
            options |= BuildOptions.Development;
            options |= BuildOptions.EnableDeepProfilingSupport;
            return options;
        }
        private static BuildOptions GetReleaseBuildOptions()
        {
            var options = new BuildOptions();
            return options;
        }

        public static void Build(BuildTarget target, BuildOptions addedOptions, string extension = null)
        {
            BuildPlayerOptions options = new BuildPlayerOptions();
            List<string> activeScenes = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    activeScenes.Add(scene.path);
                }
            }
            options.scenes = activeScenes.ToArray();
            options.target = target;

            string buildpath = "Build/";
            FileUtil.DeleteFileOrDirectory(buildpath);
            options.locationPathName = buildpath + target.ToString();
            if (extension != null)
            {
                options.locationPathName += extension;
            }
            options.options |= addedOptions;

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.totalErrors > 0)
            {
                throw new System.Exception("Build process ended with errors: " + System.Environment.NewLine + SummarizeErrors(report));
            }
        }

        private static string SummarizeErrors(BuildReport report)
        {
            StringBuilder  errors = new StringBuilder();
            foreach (BuildStep step in report.steps)
            {
                foreach (BuildStepMessage message in step.messages)
                {
                    if (message.type == LogType.Error || message.type == LogType.Exception ||
                        message.type == LogType.Assert)
                    {
                        errors.AppendLine(message.content);
                    }
                }
            }
            return errors.ToString();
        }
        
        private static string GetProjectDirName()
        {
            string dataPath = Application.dataPath;
            string[] splitPath = dataPath.Split("/"[0]);
            return splitPath[splitPath.Length - 2];
        }
    }

}
