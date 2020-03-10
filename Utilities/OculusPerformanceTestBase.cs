using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OculusPerformanceTestBase
{
#if UNITY_ANDROID || UNITY_IOS
    protected readonly float CoolOffDuration = 30f;
#else
    protected readonly float CoolOffDuration = 2f;
#endif
    protected readonly float SettleTime = 10f;
    protected readonly string CoolDownSceneName = "cool_down";

    /// <summary>
    ///  This method will allow the device to cooldown then load test scene. Finally it will allow the scene to settle before returning.
    /// </summary>
    /// <param name="scene">The scene to meaure perforance against</param>
    /// <returns></returns>


    
    protected IEnumerator SetupTestRun(string scene)
    {
        // TODO, even though we're setting these values in ADB, the values always come back as 2. JJ to follow up via Favro card
        //var expCpuLevel = 3;
        //var expGpuLevel = 3;
        //var actCpuLevel = OculusStats.AdaptivePerformance.CPULevel;
        //var actGpuLevel = OculusStats.AdaptivePerformance.GPULevel;
        //Assert.AreEqual(
        //    expCpuLevel,
        //    actCpuLevel,
        //    string.Format("Expected CPU Level for the test device to be {0}, but is {1}. Lock the CPU level via command line with `adb shell setprop debug.oculus.cpuLevel 3`", expCpuLevel, actCpuLevel));
        //Assert.AreEqual(
        //    expGpuLevel,
        //    actGpuLevel,
        //    string.Format("Expected GPU Level for the test device to be {0}, but is {1}. Lock the GPU level via command line with `adb shell setprop debug.oculus.gpuLevel 3`", expGpuLevel, actGpuLevel));
        yield return CoolDown();
        yield return StartTestRun(scene);
    }

    protected IEnumerator TearDownTestRun(string scene)
    {
        yield return SceneManager.UnloadSceneAsync(scene);
    }
    
    IEnumerator StartTestRun(string scene)
    {
        yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        SetActiveScene(scene);
        yield return new WaitForSecondsRealtime(SettleTime);
    }

    void SetActiveScene(string sceneName)
    {
        var scene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(scene);
    }
    
    IEnumerator CoolDown()
    {
        // Device is getting hot after test. Spend some time at very low framerate
        int previousTargetFrameRate = Application.targetFrameRate;
        Application.targetFrameRate = 1;

        yield return SceneManager.LoadSceneAsync(CoolDownSceneName, LoadSceneMode.Additive);
        SetActiveScene(CoolDownSceneName);
        yield return new WaitForSecondsRealtime(CoolOffDuration);

        // After cool down, restore framerate
        Application.targetFrameRate = previousTargetFrameRate;

        yield return SceneManager.UnloadSceneAsync(CoolDownSceneName);
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaObject _androidActivity;
    private static AndroidJavaObject GetAndroidActivity()
    {
        if (_androidActivity == null)
        {
            var actClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            _androidActivity = actClass.GetStatic<AndroidJavaObject>("currentActivity");
        }

        return _androidActivity;
    }

    public static AndroidJavaObject GetAndroidIntent()
    {
        AndroidJavaObject androidActivity = GetAndroidActivity();
        AndroidJavaObject context = androidActivity.Call<AndroidJavaObject>("getApplicationContext");
        AndroidJavaObject intentFilter = new AndroidJavaObject("android.content.IntentFilter", "android.intent.action.BATTERY_CHANGED");

        return context.Call<AndroidJavaObject>("registerReceiver", null, intentFilter);
    }
#endif

    public static float GetBatteryTemp()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaObject intent = GetAndroidIntent();
        float temp = intent.Call<int>("getIntExtra", "temperature", 0) / 10.0f; // temp now
        return temp;
#elif UNITY_IOS && !UNITY_EDITOR
        //return GetBatteryTempiOS();
        return 0f;
#else
        return 0f;
#endif
    }

    public static bool HasDeviceOverheated()
    {
        float temp = GetBatteryTemp();

        if (Application.platform == RuntimePlatform.Android)
            return temp > 28.5f;

        if (Application.platform == RuntimePlatform.IPhonePlayer)
            return temp > 0.0f;

        return false;
    }
}
