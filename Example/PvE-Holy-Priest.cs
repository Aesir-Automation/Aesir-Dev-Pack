using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Midgard;
using Midgard.API.Engine;


public class Holy : Simulation
{
    private Buff? FC => Player.Buff(336267);
    private Buff? BoonBuff => Player.Buff(325013);
    private Buff? Spirit => Player.Buff(20711);
    private Buff? Surge => Player.Buff(114255);
    private Buff? LowestRenew => LowestFriendly.Buff(139);
    //private Buff? ApotheosisBuff => Player.Buff(200183);
    private Buff? EmphemeralEffusion => Player.Buff(366438);
    private Buff? ResonantWords => Player.Buff(337948);

    private Buff? BenevolentFaerie => Player.Buff(327710);
    private Buff? GuardianFaerie => LowestTank.Buff(327694);
    private Debuff? WrathfulFaerie => Target.Debuff(342132);

    private Debuff? WeakenedSoulTank => LowestTank.Debuff(6788);
    private Debuff? WeakenedSoulPlayer => Player.Debuff(6788);

    private bool FCEquipped => LegendaryEffect(336266);
    private bool ResWords => SoulbindConduit(337947);

    private Spell? GuardianSpirit => Player.Spell(47788);
    private Spell? Salvation => Player.Spell(265202);
    private Spell? DivineHymn => Player.Spell(64843);
    private Spell? SymbolOfHope => Player.Spell(64901);
    //private Spell? Apotheosis => Player.Spell(200183);
    private Spell? FlashHeal => Player.Spell(2061);
    private Spell? Serenity => Player.Spell(2050);
    private Spell? Sanctify => Player.Spell(34861);
    private Spell? PWS => Player.Spell(17);
    //private Spell? UHNova => Player.Spell(324724);
    //private Spell? Halo => Player.Spell(120517);
    //private Spell? Star => Player.Spell(110744);
    private Spell? Circle => Player.Spell(204883);
    private Spell? Blast => Player.Spell(325283);
    private Spell? ANova => Player.Spell(325020);
    private Spell? PoH => Player.Spell(596);
    private Spell? CircleOfHealing => Player.Spell(204883);
    private Spell? Mindgames
    {
        get
        {
            if (Player?.IsVenthyr is true)
                return Player?.Spell(323673);

            return null;
        }
    }
    private Spell? FaeGuardians
    {
        get
        {
            if (Player?.IsNightFae is true)
                return Player?.Spell(327661);

            return null;
        }
    }
    private Spell? BoonOfTheAscended
    {
        get
        {
            if (Player?.IsKyrian is true)
                return Player?.Spell(325013);

            return null;
        }
    }

    private Spell? PoM => Player.Spell(33076);
    private Spell? Heal => Player.Spell(2060);
    private Spell? Chastise => Player.Spell(88625);
    private Spell? Renew => Player.Spell(139);
    private Spell? RenewedFaith => Player.Spell(341997);

    private Spell? SWP => Player.Spell(589);
    private Debuff? SWPTargetDebuff => Target.Debuff(589);

    private Spell? Smite => Player.Spell(585);
    private Spell? HolyFire => Player.Spell(14914);
    private Spell? SWD => Player.Spell(32409);

    private bool CanRefreshSWP
    {
        get
        {
            var refresh = Target?.Debuff(589)?.InRefreshWindow;
            if (refresh != null)
            {
                return (bool)refresh;
            }
            return false;
        }
    }
    private bool FaeBuff
    {
        get
        {
            foreach (var unit in CollectionsMarshal.AsSpan(FriendlyUnits))
            {
                var benevolentFaerie = unit.Buff(327710);
                var guardianFaerie = unit.Buff(327694);

                if (benevolentFaerie?.HasBuffFromPlayer is true || guardianFaerie?.HasBuffFromPlayer is true)
                    return true;
            }
            return false;
        }
    }
    private Unit? PoMUnit
    {
        get
        {
            if (LowestTank != null)
            {
                return LowestTank;
            }
            else if (LowestMelee != null)
            {
                return LowestMelee;
            }
            else
            {
                return LowestFriendly;
            }
        }
    }
    private Unit? CircleOfHealingUnit => ClusterHealTarget();

    public override void Initialization()
    {
        // Add spell Ids here that you want to cast on specific units, on cursor, or on cursor where the player has to click (reticle).
        Config.FocusMacros = new List<int> { };
        Config.MouseoverMacros = new List<int> { 34861 };
        Config.CursorMacros = new List<int> { };
        Config.ReticleMacros = new List<int> { };

        Config.EnableFocusTargeting = true;
        Config.SpellCollectionMode = true;

        AddLegendaryId(336266); // Flash Concentration https://www.wowhead.com/spell=336266
        AddExtraBuff(366438); // Ephemeral Effusion https://www.wowhead.com/spell=366438
    }

    public bool MaintainFlashConcentration()
    {
        var standardStack = FC?.BuffStacksFromPlayer < 5;
        var standardMaintain = FC?.BuffStacksFromPlayer == 5 && Player?.CurrentSpellCast.Id != FlashHeal.Id && FC?.InRefreshWindow is true;
        var surgeShedStacks = Surge?.BuffStacksFromPlayer > 1;
        var surgeExpiring = Surge?.BuffStacksFromPlayer == 1 && Surge?.BuffRemainingFromPlayer <= Surge?.RefreshWindow;

        // FC maintenance while not moving
        if (standardStack || standardMaintain || surgeExpiring)
        {
            if (Cast(FlashHeal, LowestFriendly))
                return true;
        }
        // FC maintenance while moving
        else if (Player?.IsMoving is true && (surgeShedStacks || surgeExpiring))
        {
            if (Cast(FlashHeal, LowestFriendly, Midgard.Enums.Frames.Focus, true))
                return true;
        }
        return false;
    }

    public bool CooldownsRotation()
    {
        if (FastestDroppingFriendly?.Health <= 25)
            if (Cast(GuardianSpirit, FastestDroppingFriendly))
                return true;

        if (FriendlyUnitsAverageHealth() <= 60)
            if (Cast(Salvation, Player))
                return true;

        if (FriendlyUnitsAverageHealth() <= 50)
            if (Cast(DivineHymn, Player))
                return true;

        if (FriendlyUnitsAverageHealth() <= 85)
            if (Cast(FaeGuardians, LowestTank))
                return true;

        if (FriendlyUnitsAverageHealth() <= 80)
            if (Cast(BoonOfTheAscended, Player))
                return true;

        if (FriendlyUnitsAverageMana() <= 75)
            if (Cast(SymbolOfHope, Player))
                return true;

        return false;
    }

    public bool GroupRotation()
    {
        if (FCEquipped is true)
            if (MaintainFlashConcentration())
                return true;

        if (BoonBuff?.HasBuffFromPlayer is true)
        {
            if (Cast(Blast))
                return true;
            else if (Cast(ANova))
                return true;
            else
                return false;
        }

        if (FaeBuff)
        {
            if (BenevolentFaerie?.HasBuffFromPlayer is false)
                if (Cast(FlashHeal, Player))
                    return true;

            if (GuardianFaerie?.HasBuffFromPlayer is false && WeakenedSoulTank?.HasDebuffFromPlayer is false)
                if (Cast(PWS, LowestTank))
                    return true;

            if (WrathfulFaerie?.HasDebuffFromPlayer is false)
                if (Cast(SWP))
                    return true;
        }

        if (CooldownsRotation())
            return true;

        if (EmphemeralEffusion?.HasBuffFromPlayer is true && Surge?.HasBuffFromPlayer is true && LowestFriendly.Health <= 90)
            if (Cast(FlashHeal, LowestFriendly))
                return true;

        // Mouseover healing for Mage Tower would be here

        if (ResonantWords?.HasBuffFromPlayer is true && LowestFriendly.Health <= 75)
            if (Cast(Heal, LowestFriendly))
                return true;

        if (NumberMeleeHealingTargets(90, 40, 10) >= 3 && Mouseover?.Health <= 90 && FriendlyUnits.Any(unit => unit.Guid == Mouseover?.Guid && unit.IsMelee))
            if (Cast(Sanctify, Mouseover))
                return true;

        if (LowestFriendly?.Health <= 75)
            if (Cast(Serenity, LowestFriendly))
                return true;

        if (Target?.IsEnemy is true)
            if (Cast(Chastise))
                return true;

        if (CircleOfHealingUnit?.Health <= 90)
            if (Cast(CircleOfHealing, CircleOfHealingUnit))
                return true;

        if (Cast(PoM, PoMUnit))
            return true;

        if (LowestFriendly?.Health <= 90)
            if (Cast(Heal, LowestFriendly))
                return true;

        if (Player?.IsMoving is true && LowestFriendly?.Health <= 90)
            if (Cast(Renew, LowestFriendly))
                return true;

        if (Target?.Health <= 20)
            if (Cast(SWD))
                return true;

        if (Cast(Mindgames))
            return true;

        if (SWPTargetDebuff?.InRefreshWindow is true)
            if (Cast(SWP))
                return true;

        if (Cast(HolyFire))
            return true;

        if (Cast(Smite))
            return true;

        return false;
    }
    public bool SoloRotation()
    {
        if (Target?.IsEnemy is true)
        {
            if (Player?.Health < 95)
                if (Cast(Renew))
                    return true;

            if (WeakenedSoulPlayer?.HasDebuffFromPlayer is false)
                if (Cast(PWS))
                    return true;

            if (Player?.Health < 75)
                if (Cast(Serenity))
                    return true;

            if (Player?.Health < 85)
                if (Cast(FlashHeal))
                    return true;

            if (Target?.Health <= 20)
                if (Cast(SWD))
                    return true;

            if (Cast(BoonOfTheAscended))
                return true;

            if (BoonBuff?.HasBuffFromPlayer is true)
            {
                if (Cast(Blast))
                    return true;

                if (Cast(ANova))
                    return true;

                return false;
            }

            if (Cast(Chastise))
                return true;

            if (Cast(HolyFire))
                return true;

            if (SWPTargetDebuff?.InRefreshWindow is true)
                if (Cast(SWP))
                    return true;

            if (Cast(Smite))
                return true;
        }
        return false;
    }

    public bool OutOfCombatPrep()
    {
        if (FC?.BuffStacksFromPlayer < 5 && FCEquipped is true)
            if (Cast(FlashHeal, LowestFriendly))
                return true;

        if (Cast(PoM, PoMUnit))
            return true;

        return false;
    }

    public override void CombatAction()
    {
        if (Player?.InRaid is true)
        {
            if (GroupRotation())
                return;
        }
        else if (Player?.InParty is true)
        {
            if (GroupRotation())
                return;
        }
        else
        {
            if (SoloRotation())
                return;
        }
    }
    public override void OutOfCombatAction()
    {
        if (Player?.InRaid is true || Player?.InParty is true)
        {
            if (OutOfCombatPrep())
                return;
        }
        else
        {
            if (SoloRotation())
                return;
        }
    }
}