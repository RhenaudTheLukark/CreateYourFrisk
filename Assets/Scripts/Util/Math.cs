using System;
public static class Math
{
    private static Random rng = new Random(); // unityengine's random can't be used outside of its main thread, so just to be safe here

    public static int mod(int x, int mod) { return ((x % mod) + mod) % mod; }

    public static float mod(float x, float mod) { return ((x % mod) + mod) % mod; }

    public static int randomRange(int minInclusive, int maxExclusive) { return rng.Next(minInclusive, maxExclusive); }
}