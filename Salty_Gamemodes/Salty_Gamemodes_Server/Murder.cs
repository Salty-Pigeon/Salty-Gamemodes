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

        public Murder( MapManager manager, int ID, string MapTag ) : base( manager, ID, MapTag ) {

        }

        public override void Start() {
            Debug.WriteLine( "Murder starting on " + GameMap.Name );

            Random rand = new Random();
            List<Player> players = Players.ToList();

            int driverID = rand.Next(0, players.Count);
            Player driver = players[driverID];
            SetTeam(driver, (int)Teams.Murderer);
            SpawnClient(driver, (int)Teams.Murderer);
            players.RemoveAt(driverID);


            foreach (var ply in players) {
                SetTeam(ply, (int)Teams.Civilian);
                SpawnClient(ply, (int)Teams.Civilian);
            }

            if (players.Count > 0) {
                Player playerGun = players[rand.Next(0, players.Count)];
                playerGun.TriggerEvent("salty::GiveGun", "WEAPON_PISTOL", 1);
            }

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
