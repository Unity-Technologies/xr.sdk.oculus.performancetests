﻿using System.Collections;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.TestTools;

public class OculusStatsTests : OculusPerformanceTestBase
{
    protected SampleGroupDefinition[] ProfilerStats = {
        new SampleGroupDefinition("Camera.Render"),
        new SampleGroupDefinition("Render.Mesh")
    };

    [Version("5")]
    [UnityTest, Performance]
    [Category("XR")]
    [Category("Performance")]
    [Category("OCULUS_XRSDK")]
    [Timeout(120000)]
    [PerformanceTestSource("StatsTests")]
    public IEnumerator StatsTest(string scene)
    {
        yield return SetupTestRun(scene);
        using (Measure.Scope())
        {
            using (Measure.Frames().Scope())
            {
                using (Measure.ProfilerMarkers(ProfilerStats))
                {
                    yield return new MonoBehaviourTest<StatsTestMonobehavior>();
                }
            }
        }
        yield return TearDownTestRun(scene);
    }

    public class StatsTestMonobehavior : MonoBehaviour, IMonoBehaviourTest
    {
        private readonly SampleGroupDefinition GPUUtilization = new SampleGroupDefinition("GPU Utilization", SampleUnit.None);
        private readonly SampleGroupDefinition CPUUtilizationAverage = new SampleGroupDefinition("CPU Utilization - Average", SampleUnit.None);
        private readonly SampleGroupDefinition CPUUtilizationWorst = new SampleGroupDefinition("CPU Utilization - Worst", SampleUnit.None);
        private readonly SampleGroupDefinition CompositorGPUTIme = new SampleGroupDefinition("Compositor GPU Time", SampleUnit.Millisecond);
        private readonly SampleGroupDefinition AppGPUTime = new SampleGroupDefinition("App GPU Time", SampleUnit.Millisecond);

        public int numSampleFrames = 1000;        
        public bool IsTestFinished { get; set; }

        IEnumerator Start()
        {
            OculusStats.PerfMetrics.EnablePerfMetrics(true);
            for (int i = 0; i < numSampleFrames; i++)
            {
                yield return 0;
            }
            IsTestFinished = true;
        }

        void Update()
        {
            if (!IsTestFinished)
            {
                Measure.Custom(GPUUtilization, OculusStats.PerfMetrics.GPUUtilization);
                Measure.Custom(CPUUtilizationAverage, OculusStats.PerfMetrics.CPUUtilizationAverage);
                Measure.Custom(CPUUtilizationWorst, OculusStats.PerfMetrics.CPUUtilizationWorst);
                Measure.Custom(CompositorGPUTIme, OculusStats.PerfMetrics.CompositorGPUTime * 1000);
                Measure.Custom(AppGPUTime, OculusStats.PerfMetrics.AppGPUTime * 1000);
            }
        }
    }
}