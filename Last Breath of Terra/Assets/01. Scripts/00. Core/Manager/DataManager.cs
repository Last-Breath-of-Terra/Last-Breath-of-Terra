using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataManager : MonoBehaviour
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
        public string name;             // 플레이어 이름
        public List<StageData> stages;  // 스테이지 데이터
    }

    [System.Serializable]
    public class GameData
    {
        public List<PlayerData> players; // 플레이어 목록
    }

    public static DataManager Instance;
    
    public GameObject scrollView;
    public GameObject buttonPrefab;
    public TMP_InputField inputField;  
    public int playerIndex;

    
    private GameData gameData;
    private string path;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    private void Start()
    {
        // JSON 파일 경로 설정
        path = Path.Combine(Application.persistentDataPath, "GameData.json");

        if (File.Exists(path))
        {
            LoadPlayerData(path);
            foreach (var player in gameData.players)
            {
                GameObject newBtn = Instantiate(buttonPrefab, scrollView.transform);
                newBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = player.name;
            }
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
                    name = playerName,
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
    public void AddPlayer()
    {
        List<StageData> stages = new List<StageData>();
        string playerName = inputField.text;
        GameObject newBtn = Instantiate(buttonPrefab, scrollView.transform);
        // 파일이 존재하지 않으면 기본 데이터로 파일을 생성
        if (!File.Exists(path))
        {
            CreateGameDataJsonFile(path, playerName);
        }
        else
        {
            // 파일이 존재하면 데이터 로드
            LoadPlayerData(path);
            if (gameData != null)
            {
                PlayerData newPlayer = new PlayerData
                {
                    name = playerName,
                    stages = new List<StageData>
                    {
                        new StageData { stageId = 1, isCleared = false },
                        new StageData { stageId = 2, isCleared = false }
                    }
                };

                // 플레이어 목록에 추가
                gameData.players.Add(newPlayer);

                // 추가된 데이터 저장
                SavePlayerData();
                Debug.Log($"{playerName} 캐릭터가 추가되었습니다.");
            }
            else
            {
                Debug.Log(path);
                Debug.LogError("게임 데이터가 로드되지 않았습니다.");
            }
        }
        newBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = playerName;
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

    public int FindPlayerIndexByName(string targetName)
    {
        for (int i = 0; i < gameData.players.Count; i++)
        {
            if (gameData.players[i].name == targetName)
            {
                return i;  // 이름이 일치하면 해당 인덱스를 반환
            }
        }
        return -1;  // 플레이어가 없으면 -1 반환
    }
    
}

