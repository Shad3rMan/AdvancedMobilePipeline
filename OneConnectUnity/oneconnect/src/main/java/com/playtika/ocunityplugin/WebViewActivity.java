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

    public void OnCloseClick(View view)
    {
        UnityBridge.OnClose();
    }
}
