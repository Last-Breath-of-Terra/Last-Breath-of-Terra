using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] private bool dontDestroyOnLoad = true;
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                // 씬에 있는 인스턴스를 찾아봄
                _instance = FindObjectOfType<T>();

                if (_instance == null)
                {
                    // 없으면 새로 생성
                    GameObject obj = new GameObject(typeof(T).Name);
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        // 중복 방지
        if (_instance == null)
        {
            _instance = this as T;
            if(dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);  
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
}