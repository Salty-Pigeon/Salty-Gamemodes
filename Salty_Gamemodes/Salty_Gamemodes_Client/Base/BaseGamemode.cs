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

        public SaltyText HealthText;
        public SaltyText AmmoText;
        public SaltyText GameTimeText;
        public SaltyText ScoreText;
        public SaltyText AddScoreText;
        public SaltyText BoundText;
        public SaltyText HUDText;
        public SaltyText GoalText;
        public float GoalTextTime = 0;

        public int Score;
        private float showScoreTimer = 0;
        private float showScoreLength = 500;

        public double GameTime = 0;
        public bool isTimed = false;

        public List<SaltyText> WeaponTexts = new List<SaltyText>();

        public Dictionary<int, Dictionary<string, int>> OtherPlayerInfo = new Dictionary<int, Dictionary<string, int>>();

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

        public Dictionary<uint, string> HashToModel = new Dictionary<uint, string>();

        //private Scaleform playerNameHUD = new Scaleform( "mp_mission_name_freemode" );
        //private Scaleform playerNameHUD = new Scaleform( "RACE_MESSAGE" );

        public Dictionary<string, string> GameWeapons = new Dictionary<string, string>();
        public float lastLooked = 0;

        

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
            HealthText = new SaltyText( 0.085f, 0.895f, 0, 0, 0.5f, "Health: ", 255, 255, 255, 255, false, true, 0, true );
            AmmoText = new SaltyText( 0.085f, 0.935f, 0, 0, 0.5f, "Ammo: ", 255, 255, 255, 255, false, true, 0, true );

            GameTimeText = new SaltyText( 0.121f, 0.855f, 0, 0, 0.5f, "", 255, 255, 255, 255, false, true, 0, true );
            ScoreText = new SaltyText( 0.5f, 0.01f, 0, 0, 0.7f, "Score: 0", 255, 255, 255, 255, false, true, 0, true );
            AddScoreText = new SaltyText( 0.5f + (ScoreText.Caption.Length), 0.025f, 0, 0, 0.3f, "", 255, 255, 255, 255, false, true, 0, true );
            BoundText = new SaltyText( 0.2f, 0.1f, 0, 0, 1, "", 255, 255, 255, 255, false, true, 0, true );
            HUDText = new SaltyText( 0.5f, 0.5f, 0, 0, 0.5f, "", 255, 255, 255, 255, false, true, 0, true );

            GoalText = new SaltyText( 0.5f, 0.1f, 0, 0, 1f, "", 255, 255, 255, 255, false, true, 0, true );

            StripWeapons();

        }

        public virtual void Start() {           

            foreach( var wep in GameMap.Weapons.ToList() ) {        
                if( !GameWeapons.ContainsKey(wep.Key) ) {
                    GameMap.Weapons.Remove( wep.Key );
                }
            }

            foreach( var wep in GameWeapons ) {
                HashToModel.Add( (uint)GetHashKey( wep.Key ), wep.Key );
                Debug.WriteLine( wep.Key + " : " + (uint)GetHashKey( wep.Key ) );
            }

            inGame = true;
        }


        public void DrawGameTimer( ) {
            TimeSpan time = TimeSpan.FromMilliseconds( GameTime - GetGameTimer() );

            GameTimeText.Caption = string.Format( "{0:00}:{1:00}", Math.Ceiling(time.TotalMinutes-1), time.Seconds );

            GameTimeText.Draw();
        }

        public void GiveGun( string weapon, int ammo ) {
            GiveWeaponToPed(PlayerPedId(), (uint)GetHashKey(weapon), ammo, false, true);
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
            float time = showScoreTimer - GetGameTimer();
            if( time >= 0 ) {
                if( time <= (showScoreLength/2) ) {
                    float percent = time / (showScoreLength/2);
                    AddScoreText.Scale = percent * 0.3f;
                } else {
                    float percent = (time-(showScoreLength/2)) / (showScoreLength/2);
                    AddScoreText.Scale = 0.3f - (percent * 0.3f);
                }
                AddScoreText.Draw();
            }
        }

        public void UpdateScore( int score ) {
            AddScoreText.Caption = "+" + (score - Score);
            Score = score;
            showScoreTimer = GetGameTimer() + showScoreLength;
            ScoreText.Caption = "Score: " + score;
            AddScoreText.Position = new Vector2( 0.5f + (ScoreText.Size.X), 0.025f );
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
                foreach( var weapon in PlayerWeapons ) {
                    if( WeaponTexts.Count <= index ) {
                        WeaponTexts.Add( new SaltyText( 0.85f, 0.85f + (index * 0.4f), 0, 0, 0.3f, weapon.Value, 255, 255, 255, 255, false, false, 0, true ) );
                    }

                    if( (uint)Game.PlayerPed.Weapons.Current.Hash.GetHashCode() == (uint)GetHashKey( weapon.Value ) ) {
                        DrawRectangle( 0.85f, 0.85f + (0.04f * index), 0.1f, 0.03f, 200, 200, 0, 200 );
                    }
                    else {
                        DrawRectangle( 0.85f, 0.85f + (0.04f * index), 0.1f, 0.03f, 0, 0, 0, 200 );
                    }

                    WeaponTexts[index].Caption = GameWeapons[weapon.Value];
                    WeaponTexts[index].Position = new Vector2(0.85f, 0.85f + (index * 0.04f) );
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
            uint currentWep = (uint)Game.PlayerPed.Weapons.Current.Hash.GetHashCode();
            var playerWeapons = PlayerWeapons.OrderBy( i => i.Key );
            foreach( var wep in playerWeapons ) {
                if( (uint)GetHashKey( wep.Value ) == currentWep ) {

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

        public void SetGoalTimer( float time ) {
            GoalTextTime = GetGameTimer() + (time * 1000);
        }

        public void DrawGoal() {
            if( GoalTextTime > GetGameTimer() ) {
                GoalText.Draw();
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
                    Debug.WriteLine( weps.Key + " : something" );
                    int slot;
                    if( WeaponSlots.Count == 1 )
                        slot = PlayerWeapons.Count;
                    else {
                        slot = WeaponSlots[weps.Key];
                    }
                    if( PlayerWeapons.ContainsKey(slot) ) {
                        Debug.WriteLine( "Removing!!!" );
                        RemoveWeaponFromPed( PlayerPedId(), wepHash );
                    } else {
                        Debug.WriteLine( "Adding" );
                        PlayerWeapons.Add( slot, weps.Key );
                    }
                    PlayerPickedUpWeapon(weps.Key, PlayerWeapons.Count);
                }

                if( !HasPedGotWeapon( PlayerPedId(), wepHash, false ) && PlayerWeapons.ContainsValue( weps.Key ) ) {
                    RemoveWeapon( weps.Key );
                    WeaponTexts = new List<SaltyText>();
                    PlayerDroppedWeapon( weps.Key, PlayerWeapons.Count );
                }

            }
                
        }

        public static RaycastResult Raycast( Vector3 source, Vector3 direction, float maxDistance, IntersectOptions options, Entity ignoreEntity = null ) {
            Vector3 target = source + direction * maxDistance;

            return new RaycastResult( Function.Call<int>( Hash._CAST_RAY_POINT_TO_POINT, source.X, source.Y, source.Z, target.X, target.Y, target.Z, options, ignoreEntity == null ? 0 : ignoreEntity.Handle, 7 ) );
        }

        public virtual void PlayerPickedUpWeapon(string wepName, int count) {
            PlayerWeapons[WeaponSlots[wepName]] = wepName;
        }

        public virtual void PlayerDroppedWeapon( string wepName, int count ) {
            PlayerWeapons.Remove( WeaponSlots[wepName] );

        }

        public virtual void ShowNames() {
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

        public virtual void UpdatePlayerInfo( int entID, string key, int value  ) {
            if (OtherPlayerInfo.ContainsKey(entID)) {
                if (OtherPlayerInfo[entID].ContainsKey(key)) {
                    OtherPlayerInfo[entID][key] = value;
                }
                else {
                    OtherPlayerInfo[entID].Add(key, value);
                }
            }
            else {
                OtherPlayerInfo.Add(entID, new Dictionary<string, int>() { { key, value } });
            }
        }

        public bool GetPlayerBool( int entID, string key ) {
            if( OtherPlayerInfo.ContainsKey(entID)) {
                if (OtherPlayerInfo[entID].ContainsKey(key))
                    return OtherPlayerInfo[entID][key] != 0;
            }
            return false;
        }

        public virtual void HUD() {
            if( inGame ) {
                GameMap.DrawBoundarys();
            }

            DrawGoal();

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

        public virtual void RemoveWeaponCheck() {
            if( Game.PlayerPed.Weapons.Current.Hash.ToString() != "Unarmed" && inGame ) {
                if( !HashToModel.ContainsKey( (uint)Game.PlayerPed.Weapons.Current.Hash.GetHashCode() ) ) {
                    Debug.WriteLine( "Weapon removed" );
                    Game.PlayerPed.Weapons.Remove( Game.PlayerPed.Weapons.Current );
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
                    BoundText.Colour = System.Drawing.Color.FromArgb( 255, 0, 0 );
                    BoundText.Caption = "You have " + Math.Round( secondsLeft / 1000 ) + " seconds to return or you will die.";
                    BoundText.Draw();

                }
            }

            RemoveWeaponCheck();

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
