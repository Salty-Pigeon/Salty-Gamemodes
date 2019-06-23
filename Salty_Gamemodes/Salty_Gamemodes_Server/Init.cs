﻿using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server
{
    public class Init : BaseScript {

        Database SQLConnection;
        MapManager MapManager;

        public Init() {
            SQLConnection = new Database();
            MapManager = new MapManager( SQLConnection.Load() );
            foreach( var map in MapManager.Maps ) {
                Debug.WriteLine( map.Key + " - Loaded" );
            }
        }

    }
}