using CitizenFX.Core;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {
    class SaltyRooms : BaseScript {

        public SaltyRooms( List<string> rooms ) {
            MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Right;
            Menu menu = new Menu( "Rooms", "Join a room" ) { Visible = true };
            MenuController.AddMenu( menu );

            foreach( var room in rooms ) {
                MenuItem roomItem = new MenuItem( room );
                menu.AddMenuItem( roomItem );
            }

            menu.OnItemSelect += ( _menu, _item, _index ) => {
                Debug.WriteLine( _index.ToString() );
                TriggerServerEvent( "salty::netJoinRoom", _item.Text );
                menu.CloseMenu();
                MenuController.Menus.Remove( menu );
            };


        }
    }
}
