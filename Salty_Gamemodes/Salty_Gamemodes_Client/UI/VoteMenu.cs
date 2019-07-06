using CitizenFX.Core;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {

    class VoteMenu : BaseScript {

        Menu mapMenu;

        public VoteMenu( Init instance, string name, string subtitle, List<string> VoteList ) {

            MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Right;
            mapMenu = new Menu( name, subtitle ) { Visible = true };
            MenuController.AddMenu( mapMenu );

            foreach( var map in VoteList ) {
                MenuItem mapItem = new MenuItem( map );
                mapMenu.AddMenuItem( mapItem );
            }

            mapMenu.OnItemSelect += ( _menu, _item, _index ) => {
                TriggerServerEvent( "salty::netVoteMap", _item.Text );
                
            };

        }

        public void Close() {
            mapMenu.CloseMenu();
            mapMenu.ClearMenuItems();
            MenuController.Menus.Remove( mapMenu );
            mapMenu = null;
        }

    }
}
