using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    class SaltyTown {

        Salty ActiveGame;
        MapManager Maps;
        List<Player> ActivePlayers = new List<Player>();
        Dictionary<string, BaseGamemode> ActiveGamemodes;
        Dictionary<uint, List<Player>> ActiveRooms;
        
        public SaltyTown() {
            ActiveGame = new Salty();
            ActivePlayers = new PlayerList().ToList();
        }

        public void JoinRoom( Player ply, string gamemode ) {
            if( !ActivePlayers.Contains( ply ) ) {
                Debug.WriteLine( ply.Name + " is already in game" );
                return;
            }

        }

        public void StartGamemodeRoom() {

        }

        public void LeaveGamemodeRoom() {

        }

        public void StartGamemode( uint gamemode ) {
            if( !ActiveRooms.ContainsKey( gamemode ) ) {
                Debug.WriteLine( "Room not active" );
            }
            foreach( var player in ActiveRooms[gamemode] ) {
                
            }
        }

    }
}
