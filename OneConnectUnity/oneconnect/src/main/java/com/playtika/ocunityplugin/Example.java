package com.playtika.ocunityplugin;

import org.json.JSONException;
import org.json.JSONObject;

public class Example {

    public static void getSomeDataWithCallback(String key, final CallbackJsonHandler callback) {
        // В качестве примера выполним какие-то действия в background потоке
        new Thread(new Runnable() {

            @Override
            public void run() {
                // Колбек требуется вызывать в Unity потоке
                UnityBridge.runOnUnityThread(new Runnable() {

                    @Override
                    public void run() {
                        try {
                            callback.onHandleResult(new JSONObject().put("Success", true).put("ValueStr", "someResult").put( "ValueInt", 42));
                        } catch (JSONException e) {
                            e.printStackTrace();
                        }
                    }
                });
            }
        });
    }
}