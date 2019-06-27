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
    public class BaseGamemode : BaseScript {

        public static int WeaponCount = 0;

        public bool isNoclip = false;

        List<string> weapons = new List<string>();


        public Text TeamText;
        public Text BoundText;

        public bool inGame = false;
        public Map GameMap;

        public Vector3 noclipPos = Vector3.Zero;
        Vector3 deathPos;
        private float deathTimer = 0;
        private float gracePeriod = 10 * 1000;

        public int Team;

        public BaseGamemode() {
            TeamText = new Text( "Spectator", new System.Drawing.PointF( 20, 100 ), 1.0f );
            BoundText = new Text( "", new System.Drawing.PointF( Screen.Width * 0.2f, Screen.Height * 0.1f), 1.0f );
        }

        public virtual void Start() {
            RemoveAllPedWeapons( PlayerPedId(), true );
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

        public void SetTeam( int team ) {
            Team = team;
            switch( team ) {
                case ( 0 ):
                    TeamText.Caption = "Spectator";
                    TeamText.Color = System.Drawing.Color.FromArgb( 150, 150, 0 );
                    break;
                case ( 1 ):
                    TeamText.Caption = "Traitor";
                    TeamText.Color = System.Drawing.Color.FromArgb( 200, 0, 0 );
                    break;
                case ( 2 ):
                    TeamText.Caption = "Innocent";
                    TeamText.Color = System.Drawing.Color.FromArgb( 0, 200, 0 );
                    break;
            }
        }

        private void Controls() {
            if( IsControlJustPressed(0, 23) && Game.PlayerPed.Weapons.Current.Hash.ToString() != "Unarmed" ) {


                // Drop current weapon, basegameode handles everything weapon related, grab the name of weapon from current weapon that's all that is needed from weapons.
                foreach( WeaponPickup wep in GameMap.SpawnedWeapons ) {

                    if( (int)wep.WeaponHash == Game.PlayerPed.Weapons.Current.Hash.GetHashCode() ) {
                        WeaponPickup item = new WeaponPickup( GameMap, wep.WeaponHash, wep.WorldModel, Game.Player.Character.Position + new Vector3(0,0,1), true );
                        GameMap.SpawnedWeapons.Add( item );
                        break;
                    }
                }
                Game.PlayerPed.Weapons.Remove( Game.PlayerPed.Weapons.Current );
            }
        }


        private void Events() {

            if( GameMap == null )
                return;

            // Weapon change
            foreach( var weps in GameMap.Weapons ) {
                uint wepHash = (uint)GetHashKey( weps.Key  );
                //uint wepHash = (uint)GetWeaponHashFromPickup( GetHashKey( weps.Value ) );
                if( HasPedGotWeapon( PlayerPedId(), wepHash, false) && !weapons.Contains(weps.Key) ) {
                    weapons.Add( weps.Key );
                    WeaponCount++;
                    PlayerPickedUpWeapon(weps.Key, weapons.Count);
                }

                if( !HasPedGotWeapon( PlayerPedId(), wepHash, false ) && weapons.Contains( weps.Key ) ) {
                    weapons.Remove( weps.Key );
                    WeaponCount--;
                    PlayerDroppedWeapon( weps.Key, weapons.Count );
                }

            }
                
        }

        public int GetWeaponCount() {
            return weapons.Count;
        }

        public virtual void PlayerPickedUpWeapon(string wepName, int count) {
            Debug.WriteLine( "Player has picked up " + wepName );
        }

        public virtual void PlayerDroppedWeapon( string wepName, int count ) {
            Debug.WriteLine( "Player has dropped " + wepName );
        }

        public virtual void Update() {

            Events();
            Controls();

            if( GameMap == null ) {
                TeamText.Color = System.Drawing.Color.FromArgb( 150, 150, 0 );
                TeamText.Caption = "Spectator";
                if( GetFollowPedCamViewMode() != 1 ) {
                    SetFollowPedCamViewMode( 1 );
                }
            } else {
                GameMap.Update();
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

            TeamText.Draw();
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
