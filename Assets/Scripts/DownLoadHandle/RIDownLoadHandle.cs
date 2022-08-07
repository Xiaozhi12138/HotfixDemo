using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;


public class RIDownloadHandler :DownloadHandlerScript
{
    string m_SavePath = "";
    string m_TempFilePath = "";
    FileStream fs;

    public ulong totalFileLen { get; private set; }
    public ulong downloadedFileLen { get; private set; }
    public string fileName { get; private set; }
    public string dirPath { get; private set; }

    #region �¼�
    /// <summary>
    /// ��������URL����Ҫ���ص��ļ����ܴ�С
    /// </summary>
    public event Action<ulong> eventTotalLength = null;

    /// <summary>
    /// �����������ʱ��Ҫ���صĴ�С����ʣ���ļ���С��
    /// </summary>
    public event Action<ulong> eventContentLength = null;

    /// <summary>
    /// ÿ�����ص����ݺ�ص�����
    /// </summary>
    public event Action<float> eventProgress = null;

    /// <summary>
    /// ��������ɺ�ص����ص��ļ�λ��
    /// </summary>
    public event Action<string> eventComplete = null;
    #endregion

    /// <summary>
    /// ��ʼ�����ؾ��������ÿ�����ص���������Ϊ200kb
    /// </summary>
    /// <param name="filePath">���浽���ص��ļ�·��</param>
    public RIDownloadHandler( string filePath ) : base( new byte[1024 * 200] )
    {
        m_SavePath = filePath.Replace( '\\' , '/' );
        fileName = m_SavePath.Substring( m_SavePath.LastIndexOf( '/' ) + 1 );
        dirPath = m_SavePath.Substring( 0 , m_SavePath.LastIndexOf( '/' ) );
        m_TempFilePath = Path.Combine( dirPath , fileName + ".temp" );

        fs = new FileStream( m_TempFilePath , FileMode.Append , FileAccess.Write );
        downloadedFileLen = (ulong)fs.Length;
    }

    /// <summary>
    /// ��������ʱ�ĵ�һ���ص��������᷵����Ҫ���յ��ļ��ܳ���
    /// </summary>
    /// <param name="contentLength">���������������ʣ�µ��ļ���С�����ؿ��������ļ��ܳ���</param>
    protected override void ReceiveContentLengthHeader( ulong contentLength )
    {
        if( contentLength == 0 )
        {
            Debug.Log( "�������Ѿ���ɡ�" );
            CompleteContent( );
        }
        totalFileLen = contentLength + downloadedFileLen;
        eventTotalLength?.Invoke( totalFileLen );
        eventContentLength?.Invoke( contentLength );
    }

    /// <summary>
    /// �������ȡ����ʱ��Ļص���ÿ֡����һ��
    /// </summary>
    /// <param name="data">���յ��������ֽ������ܳ���Ϊ���캯�������200kb���������е����ݶ����µ�</param>
    /// <param name="dataLength">���յ������ݳ��ȣ���ʾdata�ֽ����������ж����������½��յ��ģ���0-dataLength֮��������Ǹս��յ���</param>
    /// <returns>����trueΪ�������أ�����falseΪ�ж�����</returns>
    protected override bool ReceiveData( byte[] data , int dataLength )
    {
        if( data == null || data.Length == 0 )
        {
            Debug.LogFormat( "�������С�<color=yellow>�����ļ�{0}�У�û�л�ȡ�����ݣ�������ֹ</color>" , fileName );
            return false;
        }
        fs?.Write( data , 0 , dataLength );
        downloadedFileLen += (ulong)dataLength;

        var progress = (float)downloadedFileLen / totalFileLen;
        eventProgress?.Invoke( progress );

        return true;
    }

    /// <summary>
    /// �������������ʱ�Ļص�
    /// </summary>
    protected override void CompleteContent( )
    {
        Debug.LogFormat( "��������ɡ�<color=green>��ɶ�{0}�ļ������أ�����·��Ϊ{1}</color>" , fileName , m_SavePath );
        fs.Close( );
        fs.Dispose( );
        if( File.Exists( m_TempFilePath ) )
        {
            if( File.Exists( m_SavePath ) )
                File.Delete( m_SavePath );
            File.Move( m_TempFilePath , m_SavePath );
        }
        else
        {
            Debug.LogFormat( "������ʧ�ܡ�<color=red>�����ļ�{0}ʱʧ��</color>" , fileName );
        }
        eventComplete?.Invoke( m_SavePath );
    }

    public void ErrorDispose( )
    {
        fs.Close( );
        fs.Dispose( );
        if( File.Exists( m_TempFilePath ) )
        {
            File.Delete( m_TempFilePath );
        }
        Dispose( );
    }
}

