using System;
using SLZ.UI;
using static Fusion5vs5Gamemode.Utilities.RadialMenu;

namespace Fusion5vs5Gamemode.Client
{
    public static class TeamSelectionMenu
    {
        // Root Menu
        private static RadialSubMenu _TeamsMenu;

        // Weapon Category
        private static RadialMenuItem _Attackers;
        private static RadialMenuItem _Defenders;
        private static RadialMenuItem _Spectators;

        public static Action OnAttackersSelected;
        public static Action OnDefendersSelected;
        public static Action OnSpectatorsSelected;

        static TeamSelectionMenu()
        {
            _TeamsMenu = new RadialSubMenu("Teams", PageItem.Directions.NORTH);
            _Attackers = new RadialMenuItem("Attackers", PageItem.Directions.EAST,
                Internal_OnAttackersSelected);
            _Defenders = new RadialMenuItem("Defenders", PageItem.Directions.WEST,
                Internal_OnDefendersSelected);
            _Spectators = new RadialMenuItem("Spectators", PageItem.Directions.NORTH,
                Internal_SpectatorsSelected);
            _TeamsMenu.Add(_Attackers);
            _TeamsMenu.Add(_Defenders);
            _TeamsMenu.Add(_Spectators);
        }

        public static void AddTeamsMenu()
        {
            AddRootMenu(_TeamsMenu);
        }

        public static void RemoveTeamsMenu()
        {
            RemoveRootMenu(_TeamsMenu);
        }

        internal static void Internal_OnAttackersSelected()
        {
            if (OnAttackersSelected != null)
            {
                OnAttackersSelected();
            }
        }

        internal static void Internal_OnDefendersSelected()
        {
            if (OnDefendersSelected != null)
            {
                OnDefendersSelected();
            }
        }

        internal static void Internal_SpectatorsSelected()
        {
            if (OnSpectatorsSelected != null)
            {
                OnSpectatorsSelected();
            }
        }
    }
}