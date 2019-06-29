﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using MenuAPI;
using System.Drawing;


namespace Salty_Gamemodes_Client {
    class MapMenu : BaseScript {


        private double DegreeToRadian( double angle ) {
            return Math.PI * angle / 180.0;
        }

        public MapMenu( Init instance, string name, string subtitle, Dictionary<string, Map> Maps ) {

            MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Right;
            Menu mapMenu = new Menu( name, subtitle ) { Visible = true };
            MenuController.AddMenu( mapMenu );



            foreach( var map in Maps ) {
                Vector3 selectedVector = map.Value.SpawnPoints.ElementAt( 0 ).Value[0];
                int selectedTeam = map.Value.SpawnPoints.ElementAt( 0 ).Key;

                Menu mapEditor = AddSubMenu( mapMenu, "Edit " + map.Key );


                MenuItem mapItem = AddMenuItem( mapMenu, mapEditor, map.Key, "Modify Map", ">>>", true );

                Menu playerSpawnMenu = AddSubMenu( mapEditor, "Edit " + map.Key + " player spawns" );
                Menu deleteMapMenu = AddSubMenu( mapEditor, "Delete " + map.Key + "?" );
                deleteMapMenu.AddMenuItem( new MenuItem( "Yes", "" ) );
                deleteMapMenu.AddMenuItem( new MenuItem( "No", "" ) );
                deleteMapMenu.OnItemSelect += ( _menu, _item, _index ) => {
                    if( _item.Text == "Yes" ) {
                        TriggerServerEvent( "salty::netModifyMap", "delete", map.Key );
                        MenuController.CloseAllMenus();
                        TriggerServerEvent( "salty::netSpawnPointGUI" );
                    }
                    if( _item.Text == "No" ) {
                        deleteMapMenu.CloseMenu();
                    }
                };
                mapEditor.AddMenuItem( new MenuItem( "Show/Hide" ) );

                mapEditor.OnItemSelect += ( _menu, _item, _index ) => {
                    if( _item.Text == "Show/Hide" ) {
                        if( instance.Maps[map.Key].isVisible )
                            instance.Maps[map.Key].ClearBlip();
                        else
                            instance.Maps[map.Key].CreateBlip();
                    }
                };

                Menu modifyPosMenu = AddSubMenu( playerSpawnMenu, "Edit position" );

                MenuSliderItem sliderOffset = new MenuSliderItem( "Offset", -25, 25, 0, false );
                MenuSliderItem sliderX = new MenuSliderItem( "Centre X", -999999, 999999, (int)map.Value.Position.X, false );
                MenuSliderItem sliderY = new MenuSliderItem( "Centre Y", -999999, 999999, (int)map.Value.Position.Y, false );
                MenuSliderItem sliderWidth = new MenuSliderItem( "Width", -9999, 9999, (int)map.Value.Size.X, false );
                MenuSliderItem sliderLength = new MenuSliderItem( "Length", -9999, 9999, (int)map.Value.Size.Y, false );

                mapEditor.AddMenuItem( sliderOffset );
                mapEditor.AddMenuItem( sliderX );
                mapEditor.AddMenuItem( sliderY );
                mapEditor.AddMenuItem( sliderWidth );
                mapEditor.AddMenuItem( sliderLength );

                MenuItem playerSpawnItem = AddMenuItem( mapEditor, playerSpawnMenu, "Player Spawns", "Modify player spawn points", ">>", true );
                MenuItem deleteMapItem = AddMenuItem( mapEditor, deleteMapMenu, "Delete Map", "Delete entire map", ">", true );

                modifyPosMenu.AddMenuItem( new MenuItem( "Delete", "Deletes the selected position" ) );
                mapEditor.AddMenuItem( new MenuItem( "Save", "Saves new position and size" ) );




                mapEditor.OnSliderPositionChange += ( _menu, _sliderItem, _oldPosition, _newPosition, _itemIndex ) => {
                    if( _sliderItem.Text == "Centre X" ) {
                        map.Value.Position.X += (_newPosition - _oldPosition) * sliderOffset.Position;
                    }
                    if( _sliderItem.Text == "Centre Y" ) {
                        map.Value.Position.Y += (_newPosition - _oldPosition) * sliderOffset.Position;
                    }
                    if( _sliderItem.Text == "Width" ) {
                        map.Value.Size.X += (_newPosition - _oldPosition) * sliderOffset.Position;
                    }
                    if( _sliderItem.Text == "Length" ) {
                        map.Value.Size.Y += (_newPosition - _oldPosition) * sliderOffset.Position;
                    }
                    

                };

                mapEditor.OnItemSelect += ( _menu, _item, _index ) => {
                    if( _item.Text == "Save" ) {
                        TriggerServerEvent( "salty::netModifyMap", "edit", map.Key, 0, map.Value.Position, map.Value.Size );
                    }
                };

                modifyPosMenu.OnItemSelect += ( _menu, _item, _index ) => {
                    if( _item.Text == "Delete" ) {
                        TriggerServerEvent( "salty::netModifyMapPos", "delete", map.Key, selectedTeam, selectedVector );
                        MenuController.CloseAllMenus();
                        TriggerServerEvent( "salty::netSpawnPointGUI" );

                    }
                };

                foreach( var spawns in map.Value.SpawnPoints ) {
                    foreach( var spawn in spawns.Value ) {
                        MenuItem playerPositionItem = AddMenuItem( playerSpawnMenu, modifyPosMenu, string.Format( "{0} | {1}", spawns.Key.ToString(), spawn.ToString() ), "Modify player spawn point", ">", true );

                    }

                    playerSpawnMenu.OnIndexChange += ( _menu, _oldItem, _newItem, _oldIndex, _newIndex ) => {
                        string[] vector = _newItem.Text.Split( '|' );
                        selectedTeam = Convert.ToInt32( vector[0].Split( ' ' )[0] );
                        selectedVector = BaseGamemode.StringToVector3( vector[1].Substring(1) );
                    };
                }


                mapMenu.OnMenuClose += ( _menu ) => {

                };

                mapMenu.OnIndexChange += ( _menu, _oldItem, _newItem, _oldIndex, _newIndex ) => {
                    // Code in here would get executed whenever the up or down key is pressed and the index of the menu is changed.
                    //Debug.WriteLine( $"OnIndexChange: [{_menu}, {_oldItem}, {_newItem}, {_oldIndex}, {_newIndex}]" );

                };

            }
        }

        public MenuItem AddMenuItem( Menu parent, Menu child, string name, string description, string label, bool bindMenu ) {
            MenuItem menuItem = new MenuItem( name, description ) { Label = label };
            parent.AddMenuItem( menuItem );
            if( bindMenu ) {
                MenuController.BindMenuItem( parent, child, menuItem );
            }
            return menuItem;
        }

        public Menu AddSubMenu( Menu parent, string name ) {
            Menu subMenu = new Menu( null, name );
            MenuController.AddSubmenu( parent, subMenu );
            return subMenu;
        }

        public void CreatePosMenu(  ) {

        }


                                        /*
                                 ########################################################
                                                     Event handlers
                                 ########################################################


                                menu.OnItemSelect += ( _menu, _item, _index ) => {
                                    // Code in here would get executed whenever an item is pressed.
                                    Debug.WriteLine( $"OnItemSelect: [{_menu}, {_item}, {_index}]" );
                                };

                                menu.OnIndexChange += ( _menu, _oldItem, _newItem, _oldIndex, _newIndex ) => {
                                    // Code in here would get executed whenever the up or down key is pressed and the index of the menu is changed.
                                    Debug.WriteLine( $"OnIndexChange: [{_menu}, {_oldItem}, {_newItem}, {_oldIndex}, {_newIndex}]" );
                                };

                                menu.OnListIndexChange += ( _menu, _listItem, _oldIndex, _newIndex, _itemIndex ) => {
                                    // Code in here would get executed whenever the selected value of a list item changes (when left/right key is pressed).
                                    Debug.WriteLine( $"OnListIndexChange: [{_menu}, {_listItem}, {_oldIndex}, {_newIndex}, {_itemIndex}]" );
                                };

                                menu.OnListItemSelect += ( _menu, _listItem, _listIndex, _itemIndex ) => {
                                    // Code in here would get executed whenever a list item is pressed.
                                    Debug.WriteLine( $"OnListItemSelect: [{_menu}, {_listItem}, {_listIndex}, {_itemIndex}]" );
                                };

                                menu.OnSliderPositionChange += ( _menu, _sliderItem, _oldPosition, _newPosition, _itemIndex ) => {
                                    // Code in here would get executed whenever the position of a slider is changed (when left/right key is pressed).
                                    Debug.WriteLine( $"OnSliderPositionChange: [{_menu}, {_sliderItem}, {_oldPosition}, {_newPosition}, {_itemIndex}]" );
                                };

                                menu.OnSliderItemSelect += ( _menu, _sliderItem, _sliderPosition, _itemIndex ) => {
                                    // Code in here would get executed whenever a slider item is pressed.
                                    Debug.WriteLine( $"OnSliderItemSelect: [{_menu}, {_sliderItem}, {_sliderPosition}, {_itemIndex}]" );
                                };

                                menu.OnMenuClose += ( _menu ) => {
                                    // Code in here gets triggered whenever the menu is closed.
                                    Debug.WriteLine( $"OnMenuClose: [{_menu}]" );
                                };

                                menu.OnMenuOpen += ( _menu ) => {
                                    // Code in here gets triggered whenever the menu is opened.
                                    Debug.WriteLine( $"OnMenuOpen: [{_menu}]" );
                                };

                                menu.OnDynamicListItemCurrentItemChange += ( _menu, _dynamicListItem, _oldCurrentItem, _newCurrentItem ) => {
                                    // Code in here would get executed whenever the value of the current item of a dynamic list item changes.
                                    Debug.WriteLine( $"OnDynamicListItemCurrentItemChange: [{_menu}, {_dynamicListItem}, {_oldCurrentItem}, {_newCurrentItem}]" );
                                };

                                menu.OnDynamicListItemSelect += ( _menu, _dynamicListItem, _currentItem ) => {
                                    // Code in here would get executed whenever a dynamic list item is pressed.
                                    Debug.WriteLine( $"OnDynamicListItemSelect: [{_menu}, {_dynamicListItem}, {_currentItem}]" );
                                };

                                   */
    }
}
