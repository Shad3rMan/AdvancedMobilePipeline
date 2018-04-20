using System;
using UnityEngine;

public static class MonoJavaCallback
{
    // Объявим класс, реализующий колбек на Java
    // и проксирующий вызов в передаваемый Action
    private class AndroidCallbackHandler<T> : AndroidJavaProxy
    {
        private readonly Action<T> _resultHandler;

        public AndroidCallbackHandler(Action<T> resultHandler) : base("com.playtika.oneconnect.CallbackJsonHandler")
        {
            _resultHandler = resultHandler;
        }

        // В качестве аргумента передаем JSONObject
        // по аналогии с примером из первой части, 
        // но можно было использовать и другие типы
        public void onHandleResult(AndroidJavaObject result)
        {
            if (_resultHandler != null)
            {
                // Переводим json объект в строку
                var resultJson = result == null ? null : result.Call<string>("toString");
                // и парсим эту строку в C# объект
                _resultHandler.Invoke(JsonUtility.FromJson<T>(resultJson));
            }
        }
    }

    // В дальнейшем будем использовать эту функцию для оборачивания C# делегата
    public static AndroidJavaProxy ActionToJavaObject<T>(Action<T> action)
    {
        return new AndroidCallbackHandler<T>(action);
    }
}