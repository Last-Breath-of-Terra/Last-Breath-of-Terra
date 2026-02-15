using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    // ===== Config =====
    private const int SlotCount = 3;     // 저장 슬롯 3개 고정
    private const int StageCount = 3;    // 스테이지 3개 고정(필요하면 늘려도 됨)
    private const string FileName = "GameData.json";

    // ===== Data Model =====
    [Serializable]
    public class StageData
    {
        public int stageId;
        public bool isCleared;
    }

    [Serializable]
    public class PlayerData
    {
        // "슬롯에 저장이 존재하는가?"를 null 대신 플래그로 표현 (구조 안정)
        public bool exists;

        public List<StageData> stages;
    }

    [Serializable]
    public class GameData
    {
        public List<PlayerData> players;
    }

    // ===== Public State =====
    public int playerIndex { get; set; }

    // ===== Private =====
    private GameData gameData;
    private string path;

    // -------- Lifecycle --------
    private void Awake()
    {
        // Singleton<T> 구현에 따라 Awake가 이미 있을 수 있음.
        // (보통 Singleton<T>.Awake()가 base.Awake()로 호출되게 되어있음)
        // 만약 네 Singleton이 Awake를 override 한다면, 거기에 이 초기화 코드를 옮겨도 됨.

        path = Path.Combine(Application.persistentDataPath, FileName);
        LoadOrCreate();
        NormalizeAndFixup();
        SavePlayerData(); // 파일 없거나 구조 보정했을 때 반영 (안전)
    }

    // -------- Core Init / Load / Save --------
    private void LoadOrCreate()
    {
        if (!File.Exists(path))
        {
            gameData = CreateDefaultGameData();
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            gameData = JsonUtility.FromJson<GameData>(json);

            // 파일이 깨졌거나 내용이 비정상일 수 있으니 아래에서 보정
            if (gameData == null)
                gameData = CreateDefaultGameData();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DataManager] Load failed, recreating default. Reason: {e.Message}");
            gameData = CreateDefaultGameData();
        }
    }

    private void SavePlayerData()
    {
        try
        {
            string json = JsonUtility.ToJson(gameData, true);
            File.WriteAllText(path, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataManager] Save failed: {e.Message}");
        }
    }

    // -------- Recommended Stable Structure --------
    // ✅ players는 항상 3개 "객체"가 존재한다.
    // ✅ 슬롯이 비어있음 = PlayerData.exists == false
    private GameData CreateDefaultGameData()
    {
        var gd = new GameData { players = new List<PlayerData>(SlotCount) };
        for (int i = 0; i < SlotCount; i++)
            gd.players.Add(CreateEmptyPlayer());
        return gd;
    }

    private PlayerData CreateEmptyPlayer()
    {
        return new PlayerData
        {
            exists = false,
            stages = CreateDefaultStages()
        };
    }

    private List<StageData> CreateDefaultStages()
    {
        var stages = new List<StageData>(StageCount);
        for (int i = 0; i < StageCount; i++)
            stages.Add(new StageData { stageId = i, isCleared = false });
        return stages;
    }

    // 로드된 데이터가 오래됐거나 깨졌어도 "항상 안전한 구조"로 보정
    private void NormalizeAndFixup()
    {
        if (gameData.players == null)
            gameData.players = new List<PlayerData>();

        // 슬롯 수 맞추기
        while (gameData.players.Count < SlotCount)
            gameData.players.Add(CreateEmptyPlayer());
        if (gameData.players.Count > SlotCount)
            gameData.players.RemoveRange(SlotCount, gameData.players.Count - SlotCount);

        // 각 슬롯 보정
        for (int i = 0; i < SlotCount; i++)
        {
            var p = gameData.players[i];
            if (p == null)
            {
                // 예전 버전(null 슬롯)에서 넘어왔을 가능성 보정
                gameData.players[i] = CreateEmptyPlayer();
                continue;
            }

            if (p.stages == null)
                p.stages = CreateDefaultStages();

            // stages 개수/ID 보정 (StageCount 변경에도 대응)
            // - 부족하면 추가
            while (p.stages.Count < StageCount)
                p.stages.Add(new StageData { stageId = p.stages.Count, isCleared = false });
            // - 많으면 자름
            if (p.stages.Count > StageCount)
                p.stages.RemoveRange(StageCount, p.stages.Count - StageCount);

            // - stageId 정렬
            for (int s = 0; s < p.stages.Count; s++)
                p.stages[s].stageId = s;
        }
    }

    // -------- Public API --------
    public bool HasSave(int index)
    {
        var p = GetPlayerData(index);
        return p != null && p.exists;
    }

    public PlayerData GetPlayerData(int index)
    {
        if (gameData == null || gameData.players == null) return null;
        if (index < 0 || index >= SlotCount) return null;
        return gameData.players[index];
    }

    // 슬롯에 저장 생성(초기화)
    public void AddPlayerAtIndex(int index)
    {
        if (index < 0 || index >= SlotCount) return;

        var p = GetPlayerData(index);
        if (p == null)
        {
            // 이 경우는 거의 없지만, 혹시 모를 방어
            NormalizeAndFixup();
            p = GetPlayerData(index);
            if (p == null) return;
        }

        p.exists = true;
        // 새로 만들 때는 초기 스테이지로 리셋
        p.stages = CreateDefaultStages();

        SavePlayerData();
    }

    // 슬롯 삭제(비우기)
    public void RemovePlayerAtIndex(int index)
    {
        if (index < 0 || index >= SlotCount) return;

        var p = GetPlayerData(index);
        if (p == null) return;

        p.exists = false;
        // 데이터도 초기화(원하면 유지해도 됨)
        for (int i = 0; i < p.stages.Count; i++)
            p.stages[i].isCleared = false;

        SavePlayerData();
    }

    // 스테이지 클리어 상태 수정
    public void ModifyPlayerData(int slotIndex, int stageIndex, bool isCleared)
    {
        var p = GetPlayerData(slotIndex);
        if (p == null) return;
        if (!p.exists) return; // 저장 없는 슬롯이면 수정 불가(정책)
        if (p.stages == null) return;
        if (stageIndex < 0 || stageIndex >= p.stages.Count) return;

        p.stages[stageIndex].isCleared = isCleared;
        SavePlayerData();
    }

    // 저장 판별용: 한 번이라도 클리어 했는지
    public bool HasAnyStageCleared(int index)
    {
        var p = GetPlayerData(index);
        if (p == null || !p.exists || p.stages == null) return false;

        for (int i = 0; i < p.stages.Count; i++)
            if (p.stages[i].isCleared) return true;

        return false;
    }

    // (옵션) 외부에서 강제로 저장하고 싶을 때
    public void ForceSave()
    {
        if (gameData == null) return;
        NormalizeAndFixup();
        SavePlayerData();
    }
}
