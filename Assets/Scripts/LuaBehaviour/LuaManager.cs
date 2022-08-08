using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System.IO;
using System.Text;

public class LuaManager : MonoSingleton<LuaManager> {
    LuaEnv luaEnv;
    List<string> editorChildrenDirs = new List<string>();
    float GCInterval = 5;
    float lastTime;

    public LuaEnv LuaEnv {
        get {
            if (luaEnv == null) {
                luaEnv = new LuaEnv();
            }
            return luaEnv;
        }
    }

    public LuaTable Global {
        get {
            return luaEnv.Global;
        }
    }

    private void Awake() {
        lastTime = 0;
        if (luaEnv == null) {
            luaEnv = new LuaEnv();
        }
        //luaEnv = new LuaEnv();
        //luaEnv.AddLoader(CustomLoaderStreamingAssets);
        //luaEnv.AddLoader(CustomLoader);
        luaEnv.AddLoader(CustomAssetBundleLoader);
    }

    void Update() {
        //定期调用Lua的Tick
        if (Time.time - lastTime > GCInterval) {
            luaEnv.Tick();
            lastTime = Time.time;
        }
    }

    public void DoLua(string luaStr, string chunk = "chunk", LuaTable lt = null) {
        luaEnv.DoString(luaStr, chunk, lt);
    }

    public void DoRequire(string fileName, string chunk = "chunk", LuaTable lt = null) {
        luaEnv.DoString(string.Format("require('{0}')", fileName), chunk, lt);
    }

    public void DoLuaInFile(string fileName, string chunk = "chunk", LuaTable lt = null) {
        if (fileName == "") {
            return;
        }
        foreach (var loader in luaEnv.customLoaders) {
            byte[] bytes = loader(ref fileName);
            if (bytes != null) {
                string luaStr = Encoding.UTF8.GetString(bytes);
                //Debug.Log("Find:" +luaStr);
                DoLua(luaStr, chunk, lt);
                return;
            }
        }
    }
    /// <summary>
    /// 自定义加载器
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    byte[] CustomLoader(ref string fileName) {
        if (editorChildrenDirs == null || editorChildrenDirs.Count <= 0) {
            string folder = Application.dataPath + "/Lua/";
            //editorChildrenDirs = new List<string>();
            editorChildrenDirs.Add(folder);
            string[] childenDirs = Directory.GetDirectories(folder);
            for (int i = 0; i < childenDirs.Length; i++) {
                //Debug.Log(childenDirs[i]);
                editorChildrenDirs.Add(childenDirs[i]);
            }
        }

        for (int i = 0; i < editorChildrenDirs.Count; i++) {
            string finalPath = editorChildrenDirs[i] + "/" + fileName + ".lua";
            //Debug.Log(finalPath);
            if (File.Exists(finalPath)) {
                return File.ReadAllBytes(finalPath);
            }
        }

        return null;
    }
    byte[] CustomLoaderStreamingAssets(ref string fileName)
    {
        if (editorChildrenDirs == null || editorChildrenDirs.Count <= 0)
        {   
            string folder = Application.streamingAssetsPath + "/Lua/";
            //editorChildrenDirs = new List<string>();
            editorChildrenDirs.Add(folder);
            string[] childenDirs = Directory.GetDirectories(folder);
            for (int i = 0; i < childenDirs.Length; i++)
            {               
                editorChildrenDirs.Add(childenDirs[i]);                                       
            }
        }

        for (int i = 0; i < editorChildrenDirs.Count; i++)
        {
            string finalPath = editorChildrenDirs[i] + "/" + fileName + ".lua";
            //Debug.Log(finalPath);
            if (File.Exists(finalPath))
            {
                return File.ReadAllBytes(finalPath);
            }
        }

        return null;                 
    }
    /// <summary>
    /// 从AB包中加载
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    byte[] CustomAssetBundleLoader(ref string fileName) {
        //名字为lua的AB包存储所有的lua文件
        TextAsset asset = ABManager.Instance.LoadAsset<TextAsset>("test.ii", fileName );
        Debug.Log( asset );
        //不一定加载到资源
        if (asset != null) {
            return asset.bytes;
        }
        return null;
    }
}
