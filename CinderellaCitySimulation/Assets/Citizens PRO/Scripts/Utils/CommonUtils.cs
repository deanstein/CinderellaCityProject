using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public static class CommonUtils
{
    // random distribution
    public static int[] GetRandomPrefabIndexes(int numRequired, ref GameObject[] peoplePrefabs)
    {
        //int[] result = new int[numRequired];
        List<int> r = new List<int>();
        List<GameObject> prfb = new List<GameObject>(peoplePrefabs);
        prfb.Shuffle();
        peoplePrefabs = prfb.ToArray();
        for (int i = 0, e = 0; i < numRequired; i++)
        {
            r.Add(e < peoplePrefabs.Length ? e++ : e = 0);
        }
        r.Shuffle();
        return r.ToArray();
        //if (peoplePrefabs.Length < 3)
        //{
        //    for (int i = 0; i < result.Length; i++)
        //    {
        //        result[i] = UnityEngine.Random.Range(0, peoplePrefabs.Length);
        //    }
        //    return result;
        //}

        //int numToPick = numRequired;

        //List<int> tmpPrefabList = new List<int>();
        //while (tmpPrefabList.Count < numRequired)
        //{
        //    var rndList = Enumerable.Range(0, peoplePrefabs.Length).ToList();
        //    rndList.Shuffle();
        //    tmpPrefabList.AddRange(rndList);
        //}
        //tmpPrefabList.RemoveRange(numRequired, tmpPrefabList.Count - numRequired);

        //for (int leftCount = tmpPrefabList.Count; leftCount > 0; leftCount--)
        //{
        //    float pickValue = (float)numToPick / (float)leftCount;
        //    if (UnityEngine.Random.value <= pickValue)
        //    {
        //        numToPick--;
        //        result[numToPick] = tmpPrefabList[leftCount - 1];
        //        if (numToPick == 0) break;
        //    }
        //}
        //return result;
    } 
}
