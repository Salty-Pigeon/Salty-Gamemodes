using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

namespace Salty_Gamemodes_Client
{
    public class Init : BaseScript
    {
        BaseGamemode ActiveGame = new BaseGamemode();
        Vector3 deathPos;

        public Init() {
            EventHandlers[ "onClientResourceStart" ] += new Action<string>( OnClientResourceStart );
            EventHandlers[ "playerSpawned" ] += new Action<ExpandoObject>( PlayerSpawn );
            EventHandlers[ "baseevents:onPlayerDied" ] += new Action<int, List<dynamic>>( PlayerDied );
            EventHandlers[ "salty::StartGame" ] += new Action<int, int, Vector3, Vector2, Vector3>( StartGame );
            EventHandlers[ "salty::EndGame" ] += new Action( ActiveGame.End );
            EventHandlers[ "salty::CreateMap" ] += new Action( ActiveGame.CreateMap );
            ActiveGame.SetNoClip( false );
            Tick += Update;
        }

        public void StartGame( int id, int team, Vector3 mapPos, Vector2 mapSize, Vector3 startPos ) {
            Map map = new Map( mapPos, mapSize );
            if( id == 1 ) { // Trouble in Terrorist Town
                ActiveGame = new TTT(map, team);
                if( team == 1 ) { // Traitor
                    
                } else if( team == 2 ) { // Innocent

                } else { // Spectator

                }
            }
            ActiveGame.Start();
            Game.Player.Character.Position = startPos;
        }

        private void PlayerDied( int killerType, List<dynamic> deathcords ) {
            Vector3 coords = new Vector3( (float)deathcords[ 0 ], (float)deathcords[ 1 ], (float)deathcords[ 2 ] );
            ActiveGame.PlayerDied( killerType, coords );
        }

        private void PlayerSpawn( ExpandoObject spawnInfo ) {
            ActiveGame.PlayerSpawned( spawnInfo );
        }

        public async Task Update() {
            if( ActiveGame != null )
                ActiveGame.Update();
        }

        private void OnClientResourceStart( string resourceName ) {
            if( GetCurrentResourceName() != resourceName ) return;

            RegisterCommand( "noclip", new Action<int, List<object>, string>( ( source, args, raw ) => {
                ActiveGame.SetNoClip(!ActiveGame.isNoclip);
            } ), false );

            RegisterCommand( "spawnPoints", new Action<int, List<object>, string>( ( source, args, raw ) => {
                new MapMenu("Spawn Points", "Modify spawn points below");
            } ), false );

        }
    }
}
