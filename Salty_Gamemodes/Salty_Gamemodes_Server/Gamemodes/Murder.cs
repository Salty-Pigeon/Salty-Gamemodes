using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    class Murder : BaseGamemode {

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

        public Murder( MapManager manager, int ID, string MapTag ) : base( manager, ID, MapTag ) {

        }

        public Murder( MapManager manager, int ID, Map map ) : base( manager, ID, map ) {

        }

        public override void Start() {
            WriteChat( "Murder", "Game starting", 255, 0, 0 );

            Random rand = new Random();
            List<Player> players = Players.ToList();

            int driverID = rand.Next(0, players.Count);
            Player driver = players[driverID];
            SetTeam(driver, (int)Teams.Murderer);
            SpawnClient(driver, 1);
            players.RemoveAt(driverID);


            foreach (var ply in players) {
                SetTeam(ply, (int)Teams.Civilian);
                SpawnClient(ply, 1);
            }

            if (players.Count > 0) {
                Player playerGun = players[rand.Next(0, players.Count)];
                playerGun.TriggerEvent("salty::GiveGun", "WEAPON_PISTOL", 1);
            }

            base.Start();
        }


        public override void PlayerDied( Player player, int killerType, Vector3 deathcords ) {

            if( GetTeam(player) == (int)Teams.Murderer ) {
                WriteChat( "Murder", "Murderer dead, civilians win!", 255, 0, 0 );
                End();
            }

            SetTeam( player, (int)Teams.Spectators );

            if( TeamCount((int)Teams.Civilian) == 0 ) {
                WriteChat( "Murder", "Civilians dead, murderer wins!", 255, 0, 0 );
                End();
            }

            base.PlayerDied( player, killerType, deathcords );
        }

        public override void End() {
            WriteChat( "Murder", "Game ending", 255, 0, 0 );

            base.End();
        }
    }
}
