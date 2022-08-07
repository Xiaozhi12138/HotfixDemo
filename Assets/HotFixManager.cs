using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;

public class HotFixManager :MonoBehaviour
{
    public const string url = @"http://124.223.206.85/AB/";

    private GameObject testPanel;

    [SerializeField]
    private Button openButton;


    public string SavePath
    {
        get
        {
#if UNITY_EDITOR

            return Application.streamingAssetsPath;

#else
            return  Application.persistentDataPath;
#endif
        }
    }

    private void Awake( )
    {
        openButton.onClick.AddListener( ( ) =>
        {
            Instantiate( testPanel , transform );
        } );
    }

    private void Start( )
    {
        StartCoroutine( DownloadABAsset( ) );
    }

    private IEnumerator DownloadABAsset( )
    {
        var loadHandler = new RIDownloadHandler( Path.Combine( SavePath , "test.ii" ) );
        loadHandler.eventProgress += LoadProcessEvent;
        loadHandler.eventComplete += LoadCompleteEvent;
        loadHandler.eventTotalLength += LoadTotalLenEvent;
        loadHandler.eventContentLength += LoadContentLengthEvent;

        using( var www = UnityWebRequest.Get( Path.Combine( url , "test.ii" ) ) )
        {
            www.disposeDownloadHandlerOnDispose = true;
            www.SetRequestHeader( "Range" , "bytes=" + loadHandler.downloadedFileLen + "-" );
            www.downloadHandler = loadHandler;
            yield return www.SendWebRequest( );
            if( www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError )
            {
                Debug.LogFormat( "【下载失败】下载文件{0}失败，失败原因：{1}" , loadHandler.fileName , www.error );

                loadHandler.ErrorDispose( );
            }
        }
    }

    private void LoadContentLengthEvent( ulong len )
    {
        Debug.Log( "LoadContentLengthEvent:" + len );
    }

    private void LoadTotalLenEvent( ulong len )
    {
        Debug.Log( "LoadTotalLenEvent:" + len );
    }

    private void LoadCompleteEvent( string filePath )
    {
        testPanel = ABManager.Instance.LoadAsset( "test.ii" , "TestPanel.prefab" ) as GameObject;
        openButton.gameObject.SetActive( true );
    }

    private void LoadProcessEvent( float process )
    {
        Debug.Log( "LoadProcessEvent:" + process );
    }
}
