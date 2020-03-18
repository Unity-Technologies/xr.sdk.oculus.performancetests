using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.TestTools;
using System.Text;

public class ObjectCountStressTest : OculusPerformanceTestBase
{
    [Version("5")]
    [UnityTest, Performance]
    [Category("XR")]
    [Category("Performance")]
    [Category("OCULUS_XRSDK")]
    [Category("URP")]
    [Timeout(500000)]
    [PerformanceTestSource("ObjectCountStressTests")]
    public IEnumerator PerformObjectCountStressTest(string scene)
    {
        yield return SetupTestRun(scene);
        yield return new MonoBehaviourTest<ObjectCountStressTestMonoBehaviour>();
        yield return TearDownTestRun(scene);
    }


    public class ObjectCountStressTestMonoBehaviour : MonoBehaviour, IMonoBehaviourTest
    {
        private readonly SampleGroupDefinition ObjectCount = new SampleGroupDefinition("Number Of Objects", SampleUnit.None, increaseIsBetter: true);
        private readonly SampleGroupDefinition BatteryTemperature = new SampleGroupDefinition("Battery Temperature", SampleUnit.None);

        public bool IsTestFinished { get; set; }

        private float m_StartTime;
        private int m_StartFrameCount;
        private float m_RenderedFPS;

        

        private bool m_SpawnObjects = true;

#if OCULUS_SDK_PERF
        private readonly float m_MinimumFPS = 68.0f;
#else
        private readonly float m_MinimumFPS = 56.0f;
#endif
        private int m_ObjectSpawnCount = 32;
        private readonly int m_StabilizationFrames = 72;

        IEnumerator Start()
        {
            StringBuilder sb = new StringBuilder();
            Text debugText = (Text)FindObjectOfType(typeof(Text));

            GameObject objectToSpawn = GameObject.Find("Prototype");

            ResetFrameTime();

            while (true)
            {
                if (m_SpawnObjects)
                    StressTestFactory.SpawnObjects(m_ObjectSpawnCount, objectToSpawn);
                else
                    StressTestFactory.DestroyObjects(m_ObjectSpawnCount);

                for (int i = 0; i < m_StabilizationFrames; i++)
                {
                    yield return 0;

                }
                m_RenderedFPS = ((Time.renderedFrameCount - m_StartFrameCount) / (Time.time - m_StartTime)); // Get's the average over the m_StabilizationFrames
                ResetFrameTime(); // reset so we can get a new average

                if (m_RenderedFPS < m_MinimumFPS)
                {
                    if (m_ObjectSpawnCount == 1)
                        break;
                    m_SpawnObjects = false;
                }
                else
                {
                    if (!m_SpawnObjects)
                        m_ObjectSpawnCount = m_ObjectSpawnCount / 2;

                    m_SpawnObjects = true;
                }
            }

            Measure.Custom(ObjectCount, StressTestFactory.GetObjectCount());
            Measure.Custom(BatteryTemperature, OculusStats.AdaptivePerformance.BatteryTemp);
            IsTestFinished = true;
        }

        void ResetFrameTime()
        {
            m_StartTime = Time.time;
            m_StartFrameCount = Time.renderedFrameCount;
        }
    }
}
