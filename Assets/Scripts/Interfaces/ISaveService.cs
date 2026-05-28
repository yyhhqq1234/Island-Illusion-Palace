// PermanentProgressData is defined in SaveSystem.cs (global namespace)
namespace IIP.SaveLoad
{
    public interface ISaveService
    {
        bool SaveSlotExists(int slot);
        void SaveGame(int slot);
        void LoadGame(int slot);
        PermanentProgressData GetPermanentProgress();
        void ApplyPermanentProgress(PermanentProgressData progress);
        void StartNewWeek();
    }
}
