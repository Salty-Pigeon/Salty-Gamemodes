﻿using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

namespace Salty_Gamemodes_Client {
    public class BaseGamemode : BaseScript {

        public bool isNoclip = false;

        public List<string> PlayerWeapons = new List<string>();


        
        public Text BoundText;

        public bool inGame = false;
        public Map GameMap;

        public Vector3 noclipPos = Vector3.Zero;
        Vector3 deathPos;
        private float deathTimer = 0;
        private float gracePeriod = 10 * 1000;

        public int Team;

        public BaseGamemode() {
            BoundText = new Text( "", new System.Drawing.PointF( Screen.Width * 0.2f, Screen.Height * 0.1f), 1.0f );
        }

        public virtual void Start() {
            StripWeapons();
            GameMap.SpawnWeapons();
            inGame = true;
        }

        public virtual void End() {
            GameMap.ClearWeapons();
            inGame = false;
        }

        public virtual void PlayerDied( int killerType, Vector3 deathcords ) {
            deathPos = deathcords;
        }

        public virtual void PlayerSpawned( ExpandoObject spawnInfo ) {

            if( Team == 0 ) {
                SetNoClip( true );
            }
            
            if( inGame ) {
                SetTeam( 0 );
                SetNoClip( true );
                Game.Player.Character.Position = deathPos;
                noclipPos = deathPos;
            }
        }

        public virtual bool IsBase() {
            return !(GetType().IsSubclassOf( typeof( BaseGamemode ) ));
        }

        public void StripWeapons() {
            RemoveAllPedWeapons( PlayerPedId(), true );
            PlayerWeapons = new List<string>();

        }

        public virtual void SetTeam( int team ) {
            Team = team;
        }

        public void DrawRectangle( float x, float y, float width, float height, int r, int g, int b, int alpha ) {
            DrawRect( x + (width / 2), y + (height / 2), width, height, r, g, b, alpha );
        }

        public virtual void Controls() {

        }

        public virtual bool CanPickupWeapon( string weaponModel ) {
            return true;
        }

        public virtual void Events() {

            if( GameMap == null )
                return;

            // Weapon change
            foreach( var weps in GameMap.Weapons ) {
                uint wepHash = (uint)GetHashKey( weps.Key  );
                //uint wepHash = (uint)GetWeaponHashFromPickup( GetHashKey( weps.Value ) );
                if( HasPedGotWeapon( PlayerPedId(), wepHash, false) && !PlayerWeapons.Contains(weps.Key) ) {
                    PlayerWeapons.Add( weps.Key );
                    PlayerPickedUpWeapon(weps.Key, PlayerWeapons.Count);
                }

                if( !HasPedGotWeapon( PlayerPedId(), wepHash, false ) && PlayerWeapons.Contains( weps.Key ) ) {
                    PlayerWeapons.Remove( weps.Key );
                    PlayerDroppedWeapon( weps.Key, PlayerWeapons.Count );
                }

            }
                
        }

        public virtual void PlayerPickedUpWeapon(string wepName, int count) {

        }

        public virtual void PlayerDroppedWeapon( string wepName, int count ) {

        }

        public virtual void HUD() {
            if( inGame ) {
                GameMap.DrawBoundarys();
            }
        }

        public virtual void Update() {

            HUD();
            Events();
            Controls();

            if( GameMap != null ) {
                GameMap.Update();
            }

            if( Game.PlayerPed.Weapons.Current.Ammo <= 0 ) {
                Game.PlayerPed.Weapons.Remove( Game.PlayerPed.Weapons.Current );
            }
            
            if( Team == 0 ) {
                if( GetFollowPedCamViewMode() != 1 ) {
                    SetFollowPedCamViewMode( 1 );
                }
            }

            if( isNoclip ) {
                NoClipUpdate();
            }


            if( !isNoclip && inGame && Game.Player.IsAlive && Team != 0 ) {

                if( GameMap.IsInZone( Game.Player.Character.Position ) ) {
                    deathTimer = 0;
                }
                else {
                    if( deathTimer == 0 )
                        deathTimer = GetGameTimer();

                    float secondsLeft = deathTimer + gracePeriod - GetGameTimer();
                    if( secondsLeft < 0 ) {
                        Game.Player.Character.Kill();
                        deathTimer = 0;
                    }
                    BoundText.Color = System.Drawing.Color.FromArgb( 255, 0, 0 );
                    BoundText.Caption = "You have " + Math.Round( secondsLeft / 1000 ) + " seconds to return or you will die.";
                    BoundText.Draw();

                }
            }

            
        }

        public virtual void SetNoClip( bool toggle ) {
            deathTimer = 0;
            noclipPos = Game.Player.Character.Position;
            isNoclip = toggle;
            SetEntityVisible( PlayerPedId(), !isNoclip, false );
            SetEntityCollision( PlayerPedId(), !isNoclip, !isNoclip );
            SetEntityInvincible( PlayerPedId(), isNoclip );
            SetEveryoneIgnorePlayer( PlayerPedId(), isNoclip );
        }

        public virtual void CreateMap() {

        }

        private void NoClipUpdate() {
            SetEntityCoordsNoOffset( PlayerPedId(), noclipPos.X, noclipPos.Y, noclipPos.Z, false, false, false );

            Vector3 heading = GetGameplayCamRot( 0 );
            SetEntityRotation( PlayerPedId(), heading.X, heading.Y, -heading.Z, 0, true );
            SetEntityHeading( PlayerPedId(), heading.Z );

            int speed = 1;

            if( IsControlPressed( 0, 21 ) ) {
                speed *= 6;
            }

            Vector3 offset = new Vector3( 0, 0, 0 );

            if( IsControlPressed( 0, 36 ) ) {
                offset.Z = -speed;
            }

            if( IsControlPressed( 0, 22 ) ) {
                offset.Z = speed;
            }

            if( IsControlPressed( 0, 33 ) ) {
                offset.Y = -speed;
            }

            if( IsControlPressed( 0, 32 ) ) {
                offset.Y = speed;
            }

            if( IsControlPressed( 0, 35 ) ) {
                offset.X = speed;
            }

            if( IsControlPressed( 0, 34 ) ) {
                offset.X = -speed;
            }

            noclipPos = GetOffsetFromEntityInWorldCoords( PlayerPedId(), offset.X, offset.Y, offset.Z );
        }

        public void FirstPersonForAlive() {
            if( Team != 0 ) {
                if( GetFollowPedCamViewMode() != 4 )
                    SetFollowPedCamViewMode( 4 );
            }
        }

        public static Vector3 StringToVector3( string vector ) {
            vector = vector.Replace( "X:", "" );
            vector = vector.Replace( "Y:", "" );
            vector = vector.Replace( "Z:", "" );
            string[] vector3 = vector.Split( ' ' );
            return new Vector3( float.Parse( vector3[ 0 ] ), float.Parse( vector3[ 1 ] ), float.Parse( vector3[ 2 ] ) );
           
        }

        public void WriteChat( string str ) {
            TriggerEvent( "chat:addMessage", new {
                color = new[] { 255, 0, 0 },
                args = new[] { GetType().ToString() , str }
            } );
        }

    }
}
