using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System.IO;
             
public class ABManager : MonoSingleton<ABManager> {
    Dictionary<string, AssetBundle> loadedABDict = new Dictionary<string, AssetBundle>();

    private string path;

    public string Path {
        get {
            if (path == null || path == "") {
                //根据平台判断路径
                #if UNITY_STANDALONE_WIN
                path = Application.streamingAssetsPath + "/";
                #elif UNITY_ANDROID
                path = Application.dataPath + "/";
                #endif
            }
            return path;
        }
    }

    public string MainABName {
        get {
#if UNITY_STANDALONE_WIN
            return "Android";
#elif UNITY_ANDROID
            return "Android";
#elif UNITY_IOS
            return "iOS";
#else
            return "AssetBundle";
#endif
        }
    }
    //主包
    private AssetBundle main;
    //主manifest文件
    private AssetBundleManifest manifest;
    /// <summary>
    /// 加载指定AB包
    /// </summary>
    /// <param name="abName"></param>
    /// <returns></returns>
    public AssetBundle Load(string abName) {
        if ( !File.Exists( Path + abName ) )
        {
            Debug.LogError( "指定加载的AB包文件不存在，请检查加载路径：" + Path + abName );
            return null;
        }
        //判断主包和主包.Manifest是否为空
        if ( main == null) {
            if ( !File.Exists( Path + abName ) )
            {
                Debug.LogError( "指定加载的AB包文件不存在，请检查加载路径：" + Path + abName );
                return null;
            }
            main = AssetBundle.LoadFromFile(Path + MainABName); 
        }
        if (manifest == null) {
            manifest = main.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
        //获取所有该AB包的依赖
        string[] deps = manifest.GetAllDependencies(abName);
        //遍历查看是否加载过该依赖，如果加载过就跳过，没加载则加载
        for (int i = 0; i < deps.Length; i++) {
            if (!loadedABDict.ContainsKey(deps[i])) {
                AssetBundle depAB = AssetBundle.LoadFromFile(Path + deps[i]);
                loadedABDict[deps[i]] = depAB;
            }
        }
        //现在所有依赖都加载过了，再加载该AB包

        AssetBundle ab = null;
        if (!loadedABDict.TryGetValue(abName, out ab))
        {                   
            ab = AssetBundle.LoadFromFile(Path + abName);
            loadedABDict[abName] = ab;
        }            
        return ab;
    }
    /// <summary>
    /// 卸载指定AB包
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="isUnloadObjects"></param>
    public void Unload(string abName, bool isUnloadObjects = false) {
        AssetBundle ab;
        if (loadedABDict.TryGetValue(abName, out ab)) {
            ab.Unload(isUnloadObjects);
        }
        else {
            Debug.Log(string.Format("不存在{0}", abName));
        }
    }
    /// <summary>
    /// 卸载全部AB包
    /// </summary>
    /// <param name="isUnloadObjects"></param>
    public void UnloadAll(bool isUnloadObjects = false) {
        foreach (var item in loadedABDict.Values) {
            item.Unload(isUnloadObjects);
        }
        loadedABDict.Clear();
    }

    /// <summary>
    /// 加载指定AB包内的资源
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="assetName"></param>
    /// <returns></returns>
    /// 
    
    public Object LoadAsset(string abName, string assetName) {
        AssetBundle ab = Load(abName);
        if (ab != null) {
            return ab.LoadAsset(assetName);
        }
        return null;
    }
    /// <summary>
    /// 加载指定AB包内的资源
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="assetName"></param>
    /// <returns></returns>
    ///   
    public Object LoadAssetLua(string abName, string assetName, System.Type type)
    {
        Debug.Log("AB包加载资源");
        AssetBundle ab = Load(abName);
        if (ab != null) {
            return ab.LoadAsset(assetName, type);
        }
        return null;
    }
    /// <summary>
    /// 加载指定AB包内的资源
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public T LoadAsset<T>(string abName, string assetName) where T : Object { 
        AssetBundle ab = Load(abName); 
        if (ab != null) {
            return ab.LoadAsset<T>(assetName);
        }
        Debug.Log("加载失败 没有加载到资源");
        return default(T);
    }
}
