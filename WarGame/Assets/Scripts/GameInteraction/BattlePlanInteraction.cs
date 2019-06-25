using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePlanInteraction : Interaction
{
    public enum Mode
    {
        ATTACK,
        DEFEND
    }

    public Mode mode = Mode.ATTACK;

    private void Update()
    {
        bool clicked = Input.GetButtonDown("Fire1");
        WorldButtonCheck(clicked);

        if (clicked)
        {
            if (mode == Mode.ATTACK)
                PlanAttack();
            else if (mode == Mode.DEFEND)
                PlanDefense();
        }
    }

    void PlanAttack()
    {
    }

    void PlanDefense()
    {

    }
}
