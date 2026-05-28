public interface ISaveService
{
    bool SaveSlotExists(int slot);
    void SaveGame(int slot);
    void LoadGame(int slot);
    void ApplyPermanentProgress(PermanentProgressData progress);
    void StartNewWeek();
}
