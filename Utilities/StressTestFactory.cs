using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StressTestFactory
{
    private static Stack<GameObject> objectStack = new Stack<GameObject>();
    
	static int height = 10;
	static int width = 20;

	static float spacing = 1.5f;

    public static void SpawnObjects(int count, GameObject objectToSpawn)
    {
		// use the prototype object's original position as the grid offset.
		float xOffset = objectToSpawn.transform.position.x;
		float yOffset = objectToSpawn.transform.position.y;
        float zOffset = objectToSpawn.transform.position.z;

        for (int i = 0; i < count; i++)
        {
			var c = objectStack.Count + 1; 
            var gameObject = GameObject.Instantiate(objectToSpawn);
            gameObject.transform.localPosition = new Vector3((c % width + xOffset) * spacing, (((c / width) % height) + yOffset) * spacing, (c / (height * width) + zOffset) * spacing);
            objectStack.Push(gameObject);
        }
    }

    public static void DestroyObjects(int count)
    {
        for (int i = 0; i < count; i++)
        { 
            if (objectStack.Count == 0)
                break;
			GameObject.Destroy(objectStack.Pop());
        }
    }

	public static int GetObjectCount()
	{
		return objectStack.Count;
	}
}
