using UnityEngine;

public static class MessageHandler
{
    // Данный класс будет реализовывать Java Interface, который описан ниже
    private class JavaMessageHandler : AndroidJavaProxy
    {
        public JavaMessageHandler() : base("com.playtika.ocunityplugin.JavaMessageHandler") { }

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
        //StartPackage("com.playtika.ocunityplugin.LoginActivity");
#if !UNITY_EDITOR
        Debug.Log("Initialize()");
        // Создаем инстанс JavaMessageHandler и передаем его 
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        var c = new AndroidJavaClass("com.playtika.ocunityplugin.UnityBridge");
        c.CallStatic("init", currentActivity);
        c.CallStatic("registerMessageHandler", new JavaMessageHandler());

        //new AndroidJavaObject("com.playtika.ocunityplugin.LoginActivity").Call("onCreate");

#endif
    }

    static void StartPackage(string package)
    {
 #if !UNITY_EDITOR
       AndroidJavaClass activityClass;
        AndroidJavaObject activity, packageManager;
        AndroidJavaObject launch;


        activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
        packageManager = activity.Call<AndroidJavaObject>("getPackageManager");
        launch = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", package);
        activity.Call("startActivity", launch);
#endif
    }
}