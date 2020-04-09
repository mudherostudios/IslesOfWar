using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IslesOfWar.ClientSide
{
    public class IslandList : MonoBehaviour
    {
        public WorldNavigator navigator;
        public Dropdown islandListDropdown;
        private CommunicationInterface comms;

        private void Start()
        {
            comms = navigator.communicationScript;
        }

        List<string> GetIslandNames(string playerName)
        {
            State state = comms.state;
            List<string> islandNames = new List<string>();

            if (state.players.ContainsKey(playerName))
            {
                for (int i = 0; i < state.players[playerName].islands.Count; i++)
                {
                    string technicalName = state.players[playerName].islands[i];
                    string islandName = SaveLoad.GetIslandName(technicalName);
                    islandNames.Add(islandName);
                }
            }

            return islandNames;
        }

        public void LoadIslandList()
        {
            List<string> islandNames = GetIslandNames(comms.player);
            islandListDropdown.ClearOptions();
            if (islandNames.Count > 0)
                islandListDropdown.AddOptions(islandNames);
        }

        public void GotoSelectedIsland()
        {
            navigator.GotoIslandInList(islandListDropdown.value);
        }
    }
}