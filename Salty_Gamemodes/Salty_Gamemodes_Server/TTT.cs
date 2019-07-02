using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    class TTT : BaseGamemode {


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

            base.Start();
        }

        public override void PlayerDied( Player player, int killerType, Vector3 deathcords ) {
            SetTeam(player, (int)Teams.Spectators);
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
