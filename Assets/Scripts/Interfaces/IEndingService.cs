using IIP.Ending;

public interface IEndingService
{
    EndingType DetermineEnding();
    string GetEndingHint();
}
