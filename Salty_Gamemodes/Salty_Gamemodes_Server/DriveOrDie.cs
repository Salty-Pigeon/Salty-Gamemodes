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

        public DriveOrDie( MapManager manager, PlayerList players, int ID, string MapTag ) : base( manager, ID, MapTag ) {
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
            SpawnClient( truck, (int)Teams.Trucker );
            players.RemoveAt( truckID );
            spawns = GameMap.SpawnPoints[1].ToList();
            // Set innocents
            foreach( var ply in players ) {
                bikie.Add( ply );
                if( spawns.Count > 0 ) {
                    spawn = rand.Next( 0, spawns.Count );
                    SpawnClient( ply, (int)Teams.Bikie );
                    spawns.RemoveAt( spawn );
                }
                else {
                    SpawnClient( ply, (int)Teams.Bikie );
                }

            }

            base.Start();

        }

        public override void End() {
            base.End();
        }
    }
}
