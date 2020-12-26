// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Thry
{
    public class WebHelper
    {
        public static string FixUrl(string url)
        {
            if (!url.StartsWith("http"))
                url = "http://" + url;
            return url;
        }

        public static void SendAnalytics()
        {
            string url_values_postfix = "?hash=" + GetMacAddress().GetHashCode();
            if (Config.Get().share_installed_editor_version) url_values_postfix += "&editor=" + Config.Get().verion;
            if (Config.Get().share_installed_unity_version) url_values_postfix += "&unity=" + Application.unityVersion;
            if (Config.Get().share_used_shaders)
            {
                url_values_postfix += "&shaders=[";
                foreach (ShaderHelper.ShaderEditorShader s in ShaderHelper.thry_editor_shaders)
                {
                    url_values_postfix += "{\"name\":\"" + s.name + "\",\"version\":\"";
                    if (s.version != null && s.version != "null") url_values_postfix += s.version;
                    url_values_postfix += "\"},";
                }
                url_values_postfix = url_values_postfix.TrimEnd(new char[] { ',' }) + "]";
            }
            DownloadStringASync(URL.DATA_SHARE_SEND + url_values_postfix, null);
        }

        public static string GetMacAddress()
        {
            return (from nic in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                    where nic.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up
                    select nic.GetPhysicalAddress().ToString()
            ).FirstOrDefault();
        }

        public static string GetFinalRedirect(string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;
            try
            {
                UnityWebRequest request = new UnityWebRequest(url);
                request.method = UnityWebRequest.kHttpVerbHEAD;
                DownloadHandlerBuffer response = new DownloadHandlerBuffer();
                request.downloadHandler = response;
                request.SendWebRequest();
                bool fetching = true;
                while (fetching)
                {
                    if (request.isHttpError || request.isNetworkError)
                    {
                        fetching = false;
                        Debug.Log(request.error);
                    }
                    if (request.isDone)
                    {
                        fetching = false;
                    }
                }
                return request.url;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //-------------------Downloaders-----------------------------

        [InitializeOnLoad]
        public class MainThreader
        {
            private struct CallData
            {
                public Action<string> action;
                public object[] arguments;
            }
            static List<CallData> queue;

            static MainThreader()
            {
                queue = new List<CallData>();
                EditorApplication.update += Update;
            }

            public static void Call(Action<string> action, params object[] args)
            {
                if (action == null)
                    return;
                CallData data = new CallData();
                data.action = action;
                data.arguments = args;
                if (args == null || args.Length == 0 || args[0] == null)
                    data.arguments = new object[] { "" };
                else
                    data.arguments = args;
                queue.Add(data);
            }

            public static void Update()
            {
                if (queue.Count > 0)
                {
                    try
                    {
                        queue[0].action.DynamicInvoke(queue[0].arguments);
                    }
                    catch { }
                    queue.RemoveAt(0);
                }
            }
        }

        public static void DownloadFile(string url, string path)
        {
            DownloadAsFile(url, path);
        }

        public static void DownloadFileASync(string url, string path, Action<string> callback)
        {
            DownloadAsBytesASync(url, delegate (object o, DownloadDataCompletedEventArgs a)
            {
                if (a.Cancelled || a.Error != null)
                    MainThreader.Call(callback, null);
                else
                {
                    FileHelper.writeBytesToFile(a.Result, path);
                    MainThreader.Call(callback, path);
                }
            });
        }

        public static string DownloadString(string url)
        {
            return DownloadAsString(url);
        }

        public static void DownloadStringASync(string url, Action<string> callback)
        {
            DownloadAsStringASync(url, delegate (object o, DownloadStringCompletedEventArgs e)
            {
                if (e.Cancelled || e.Error != null)
                {
                    Debug.LogWarning(e.Error);
                    MainThreader.Call(callback, null);
                }
                else
                    MainThreader.Call(callback, e.Result);
            });
        }

        private static void SetCertificate()
        {
            ServicePointManager.ServerCertificateValidationCallback =
        delegate (object s, X509Certificate certificate,
                 X509Chain chain, SslPolicyErrors sslPolicyErrors)
        { return true; };
        }

        private static string DownloadAsString(string url)
        {
            SetCertificate();
            string contents = null;
            using (var wc = new System.Net.WebClient())
                contents = wc.DownloadString(url);
            return contents;
        }

        private static void DownloadAsStringASync(string url, Action<object, DownloadStringCompletedEventArgs> callback)
        {
            SetCertificate();
            using (var wc = new System.Net.WebClient())
            {
                wc.Headers["User-Agent"] = "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0)";
                wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(callback);
                wc.DownloadStringAsync(new Uri(url));
            }
        }

        private static void DownloadAsFileASync(string url, string path, Action<object, AsyncCompletedEventArgs> callback)
        {
            SetCertificate();
            using (var wc = new System.Net.WebClient())
            {
                wc.DownloadFileCompleted += new AsyncCompletedEventHandler(callback);
                wc.DownloadFileAsync(new Uri(url), path);
            }
        }

        private static void DownloadAsFile(string url, string path)
        {
            SetCertificate();
            using (var wc = new System.Net.WebClient())
                wc.DownloadFile(url, path);
        }

        private static byte[] DownloadAsBytes(string url)
        {
            SetCertificate();
            byte[] contents = null;
            using (var wc = new System.Net.WebClient())
                contents = wc.DownloadData(url);
            return contents;
        }

        private static void DownloadAsBytesASync(string url, Action<object, DownloadDataCompletedEventArgs> callback)
        {
            SetCertificate();
            using (var wc = new System.Net.WebClient())
            {
                wc.DownloadDataCompleted += new DownloadDataCompletedEventHandler(callback);
                wc.DownloadDataAsync(new Uri(url));
            }
        }
    }

    public class WebHelper2
    {

        public static void DownloadStringASync(string url, Action<string> callback)
        {
            Downloader downloader = new Downloader();
            downloader.url = url;
            downloader.callback = callback;
            Coroutines.AddRoutine(downloader.DownloadStringCorroutine());
        }

        public static void DownloadFileASync(string url, string path, Action<string> callback)
        {
            Downloader downloader = new Downloader();
            downloader.url = url;
            downloader.path = path;
            downloader.callback = callback;
            Coroutines.AddRoutine(downloader.DownloadFileCorroutine());
        }

        public class Downloader{

            public Action<string> callback;
            public string path;
            public string url;

            public IEnumerator<float> DownloadStringCorroutine()
            {
                UnityWebRequest request = new UnityWebRequest(url);
                request.method = UnityWebRequest.kHttpVerbGET;
                DownloadHandlerBuffer response = new DownloadHandlerBuffer();
                request.downloadHandler = response;
                request.SendWebRequest();
                bool fetching = true;
                while (fetching)
                {
                    yield return 0.3f;
                    if (request.isHttpError || request.isNetworkError)
                    {
                        fetching = false;
                        Debug.Log(request.error);
                    }
                    if (request.isDone)
                    {
                        fetching = false;
                        callback(response.text);
                    }
                }
            }

            public IEnumerator<float> DownloadFileCorroutine()
            {
                UnityWebRequest request = new UnityWebRequest(url);
                request.method = UnityWebRequest.kHttpVerbGET;
                DownloadHandlerBuffer response = new DownloadHandlerBuffer();
                request.downloadHandler = response;
                request.SendWebRequest();
                bool fetching = true;
                while (fetching)
                {
                    yield return 0.3f;
                    if (request.isHttpError || request.isNetworkError)
                    {
                        fetching = false;
                        Debug.Log(request.error);
                    }
                    if (request.isDone)
                    {
                        fetching = false;
                        FileHelper.writeBytesToFile(response.data, path);
                        if(callback!=null)
                            callback(null);
                    }
                }
            }
        }

        

    }

    public class Coroutines
    {
        private static List<TimedCoroutine> active_routines = new List<TimedCoroutine>();
        private static DateTime previousTime;

        public static void AddRoutine(IEnumerator<float> coroutine)
        {
            active_routines.Add(new TimedCoroutine(coroutine));
            if(active_routines.Count==1)
                EditorApplication.update += Update;
        }

        private static void Update()
        {
            float deltaTime = (float)(DateTime.Now.Subtract(previousTime).TotalMilliseconds / 1000.0f);
            previousTime = DateTime.Now;

            if (active_routines.Count > 0)
            {
                for(int i=0;i<active_routines.Count;i++)
                {
                    if(active_routines[i].IsDoneWaiting(deltaTime)){
                        if (active_routines[i].Continue())
                            active_routines[i].ResetTimeLeft();
                        else
                            active_routines.Remove(active_routines[i]);
                    }
                }
            }
            else
            {
                EditorApplication.update -= Update;
            }
        }

        private class TimedCoroutine
        {
            private IEnumerator<float> coroutine;
            private float wait_time_left;

            public TimedCoroutine(IEnumerator<float> coroutine)
            {
                this.coroutine = coroutine;
                wait_time_left = this.coroutine.Current;
            }

            public bool Continue()
            {
                return coroutine.MoveNext();
            }

            public void ResetTimeLeft()
            {
                wait_time_left = coroutine.Current;
            }

            public bool IsDoneWaiting(float deltaTime)
            {
                wait_time_left -= deltaTime;
                return wait_time_left < 0;
            }

        }
    }
}