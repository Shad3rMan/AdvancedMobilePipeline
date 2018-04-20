package com.playtika.ocunityplugin;

import android.app.Activity;
import android.content.Intent;
import android.os.Handler;
import android.util.Log;
import android.view.View;
import android.view.ViewGroup;
import android.webkit.WebView;
import android.widget.FrameLayout;

public final class UnityBridge {

    private static JavaMessageHandler javaMessageHandler;
    private static Handler unityMainThreadHandler;

    public static void init(Activity activity) {
        Log.d("Unity", "init");
        //Intent myIntent = new Intent(activity, LoginActivity.class);
        //activity.startActivity(myIntent);
        AddView(activity);
    }

    public static void AddView(final Activity activity) {
        activity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                AddWebView(activity);
            }
        });
    }

    public static void AddWebView(Activity activity) {
        WebView webView = new WebView(activity);
        FrameLayout frameLayout = new FrameLayout(activity);
        frameLayout.setLayoutParams(new FrameLayout.LayoutParams(
                FrameLayout.LayoutParams.WRAP_CONTENT,
                FrameLayout.LayoutParams.WRAP_CONTENT
        ));
        activity.addContentView(frameLayout, new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.WRAP_CONTENT,
                ViewGroup.LayoutParams.WRAP_CONTENT
        ));
        frameLayout.addView(webView);
        MoveTo(frameLayout, 100, 200);
        webView.loadUrl("http://www.tut.by");
    }

    public static void MoveTo(View view, int newX, int newY){
        view.setX(newX);
        view.setY(newY);
        view.setMinimumWidth(200);
        view.setMinimumHeight(100);
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
}
