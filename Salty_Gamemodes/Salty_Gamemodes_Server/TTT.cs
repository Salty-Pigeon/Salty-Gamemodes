using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    class TTT : BaseGamemode {

        List<Player> traitors = new List<Player>();
        List<Player> innocents = new List<Player>();


        public enum Teams {
            Spectators,
            Traitors,
            Innocents,
            Detectives
        }

        public enum GameState {
            None,
            PreRound,
            InGame,
            PostRound
        }

        public GameState CurrentState = GameState.None;


        public TTT( MapManager manager, int ID, string MapTag ) : base ( manager, ID, MapTag ) {

        }

        public override void PlayerJoined( Player ply ) {
            base.PlayerJoined( ply );
        }

        public override void Start() {
            Debug.WriteLine( "TTT starting on " + GameMap.Name );

            Random rand = new Random();
            List<Player> players = Players.ToList();

            int driverID = rand.Next(0, players.Count);
            Player driver = players[driverID];
            SetTeam(driver, (int)Teams.Traitors);
            SpawnClient(driver, (int)Teams.Traitors);
            players.RemoveAt(driverID);


            foreach (var ply in players) {
                SetTeam(ply, (int)Teams.Innocents);
                SpawnClient(ply, (int)Teams.Innocents);
            }

            /*
            List<Vector3> spawns = GameMap.SpawnPoints[0].ToList();
            if( spawns.Count == 0 ) {
                spawns.Add( GameMap.Position );
            }
            // Set traitor
            int traitorID = rand.Next( 0, players.Count );
            int spawn = rand.Next( 0, spawns.Count );
            Player traitor = players[ traitorID ];

            SetTeam(traitor, (int)Teams.Traitors);

            SpawnClient( traitor, (int)Teams.Traitors );
            players.RemoveAt( traitorID );
            spawns.RemoveAt( spawn );
            // Set innocents
            foreach( var ply in players ) {
                SetTeam(ply, (int)Teams.Innocents);
                if( spawns.Count > 0 ) {
                    spawn = rand.Next( 0, spawns.Count );
                    SpawnClient( ply, (int)Teams.Traitors );
                    spawns.RemoveAt( spawn );
                } else {
                    SpawnClient( ply, (int)Teams.Traitors );

                }

            }
            */
            base.Start();
        }

        public override void PlayerDied( Player player, int killerType, Vector3 deathcords ) {

            base.PlayerDied(player, killerType, deathcords);
        }

        public override void End() {
            Debug.WriteLine( "Game ending" );

            base.End();
        }

        public override void Update() {

            base.Update();
        }

    }
}
