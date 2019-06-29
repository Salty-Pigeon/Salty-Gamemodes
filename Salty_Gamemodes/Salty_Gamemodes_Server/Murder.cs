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

        public Murder( Map gameMap, PlayerList players, int ID ) : base( ID ) {
            GameMap = gameMap;
            this.players = players;
        }

        public override void Start() {

        }

        public override void End() {

        }
    }
}
