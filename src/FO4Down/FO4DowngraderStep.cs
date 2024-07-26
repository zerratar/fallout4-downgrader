namespace FO4Down
{
    public enum FO4DowngraderStep
    {
        LookingForFallout4Path,
        UserSettings,
        LoginToSteam,
        DownloadPatchFiles,
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
