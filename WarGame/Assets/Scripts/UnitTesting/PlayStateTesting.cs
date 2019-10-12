using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using IslesOfWar.Communication;
using IslesOfWar.GameStateProcessing;
using MudHero;
using MudHero.XayaCommunication;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class PlayStateTesting : MonoBehaviour
{
    public string pathToTestBlockData;
    public int blocksToGenerate = 1;
    public int lastHeight = 190, lastTime = 1572;
    public string nameData = "\\\"cairo\\\""; 
    public string moveData = "\\\"srch\\\":\\\"norm\\\"";
    public string[] blockData;

    private void Start()
    {
        GetTestData();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            PlayState();
        if (Input.GetKeyDown(KeyCode.M))
            GetMoveData();
        if (Input.GetKeyDown(KeyCode.G))
            Debug.Log(GenerateBlocks()[0]);
        if (Input.GetKeyDown(KeyCode.J))
            CheckJson();
    }

    void CheckJson()
    {
        Debug.Log(Validity.JSON("{\"message\"}"));
    }

    void GetTestData()
    {
        BlockDataGenerator gen = new BlockDataGenerator();
        if (File.Exists(pathToTestBlockData))
        {
            string jsonData = File.ReadAllText(pathToTestBlockData);
            blockData = gen.GetData(jsonData);
        }
    }

    string[] GenerateBlocks()
    {
        BlockDataGenerator gen = new BlockDataGenerator();
        gen.blocksToGenerate = blocksToGenerate;

        string json = gen.GenerateData(lastHeight, lastTime);

        return gen.GetData(json);
    }

    void GetMoveData()
    {
        BlockDataGenerator gen = new BlockDataGenerator();
        Debug.Log(gen.ConstructMove(nameData, moveData));
    }

    void PlayState()
    {
        Random.InitState(1337);
        string undoData = "";
        string updatedData = "";

        for (int b = 0; b < blockData.Length; b++)
        {
            if (blockData[b][0] != 'x')
            {
                string height = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<dynamic>(blockData[b])["height"]);
                string time = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<dynamic>(blockData[b])["timestamp"]);
                int.TryParse(height, out lastHeight);
                int.TryParse(time, out lastTime);
            }

            if(blockData[b][0] == 'x')
            {
                string xCount = blockData[b].Substring(1);
                int.TryParse(xCount, out blocksToGenerate);

                if (blocksToGenerate > 0)
                {
                    List<string> emptyBlocks = new List<string>(GenerateBlocks());

                    for (int e = 0; e < emptyBlocks.Count; e++)
                    {
                        undoData = Callback.PlayState(updatedData, emptyBlocks[e], undoData, out updatedData);
                    }

                    emptyBlocks.Clear();
                }
                
            }
            else
                undoData = Callback.PlayState(updatedData, blockData[b], undoData, out updatedData);
        }

        Debug.Log(updatedData);
    }
}
