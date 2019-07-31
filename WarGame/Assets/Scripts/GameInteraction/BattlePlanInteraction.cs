using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar.ClientSide;
using IslesOfWar.Combat;

public class BattlePlanInteraction : Interaction
{
    public enum Mode
    {
        ATTACK,
        DEFEND
    }

    public Mode mode = Mode.ATTACK;
    public string[] battleButtonTypes = new string[] { "SquadMarker" };
    public GameObject markerPrefab;
    public GameObject trailPointPrefab;
    public Vector3 spawnOffset;
    public IslandStats islandStats;

    private List<GameObject> squadMarkers;
    private int currentSquad = -1;
    private SquadMarker selectedSquad;
    private BattlePlan battlePlan;
    private AttackPlanner attackPlanner;
    private DefensePlanner defensePlanner;

    private void Start()
    {
        Squad[] tempSquads = new Squad[]
        {
            new Squad(new long[] { 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }),
            new Squad(new long[] { 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })
        };

        attackPlanner = new AttackPlanner(tempSquads);
        defensePlanner = new DefensePlanner(tempSquads);
    }

    private void Update()
    {
        bool clicked = Input.GetButtonDown("Fire1");
        bool affected = Input.GetButtonDown("Fire2");
        WorldButtonCheck(clicked || affected);

        if (clicked || affected)
        {
            string peeked = PeekButtonType();

            if (peeked == battleButtonTypes[0])
            {
                if (currentSquad >= 0)
                    CloseCurrentSquad();

                selectedSquad = selectedButton.GetComponent<SquadMarker>();
                currentSquad = selectedSquad.squad;

                if (mode == Mode.DEFEND)
                    ViewCurrentDefenseSquad();
                else if (mode == Mode.ATTACK)
                    ViewCurrentAttackSquad();
            }

            if (currentSquad >= 0 && peeked == buttonTypes[3] && affected)
            {
                int position = selectedButton.GetComponent<IndexedNavigationButton>().index;
                Debug.Log(position);

                if (mode == Mode.ATTACK)
                {
                    PlanAttack(position);
                }
                else if (mode == Mode.DEFEND)
                {
                    PlanDefense(position);
                }
            }
        }

        if (Input.GetButtonDown("Boost"))
        {
            battlePlan = defensePlanner.defensePlan;
        }
    }

    public void ToggleReactCommand()
    {
        if (mode == Mode.DEFEND)
            defensePlanner.ToggleSquadReactCommand(currentSquad);
    }

    public void SetMode(bool isAttack)
    {
        if (isAttack)
        {
            mode = Mode.ATTACK;
        }
        else
        {
            mode = Mode.DEFEND;
        }
    }

    public void CloseCurrentSquad()
    {
        battlePlan = defensePlanner.defensePlan;

        for (int p = 0; p < battlePlan.squadPositions[currentSquad].Length; p++)
        {
            selectedSquad.RemovePosition(battlePlan.squadPositions[currentSquad][p]);
        }

        selectedSquad.Reset();
    }

    public void ViewCurrentAttackSquad()
    {
        battlePlan = attackPlanner.attackPlan;

        for (int p = 0; p < battlePlan.squadPositions[currentSquad].Length; p++)
        {
            int tileIndex = battlePlan.squadPositions[currentSquad][p];

            if (tileIndex != -1)
            {
                selectedSquad.AddNewPosition(tileIndex, spawnOffset + islandStats.hexTiles[tileIndex].position, trailPointPrefab);
            }
        }
    }

    public void ViewCurrentDefenseSquad()
    {
        battlePlan = defensePlanner.defensePlan;

        for (int p = 0; p < battlePlan.squadPositions[currentSquad].Length; p++)
        {
            int tileIndex = battlePlan.squadPositions[currentSquad][p];
            selectedSquad.AddNewPosition(tileIndex, spawnOffset + islandStats.hexTiles[tileIndex].position, trailPointPrefab);
        }
    }

    void PlanAttack(int position)
    {
        if(attackPlanner.AddMove(currentSquad, position))
            selectedSquad.AddNewPosition(position, spawnOffset + selectedButton.transform.position, trailPointPrefab);
    }
    
    void PlanDefense(int position)
    {
        defensePlanner.ToggleDefenseZone(currentSquad, position);

        if (defensePlanner.GetStateOfPosition(currentSquad, position))
            selectedSquad.AddNewPosition(position, spawnOffset + selectedButton.transform.position, trailPointPrefab);
        else
            selectedSquad.RemovePosition(position);
    }
}
