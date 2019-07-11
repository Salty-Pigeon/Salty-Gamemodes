using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {
    public class SaltyTown : BaseScript {


        public Salty SaltyGame;
        public BaseGamemode ActiveGame;

        public Dictionary<string, Map> ActiveMaps = new Dictionary<string, Map>();

        private float zoneTimer = 0f;
        private float zoneLength = 3 * 1000;

        public bool isInRoom = false;
        public int RoomID;
        SaltyRooms RoomMenu;

        public SaltyTown() {
            SaltyGame = new Salty();
        }


        public void RoomLeft() {
            SaltyGame.StripWeapons();
            isInRoom = false;
        }

        public void RoomStarted() {

        }

        public void ShowRooms( List<string> rooms ) {
            RoomMenu = new SaltyRooms( rooms );
        }

        public void CloseRoomMenu() {
            RoomMenu = null;
        }

        public void Update() {
            if( !isInRoom ) {
                bool inZone = false;
                foreach( Map map in ActiveMaps.Values ) {
                    if( map.IsInZone( Game.PlayerPed.Position ) ) {
                        zoneTimer = 0;
                        if( !SaltyGame.isNoclip ) {
                            SaltyGame.SetNoClip( true );
                        }
                        inZone = true;
                    }
                }
                if( !inZone && SaltyGame.isNoclip ) {
                    if( zoneTimer == 0 ) {
                        zoneTimer = GetGameTimer() + zoneLength;
                    }
                    if( zoneTimer - GetGameTimer() < 0 ) {
                        float z = World.GetGroundHeight( new Vector2( Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y ) );
                        SaltyGame.SetNoClip( false );
                        Game.PlayerPed.Position = new Vector3( Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, z + 0.1f );
                        zoneTimer = 0;
                    }
                    else {
                        SaltyGame.GetHUD().DrawNoClipWarning( Convert.ToInt32(Math.Round( (zoneTimer - GetGameTimer()) / 1000 )) );
                    }
                }
                SaltyGame.Update();
            }
            else {
                if( ActiveGame == null ) { return; }
                ActiveGame.Update();
            }
        }

        public void StartGame( int id, int team, double duration, Map map, Vector3 startPos, Dictionary<string,List<Vector3>> gunSpawns ) {
            isInRoom = true;
            NetworkSetVoiceChannel( 0 );
            map.GunSpawns = gunSpawns;
            if( id == 1 ) { // Trouble in Terrorist Town
                ActiveGame = new TTT( map, team );
            }
            if( id == 2 ) { // Drive or die
                ActiveGame = new DriveOrDie( map, team );
            }
            if( id == 3 ) { // Murder
                ActiveGame = new Murder( map, team );
            }
            if( id == 4 ) { // Ice Cream Man
                ActiveGame = new IceCreamMan( map, team );
            }
            if( duration > 0 )
                ActiveGame.CreateGameTimer( duration );
            ActiveGame.PlayerSpawn = startPos;
            ActiveGame.SetNoClip( false );
            Game.Player.Character.Position = startPos;
            Debug.WriteLine( "Game started" );

            ActiveGame.Start();
        }
    }
}
