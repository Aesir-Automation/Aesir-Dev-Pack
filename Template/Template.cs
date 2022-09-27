using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Midgard;
using Midgard.API.Engine;

public class Template : Simulation
{
    // Define your spells, buffs, and debuffs here
    #region Spells
    private Spell? Example => Player?.Spell(123);

    private Buff? ExampleBuff => Player?.Buff(123);

    private Debuff? ExampleDebuff => Player?.Debuff(123);
    #endregion

    public override void Initialization()
    {
        Config.FocusMacros = new List<int> { };
        Config.MouseoverMacros = new List<int> { };
        Config.CursorMacros = new List<int> { };
        Config.ReticleMacros = new List<int> { };
        Config.TotemIds = new List<int> { };

        Config.EnableFocusTargeting = true;

        // Monitors gameplay for new game spells to add to the database for future use
        Config.SpellCollectionMode = true;
    }
    public override void CombatAction()
    {

    }
    public override void OutOfCombatAction()
    {

    }
}