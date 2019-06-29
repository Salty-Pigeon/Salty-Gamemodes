using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    class DriveOrDie : BaseGamemode {

        List<Player> trucker = new List<Player>();
        List<Player> bikie = new List<Player>();

        public enum Teams {
            Spectators,
            Trucker,
            Bikie
        }

        public enum GameState {
            None,
            PreRound,
            InGame,
            PostRound
        }

        public DriveOrDie( Map gameMap, PlayerList players, int ID ) : base( ID ) {
            GameMap = gameMap;
            this.players = players;
        }

        public override void Start() {
            Debug.WriteLine( "Drive or Die starting on " + GameMap.Name );
            Random rand = new Random();
            List<Player> players = Players.ToList();

            List<Vector3> spawns = GameMap.SpawnPoints[0].ToList();
            if( spawns.Count == 0 ) {
                spawns.Add( GameMap.Position );
            }
            // Set traitor
            int truckID = rand.Next( 0, players.Count );
            int spawn = rand.Next( 0, spawns.Count );
            Player truck = players[truckID];
            trucker.Add( truck );
            truck.TriggerEvent( "salty::StartGame", ID, (int)Teams.Trucker, GameMap.Position, GameMap.Size, spawns[spawn], GameMap.GunSpawns );
            players.RemoveAt( truckID );
            spawns.RemoveAt( spawn );
            // Set innocents
            foreach( var ply in players ) {
                bikie.Add( ply );
                if( spawns.Count > 0 ) {
                    spawn = rand.Next( 0, spawns.Count );
                    ply.TriggerEvent( "salty::StartGame", ID, (int)Teams.Bikie, GameMap.Position, GameMap.Size, spawns[spawn], GameMap.GunSpawns );
                    spawns.RemoveAt( spawn );
                }
                else {
                    ply.TriggerEvent( "salty::StartGame", ID, (int)Teams.Bikie, GameMap.Position, GameMap.Size, GameMap.SpawnPoints[rand.Next( 0, GameMap.SpawnPoints.Count )], GameMap.GunSpawns );
                }

            }
         
            // create map
            TriggerClientEvent( "salty::CreateMap", GameMap.Position, GameMap.Size, GameMap.Name );
        }

        public override void End() {

        }
    }
}
