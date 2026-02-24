namespace pacman.Services;

public interface IAudioService
{
    void PlayGameStart();
    void PlayEatDot();
    void PlayLoseLife();
    void PlayGameOver();
    void PlayWin();
    void PlayFrightened();
    void StopFrightened();
    void PlayUiClick();
}
