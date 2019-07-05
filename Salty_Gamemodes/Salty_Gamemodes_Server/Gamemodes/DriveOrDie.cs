using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    class DriveOrDie : BaseGamemode {

        List<Player> trucker = new List<Player>();
        List<Player> bikie = new List<Player>();

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

        public DriveOrDie( MapManager manager, int ID, string MapTag ) : base( manager, ID, MapTag ) {

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

            Random rand = new Random();
            List<Player> players = Players.ToList();

            int driverID = rand.Next(0, players.Count);
            Player driver = players[driverID];
            SetTeam(driver, (int)Teams.Trucker);
            SpawnClient(driver, (int)Teams.Trucker);
            players.RemoveAt(driverID);


            foreach (var ply in players) {
                SetTeam(ply, (int)Teams.Bikie);
                SpawnClient(ply, (int)Teams.Bikie);
            }

            base.Start();

        }

        public override void End() {
           
            base.End();
        }
    }
}
