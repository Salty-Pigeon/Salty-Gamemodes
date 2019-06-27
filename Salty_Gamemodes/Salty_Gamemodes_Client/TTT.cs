using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

namespace Salty_Gamemodes_Client {
    class TTT : BaseGamemode {

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

        public TTT( Map gameMap, int team ) {
            GameMap = gameMap;
            GameMap.CreateBlip();
            SetTeam( team );
        }

        public override void Start() {
            WriteChat( "Game starting" );
            base.Start();
        }

        public override void End() {
            WriteChat( "Game ending" );
            base.End();

        }

        public override void PlayerPickedUpWeapon( string wepName, int count ) {
            if( count >= 2 ) {
                WriteChat( "Disabling weapon pickup" );

            }
            base.PlayerPickedUpWeapon( wepName, count );
        }

        public override void PlayerDroppedWeapon( string wepName, int count ) {
            WriteChat( "Dropped weapon" );
            base.PlayerDroppedWeapon( wepName, count );
        }

        public override void PlayerDied( int killerType, Vector3 deathcords ) {
            SetTeam( (int)Teams.Spectators );
            base.PlayerDied( killerType, deathcords );
        }

        public override void PlayerSpawned( ExpandoObject spawnInfo ) {
            base.PlayerSpawned( spawnInfo );
        }

        public override void Update() {


            if( Team == (int)Teams.Traitors ) {
                TeamText.Color = System.Drawing.Color.FromArgb( 200, 0, 0 );
                TeamText.Caption = "Traitor";
            }
            if( Team == (int)Teams.Innocents ) {
                TeamText.Color = System.Drawing.Color.FromArgb( 0, 200, 0 );
                TeamText.Caption = "Innocent";
            }

            if( Team != (int)Teams.Spectators ) {
                if( GetFollowPedCamViewMode() != 4 )
                    SetFollowPedCamViewMode( 4 );
            }

            base.Update();

        }
    }
}
