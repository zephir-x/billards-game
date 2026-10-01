using System;
using System.Collections.Generic;
using Raylib_cs;
using BilliardsGame.Interfaces;

namespace BilliardsGame.App
{
    public class AudioManager : IAudioManager
    {
        private readonly Dictionary<string, Music> _bgmTracks = new();
        private readonly Dictionary<string, Sound> _sfxTracks = new();
        
        private readonly Dictionary<string, float> _bgmCurrentVolumes = new();
        private string _targetBgmName = "";
        
        // Fading controls
        private const float MaxBgmVolume = 0.25f; // Lowered background music volume
        private const float FadeSpeed = 0.5f;     // Volume per second

        public void Initialize()
        {
            Raylib.InitAudioDevice();
        }

        public void Deinitialize()
        {
            foreach (var music in _bgmTracks.Values)
            {
                Raylib.UnloadMusicStream(music);
            }
            _bgmTracks.Clear();

            foreach (var sound in _sfxTracks.Values)
            {
                Raylib.UnloadSound(sound);
            }
            _sfxTracks.Clear();

            Raylib.CloseAudioDevice();
        }

        public void Update()
        {
            float dt = Raylib.GetFrameTime();
            foreach (var kvp in _bgmTracks)
            {
                string name = kvp.Key;
                Music music = kvp.Value;
                
                bool isTarget = (name == _targetBgmName);
                float targetVol = isTarget ? MaxBgmVolume : 0f;
                float currentVol = _bgmCurrentVolumes[name];

                if (currentVol < targetVol)
                {
                    currentVol += FadeSpeed * dt;
                    if (currentVol > targetVol) currentVol = targetVol;
                }
                else if (currentVol > targetVol)
                {
                    currentVol -= FadeSpeed * dt;
                    if (currentVol < 0f) currentVol = 0f;
                }

                _bgmCurrentVolumes[name] = currentVol;
                Raylib.SetMusicVolume(music, currentVol);

                if (currentVol > 0f)
                {
                    if (!Raylib.IsMusicStreamPlaying(music))
                    {
                        Raylib.ResumeMusicStream(music);
                        if (!Raylib.IsMusicStreamPlaying(music))
                        {
                            Raylib.PlayMusicStream(music);
                        }
                    }
                    Raylib.UpdateMusicStream(music);
                }
                else
                {
                    if (Raylib.IsMusicStreamPlaying(music))
                    {
                        Raylib.PauseMusicStream(music);
                    }
                }
            }
        }

        public void LoadBGM(string name, string relativePath)
        {
            if (!_bgmTracks.ContainsKey(name))
            {
                string fullPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, relativePath);
                var music = Raylib.LoadMusicStream(fullPath);
                _bgmTracks[name] = music;
                _bgmCurrentVolumes[name] = 0f;
                Raylib.SetMusicVolume(music, 0f);
            }
        }

        public void LoadSFX(string name, string relativePath)
        {
            if (!_sfxTracks.ContainsKey(name))
            {
                string fullPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, relativePath);
                var sound = Raylib.LoadSound(fullPath);
                _sfxTracks[name] = sound;
            }
        }

        public void PlayBGM(string name, float volume = 0.5f) // Ignoring manual volume for crossfader logic
        {
            _targetBgmName = name;
        }

        public void StopBGM(string name)
        {
            if (_targetBgmName == name) _targetBgmName = "";
        }

        public void PlaySFX(string name, float volume = 1.0f)
        {
            if (_sfxTracks.TryGetValue(name, out var sound))
            {
                Raylib.SetSoundVolume(sound, volume);
                Raylib.PlaySound(sound);
            }
        }

        public void PlayCushionHit(float impactForce)
        {
            float volume = Math.Clamp(impactForce / 300f, 0.1f, 1.0f);
            PlaySFX("cushion", volume);
        }

        public void PlayCueHit(float power)
        {
            float volume = Math.Clamp(power + 0.2f, 0.2f, 1.0f);
            PlaySFX("cue_hit", volume);
        }

        public void PlayUIClick()
        {
            PlaySFX("ui_click", 0.20f);
        }

        public void PlayUIHover()
        {
            PlaySFX("ui_click", 0.05f);
        }

        public void PlayGameOver()
        {
            PlaySFX("game_over", 0.3f);
        }

        public void PlayCollision(float impactForce)
        {
            float volume = Math.Clamp(impactForce / 300f, 0.1f, 1.0f);
            PlaySFX("collision", volume);
        }

        public void PlayPocketed()
        {
            PlaySFX("pocket", 1.0f);
        }
    }
}
