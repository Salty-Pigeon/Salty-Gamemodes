using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    class IceCreamMan : BaseGamemode {

        public enum Teams {
            Spectators,
            Runner,
            Driver
        }

        public enum GameState {
            None,
            PreRound,
            InGame,
            PostRound
        }

        public IceCreamMan( MapManager manager, int ID, string MapTag ) : base( manager, ID, MapTag ) {

        }

        public override void Update() {
            base.Update();
        }

        public override void Start() {

            Debug.WriteLine( "Drive or Die starting on " + GameMap.Name );

            Random rand = new Random();
            List<Player> players = Players.ToList();

            int driverID = rand.Next(0, players.Count);
            Player driver = players[driverID];
            SetTeam(driver, (int)Teams.Driver);
            SpawnClient(driver, (int)Teams.Driver );
            players.RemoveAt(driverID);


            foreach( var ply in players) {
                SetTeam(ply, (int)Teams.Runner);
                SpawnClient(ply, (int)Teams.Runner );
            }


            /*
            Random rand = new Random();
            List<Player> players = Players.ToList();

            List<Vector3> spawns = GameMap.SpawnPoints[(int)Teams.Driver].ToList();
            if( spawns.Count == 0 ) {
                spawns.Add( GameMap.Position );
            }
            // Set traitor
            int driverID = rand.Next( 0, players.Count );
            int spawn = rand.Next( 0, spawns.Count );
            Player driver = players[driverID];
            drivers.Add( driver );
            SpawnClient( driver, (int)Teams.Driver, spawns[spawn] );
            players.RemoveAt( driverID );
            spawns = GameMap.SpawnPoints[(int)Teams.Runner].ToList();
            // Set innocents
            foreach( var ply in players ) {
                runners.Add( ply );
                if( spawns.Count > 0 ) {
                    spawn = rand.Next( 0, spawns.Count );
                    SpawnClient( ply, (int)Teams.Runner, spawns[spawn] );
                    spawns.RemoveAt( spawn );
                }
                else {
                    SpawnClient( ply, (int)Teams.Runner, GameMap.SpawnPoints[1][rand.Next( 0, GameMap.SpawnPoints.Count )] );
                }

            }
            */

            base.Start();
        }

        public override void End() {
            var winner = GetScores().OrderBy( x => x.Value ).ElementAt( 0 );
            WriteChat( string.Format( "{0} is winner with score of {1}", winner.Key.Name, winner.Value ) );
            base.End();
        }

    }
}
