﻿using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    class DriveOrDie : BaseGamemode {

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


        public DriveOrDie( int ID, Map map, List<Player> players ) : base( ID, map, players ) {

        }

        public override void PlayerDied( Player player, int killerType, Vector3 deathcords ) {
            int team = GetTeam( player );
            if( team == (int)Teams.Bikie ) {
                SetTeam( player, (int)Teams.Trucker );
            }
            if( TeamCount((int)Teams.Bikie) == 0 ) {
                WriteChat( "Drive or Die", "All bikers defeated", 255, 0, 0 );
                End();
            }
            base.PlayerDied( player, killerType, deathcords );
        }

        public override void Start() {
            WriteChat( "Drive or Die", "Game starting", 255, 0, 0 );

            Random rand = new Random();
            List<Player> players = InGamePlayers.ToList();

            int bikeCount = (int)Math.Ceiling( (double)players.Count / 3 );
            for( var i = 0; i < bikeCount; i++ ) {
                int bikeID = rand.Next( 0, players.Count );
                Player biker = players[bikeID];
                SpawnClient( biker, (int)Teams.Bikie, (int)Teams.Bikie );
                players.RemoveAt( bikeID );
            }

            foreach (var ply in players) {
                SpawnClient(ply, (int)Teams.Trucker, (int)Teams.Trucker );
            }

            base.Start();

        }

        public override void End() {
            WriteChat( "Drive or Die", "Game ending", 255, 0, 0 );

            base.End();
        }
    }
}
