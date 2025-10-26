#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildScript
{
    // GitHub Actions에서 호출할 엔트리포인트
    public static void BuildWebGL()
    {
        // 빌드 출력 경로
        const string buildPath = "Build/WebGL";

        // 활성화된(체크된) 씬 자동 수집
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            throw new System.Exception(
                "No scenes are enabled in Build Settings. " +
                "Open File > Build Settings... and check at least one scene.");
        }

        // 기존 빌드 폴더 정리
        if (Directory.Exists(buildPath))
        {
            Directory.Delete(buildPath, true);
        }

        // WebGL 빌드 실행
        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new System.Exception($"WebGL build failed: {report.summary.result}");
        }

    }
}
#endif
