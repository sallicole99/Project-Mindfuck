using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    public static T Instance => _instance ? _instance : (_instance = FindObjectOfType<T>() ?? CreateInstance());

    static T CreateInstance()
    {
        GameObject obj = new GameObject(typeof(T).Name);
        DontDestroyOnLoad(obj);
        return obj.AddComponent<T>();
    }

    protected virtual void Awake()
    {
        if (_instance && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
    }
}