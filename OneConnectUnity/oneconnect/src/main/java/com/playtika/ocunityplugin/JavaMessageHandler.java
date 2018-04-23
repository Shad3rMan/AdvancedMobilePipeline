package com.playtika.ocunityplugin;

public interface JavaMessageHandler {
    void onMessage(String message, String data);

    void onPageLoaded();

    void onError(String error);

    void onCloseRequested();
}
