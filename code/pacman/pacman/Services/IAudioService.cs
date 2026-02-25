namespace pacman.Services;

public interface IAudioService
{
    bool IsMuted { get; set; }
    void PlayGameStart();
    void PlayEatDot();
    void PlayLoseLife();
    void PlayGameOver();
    void PlayWin();
    void PlayFrightened();
    void StopFrightened();
    void PlayUiClick();
}
