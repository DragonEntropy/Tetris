using System;
using System.Media;

namespace Tetris
{
    internal class MusicPlayer
    {
        string bgmSource = "Assets\bgm.mp3";

        public void PlayBackgroundMusic()
        {
            SoundPlayer backgroundPlayer = new SoundPlayer(bgmSource);
            backgroundPlayer.Play();
        }
    }
}
