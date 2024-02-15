using System;
using BoneLib;
using SLZ.UI;
using static Fusion5vs5Gamemode.Utilities.RadialMenu;

namespace Fusion5vs5Gamemode.Client
{
    public static class BuyMenu
    {
        // Root Menu
        private static RadialSubMenu _BuyMenu;

        // Weapon Category
        private static RadialSubMenu _Pistols;
        private static RadialSubMenu _SMGs;
        private static RadialSubMenu _Shotguns;

        private static RadialSubMenu _Rifles;
        // private static RadialSubMenu _SniperRifles;
        // private static RadialSubMenu _Utilities;

        // Pistols
        private static RadialMenuItem EDER22;
        private static RadialMenuItem _1911;
        private static RadialMenuItem P350;

        // SMGs
        private static RadialMenuItem UMP;
        private static RadialMenuItem MP5;
        private static RadialMenuItem Vector;

        // Shotguns
        private static RadialMenuItem FAB;
        private static RadialMenuItem M4;
        private static RadialMenuItem _590A1;

        // Rifles
        private static RadialMenuItem MK18;
        private static RadialMenuItem AKM;
        private static RadialMenuItem PDRC;

        // Sniper Rifles

        // Utilities

        public static Action<string> OnBuyMenuItemClicked;

        static BuyMenu()
        {
            _BuyMenu = new RadialSubMenu("Buy", PageItem.Directions.WEST);

            _Pistols = _BuyMenu.CreateSubMenu("Pistols", PageItem.Directions.SOUTHWEST);
            EDER22 = new RadialMenuItem("EDER22", PageItem.Directions.WEST,
                () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.Eder22));
            _1911 = new RadialMenuItem("1911", PageItem.Directions.EAST,
                () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.M1911));
            P350 = new RadialMenuItem("P350", PageItem.Directions.NORTH,
                () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.P350));
            _Pistols.Add(EDER22);
            _Pistols.Add(_1911);
            _Pistols.Add(P350);
            _BuyMenu.Add(_Pistols);

            _SMGs = _BuyMenu.CreateSubMenu("SMGs", PageItem.Directions.WEST);
            UMP = new RadialMenuItem("UMP", PageItem.Directions.WEST,
                () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.UMP));
            MP5 = new RadialMenuItem("MP5", PageItem.Directions.NORTH,
                () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.MP5));
            Vector = new RadialMenuItem("Vector", PageItem.Directions.EAST,
                () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.Vector));
            _SMGs.Add(UMP);
            _SMGs.Add(MP5);
            _SMGs.Add(Vector);
            _BuyMenu.Add(_SMGs);

            _Shotguns = _BuyMenu.CreateSubMenu("Shotguns", PageItem.Directions.NORTHEAST);
            FAB = new RadialMenuItem("FAB", PageItem.Directions.WEST,
                () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.FAB));
            M4 = new RadialMenuItem("M4", PageItem.Directions.EAST,
                () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.M4));
            _590A1 = new RadialMenuItem("590A1", PageItem.Directions.NORTH,
                () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.M590A1));
            _Shotguns.Add(FAB);
            _Shotguns.Add(M4);
            _Shotguns.Add(_590A1);
            _BuyMenu.Add(_Shotguns);

            _Rifles = _BuyMenu.CreateSubMenu("Rifles", PageItem.Directions.NORTH);
            MK18 = new RadialMenuItem("MK18", PageItem.Directions.NORTH,
                () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.MK18HoloForegrip));
            AKM = new RadialMenuItem("AKM", PageItem.Directions.EAST,
                () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.AKM));
            PDRC = new RadialMenuItem("PDRC", PageItem.Directions.WEST,
                () => Internal_OnBuyMenuItemClicked(CommonBarcodes.Guns.PDRC));
            _Rifles.Add(FAB);
            _Rifles.Add(M4);
            _Rifles.Add(_590A1);
            _BuyMenu.Add(_Rifles);
        }

        public static void AddBuyMenu()
        {
            AddRootMenu(_BuyMenu);
        }

        public static void RemoveBuyMenu()
        {
            RemoveRootMenu(_BuyMenu);
        }

        internal static void Internal_OnBuyMenuItemClicked(string barcode)
        {
            SafeActions.InvokeActionSafe(OnBuyMenuItemClicked, barcode);
        }
    }
}