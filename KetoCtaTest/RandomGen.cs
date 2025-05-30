/*
 * Copyright (c) 2015-2025, Jillian England
 */

namespace StatTest;

public static class RandomGen
{
    private static readonly Random Lcg = new();

    #region Random Number Tools (It is better to use one sequence than several, using several will overlap sooner, be less 'random', than just using one.)

    public static int Next()
    {
        return Lcg.Next();
    }

    public static int Next(int maxValue)
    {
        return Lcg.Next(maxValue);
    }

    public static int Next(int minValue, int maxValue)
    {
        return Lcg.Next(minValue, maxValue);
    }

    public static bool NextBool()
    {
        return Next(2) == 1 ? true : false;
    }

    public static double NextDouble()
    {
        return Lcg.NextDouble();
    }

    #endregion
}
