using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GimmickManager : Singleton<GimmickManager>
{
    public LifeInfuserSO lifeInfuserSO;
    private Coroutine _coroutine;

    /*
    private void Start()
    {
        if (InfuserManager.Instance == null)
        {
            Debug.LogError("[GimmickManager] InfuserManager.Instance is NULL (초기화 전?)", this);
            return;
        }

        lifeInfuserSO = InfuserManager.Instance.LifeInfuserSO;
        if (lifeInfuserSO == null)
            Debug.LogError("[GimmickManager] lifeInfuserSO is NULL! (매니저에 할당 안 됨)", this);
    }*/

    public void ChangeLifeInfuserUISize()
    {
        if (lifeInfuserSO != null)
        {
            lifeInfuserSO.SetUIForInfuserStatus(true);
            if (_coroutine != null) StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(HideAfterDelay());
        }
    }
    
    private IEnumerator HideAfterDelay()
    {
        Debug.Log("Hide after delay");
        yield return new WaitForSeconds(UIManager.Instance.hideDelay + lifeInfuserSO.uiTweenDuration);
        if (lifeInfuserSO != null)
        {
            Debug.Log("Player entered");
            lifeInfuserSO.SetUIForInfuserStatus(false);
        }
        _coroutine = null;
    }
}