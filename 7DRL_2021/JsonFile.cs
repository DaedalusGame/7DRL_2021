using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    abstract class JsonFile
    {
        protected string Filename;
        protected JObject Json = new JObject();

        public bool Exists => File.Exists(Filename);

        public JsonFile(string filename)
        {
            Filename = filename;
        }

        public void WriteToFile()
        {
            //TODO: make extra sure files don't get voided on error

            using (StreamWriter file = File.CreateText(Filename))
            using (JsonTextWriter writer = new JsonTextWriter(file)
            {
                Formatting = Formatting.Indented,
            })
            {
                Json.WriteTo(writer);
            }
        }

        public void ReadFromFile()
        {
            using (StreamReader file = File.OpenText(Filename))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                Json = (JObject)JToken.ReadFrom(reader);
            }
        }

        public abstract void WriteToJson();

        public abstract void ReadFromJson();

        public void Reload()
        {
            ReadFromFile();
            ReadFromJson();
        }

        public void Flush()
        {
            WriteToJson();
            WriteToFile();
        }
    }

    class OptionsFile : JsonFile
    {
        public OptionsFile(string filename) : base(filename)
        {
        }

        public override void WriteToJson()
        {
            Json["masterVolume"] = SoundLoader.MasterVolume;
            Json["soundVolume"] = SoundLoader.SoundMasterVolume;
            Json["musicVolume"] = SoundLoader.MusicMasterVolume;
        }

        public override void ReadFromJson()
        {
            SoundLoader.MasterVolume = (float)Json["masterVolume"];
            SoundLoader.SoundMasterVolume = (float)Json["soundVolume"];
            SoundLoader.MusicMasterVolume = (float)Json["musicVolume"];
        }
    }
}
