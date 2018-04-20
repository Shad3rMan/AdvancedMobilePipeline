using UnityEngine;

public static class MessageHandler
{
    // Данный класс будет реализовывать Java Interface, который описан ниже
    private class JavaMessageHandler : AndroidJavaProxy
    {
        public JavaMessageHandler() : base("com.playtika.oneconnect.JavaMessageHandler") { }

        public void onMessage(string message, string data)
        {
            // Переадресуем наше сообщение всем желающим
            Debug.Log(message);
            Debug.Log(data);
        }
    }

    // Этот метод будет вызываться автоматически при инициализации Unity Engine в игре
    [RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
#if !UNITY_EDITOR
        Debug.Log("Initialize()");
        // Создаем инстанс JavaMessageHandler и передаем его 
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        var c = new AndroidJavaClass("com.playtika.oneconnect.UnityBridge");
        c.CallStatic("init", currentActivity);
        c.CallStatic("registerMessageHandler", new JavaMessageHandler());

        //new AndroidJavaObject("com.playtika.oneconnect.LoginActivity").Call("onCreate");
#endif
    }
}