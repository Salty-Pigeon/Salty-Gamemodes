using CitizenFX.Core;
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

        public Murder( MapManager manager, PlayerList players, int ID, string MapTag ) : base( manager, ID, MapTag ) {
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
            SpawnClient( murder, (int)Teams.Murderer );
            players.RemoveAt( murdererID );
            spawns.RemoveAt( spawn );
            Player playerGun = null;
            if( players.Count > 0 ) {
                playerGun = players[rand.Next( 0, players.Count )];
            }
            // Set innocents
            foreach( var ply in players ) {
                civilians.Add( ply );
                if( spawns.Count > 0 ) {
                    spawn = rand.Next( 0, spawns.Count );
                    SpawnClient( ply, (int)Teams.Civilian );
                    spawns.RemoveAt( spawn );
                }
                else {
                    SpawnClient( ply, (int)Teams.Civilian );
                }

            }
            if( playerGun != null )
                playerGun.TriggerEvent( "salty::GiveGun", "WEAPON_PISTOL", 1 );
            // create map

            base.Start();
        }


        public override void PlayerDied( Player player, int killerType, Vector3 deathcords ) {
            if( murderer.Contains(player) ) {
                WriteChat( "Mike Tyson defeated, civilians win." );

            }
            base.PlayerDied( player, killerType, deathcords );
        }

        public override void End() {
            base.End();
        }
    }
}
