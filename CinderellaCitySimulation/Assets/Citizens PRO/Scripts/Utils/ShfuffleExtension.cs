using System.Collections.Generic;

// simple shuffle realization
public static class ShfuffleExtension {
    private static readonly System.Random RandomGenerator = new System.Random();

    public static void Shuffle<T>(this IList<T> shuffleList)
    {
        int count = shuffleList.Count;
        while (count > 1)
        {
            count--;
            int randomValue = RandomGenerator.Next(count + 1);
            T value = shuffleList[randomValue];
            shuffleList[randomValue] = shuffleList[count];
            shuffleList[count] = value;
        }
    }
}