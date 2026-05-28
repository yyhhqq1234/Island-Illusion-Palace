using System.Collections.Generic;

namespace IIP.Ending
{
    public interface IEndingService
    {
        EndingType DetermineEnding(
            int coreFragments, int memoryFragments, float finalBurden,
            bool isHardcoreMode, bool qteSuccess, bool choseAcceptReality,
            bool purificationDone, int forbiddenAlchemyCount);
        string GetEndingHint();
        void TriggerEnding(EndingType type);
    }
}
