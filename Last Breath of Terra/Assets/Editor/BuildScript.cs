using UnityEditor;
using System.IO;

public class BuildScript
{
    // GitHub Actions가 이 메서드를 호출합니다.
    public static void BuildWebGL()
    {
        // 빌드될 WebGL 파일이 저장될 경로를 지정합니다.
        string buildPath = "Build/WebGL";

        // 기존 빌드 폴더를 삭제해서 깨끗하게 시작합니다.
        if (Directory.Exists(buildPath))
        {
            Directory.Delete(buildPath, true);
        }

        // 빌드할 씬을 추가합니다. 
        // 씬 파일 경로를 정확하게 입력해주세요.
        string[] scenes = new[] { "Assets/Scenes/SampleScene.unity" };

        // 빌드 시작!
        BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.WebGL, BuildOptions.None);
    }
}