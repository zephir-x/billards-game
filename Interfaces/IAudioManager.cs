using System;

namespace BilliardsGame.Interfaces
{
    /// <summary>
    /// Manages the registration and playback of music and sound effects.
    /// </summary>
    public interface IAudioManager
    {
        void Initialize();
        void Deinitialize();
        void Update(); // For streaming music buffers

        void LoadBGM(string name, string relativePath);
        void LoadSFX(string name, string relativePath);

        void PlayBGM(string name, float volume = 0.5f);
        void StopBGM(string name);

        void PlaySFX(string name, float volume = 1.0f);
        void PlayCollision(float impactForce);
        void PlayCushionHit(float impactForce);
        void PlayCueHit(float power);
        void PlayUIClick();
        void PlayUIHover();
        void PlayGameOver();
        void PlayPocketed();
    }
}
