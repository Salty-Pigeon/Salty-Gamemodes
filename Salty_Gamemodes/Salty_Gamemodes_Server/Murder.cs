﻿using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    class Murder : BaseGamemode {


        List<Player> murderer = new List<Player>();
        List<Player> civilians = new List<Player>();

        public enum Teams {
            Spectators,
            Murderer,
            Civilian
        }

        public enum GameState {
            None,
            PreRound,
            InGame,
            PostRound
        }

        public Murder( Map gameMap, PlayerList players, int ID ) : base( ID ) {
            GameMap = gameMap;
            this.players = players;
        }

        public override void Start() {
            Debug.WriteLine( "Murder starting on " + GameMap.Name );
            Random rand = new Random();
            List<Player> players = Players.ToList();

            List<Vector3> spawns = GameMap.SpawnPoints[0].ToList();
            if( spawns.Count == 0 ) {
                spawns.Add( GameMap.Position );
            }
            // Set traitor
            int murdererID = rand.Next( 0, players.Count );
            int spawn = rand.Next( 0, spawns.Count );
            Player murder = players[murdererID];
            murderer.Add( murder );
            murder.TriggerEvent( "salty::StartGame", ID, (int)Teams.Murderer, GameMap.Position, GameMap.Size, spawns[spawn], GameMap.GunSpawns );
            players.RemoveAt( murdererID );
            spawns.RemoveAt( spawn );
            // Set innocents
            foreach( var ply in players ) {
                civilians.Add( ply );
                if( spawns.Count > 0 ) {
                    spawn = rand.Next( 0, spawns.Count );
                    ply.TriggerEvent( "salty::StartGame", ID, (int)Teams.Civilian, GameMap.Position, GameMap.Size, spawns[spawn], GameMap.GunSpawns );
                    spawns.RemoveAt( spawn );
                }
                else {
                    ply.TriggerEvent( "salty::StartGame", ID, (int)Teams.Civilian, GameMap.Position, GameMap.Size, GameMap.SpawnPoints[rand.Next( 0, GameMap.SpawnPoints.Count )], GameMap.GunSpawns );
                }

            }

            // create map
            TriggerClientEvent( "salty::CreateMap", GameMap.Position, GameMap.Size, GameMap.Name );
        }

        public override void End() {

        }
    }
}
