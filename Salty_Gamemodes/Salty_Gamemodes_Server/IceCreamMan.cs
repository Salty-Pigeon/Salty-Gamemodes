using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

namespace Salty_Gamemodes_Server {
    class IceCreamMan : BaseGamemode {

        Player Driver;

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

        public IceCreamMan( MapManager manager, int ID, string MapTag ) : base( manager, ID, MapTag ) {

        }

        public override void Update() {
            base.Update();
        }

        public override void OnTimerEnd() {
            WriteChat( "Ice cream man delivered ice cream safely" );
            base.OnTimerEnd();
        }

        public override void PlayerKilled( Player player, int killerID, ExpandoObject deathData ) {
            if( PlayerDetails[player]["Team"] == (int)Teams.Driver ) {
                WriteChat( "Ice cream man defeated. Bikers win." );
                End();
            }
            base.PlayerKilled( player, killerID, deathData );
        }


        public override void Start() {

            Debug.WriteLine( "Drive or Die starting on " + GameMap.Name );

            Random rand = new Random();
            List<Player> players = Players.ToList();

            int driverID = rand.Next(0, players.Count);
            Driver = players[driverID];
             
            SetTeam( Driver, (int)Teams.Driver);
            SpawnClient( Driver, (int)Teams.Driver );
            players.RemoveAt(driverID);


            foreach( var ply in players) {
                SetTeam(ply, (int)Teams.Runner);
                SpawnClient(ply, (int)Teams.Runner );
            }

            base.Start();
        }

        public override void End() {
            var winner = GetScores().OrderBy( x => x.Value ).ElementAt( 0 );

            WriteChat( string.Format( "{0} got a score of {1}", Driver.Name, PlayerDetails[Driver]["Score"] ) );

            base.End();
        }

    }
}
