using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Combat;

public class CombatTest : MonoBehaviour
{
    public Squad blufor, opfor;
    public Engagement engagement;

    // Start is called before the first frame update
    void Start()
    {
        long[] opforUnits = new long[]  {100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
        long[] bluforUnits = new long[] {100, 0, 0, 0, 0, 0, 0, 0, 0, 1000, 0, 0};

        opfor = new Squad(opforUnits);
        blufor = new Squad(bluforUnits);

        engagement = new Engagement(blufor, opfor);

        EngagementHistory history = engagement.ResolveEngagement();

        Debug.Log(history.winner);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
