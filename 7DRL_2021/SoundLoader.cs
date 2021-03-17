using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            instance.Volume = volume;
            instance.Pitch = pitch;
            instance.Pan = pan;
            instance.Play();
            return instance;
        }

        public SoundEffectInstance CreateInstance()
        {
            return Sound.CreateInstance();
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
            Music.Volume = Volume;
            Music.Pitch = Pitch;
            Music.Pan = Pan;
        }
    }

    class SoundLoader
    {
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
