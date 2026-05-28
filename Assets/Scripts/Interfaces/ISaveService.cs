public interface ISaveService
{
    bool SaveSlotExists(int slot);
    void SaveGame(int slot);
    void LoadGame(int slot);
    PermanentProgressData GetPermanentProgress();
    void ApplyPermanentProgress(PermanentProgressData progress);
    void StartNewWeek();
}
