using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    class IceCreamMan : BaseGamemode {

        List<Player> drivers = new List<Player>();
        List<Player> runners = new List<Player>();

        public enum Teams {
            Spectators,
            Driver,
            Runner
        }

        public enum GameState {
            None,
            PreRound,
            InGame,
            PostRound
        }

        public IceCreamMan( MapManager manager, PlayerList players, int ID, string MapTag ) : base( manager, ID, MapTag ) {
            this.players = players;
        }

        public override void Update() {
            base.Update();
        }

        public override void Start() {

            base.Start();



            Debug.WriteLine( "Drive or Die starting on " + GameMap.Name );
            Random rand = new Random();
            List<Player> players = Players.ToList();

            List<Vector3> spawns = GameMap.SpawnPoints[0].ToList();
            if( spawns.Count == 0 ) {
                spawns.Add( GameMap.Position );
            }
            // Set traitor
            int driverID = rand.Next( 0, players.Count );
            int spawn = rand.Next( 0, spawns.Count );
            Player driver = players[driverID];
            PlayerScores.Add( driver, 0 );
            drivers.Add( driver );
            SpawnClient( driver, (int)Teams.Driver, spawns[spawn] );
            players.RemoveAt( driverID );
            spawns = GameMap.SpawnPoints[1].ToList();
            // Set innocents
            foreach( var ply in players ) {
                runners.Add( ply );
                PlayerScores.Add( ply, 0 );
                if( spawns.Count > 0 ) {
                    spawn = rand.Next( 0, spawns.Count );
                    SpawnClient( ply, (int)Teams.Runner, spawns[spawn] );
                    spawns.RemoveAt( spawn );
                }
                else {
                    SpawnClient( ply, (int)Teams.Runner, GameMap.SpawnPoints[1][rand.Next( 0, GameMap.SpawnPoints.Count )] );
                }

            }

            // create map
            TriggerClientEvent( "salty::CreateMap", GameMap.Position, GameMap.Size, GameMap.Name );
        }

        public override void End() {
            var winner = PlayerScores.OrderBy( x => x.Value ).ElementAt( 0 );
            WriteChat( string.Format( "{0} is winner with score of {1}", winner.Key.Name, winner.Value ) );
            base.End();
        }

    }
}
