namespace FO4Down
{
    public enum FO4DowngraderStep
    {
        LookingForFallout4Path,
        UserSettings,
        LoginToSteam,
        DownloadDepotFiles,
        DownloadGameDepotFiles,
        DownloadCreationKitDepotFiles,
        DeleteNextGenFiles,
        CopyDepotFiles,
        Finished,
        DeleteCreationClubData,
        ApplyLanguage,
        Patch,
    }
}
