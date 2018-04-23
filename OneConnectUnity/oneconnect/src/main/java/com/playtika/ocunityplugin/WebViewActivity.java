package com.playtika.ocunityplugin;

import android.app.Activity;
import android.content.Intent;
import android.os.Handler;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.Log;
import android.view.View;

public class WebViewActivity extends Activity {

    private static JavaMessageHandler javaMessageHandler;
    private static Handler unityMainThreadHandler;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_web_view);
    }

    public static void init(Activity activity) {
        Intent myIntent = new Intent(activity, WebViewActivity.class);
        activity.startActivity(myIntent);
    }

    public static void registerMessageHandler(JavaMessageHandler handler) {
        javaMessageHandler = handler;
        if (unityMainThreadHandler == null) {
            // Так как эту функцию вызываем всегда на старте Unity,
            // этот вызов идет из нужного нам в дальнейшем потока,
            // создадим для него Handler
            unityMainThreadHandler = new Handler();
        }
    }

    // Функция перевода выполнения в Unity поток, потребуется в дальнейшем
    public static void runOnUnityThread(Runnable runnable) {
        if (unityMainThreadHandler != null && runnable != null) {
            unityMainThreadHandler.post(runnable);
        }
    }

    // Пишем какую-нибудь функцию, которая будет отправлять сообщения в Unity
    public static void SendMessageToUnity(final String message, final String data) {
        runOnUnityThread(new Runnable() {

            @Override
            public void run() {
                if (javaMessageHandler != null) {
                    javaMessageHandler.onMessage(message, data);
                }
            }
        });
    }

    public static  void OnCloseClick(View view)
    {
        SendMessageToUnity("Message", "Data");
    }
}
