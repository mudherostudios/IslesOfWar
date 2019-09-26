using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class BlockDataGenerator : MonoBehaviour
{
    public int blocksToGenerate = 1;

    string hex = "0123456789abcdef";
    string ascii = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    string block = "\"block\":";
    string hash = "\"hash\":";
    string height = "\"height\":";
    string parent = "\"parent\":";
    string seed = "\"rngseed\":";
    string timestamp = "\"timestamp\":";
    string moves = "\"moves\":[{}]";
    string token = "\"reqtoken\":";

    private int txidIncrementer = 0;
    private int lastHeight = 0;
    private int lastTime = 0;

    struct TestBlockData
    {
        public List<string> testData;
    }
    

    public string GenerateData(int _lastHeight, int _lastTime)
    {
        lastTime = _lastTime;
        lastHeight = _lastHeight;
        string[] blocks = new string[blocksToGenerate];

        for (int b = 0; b < blocksToGenerate; b++)
        {
            string hashValue = GetRandomString(hex, 64);
            int heightValue = (int)(Random.Range(1, 100) + lastHeight);
            lastHeight = heightValue;
            string parentValue = GetRandomString(hex, 64);
            string seedValue = GetRandomString(hex, 64);
            int timeValue = (int)(Random.Range(1, 1000) + lastTime);
            lastTime = timeValue;
            string tokenValue = GetRandomString(ascii, 32);
            string[] args = new string[] 
            {
                block, hash, hashValue, height, heightValue.ToString(),   
                parent, parentValue, seed, seedValue,
                timestamp, timeValue.ToString()
            };

            blocks[b] = string.Format("{0}{{{1}{2},{3}{4},{5}{6},{7}{8},{9}{10}}}", args);
            blocks[b] = string.Format("{{{0},{1},{2}{3}}}", blocks[b], moves, token, tokenValue);
        }

        TestBlockData test = new TestBlockData();
        test.testData = new List<string>(blocks);

        return JsonConvert.SerializeObject(test);
    }

    public string[] GetData(string jsonData)
    {
        TestBlockData data = JsonConvert.DeserializeObject<TestBlockData>(jsonData);
        return data.testData.ToArray();
    }

    public string ConstructMove(string _name, string _move)
    {
        string move = string.Format("\\\"move\\\":{{{0}}}", _move);
        string name = string.Format("\\\"name\\\":{0}", _name);
        string _out = string.Format("\\\"out\\\":{{\\\"{0}\\\":{1}}}", GetRandomString(ascii, 34, false), Random.Range(10.0f, 10000.0f));
        string txid = string.Format("\\\"txid\\\":\\\"{0}\\\"", GetRandomString(hex, 16, false));

        return string.Format("{{{0},{1},{2},{3}}}", move, name, _out, txid); 
    }

    string GetRandomString(string possible, int count)
    {
        return GetRandomString(possible, count, true);
    }

    string GetRandomString(string possible, int count, bool withQuotes)
    {
        string random = "";

        for (int c = 0; c < count; c++)
        {
            random += possible[Mathf.FloorToInt(Random.value*possible.Length)];
        }

        if (withQuotes)
            return string.Format("\"{0}\"", random);
        else
            return string.Format("{0}", random);
    }
}
