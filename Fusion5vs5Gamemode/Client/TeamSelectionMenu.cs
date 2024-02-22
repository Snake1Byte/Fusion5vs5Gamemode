using System;
using SLZ.UI;
using static Fusion5vs5Gamemode.Utilities.RadialMenu;

namespace Fusion5vs5Gamemode.Client;

public static class TeamSelectionMenu
{
    // Root Menu
    private static readonly RadialSubMenu TeamsMenu;

    // Weapon Category

    public static Action? OnAttackersSelected;
    public static Action? OnDefendersSelected;
    public static Action? OnSpectatorsSelected;


    static TeamSelectionMenu()
    {
        TeamsMenu = new RadialSubMenu("Teams", PageItem.Directions.NORTH);
        RadialMenuItem attackers = new RadialMenuItem("Attackers", PageItem.Directions.EAST,
            AttackersSelected);
        RadialMenuItem defenders = new RadialMenuItem("Defenders", PageItem.Directions.WEST,
            DefendersSelected);
        RadialMenuItem spectators = new RadialMenuItem("Spectators", PageItem.Directions.NORTH,
            SpectatorsSelected);
        TeamsMenu.Add(attackers);
        TeamsMenu.Add(defenders);
        TeamsMenu.Add(spectators);
    }

    public static void AddTeamsMenu()
    {
        AddRootMenu(TeamsMenu);
    }

    public static void RemoveTeamsMenu()
    {
        RemoveRootMenu(TeamsMenu);
    }

    private static void AttackersSelected()
    {
        OnAttackersSelected?.Invoke();
    }

    private static void DefendersSelected()
    {
        OnDefendersSelected?.Invoke();
    }

    private static void SpectatorsSelected()
    {
        OnSpectatorsSelected?.Invoke();
    }
}