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

        public Vector3 PlayerSpawn =  Vector3.Zero;

        public Text HealthText;
        public Text AmmoText;
        public Text GameTimeText;
        public Text ScoreText;

        public int Score;

        public double GameTime = 0;
        public bool isTimed = false;

        public List<Text> WeaponTexts = new List<Text>();

        public bool isNoclip = false;

        public float lastScroll = 0;


        public Dictionary<int, string> PlayerWeapons = new Dictionary<int, string> {
            { 0, "WEAPON_UNARMED" }
        };

        public Dictionary<string, int> WeaponSlots = new Dictionary<string, int>() {
            { "WEAPON_UNARMED", 0 },
        };

        public Dictionary<string, int> WeaponMaxAmmo = new Dictionary<string, int>() {
            { "WEAPON_UNARMED", 0 },
        };

        private Dictionary<int, string> HashToModel = new Dictionary<int, string>();

        //private Scaleform playerNameHUD = new Scaleform( "mp_mission_name_freemode" );
        //private Scaleform playerNameHUD = new Scaleform( "RACE_MESSAGE" );

        public Dictionary<string, string> GameWeapons = new Dictionary<string, string>();
        public float lastLooked = 0;

        public Text BoundText;
        public Text HUDText;

        public bool isScoped = false;

        public bool inGame = false;
        public Map GameMap;
        public string MapTag = "";

        public Vector3 noclipPos = Vector3.Zero;
        Vector3 deathPos;
        private float deathTimer = 0;
        private float gracePeriod = 10 * 1000;

        public int Team;

        public BaseGamemode() {
            HealthText = new Text( "Health: ", new System.Drawing.PointF( Screen.Width * 0.033f, Screen.Height * 0.895f ), 0.5f );
            AmmoText = new Text( "Ammo: ", new System.Drawing.PointF( Screen.Width * 0.033f, Screen.Height * 0.935f ), 0.5f );

            GameTimeText = new Text( "", new System.Drawing.PointF( Screen.Width * 0.1f, Screen.Height * 0.855f ), 0.5f );
            ScoreText = new Text( "Score: 0", new System.Drawing.PointF( Screen.Width * 0.5f, Screen.Height * 0.01f ), 0.7f, System.Drawing.Color.FromArgb(255,255,255) );
            ScoreText.Centered = true;

            BoundText = new Text( "", new System.Drawing.PointF( Screen.Width * 0.2f, Screen.Height * 0.1f ), 1.0f );
            HUDText = new Text( "", new System.Drawing.PointF( Screen.Width * 0.5f, Screen.Height * 0.5f ), 0.5f );
            HUDText.Centered = true;
            StripWeapons();

        }

        public virtual void Start() {
            foreach( var wep in GameMap.Weapons.ToList() ) {        
                if( !GameWeapons.ContainsKey(wep.Key) ) {
                    GameMap.Weapons.Remove( wep.Key );
                }
            }

            foreach( var wep in GameWeapons ) {
                HashToModel.Add( GetHashKey( wep.Key ), wep.Key );
            }

            inGame = true;
        }


        public void DrawGameTimer( ) {
            TimeSpan time = TimeSpan.FromMilliseconds( GameTime - GetGameTimer() );

            GameTimeText.Caption = string.Format( "{0:00}:{1:00}", Math.Ceiling(time.TotalMinutes-1), time.Seconds );

            GameTimeText.Draw();
        }

        public void DrawBaseHealthHUD() {

            HideHudAndRadarThisFrame();

            if (Team == 0)
                return;

            HealthText.Caption = Game.Player.Character.Health.ToString();
            AmmoText.Caption = Game.PlayerPed.Weapons.Current.AmmoInClip + " / " + Game.PlayerPed.Weapons.Current.Ammo;

            DrawRectangle( 0.025f, 0.9f, 0.12f, 0.03f, 0, 0, 0, 200 );
            float healthPercent = (float)Game.Player.Character.Health / Game.Player.Character.MaxHealth;
            if( healthPercent < 0 )
                healthPercent = 0;
            if( healthPercent > 1 )
                healthPercent = 1;
            DrawRectangle( 0.025f, 0.9f, healthPercent * 0.12f, 0.03f, 200, 0, 0, 200 );

            float ammoPercent = (float)Game.PlayerPed.Weapons.Current.AmmoInClip / Game.PlayerPed.Weapons.Current.MaxAmmoInClip;

            DrawRectangle( 0.025f, 0.94f, 0.12f, 0.03f, 0, 0, 0, 200 );

            DrawRectangle( 0.025f, 0.94f, ammoPercent * 0.12f, 0.03f, 200, 200, 0, 200 );

            HealthText.Draw();
            AmmoText.Draw();

        }

        public void DrawScore() {
            ScoreText.Draw();
        }

        public void UpdateScore( int score ) {
            WriteChat( "New score " + score );
            Score += score;
            ScoreText.Caption = "Score: " + score;
        }

        public void AddScore(int offset) {
            UpdateScore( Score + offset );
            TriggerServerEvent( "salty::netAddScore", offset );
        }

        public void DrawBaseWeaponHUD() {

            if( Team == 0)
                return;

            if ( lastScroll + (2 * 1000) > GetGameTimer() ) {
                int index = 0;
                var resolution = Screen.Resolution;
                foreach( var weapon in PlayerWeapons ) {
                    if( WeaponTexts.Count <= index ) {
                        WeaponTexts.Add( new Text( weapon.Value, new System.Drawing.PointF( Screen.Width * 0.85f, Screen.Height * 0.85f + (index * 0.4f) ), 0.3f ) );
                    }

                    if( Game.PlayerPed.Weapons.Current.Hash.GetHashCode() == GetHashKey( weapon.Value ) ) {
                        DrawRectangle( 0.85f, 0.85f + (0.04f * index), 0.1f, 0.03f, 200, 200, 0, 200 );
                    }
                    else {
                        DrawRectangle( 0.85f, 0.85f + (0.04f * index), 0.1f, 0.03f, 0, 0, 0, 200 );
                    }

                    WeaponTexts[index].Caption = GameWeapons[weapon.Value];
                    WeaponTexts[index].Position = new System.Drawing.PointF( Screen.Width * 0.85f, Screen.Height * (0.85f + (index * 0.04f)) );
                    WeaponTexts[index].Draw();

                    index++;
                }
            }

            if( IsControlJustPressed( 2, 15 ) ) {
                ChangeSelectedWeapon( +1 );
            }

            if( IsControlJustPressed( 2, 14 ) ) {
                ChangeSelectedWeapon( -1 );
            }

        }

        public int GetNextWeapon( int offset ) {
            int index = 0;
            int currentWep = Game.PlayerPed.Weapons.Current.Hash.GetHashCode();
            var playerWeapons = PlayerWeapons.OrderBy( i => i.Key );
            foreach( var wep in playerWeapons ) {
                if( GetHashKey( wep.Value ) == currentWep ) {

                    if( index + offset > PlayerWeapons.Count-1 && offset > 0 ) 
                        return playerWeapons.ElementAt( 0 ).Key;

                    if( index + offset < 0 && offset < 0 ) 
                        return playerWeapons.ElementAt( PlayerWeapons.Count - 1 ).Key;

                    if( offset > 0 ) 
                        return playerWeapons.ElementAt( index + 1 ).Key;
                    else
                        return playerWeapons.ElementAt( index - 1 ).Key;
                }
                index++;
            }
            return -1;
        }

        public virtual void ChangeSelectedWeapon( int offset ) {
            lastScroll = GetGameTimer();
            int next = GetNextWeapon( offset );
            try {
                SetCurrentPedWeapon( PlayerPedId(), (uint)GetHashKey( PlayerWeapons[next] ), true );
            } catch {
                Debug.WriteLine( "No weapons" );
            }
        }

        public virtual void End() {
            GameMap.ClearBlip();
            inGame = false;

        }


        public void CreateGameTimer( double length ) {
            GameTime = GetGameTimer() + length;
            isTimed = true;
        }

        public virtual void PlayerKilled( int killerID, ExpandoObject deathData ) {

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
            PlayerWeapons = new Dictionary<int, string>() { { 0, "WEAPON_UNARMED" } };

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
            return !isNoclip;
        }

        public bool HasWeapon( string weaponModel ) {
            return PlayerWeapons.ContainsValue( weaponModel );
        }

        public void RemoveWeapon( string weaponName ) {
            foreach( var i in PlayerWeapons.Keys.ToList() )  {
                if( PlayerWeapons[i] == weaponName )
                    PlayerWeapons.Remove( i );
            }
        }

        public virtual void Events() {

            if( GameMap == null )
                return;

            // Weapon change
            foreach( var weps in GameWeapons ) {
                uint wepHash = (uint)GetHashKey( weps.Key  );
                //uint wepHash = (uint)GetWeaponHashFromPickup( GetHashKey( weps.Value ) );
                if( HasPedGotWeapon( PlayerPedId(), wepHash, false) && !PlayerWeapons.ContainsValue(weps.Key) ) {

                    int slot;
                    if( WeaponSlots.Count == 1 )
                        slot = PlayerWeapons.Count;
                    else {
                        slot = WeaponSlots[weps.Key];
                    }
                    if( PlayerWeapons.ContainsKey(slot) ) {
                        RemoveWeaponFromPed( PlayerPedId(), wepHash );
                    } else {
                        PlayerWeapons.Add( slot, weps.Key );
                    }
                    PlayerPickedUpWeapon(weps.Key, PlayerWeapons.Count);
                }

                if( !HasPedGotWeapon( PlayerPedId(), wepHash, false ) && PlayerWeapons.ContainsValue( weps.Key ) ) {
                    RemoveWeapon( weps.Key );
                    WeaponTexts = new List<Text>();
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
        
        public void ShowNames() {
            Vector3 position = Game.PlayerPed.ForwardVector;

            RaycastResult result = Raycast( Game.PlayerPed.Position, position, 75, IntersectOptions.Peds1, null );
            if( result.DitHitEntity ) {
                if( result.HitEntity != Game.PlayerPed ) {
                    int ent = NetworkGetEntityFromNetworkId( result.HitEntity.NetworkId );
                    if (IsPedAPlayer( ent ) ) {
                        HUDText.Caption = GetPlayerName( GetPlayerPed( ent ) ).ToString();
                        lastLooked = GetGameTimer();
                    }

                }

            }
        }


        public virtual void HUD() {
            if( inGame ) {
                GameMap.DrawBoundarys();
            }

           
            if( lastLooked + 300 > GetGameTimer() ) {
                HUDText.Draw();
            }

            if( isTimed )
                DrawGameTimer();
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

            if( Team == 0 ) {
                if( GetFollowPedCamViewMode() != 1 ) {
                    SetFollowPedCamViewMode( 1 );
                }
            }

            if( Game.PlayerPed.Weapons.Current.Ammo <= 0 && Game.PlayerPed.Weapons.Current.Group.ToString() != "Melee" ) {
                Game.PlayerPed.Weapons.Remove( Game.PlayerPed.Weapons.Current );
            }

            if( Game.PlayerPed.Weapons.Current.Hash.ToString() != "Unarmed" && inGame )
                if( !HashToModel.ContainsKey( Game.PlayerPed.Weapons.Current.Hash.GetHashCode() ) )
                    Game.PlayerPed.Weapons.Remove( Game.PlayerPed.Weapons.Current );
                

            if( isNoclip ) {
                SetPlayerMayNotEnterAnyVehicle( PlayerId() );
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

            if( isTimed && GameTime != 0 ) {
                if( GameTime - GetGameTimer() < 0 ) {
                    isTimed = false;
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
