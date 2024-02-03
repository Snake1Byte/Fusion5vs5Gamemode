using System.Collections.Generic;
using System.Linq;
using Il2CppSystem;
using Il2CppSystem.Collections;
using Il2CppSystem.Text;
using LabFusion.Data;
using LabFusion.Extensions;
using SLZ.Rig;
using SLZ.UI;
using Steamworks.Ugc;
using UltEvents;
using UnityEngine;
using IEnumerable = System.Collections.IEnumerable;
using IEnumerator = System.Collections.IEnumerator;

namespace Fusion5vs5Gamemode
{
    public class RadialMenu
    {
        private static List<PageItem> RootPageItems { get; set; }
        private static RadialSubMenu RootMenu { get; set; }
        private static PopUpMenuView _PopUpMenu;
        private static Page _HomePage;
        internal static ActivatePopupParams? LastActivatePopupParams;
        internal static RadialSubMenu LastRootMenu;
        internal static PageItem _RootMenuAsPageItem;
        internal static bool InRootLevel = true;

        public static void Initialize()
        {
            PopUpMenuViewPatches.OnPopUpMenuActivate += OnPopUpMenuActivate;
        }

        private static void OnPopUpMenuActivate(Transform headTransform, Transform rootTransform,
            UIControllerInput controllerInput, BaseController controller)
        {
            LastActivatePopupParams = new ActivatePopupParams
            {
                headTransform = headTransform,
                rootTransform = rootTransform,
                controllerInput = controllerInput,
                controller = controller
            };

            if (_PopUpMenu == null || _HomePage == null)
            {
                RigManager rm = RigData.RigReferences.RigManager;
                _PopUpMenu = rm.uiRig.popUpMenu;
                _HomePage = _PopUpMenu.radialPageView.m_HomePage;

                CheckRootMenuChanged();
            }
        }

        private static void CheckRootMenuChanged()
        {
            if (RootMenu == null || LastRootMenu == RootMenu || !InRootLevel)
            {
                return;
            }

            PageItem item = new PageItem(RootMenu.Name, RootMenu.Direction,
                (System.Action)(() => { OnSubMenuClicked(RootMenu); }));
            _HomePage.items.Add(item);
            _RootMenuAsPageItem = item;

            RootPageItems = new List<PageItem>(_HomePage.items.ToArray());
            LastRootMenu = RootMenu;
        }

        private static void OnSubMenuClicked(RadialSubMenu subMenuClicked)
        {
            DeactivateRadialMenu();

            InRootLevel = false;
            _HomePage.items.Clear();
            foreach (RadialMenuElement element in subMenuClicked)
            {
                if (element is RadialSubMenu subMenu)
                {
                    PageItem item = new PageItem(element.Name, element.Direction,
                        (System.Action)(() => { OnSubMenuClicked(subMenu); }));
                    _HomePage.items.Add(item);
                }
                else if (element is RadialMenuItem menuItem)
                {
                    PageItem item = new PageItem(menuItem.Name, menuItem.Direction,
                        menuItem.OnItemClicked);
                    _HomePage.items.Add(item);
                }
            }

            ActivateRadialMenu();
        }

        private static void ActivateRadialMenu()
        {
            if (LastActivatePopupParams.HasValue)
            {
                ActivatePopupParams arg = LastActivatePopupParams.Value;
                _PopUpMenu.Activate(arg.headTransform, arg.rootTransform, arg.controllerInput, arg.controller);
            }
        }

        private static void DeactivateRadialMenu()
        {
            ActivatePopupParams? arg = null;
            if (LastActivatePopupParams.HasValue)
            {
                arg = LastActivatePopupParams.Value;
            }

            _PopUpMenu.Deactivate();

            if (arg.HasValue)
            {
                LastActivatePopupParams = arg.Value;
            }
        }

        /// <summary>
        /// Places a custom menu on the west side of the radial menu, placing it where usually the "Utilities" menu
        /// would be when holding the spawn gun. If the spawn gun is being held, the menu created here will be
        /// temporarily replaced with the "Utilities" menu again until the spawn gun is being let go of, placing
        /// back the custom menu. If this function is called twice, any subsequent call replaces preceding calls,
        /// replacing the old custom menu with the newly created one by this function.
        /// </summary>
        /// <param name="name">The display name of the menu</param>
        /// <returns>A <see cref="RadialMenu"/> object to chain method calls with. This objects represents
        /// the root menu that you can then create sub menus and buttons in.</returns>
        public static RadialSubMenu CreateRootMenu(string name)
        {
            RootMenu = new RadialSubMenu(name, PageItem.Directions.WEST);
            return RootMenu;
        }

        public static void RemoveRootMenu()
        {
            if (RootPageItems == null)
            {
                return;
            }

            if (_RootMenuAsPageItem != null)
            {
                RootPageItems.Remove(_RootMenuAsPageItem);
            }

            _HomePage.items.Clear();
            foreach (var item in RootPageItems)
            {
                _HomePage.items.Add(item);
            }

            RootMenu = null;
            RootPageItems = null;
            _RootMenuAsPageItem = null;
            LastRootMenu = null;
            LastActivatePopupParams = null;
            InRootLevel = true;
        }

        public abstract class RadialMenuElement
        {
            public string Name { get; set; }
            public PageItem.Directions Direction { get; set; }
        }

        public class RadialSubMenu : RadialMenuElement, IEnumerable<RadialMenuElement>
        {
            private List<RadialMenuElement> Items => new List<RadialMenuElement>(7);
            public int Count => Items.Count;

            internal RadialSubMenu Parent { get; private set; }
            internal RadialMenuItem BackButton;

            public RadialSubMenu(string name, PageItem.Directions direction)
            {
                Name = name;
                Direction = direction;

                BackButton = new RadialMenuItem("Back", PageItem.Directions.SOUTH,
                    () => { OnBackButtonClicked(this); });

                Items.Add(BackButton);
            }

            public RadialSubMenu CreateSubMenu(string name, PageItem.Directions direction)
            {
                if (direction == PageItem.Directions.SOUTH)
                {
                    return null;
                }

                RadialSubMenu menu = new RadialSubMenu(name, direction);

                Add(menu);
                return menu;
            }

            public RadialMenuItem CreateItem(string name, PageItem.Directions direction, System.Action onItemPressed)
            {
                if (direction == PageItem.Directions.SOUTH)
                {
                    return null;
                }

                RadialMenuItem menu = new RadialMenuItem(name, direction, onItemPressed);

                Add(menu);
                return menu;
            }

            public void Clear()
            {
                foreach (var item in Items)
                {
                    if (item is RadialSubMenu menu)
                    {
                        menu.Parent = null;
                    }
                }

                Items.Clear();
            }

            public bool Remove(RadialMenuElement item)
            {
                if (item is RadialSubMenu menu)
                {
                    menu.Parent = null;
                }

                return Items.Remove(item);
            }

            public void Add(RadialMenuElement item)
            {
                if (item.Direction == PageItem.Directions.SOUTH)
                {
                    return;
                }

                RadialMenuElement toDelete = Items.Find(element => element.Direction == item.Direction);
                if (toDelete != null)
                {
                    Remove(toDelete);
                }

                if (item is RadialSubMenu menu)
                {
                    menu.Parent = this;
                }

                Items.Add(item);
            }

            public void AddRange(IEnumerable<RadialMenuElement> collection)
            {
                foreach (var item in collection)
                {
                    Add(item);
                }
            }

            private static void OnBackButtonClicked(RadialSubMenu subMenu)
            {
                if (subMenu == RootMenu || subMenu.Parent == null)
                {
                    DeactivateRadialMenu();

                    InRootLevel = true;
                    _HomePage.items.Clear();

                    foreach (var pageItem in RootPageItems)
                    {
                        _HomePage.items.Add(pageItem);
                    }

                    if (subMenu == RootMenu)
                    {
                        ActivateRadialMenu();
                    }
                }
                else if (subMenu.Parent != null)
                {
                    OnSubMenuClicked(subMenu.Parent);
                }
            }

            public IEnumerator<RadialMenuElement> GetEnumerator()
            {
                return Items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public class RadialMenuItem : RadialMenuElement
        {
            public System.Action OnItemClicked { get; set; }

            public RadialMenuItem(string name, PageItem.Directions direction, System.Action onItemClicked)
            {
                Name = name;
                Direction = direction;
                OnItemClicked = onItemClicked;
            }
        }

        internal struct ActivatePopupParams
        {
            internal Transform headTransform;
            internal Transform rootTransform;
            internal UIControllerInput controllerInput;
            internal BaseController controller;
        }
    }
}