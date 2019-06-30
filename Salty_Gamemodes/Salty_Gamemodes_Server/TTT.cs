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
            Innocents
        }

        public enum GameState {
            None,
            PreRound,
            InGame,
            PostRound
        }

        public GameState CurrentState = GameState.None;


        public TTT( MapManager manager, PlayerList players, int ID, string MapTag ) : base ( manager, ID, MapTag ) {
            this.players = players;
        }

        public override void PlayerJoined( Player ply ) {
            base.PlayerJoined( ply );
        }

        public override void Start() {

            base.Start();
            
            Debug.WriteLine( "TTT starting on " + GameMap.Name );
            Random rand = new Random();
            List<Player> players = Players.ToList();

            List<Vector3> spawns = GameMap.SpawnPoints[0].ToList();
            if( spawns.Count == 0 ) {
                spawns.Add( GameMap.Position );
            }
            // Set traitor
            int traitorID = rand.Next( 0, players.Count );
            int spawn = rand.Next( 0, spawns.Count );
            Player traitor = players[ traitorID ];
            traitors.Add( traitor );
            SpawnClient( traitor, (int)Teams.Traitors, spawns[spawn] );
            players.RemoveAt( traitorID );
            spawns.RemoveAt( spawn );
            // Set innocents
            foreach( var ply in players ) {
                innocents.Add( ply );
                if( spawns.Count > 0 ) {
                    spawn = rand.Next( 0, spawns.Count );
                    SpawnClient( ply, (int)Teams.Traitors, spawns[spawn] );
                    spawns.RemoveAt( spawn );
                } else {
                    SpawnClient( ply, (int)Teams.Traitors, GameMap.SpawnPoints[0][rand.Next( 0, GameMap.SpawnPoints.Count )] );

                }

            }

            // create map
            TriggerClientEvent( "salty::CreateMap", GameMap.Position, GameMap.Size, GameMap.Name );
           

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
