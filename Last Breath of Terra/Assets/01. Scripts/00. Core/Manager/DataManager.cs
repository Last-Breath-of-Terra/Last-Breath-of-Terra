using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataManager : Singleton<DataManager>
{
    // 플레이어 데이터 클래스
    [System.Serializable]
    public class StageData
    {
        public int stageId;       // 스테이지 ID
        public bool isCleared;    // 스테이지 클리어 여부
    }

    [System.Serializable]
    public class PlayerData
    {
        public List<StageData> stages;  // 스테이지 데이터
    }

    public int playerIndex { get; set; }

    [System.Serializable]
    public class GameData
    {
        public List<PlayerData> players; // 플레이어 목록
    }

    private GameData gameData;
    private string path;

    private void Start()
    {
        // JSON 파일 경로 설정
        path = Path.Combine(Application.persistentDataPath, "GameData.json");

        if (File.Exists(path))
        {
            LoadPlayerData(path);
        }
        else
        {
            gameData = new GameData { players = new List<PlayerData>() };
            SavePlayerData();
        }

        while (gameData.players.Count < 3)
        {
            gameData.players.Add(null);
        }
    }

    // 기본 플레이어 데이터를 만들어서 JSON 파일로 저장
    private void CreateGameDataJsonFile(string path, string playerName)
    {
        // 기본 데이터 생성
        gameData = new GameData
        {
            players = new List<PlayerData>
            {
                new PlayerData
                {
                    stages = new List<StageData>
                    {
                        new StageData { stageId = 1, isCleared = false },
                        new StageData { stageId = 2, isCleared = false }
                    }
                }
            }
        };

        // JSON으로 직렬화
        string jsonData = JsonUtility.ToJson(gameData, true);

        // 파일에 저장
        File.WriteAllText(path, jsonData);
        Debug.Log("기본 플레이어 데이터가 생성되었습니다.");
    }

    // 플레이어 데이터 로드
    private void LoadPlayerData(string path)
    {
        string jsonData = File.ReadAllText(path); // JSON 파일 읽기
        gameData = JsonUtility.FromJson<GameData>(jsonData); // 역직렬화
        Debug.Log("플레이어 데이터 로드 완료");
        Debug.Log("저장 경로: " + Application.persistentDataPath);
    }

    // 플레이어 데이터 저장
    private void SavePlayerData()
    {
        // JSON 데이터를 직렬화
        string jsonData = JsonUtility.ToJson(gameData, true);

        // 파일에 저장
        File.WriteAllText(path, jsonData);
        Debug.Log("수정된 플레이어 데이터 저장 완료");
    }

    // 플레이어 추가
    public void AddPlayerAtIndex(int index)
    {
        if (index < 0 || index >= 3) return;

        PlayerData newPlayer = new PlayerData
        {
            stages = new List<StageData>
            {
                new StageData { stageId = 1, isCleared = false },
                new StageData { stageId = 2, isCleared = false }
            }
        };

        gameData.players[index] = newPlayer;
        SavePlayerData();
    }

    public bool HasSave(int index)
    {
        return GetPlayerData(index) != null;
    }

    public PlayerData GetPlayerData(int index)
    {
        if (index < 0 || index >= gameData.players.Count) return null;
        return gameData.players[index];
    }

    // 플레이어 데이터 수정 (스테이지 클리어 상태 수정)
    public void ModifyPlayerData(int playerIndex, int stageIndex, bool isCleared)
    {
        if (gameData != null && playerIndex >= 0 && playerIndex < gameData.players.Count)
        {
            // 스테이지 클리어 상태 수정
            gameData.players[playerIndex].stages[stageIndex].isCleared = isCleared;

            // 수정된 데이터 저장
            SavePlayerData();
        }
        else
        {
            Debug.LogError("Invalid player or stage index.");
        }
    }

    // 저장 판별용
    public bool HasAnyStageCleared(int index)
    {
        var player = GetPlayerData(index);
        if (player == null)
            return false;

        foreach (var stage in player.stages)
        {
            if (stage.isCleared)
                return true;
        }

        return false;
    }
}

