﻿using CitizenFX.Core;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {

    class VoteMenu : BaseScript {

        public VoteMenu( Init instance, string name, string subtitle, List<string> Maps ) {

            MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Right;
            Menu mapMenu = new Menu( name, subtitle ) { Visible = true };
            MenuController.AddMenu( mapMenu );

            foreach( var map in Maps ) {
                MenuItem mapItem = new MenuItem( map );
                mapMenu.AddMenuItem( mapItem );
            }

            mapMenu.OnItemSelect += ( _menu, _item, _index ) => {
                TriggerServerEvent( "salty::netVoteMap", _item.Text );
                mapMenu.CloseMenu();
            };

        }

    }
}