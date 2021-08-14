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
    class AsyncCheck
    {
        public bool Done { get; private set; }
        
        public void SetDone()
        {
            Done = true;
        }
    }

    abstract class JsonFile
    {
        protected string Filename;
        protected JObject Json = new JObject();

        public bool Exists => File.Exists(Filename);

        public JsonFile(string filename)
        {
            Filename = filename;
        }

        private void CreateDirectory()
        {
            var fileInfo = new FileInfo(Filename);
            if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();
        }

        private void WriteToFile()
        {
            //TODO: make extra sure files don't get voided on error
            CreateDirectory();

            using (StreamWriter file = File.CreateText(Filename))
            using (JsonTextWriter writer = new JsonTextWriter(file)
            {
                Formatting = Formatting.Indented,
            })
            {
                Json.WriteTo(writer);
            }
        }

        private void ReadFromFile()
        {
            using (StreamReader file = File.OpenText(Filename))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                Json = (JObject)JToken.ReadFrom(reader);
            }
        }

        private Task WriteToFileAsync()
        {
            //TODO: make extra sure files don't get voided on error
            CreateDirectory();

            Task task;

            using (FileStream file = new FileStream(Filename, FileMode.Create, FileAccess.Write, FileShare.Write, 4096, true))
            {
                task = Json.WriteToAsync(new JsonTextWriter(new StreamWriter(file))
                {
                    Formatting = Formatting.Indented,
                });
            }

            return task;
        }

        private Task<JToken> ReadFromFileAsync()
        {
            Task<JToken> task;

            using (FileStream file = new FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            { 
                task = JToken.ReadFromAsync(new JsonTextReader(new StreamReader(file)));
            }

            return task;
        }

        public abstract void WriteToJson();

        public abstract void ReadFromJson();

        public void Flush()
        {
            WriteToJson();
            WriteToFile();
        }

        public void Reload()
        {
            ReadFromFile();
            ReadFromJson();
        }

        public AsyncCheck FlushAsync()
        {
            AsyncCheck loading = new AsyncCheck();

            WriteToJson();
            var task = WriteToFileAsync();

            task.ContinueWith(t =>
            {
                loading.SetDone();
            });

            return loading;
        }

        public AsyncCheck ReloadAsync()
        {
            AsyncCheck loading = new AsyncCheck();

            var task = ReadFromFileAsync();

            task.ContinueWith(t =>
            {
                Json = (JObject)t.Result;
                ReadFromJson();
                loading.SetDone();
            });

            return loading;
        }
    }

    class OptionsFile : JsonFile
    {
        public OptionsFile(string filename) : base(filename)
        {
        }

        public override void ReadFromJson()
        {
            SoundLoader.MasterVolume = (float)Json["masterVolume"];
            SoundLoader.SoundMasterVolume = (float)Json["soundVolume"];
            SoundLoader.MusicMasterVolume = (float)Json["musicVolume"];
        }

        public override void WriteToJson()
        {
            Json["masterVolume"] = SoundLoader.MasterVolume;
            Json["soundVolume"] = SoundLoader.SoundMasterVolume;
            Json["musicVolume"] = SoundLoader.MusicMasterVolume;
        }
    }

    class RunStats
    {
        public int Level;
        public int Score;
        public List<Card> Cards = new List<Card>();
        public int Kills;
        public int Gibs;
        public int Splats;
        public int HeartsRipped;
        public int HeartsEaten;
        public int RatsHunted;
        public int CardsCrushed;
        public GameOverType GameOverType;

        public RunStats()
        {
        }
    }

    class HighscoreRunFile : JsonFile
    {
        public RunStats Score;

        public HighscoreRunFile(string filename, RunStats score) : base(filename)
        {
            Score = score;
        }

        public override void ReadFromJson()
        {
            Score.Level = (int)Json["level"];
            Score.Score = (int)Json["score"];
            Score.Cards = new List<Card>(Json["cards"].Select(x => (string)x).Select(Card.Get));
            Score.Kills = (int)Json["kills"];
            Score.Gibs = (int)Json["gibs"];
            Score.Splats = (int)Json["splats"];
            Score.HeartsRipped = (int)Json["heartsRipped"];
            Score.HeartsEaten = (int)Json["heartsEaten"];
            Score.RatsHunted = (int)Json["ratsHunted"];
            Score.CardsCrushed = (int)Json["cardsCrushed"];
            Score.GameOverType = GameOverType.Get((string)Json["gameover"]);
        }

        public override void WriteToJson()
        {
            Json["level"] = Score.Level;
            Json["score"] = Score.Score;
            Json["cards"] = new JArray(Score.Cards.Select(card => card.ID));
            Json["kills"] = Score.Kills;
            Json["gibs"] = Score.Gibs;
            Json["splats"] = Score.Splats;
            Json["heartsRipped"] = Score.HeartsRipped;
            Json["heartsEaten"] = Score.HeartsEaten;
            Json["ratsHunted"] = Score.RatsHunted;
            Json["cardsCrushed"] = Score.CardsCrushed;
            Json["gameover"] = Score.GameOverType.ID;
        }
    }
}
