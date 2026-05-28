using UnityEngine;

public interface IGameEntry
{
    bool IsGameRunning { get; }
    bool IsPaused { get; }
    void PauseGame();
    void ResumeGame();
    void RestartGame();
    void QuitGame();
}

public interface IServiceLocator
{
    T GetService<T>() where T : class;
    void RegisterService<T>(T service) where T : class;
}
