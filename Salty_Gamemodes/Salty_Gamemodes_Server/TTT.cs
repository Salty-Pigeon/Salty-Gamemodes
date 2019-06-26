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


        public TTT( Map gameMap, PlayerList players, int ID ) : base ( ID ) {
            GameMap = gameMap;
            this.players = players;
        }



        public override void Start() {
            Debug.WriteLine( "TTT starting on " + GameMap.Name );
            Random rand = new Random();
            List<Player> players = Players.ToList();

            List<Vector3> spawns = GameMap.SpawnPoints.ToList();
            if( spawns.Count == 0 ) {
                spawns.Add( GameMap.Position );
            }
            // Set traitor
            int traitorID = rand.Next( 0, players.Count );
            int spawn = rand.Next( 0, spawns.Count );
            Player traitor = players[ traitorID ];
            traitors.Add( traitor );
            traitor.TriggerEvent( "salty::StartGame", ID, (int)Teams.Traitors, GameMap.Position, GameMap.Size, spawns[spawn], GameMap.GunSpawns );
            players.RemoveAt( traitorID );
            spawns.RemoveAt( spawn );
            // Set innocents
            foreach( var ply in players ) {
                innocents.Add( ply );
                if( spawns.Count > 0 ) {
                    spawn = rand.Next( 0, spawns.Count );
                    ply.TriggerEvent( "salty::StartGame", ID, (int)Teams.Innocents, GameMap.Position, GameMap.Size, spawns[ spawn ], GameMap.GunSpawns );
                    spawns.RemoveAt( spawn );
                } else {
                    ply.TriggerEvent( "salty::StartGame", ID, (int)Teams.Innocents, GameMap.Position, GameMap.Size, GameMap.SpawnPoints[ rand.Next(0, GameMap.SpawnPoints.Count) ], GameMap.GunSpawns );
                }
                
            }

            // create map
            TriggerClientEvent( "salty::CreateMap", GameMap.Position, GameMap.Size, GameMap.Name );
        }

        public override void End() {
            Debug.WriteLine( "Game ending" );
        }

        public override void Update() {

        }

    }
}
