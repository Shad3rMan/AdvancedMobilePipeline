using UnityEngine;
using System;

public class Example : MonoBehaviour
{
    public class ResultData
    {
        public bool Success;
        public string ValueStr;
        public int ValueInt;
    }

    public static void GetSomeData(string key, Action<ResultData> completionHandler)
    {
        new AndroidJavaClass("com.playtika.oneconnect.Example").CallStatic("getSomeDataWithCallback", key, MonoJavaCallback.ActionToJavaObject(completionHandler));
    }

    private void Start()
    {
        GetSomeData("Key", CompletionHandler);
    }

    private void CompletionHandler(ResultData obj)
    {
        Debug.Log(obj.ValueStr);
    }
}
