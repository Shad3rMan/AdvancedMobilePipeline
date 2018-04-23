package com.playtika.ocunityplugin;

public interface JavaMessageHandler {
    void onPageLoaded();

    void onError(String error);

    void onCloseRequested();
}
