using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageSelectInfo
{
    public string displayName;    // 화면에 보여줄 이름
    public string sceneName;      // 실제 이동할 씬 이름
    public Sprite background;     // 배경 이미지
    public Sprite etherisSprite;  // 에테리스 이미지
}