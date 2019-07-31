using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar.Combat;

public class CombatTest : MonoBehaviour
{
    public Squad blufor, opfor;
    public Engagement engagement;
    public long[] opforUnits = new long[] { 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public long[] bluforUnits = new long[] { 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKey)
        {
            float time = Time.time;
            opfor = new Squad(opforUnits);
            blufor = new Squad(bluforUnits);

            engagement = new Engagement(blufor, opfor);

            EngagementHistory history = engagement.ResolveEngagement();

            Debug.Log(history.winner + " " + history.bluforHistory.Length + " " + (Time.time - time));
        }
    }
}
