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


        public IceCreamMan( int ID, Map map, List<Player> players ) : base( ID, map, players ) {

        }

        public override void Update() {
            base.Update();
        }

        public override void OnTimerEnd() {
            WriteChat( "Ice Cream Man", "Ice cream man delivered ice cream safely", 255, 0, 0 );
            base.OnTimerEnd();
        }

        public override void PlayerKilled( Player player, int killerID, Vector3 deathCoords ) {
            if( GetTeam(player) == (int)Teams.Driver ) {
                WriteChat( "Ice Cream Man", "Ice cream man defeated. Bikers win.", 255, 0, 0 );
                End();
            }
            base.PlayerKilled( player, killerID, deathCoords );
        }

        public override void PlayerDied( Player player, int killerType, Vector3 deathcords ) {
            if( GetTeam(player) == (int)Teams.Runner ) {
                AddScore( Driver, 3 );
            }
            base.PlayerDied( player, killerType, deathcords );
        }

        public override void Start() {
            WriteChat( "Ice Cream Man", "Game starting", 255, 0, 0 );
            Random rand = new Random();
            List<Player> players = InGamePlayers.ToList();

            int driverID = rand.Next(0, players.Count);
            Driver = players[driverID];
             
            SpawnClient( Driver, (int)Teams.Driver, (int)Teams.Driver );
            players.RemoveAt(driverID);


            foreach( var ply in players) {
                SpawnClient(ply, (int)Teams.Runner, (int)Teams.Runner );
            }

            base.Start();
        }

        public override void End() {
            var winner = GetScores().OrderBy( x => x.Value ).ElementAt( 0 );

            WriteChat( "Ice Cream Man", string.Format( "{0} got a score of {1}", Driver.Name, PlayerDetails[Driver]["Score"] ), 255, 0, 0 );

            base.End();
        }

    }
}
