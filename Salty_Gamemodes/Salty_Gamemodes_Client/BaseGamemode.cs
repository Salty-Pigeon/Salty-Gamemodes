using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using CitizenFX.Core.Native;

namespace Salty_Gamemodes_Client {
    public class BaseGamemode : BaseScript {


        public Text HealthText;
        public Text AmmoText;

        public List<Text> WeaponTexts = new List<Text>();

        public bool isNoclip = false;

        public float lastScroll = 0;


        public List<string> PlayerWeapons = new List<string>() {
            "WEAPON_UNARMED"
        };

        //private Scaleform playerNameHUD = new Scaleform( "mp_mission_name_freemode" );
        //private Scaleform playerNameHUD = new Scaleform( "RACE_MESSAGE" );

        public Dictionary<string, string> GameWeapons = new Dictionary<string, string>();
        public float lastLooked = 0;

        public Text BoundText;
        public Text HUDText;

        public bool isScoped = false;

        public bool inGame = false;
        public Map GameMap;

        public Vector3 noclipPos = Vector3.Zero;
        Vector3 deathPos;
        private float deathTimer = 0;
        private float gracePeriod = 10 * 1000;

        public int Team;

        public BaseGamemode() {
            HealthText = new Text( "Health: ", new System.Drawing.PointF( Screen.Width * 0.033f, Screen.Height * 0.895f ), 0.5f );
            AmmoText = new Text( "Ammo: ", new System.Drawing.PointF( Screen.Width * 0.033f, Screen.Height * 0.935f ), 0.5f );

            BoundText = new Text( "", new System.Drawing.PointF( Screen.Width * 0.2f, Screen.Height * 0.1f), 1.0f );
            HUDText = new Text( "", new System.Drawing.PointF( Screen.Width * 0.5f, Screen.Height * 0.5f), 1.0f );
            HUDText.Centered = true;

            
        }

        public virtual void Start() {
            StripWeapons();
            foreach( var wep in GameMap.Weapons.ToList() ) {
                if( !GameWeapons.ContainsKey(wep.Key) ) {
                    GameMap.Weapons.Remove( wep.Key );
                }
            }
            inGame = true;
        }

        public void DrawBaseHealthHUD() {
            HideHudAndRadarThisFrame();

            HealthText.Caption = Game.Player.Character.Health.ToString();
            AmmoText.Caption = Game.PlayerPed.Weapons.Current.AmmoInClip + " / " + Game.PlayerPed.Weapons.Current.Ammo;

            DrawRectangle( 0.025f, 0.9f, 0.1f, 0.03f, 0, 0, 0, 200 );
            float healthPercent = (float)Game.Player.Character.Health / Game.Player.Character.MaxHealth;
            if( healthPercent < 0 )
                healthPercent = 0;
            if( healthPercent > 1 )
                healthPercent = 1;
            DrawRectangle( 0.025f, 0.9f, (healthPercent) * 0.1f, 0.03f, 200, 0, 0, 200 );

            DrawRectangle( 0.025f, 0.94f, 0.1f, 0.03f, 200, 200, 0, 200 );
        }

        public void DrawBaseWeaponHUD() {
            if( lastScroll + (2 * 1000) > GetGameTimer() ) {
                int index = 0;
                var resolution = Screen.Resolution;
                foreach( var weapon in PlayerWeapons ) {
                    if( WeaponTexts.Count <= index ) {
                        WeaponTexts.Add( new Text( weapon, new System.Drawing.PointF( Screen.Width * 0.85f, Screen.Height * 0.85f + (index * 0.4f) ), 0.3f ) );
                    }

                    if( Game.PlayerPed.Weapons.Current.Hash.GetHashCode() == GetHashKey( weapon ) ) {
                        DrawRectangle( 0.85f, 0.85f + (0.04f * index), 0.1f, 0.03f, 200, 200, 0, 200 );
                    }
                    else {
                        DrawRectangle( 0.85f, 0.85f + (0.04f * index), 0.1f, 0.03f, 0, 0, 0, 200 );
                    }

                    WeaponTexts[index].Caption = GameWeapons[weapon];
                    WeaponTexts[index].Position = new System.Drawing.PointF( Screen.Width * 0.85f, Screen.Height * (0.85f + (index * 0.04f)) );
                    WeaponTexts[index].Draw();

                    index++;
                }
            }
        }

        public void ChangeSelectedWeapon( int offset ) {
            lastScroll = GetGameTimer();
            int index = 0;
            Weapon wepon = Game.PlayerPed.Weapons.Current;

            foreach( var wep in PlayerWeapons ) {
                if( GetHashKey( wep ) == wepon.Hash.GetHashCode() ) {
                    if( index + offset < 0 ) {
                        SetCurrentPedWeapon( PlayerPedId(), (uint)GetHashKey( PlayerWeapons[PlayerWeapons.Count - 1 ] ), true );
                    }
                    else if( index + offset >= PlayerWeapons.Count ) {
                        SetCurrentPedWeapon( PlayerPedId(), (uint)GetHashKey( PlayerWeapons[0] ), true );
                    }
                    else {
                        SetCurrentPedWeapon( PlayerPedId(), (uint)GetHashKey( PlayerWeapons[index + offset] ), true );
                    }
                    break;
                }
                index++;
            }
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

        public void AddGunToSpawns( string weaponModel, string name ) {
            if( GameWeapons.ContainsKey( weaponModel ) )
                GameWeapons[weaponModel] = name;
            else
                GameWeapons.Add( weaponModel, name );
        }

        public virtual bool IsBase() {
            return !(GetType().IsSubclassOf( typeof( BaseGamemode ) ));
        }

        public void StripWeapons() {
            RemoveAllPedWeapons( PlayerPedId(), true );
            PlayerWeapons = new List<string>() { "WEAPON_UNARMED" };

        }

        public virtual void SetTeam( int team ) {
            Team = team;
        }

        public void DrawRectangle( float x, float y, float width, float height, int r, int g, int b, int alpha ) {
            DrawRect( x + (width / 2), y + (height / 2), width, height, r, g, b, alpha );
        }

        public virtual void Controls() {
            isScoped = IsControlPressed( 1, 25 );
        }

        public virtual bool CanPickupWeapon( string weaponModel ) {
            return true;
        }

        public bool HasWeapon( string weaponModel ) {
            return PlayerWeapons.Contains( weaponModel );
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

        public static RaycastResult Raycast( Vector3 source, Vector3 direction, float maxDistance, IntersectOptions options, Entity ignoreEntity = null ) {
            Vector3 target = source + direction * maxDistance;

            return new RaycastResult( Function.Call<int>( Hash._CAST_RAY_POINT_TO_POINT, source.X, source.Y, source.Z, target.X, target.Y, target.Z, options, ignoreEntity == null ? 0 : ignoreEntity.Handle, 7 ) );
        }

        public virtual void PlayerPickedUpWeapon(string wepName, int count) {

        }

        public virtual void PlayerDroppedWeapon( string wepName, int count ) {

        }

        public virtual void HUD() {
            if( inGame ) {
                GameMap.DrawBoundarys();
            }

            Vector3 position = Game.PlayerPed.ForwardVector;

            RaycastResult result = Raycast( Game.PlayerPed.Position, position, 75, IntersectOptions.Peds1, null );
            if( result.DitHitEntity ) {
                if( result.HitEntity != Game.PlayerPed ) {
                    HUDText.Caption = result.HitEntity.Model.Hash.ToString();
                    lastLooked = GetGameTimer();
                }
            
            }

            if( lastLooked + 300 > GetGameTimer() ) {
                HUDText.Draw();
            }

        }

        public void HideReticle() {
            Weapon w = Game.PlayerPed?.Weapons?.Current;

            if( w != null ) {
                WeaponHash wHash = w.Hash;
                if( wHash.ToString() != "SniperRifle" ) {
                    HideHudComponentThisFrame( 14 );
                }
                else if( !isScoped ) {                 
                    HideHudComponentThisFrame( 14 );
                }
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
                    //SetFollowPedCamViewMode( 1 );
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
