using NugetForUnity;

namespace Editor
{
    public static class ContinuousIntegration
    {
        public static void RestoreNugetPackages()
        {
            NugetHelper.Restore();
        }
    }
}