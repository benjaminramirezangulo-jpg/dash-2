using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class VelocityBuild
{
    [MenuItem("Velocity/Build Windows Player")]
    public static void Windows()
    {
        Directory.CreateDirectory("Builds/Windows");
        var report=BuildPipeline.BuildPlayer(new[]{"Assets/Scenes/Game.unity"},"Builds/Windows/DashAndFight.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);
        if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Windows build failed: "+report.summary.result);
    }
}
