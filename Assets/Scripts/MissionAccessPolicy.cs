namespace Meridian
{
    /// <summary>Explicit pre-production setting. Applies to Editor AND device builds; not an achievement unlock.</summary>
    public static class MissionAccessPolicy
    {
        // Set false for production to restore per-vessel sequential access, without deleting test results.
        public const bool AllScenariosUnlockedForTesting = true;
    }
}
