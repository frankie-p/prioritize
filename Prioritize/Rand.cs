namespace Prioritize;

public static class Rand
{
    private static readonly Random random = new();

    public static double Generate(double mean, double stdDev)
    {
        // Box-Muller transform to generate random numbers from a normal distribution
        var u1 = 1.0 - random.NextDouble();
        var u2 = 1.0 - random.NextDouble();
        var z0 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);

        // Apply mean and standard deviation
        return mean + stdDev * z0;
    }

    public static int Next(int max)
        => random.Next(max);
}
