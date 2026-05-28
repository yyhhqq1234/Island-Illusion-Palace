using System.Collections.Generic;

public interface IMemoryFragmentService
{
    List<MemoryFragment> collectedFragments { get; }
    List<MemoryFragment> activatedFragments { get; }
    int maxFragmentSlots { get; }
    void CollectFragment(MemoryFragmentType fragmentType);
    bool ActivateFragment(MemoryFragment fragment);
    bool DeactivateFragment(MemoryFragment fragment);
    void UpdateResonanceEffects();
    int GetFragmentCount();
    int GetActivatedFragmentCount();
}
