using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using XLua;
using UnityEngine.UI;

public delegate void UIDele(params object[] args);

public enum PanelType {
    Normal,
    Tip,
    Loading,
}

public class LuaBehaviour : MonoBehaviour {
    //通过一个文件名字就能执行整个lua文件
    public string luaFileName;

    public string panelName;
    public PanelType panelType;

    //定义一个独立的存储区（LuaTable）
    public LuaTable scriptLua;

    //生命周期函数映射的委托
    private Action awake;
    private Action start;
    private Action update;
    private Action onDestroy;

    public Action Init;
    public UIDele Show;
    public Action Hide;
    public Action Unload;
    public Action Destroy;

    public void InitPanelFunc() {
        //创建一个私有的LuaTable
        scriptLua = LuaManager.Instance.LuaEnv.NewTable();
        //创建元表
        LuaTable meta = LuaManager.Instance.LuaEnv.NewTable();
        meta.Set("__index", LuaManager.Instance.Global);
        //将元表设置进私有LuaTable
        scriptLua.SetMetaTable(meta);
        meta.Dispose();
        //启动Lua脚本
        LuaManager.Instance.DoLuaInFile(luaFileName, luaFileName, scriptLua);
        //注入常用变量
        scriptLua.Set("gameObject", gameObject);
        scriptLua.Set("transform", transform);
        scriptLua.Set("panelType", panelType);
        scriptLua.Set("panelName", panelName);

        //Text t;
        //t.transform.parent.gameObject.SetActive

        //获取Lua文件里的生命周期函数
        scriptLua.Get("Awake", out awake);
        scriptLua.Get("Start", out start);
        scriptLua.Get("Update", out update);
        scriptLua.Get("OnDestroy", out onDestroy);


        scriptLua.Get("Init", out Init);
        scriptLua.Get("Show", out Show);
        scriptLua.Get("Hide", out Hide);
        scriptLua.Get("Unload", out Unload);
        scriptLua.Get("Destroy", out Destroy);
    }

    public void SetVar(string varName, object var) {
        scriptLua.Set(varName, var);
    }


    protected virtual void Awake() {
        this.InitPanelFunc( );
        //InitPanelFunc();
        if (awake != null) {
            awake();
        }
    }

    protected virtual void Start () {
        if (start != null) {
            start();
        }
    }


    protected virtual void Update () {
        if (update != null) {
            update();
        }
    }

    protected virtual void OnDestroy() {
        if (onDestroy != null) {
            onDestroy();
        }

        awake = null;
        start = null;
        update = null;
        onDestroy = null;

        Init = null;
        Show = null;
        Hide = null;
        Unload = null;
        Destroy = null;

        if (scriptLua != null) {
            scriptLua.Dispose();
        }
        

    }
}
