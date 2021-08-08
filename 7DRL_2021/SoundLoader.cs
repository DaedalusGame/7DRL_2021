using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace _7DRL_2021
{
    class SoundReference
    {
        public string FileName;
        public SoundEffect Sound;

        public SoundReference(string fileName)
        {
            FileName = fileName;
        }

        public SoundEffectInstance Play(float volume, float pitch, float pan)
        {
            var instance = Sound.CreateInstance();
            instance.Volume = SoundLoader.MasterVolume * SoundLoader.SoundMasterVolume * volume;
            instance.Pitch = pitch;
            instance.Pan = pan;
            instance.Play();
            return instance;
        }

        public SoundEffectInstance CreateInstance()
        {
            var instance = Sound.CreateInstance();
            instance.Volume = SoundLoader.MasterVolume * SoundLoader.SoundMasterVolume;
            return instance;
        }
    }

    class MusicEffect
    {
        SoundEffectInstance Music;
        public LerpFloat Volume = new LerpFloat(0);
        public LerpFloat Pitch = new LerpFloat(0);
        public LerpFloat Pan = new LerpFloat(0);

        public MusicEffect(SoundReference sound)
        {
            Music = sound.CreateInstance();
            Music.IsLooped = true;
        }

        public void Play()
        {
            Music.Play();
        }

        public void Pause()
        {
            Music.Pause();
        }

        public void Stop(bool immediate = true)
        {
            Music.Stop(immediate);
        }

        public void Resume()
        {
            Music.Resume();
        }

        public void Update()
        {
            Volume.Update();
            Pitch.Update();
            Pan.Update();
            Music.Volume = SoundLoader.MasterVolume * SoundLoader.MusicMasterVolume * Volume;
            Music.Pitch = Pitch;
            Music.Pan = Pan;
        }
    }

    class SoundLoader
    {
        public static float MasterVolume = 1.0f;
        public static float SoundMasterVolume = 0.5f;
        public static float MusicMasterVolume = 1f;

        public static Dictionary<string, SoundReference> Sounds = new Dictionary<string, SoundReference>();

        public static List<SoundReference> AllSounds = new List<SoundReference>();

        public static SoundReference AddSound(string filename)
        {
            if (Sounds.ContainsKey(filename))
                return Sounds[filename];

            var rval = new SoundReference(filename);
            Sounds.Add(filename, rval);
            AllSounds.Add(rval);
            Load(rval);
            return rval;
        }

        public static void Update(GameTime gameTime)
        {

        }

        private static void Load(SoundReference reference)
        {
            FileInfo fileinfo = new FileInfo(reference.FileName);

            if (fileinfo == null || !fileinfo.Exists)
                return;

            FileStream stream = fileinfo.OpenRead();
            reference.Sound = SoundEffect.FromStream(stream);
            stream.Close();
        }
    }
}
