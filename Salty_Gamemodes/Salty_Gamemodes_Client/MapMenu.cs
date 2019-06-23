using System;
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

        public MapMenu( string name, string subtitle, Dictionary<string, List<Vector3>> maps ) {

            Debug.WriteLine( maps.ElementAt( 0 ).Key.ToString() );

            MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Right;
            Menu mapMenu = new Menu( name, subtitle ) { Visible = true };
            MenuController.AddMenu( mapMenu );

            // Create a third menu without a banner.

            // you can use AddSubmenu or AddMenu, both will work but if you want to link this menu from another menu,
            // you should use AddSubmenu.
            foreach( var spawn in maps ) {
                Menu mapEditor = new Menu( null, "Edit " + spawn.Key );
                MenuController.AddSubmenu( mapMenu, mapEditor );

                MenuItem mapItem = new MenuItem( spawn.Key, "Modify map" ) { Label = ">>>" };
                mapMenu.AddMenuItem( mapItem );
                MenuController.BindMenuItem( mapMenu, mapEditor, mapItem );

                Menu playerSpawnMenu = new Menu( null, "Edit " + spawn.Key + " player spawns" );
                MenuController.AddSubmenu( mapEditor, playerSpawnMenu );

                Menu modifyPosMenu = new Menu( null, "Edit position" );
                MenuController.AddSubmenu( playerSpawnMenu, modifyPosMenu );


                MenuItem playerSpawnItem = new MenuItem( "Player Spawns", "Modify player spawn points" ) { Label = ">>" };
                mapEditor.AddMenuItem( playerSpawnItem );
                MenuController.BindMenuItem( mapEditor, playerSpawnMenu, playerSpawnItem );

                modifyPosMenu.AddMenuItem( new MenuItem( "Delete", "Deletes the selected position" ) );

                foreach( var spawns in maps[spawn.Key] ) {
                    //playerSpawnMenu.AddMenuItem( new MenuItem( spawns.ToString() ) );

                    MenuItem playerPositionItem = new MenuItem( spawns.ToString(), "Modify player spawn point" ) { Label = ">" };
                    playerSpawnMenu.AddMenuItem( playerPositionItem );
                    MenuController.BindMenuItem( playerSpawnMenu, modifyPosMenu, playerPositionItem );

                    playerSpawnMenu.OnIndexChange += ( _menu, _oldItem, _newItem, _oldIndex, _newIndex ) => {
                       
                    };
                }

            }

            mapMenu.OnIndexChange += ( _menu, _oldItem, _newItem, _oldIndex, _newIndex ) => {
                // Code in here would get executed whenever the up or down key is pressed and the index of the menu is changed.
                Debug.WriteLine( $"OnIndexChange: [{_menu}, {_oldItem}, {_newItem}, {_oldIndex}, {_newIndex}]" );
            };

        }

        public void CreatePosMenu() {
            
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
