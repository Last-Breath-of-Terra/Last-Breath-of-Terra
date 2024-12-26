using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public PlayerData playerData; // 데이터를 저장할 객체

    void Start()
    {
        LoadPlayerData(); // 게임 시작 시 데이터 로드
    }

    void LoadPlayerData()
    {
        // JSON 파일 경로 설정
        string path = Path.Combine(Application.dataPath, "Resources/Json/Player.json");

        if (File.Exists(path)) // 파일이 존재하는지 확인
        {
            string jsonData = File.ReadAllText(path); // JSON 파일 읽기
            playerData = JsonUtility.FromJson<PlayerData>(jsonData); // 역직렬화
            Debug.Log("player 이름: " + playerData.name);
        }
        else
        {
            Debug.LogError("playerData.json 파일을 찾을 수 없습니다.");
        }
    }

    public void SavePlayerData()
    {
        // JSON 데이터를 직렬화
        string jsonData = JsonUtility.ToJson(playerData, true);
        string path = Path.Combine(Application.dataPath, "playerData.json");

        File.WriteAllText(path, jsonData); // JSON 파일 저장
        Debug.Log("HP 데이터 저장 완료");
    }
}