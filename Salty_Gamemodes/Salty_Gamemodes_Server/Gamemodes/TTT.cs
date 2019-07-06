using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    class TTT : BaseGamemode {

        public Dictionary<int, bool> DeadBodies = new Dictionary<int, bool>();

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

        public override bool OnChatMessage( Player ply, string message ) {
            if( GetTeam(ply) == (int)Teams.Spectators ) {
                foreach( var player in PlayerDetails ) {
                    if( GetTeam(player.Key) == (int)Teams.Spectators ) {
                        WriteChat( "[DEAD] " + ply.Name, message, 200, 200, 0 );
                    }
                }
            } else {
                foreach( var player in PlayerDetails ) {
                    int team = GetTeam( player.Key );
                    if( team != (int)Teams.Spectators ) {
                        if( team == (int)Teams.Detectives )
                            WriteChat( ply.Name, message, 0, 0, 255 );
                        else
                            WriteChat( ply.Name, message, 0, 255, 0 );
                    }
                }
            }
            return true;
        }

        public override void Start() {
            Random rand = new Random();
            List<Player> players = Players.ToList();

            int traitorCount = (int)Math.Ceiling((double)players.Count / 4);
            for( var i = 0; i < traitorCount; i++ ) {
                int traitorID = rand.Next( 0, players.Count );
                Player traitor = players[traitorID];
                SetTeam( traitor, (int)Teams.Traitors );
                SpawnClient( traitor, 1 );
                players.RemoveAt( traitorID );
            }

            foreach (var ply in players) {
                SetTeam(ply, (int)Teams.Innocents);
                SpawnClient(ply, 1);
            }

            base.Start();
        }

        public override void PlayerKilled( Player player, int killerID, Vector3 deathcords ) {
            SetTeam( player, (int)Teams.Spectators );
            TriggerClientEvent( "salty::SpawnDeadBody", deathcords, Convert.ToInt32( player.Handle ), killerID );
            if( GetTeam( player ) == (int)Teams.Traitors ) {
                GameTime += 30 * 1000;
            }
            base.PlayerKilled( player, killerID, deathcords );
        }

        public override void PlayerDied( [FromSource] Player player, int killerType, Vector3 deathcords ) {
            SetTeam(player, (int)Teams.Spectators);
            TriggerClientEvent( "salty::SpawnDeadBody", deathcords, Convert.ToInt32( player.Handle ), Convert.ToInt32( player.Handle ) );
            if( TeamCount((int)Teams.Traitors) <= 0 ) {
                //WriteChat( "TTT", "Innocents Win!", 0, 230, 0 );
                //End();
            } else if( TeamCount((int)Teams.Innocents) + TeamCount((int)Teams.Detectives) <= 0 ) {
                //WriteChat( "TTT", "Traitors Win!", 230, 0, 0 );
                //End();
            }
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
