using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using Economy;
using Newtonsoft.Json;
using RandomNameGen;
using UnityEngine;
using Worker;
using Random = System.Random;

namespace Game
{
    public sealed class PlayerState
    {
        public const int WorkerCap = 5;
        public readonly List<Worker.Worker> Workers;
        public int MoneyPercentageSkillLevel;
        public PlayerState()
        {
            this.MoneyPercentageSkillLevel = 1;
            Random rand = new Random();
            RandomName nameGen = new RandomName(rand);
            Workers = new List<Worker.Worker>
            {
                new Worker.Worker() {Efficiency = 2, Name = nameGen.Generate((Sex) rand.Next(0,2), rand.Next(0,2)), Cost = 2000},
            };
        }

        public static string GetAccessToken()
        {
            try
            {
                if (!File.Exists(Application.persistentDataPath + "/token.jwt")) return null;
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/token.jwt", FileMode.Open);
                var token = (string)bf.Deserialize(file);
                file.Close();
                return token;
            }
            catch (Exception e)
            {
                File.Delete(Application.persistentDataPath + "/token.jwt");
                Console.WriteLine(e);
                throw;
            }

        }
        public static void DeleteAccessToken()
        {
            File.Delete(Application.persistentDataPath + "/token.jwt");
        }
        public static string GetUserName()
        {
            var payloadType = new { username = ""};
            var token = GetAccessToken();
            if (token == null) return null;
                var payload = JWT.JsonWebToken.Decode(token,"",false);
                return JsonConvert.DeserializeAnonymousType(payload, payloadType).username;
        }
        public static void SaveAccessToken(string token)
        {
            if (token == string.Empty) return;
            BinaryFormatter bf = new BinaryFormatter();
            if(File.Exists(Application.persistentDataPath + "/token.jwt"))
                File.Delete(Application.persistentDataPath + "/token.jwt");
            FileStream file = File.Create(Application.persistentDataPath + "/token.jwt");
            bf.Serialize(file, token);
        }
    }
}