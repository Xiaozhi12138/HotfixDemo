using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>{
    private static T instance;
    public static T Instance {
        get {
            if (instance == null) {   
                GameObject obj = new GameObject(typeof(T).ToString());
                instance = obj.AddComponent<T>();  
                instance.Init();  
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    public virtual void Init() {

    }
}
