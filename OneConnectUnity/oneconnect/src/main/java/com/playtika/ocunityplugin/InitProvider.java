package com.playtika.ocunityplugin;

import android.app.Application;
import android.content.ContentProvider;
import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.net.Uri;
import android.support.annotation.NonNull;
import android.support.annotation.Nullable;

public class InitProvider extends ContentProvider {

    private static Context context;

    @Override
    public boolean onCreate() {
        context = getContext();
        if (context != null && context instanceof Application) {
            // ActivityLifecycleListener — наша реализация интерфейса Application.ActivityLifecycleCallbacks
            ((Application) context).registerActivityLifecycleCallbacks(new ActivityLifecycleListener(context));
        }
        return false;
    }

    public static Context GetContext()
    {
        return context;
    }

    @Nullable
    @Override
    public Cursor query(@NonNull Uri uri, @Nullable String[] projection, @Nullable String selection, @Nullable String[] selectionArgs, @Nullable String sortOrder) {
        return null;
    }

    @Nullable
    @Override
    public String getType(@NonNull Uri uri) {
        return null;
    }

    @Nullable
    @Override
    public Uri insert(@NonNull Uri uri, @Nullable ContentValues values) {
        return null;
    }

    @Override
    public int delete(@NonNull Uri uri, @Nullable String selection, @Nullable String[] selectionArgs) {
        return 0;
    }

    @Override
    public int update(@NonNull Uri uri, @Nullable ContentValues values, @Nullable String selection, @Nullable String[] selectionArgs) {
        return 0;
    }

    // Далее имплементация абстрактных методов
}