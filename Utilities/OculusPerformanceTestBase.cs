using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

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
}

public class OculusPerformanceMonobehaviorBase : MonoBehaviour
{
    void Awake()
    {
#if ENABLE_VR && UNITY_2017_1_OR_NEWER
        if (XRSettings.enabled)
        {
            var thisCamera = Camera.main.gameObject.GetComponent<Camera>();
            if (thisCamera != null)
            {
                XRDevice.DisableAutoXRCameraTracking(thisCamera, true);

                // Reset orientation of the Camera after disabling tracking
                Transform camTransform = thisCamera.transform;
                camTransform.position = Vector3.up + (Vector3.back * 10);
                camTransform.forward = Vector3.forward;
                camTransform.rotation = Quaternion.identity;
                camTransform.SetParent(thisCamera.transform, true);
            }
        }
#endif
    }
}
