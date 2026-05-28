using System.Collections.Generic;

public interface IMemoryFragmentService
{
    List<MemoryFragment> CollectedFragments { get; }
    List<MemoryFragment> ActivatedFragments { get; }
    int MaxFragmentSlots { get; }
    void CollectFragment(MemoryFragmentType fragmentType);
    bool ActivateFragment(MemoryFragment fragment);
    bool DeactivateFragment(MemoryFragment fragment);
    void UpdateResonanceEffects();
    int GetFragmentCount();
    int GetActivatedFragmentCount();
}
