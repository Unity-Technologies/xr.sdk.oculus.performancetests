using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.SceneManagement;

[AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class PerformanceTestSource : Attribute, ITestBuilder
{
    private string TestSceneRoot { get; set; }
    
    /// <summary>
    /// This attribute is used to generate test cases for a given perforamnce test.
    /// It searches through the list of scenes added to the build settings and generates
    /// test cases based on scenes in directory specified by the user.
    /// </summary>
    /// <param name="testSceneRootDirectory">The directory to search for test case scenes</param>
    public PerformanceTestSource(string testSceneRootDirectory)
    {
        this.TestSceneRoot = testSceneRootDirectory;
    }

    public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test suite)
    {
        List<TestMethod> results = new List<TestMethod>();
        try
        {
            foreach (var testScene in GetScenesForPerfTest(this.TestSceneRoot))
            {
                var test = new TestMethod(method, suite)
                {
                    parms = new TestCaseParameters(new object[] {testScene})
                };
                test.parms.ApplyToTest(test);
                test.Name = System.IO.Path.GetFileNameWithoutExtension(testScene.ToString());
                test.FullName = String.Format("{0}.{1}", test.FullName, test.Name);

                results.Add(test);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to generate performance testcases!");
            Debug.LogException(ex);
            throw;
        }

        suite.Properties.Set("TestType", this.TestSceneRoot);

        Console.WriteLine("Generated {0} performance test cases.", results.Count);
        return results;
    }
    
    protected static IEnumerable GetScenesForPerfTest(string directory)
    {
        List<string> scenesInBuild = new List<string>();
        for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            if (scenePath.Contains(directory))
            {
                yield return Path.GetFileNameWithoutExtension(scenePath);
            }
        }
    }
}