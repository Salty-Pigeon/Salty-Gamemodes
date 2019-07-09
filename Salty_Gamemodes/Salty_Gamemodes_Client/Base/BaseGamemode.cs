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

        public HUD ActiveHUD;


        public Vector3 PlayerSpawn =  Vector3.Zero;

        public int Score;
        

        public double GameTime = 0;
        public bool isTimed = false;


        public Dictionary<int, Dictionary<string, int>> OtherPlayerInfo = new Dictionary<int, Dictionary<string, int>>();

        public bool isNoclip = false;

        public Dictionary<uint, List<int>> AmmoInClip = new Dictionary<uint, List<int>>();


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

        public float lastScroll = 0;

        public bool inGame = false;
        public Map GameMap;
        public string MapTag = "";

        public Vector3 noclipSpawnPos = Vector3.Zero;

        private float deathTimer = 0;
        private float gracePeriod = 10 * 1000;

        public int Team;

        public BaseGamemode() {
            ActiveHUD = new HUD(this);
            OtherPlayerInfo = new Dictionary<int, Dictionary<string, int>>();
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
            }

            inGame = true;
        }


       

        public void GiveGun( string weapon, int ammo ) {
            GiveWeaponToPed(PlayerPedId(), (uint)GetHashKey(weapon), ammo, false, true);
        }  

        public List<int> GetInGamePlayers() {
            List<int> players = new List<int>();
            foreach( var ply in OtherPlayerInfo ) {
                if( GetTeam(ply.Key) != 0 ) {
                    players.Add( ply.Key );
                }
            }
            return players;
        }

        public int GetTeam( int ply ) {
            if( OtherPlayerInfo.ContainsKey( ply ) ) {
                if( OtherPlayerInfo[ply].ContainsKey( "Team" ) )
                    return OtherPlayerInfo[ply]["Team"];
            }
            return 0;
        }

        public void UpdateScore( int score ) {
            ActiveHUD.UpdateScore(score, score - Score);
            Score = score;
        }

        public void AddScore(int offset) {
            UpdateScore( Score + offset );
            TriggerServerEvent( "salty::netAddScore", offset );
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
            string lastWepGroup = Game.PlayerPed.Weapons.Current.Group.ToString();
            int next = GetNextWeapon( offset );
            try {
                int pedId = PlayerPedId();
                uint code = (uint)Game.PlayerPed.Weapons.Current.Hash.GetHashCode();
                if( lastWepGroup != "Melee" && lastWepGroup != "Unarmed" ) {
                    if( AmmoInClip.ContainsKey( code ) ) {
                        AmmoInClip[code][0] = Game.PlayerPed.Weapons.Current.AmmoInClip;
                        AmmoInClip[code][1] = Game.PlayerPed.Weapons.Current.Ammo;
                    }
                    else {
                        AmmoInClip.Add( code, new List<int> { Game.PlayerPed.Weapons.Current.AmmoInClip, Game.PlayerPed.Weapons.Current.Ammo } );
                    }
                }
                uint newCode = (uint)GetHashKey( PlayerWeapons[next] );
                SetCurrentPedWeapon( pedId, newCode, true );
                if( AmmoInClip.ContainsKey( newCode ) ) {
                    SetAmmoInClip( pedId, newCode, AmmoInClip[newCode][0] );
                    SetPedAmmo( pedId, newCode, AmmoInClip[newCode][1]);
                    AmmoInClip.Remove( newCode );
                }
            } catch {

            }
        }

        public virtual void End() {
            SetNoClip( true );
            if( GameMap != null )
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
            noclipSpawnPos = deathcords;
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
            AmmoInClip = new Dictionary<uint, List<int>>();

        }

        public virtual void SetTeam( int team ) {
            Team = team;
            if( team == 0 ) {
                NetworkSetVoiceChannel( 0 );
            }
        }

        public virtual void Controls() {
             
        }

        public virtual bool CanPickupWeapon( string weaponModel ) {
            return !isNoclip;
        }

        public bool HasWeapon( string weaponModel ) {
            return PlayerWeapons.ContainsValue( weaponModel );
        }

        public void RemoveWeapon( string weaponName, bool fromPlayer ) {
            foreach( var i in PlayerWeapons.Keys.ToList() )  {
                if( PlayerWeapons[i] == weaponName )
                    PlayerWeapons.Remove( i );
            }
            uint key = (uint)GetHashKey( weaponName );
            if( AmmoInClip.ContainsKey( key )){
                AmmoInClip.Remove( (uint)GetHashKey( weaponName ) );
            }
            if( fromPlayer )
                RemoveWeaponFromPed( PlayerPedId(), key );
        }

        public void RemoveWeapon( Weapon removedWeapon ) {
            string weaponName = HashToModel[(uint)removedWeapon.Hash.GetHashCode()];
            foreach( var i in PlayerWeapons.Keys.ToList() ) {
                if( PlayerWeapons[i] == weaponName )
                    PlayerWeapons.Remove( i );
            }
            uint key = (uint)GetHashKey( weaponName );
            if( AmmoInClip.ContainsKey( key ) ) {
                AmmoInClip.Remove( (uint)GetHashKey( weaponName ) );
            }

            Game.PlayerPed.Weapons.Remove( removedWeapon );
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
                    RemoveWeapon( weps.Key, false );
                    ActiveHUD.WeaponTexts = new List<SaltyText>();
                    PlayerDroppedWeapon( weps.Key, PlayerWeapons.Count );
                }

            }
                
        }

        public virtual void PlayerPickedUpWeapon(string wepName, int count) {
            PlayerWeapons[WeaponSlots[wepName]] = wepName;
        }

        public virtual void PlayerDroppedWeapon( string wepName, int count ) {
            PlayerWeapons.Remove( WeaponSlots[wepName] );

        }

        public virtual void OnUpdatedPlayerInfo( int entID, string key, int value ) {

        }

        public void UpdatePlayerInfo( int entID, string key, int value  ) {
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
            OnUpdatedPlayerInfo( entID, key, value );
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
            ActiveHUD.Draw();
        }

        public virtual void RemoveWeaponCheck() {
            if( Game.PlayerPed.Weapons.Current.Hash.ToString() != "Unarmed" && inGame ) {
                if( !HashToModel.ContainsKey( (uint)Game.PlayerPed.Weapons.Current.Hash.GetHashCode() ) ) {
                    RemoveWeapon( Game.PlayerPed.Weapons.Current );
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
                DisableControlAction( 0, 0, true );
                if( GetFollowPedCamViewMode() != 1 ) {
                    SetFollowPedCamViewMode( 1 );
                }
            }

            if( Game.PlayerPed.Weapons.Current.Ammo <= 0 && Game.PlayerPed.Weapons.Current.Group.ToString() != "Melee" ) {
                RemoveWeapon( Game.PlayerPed.Weapons.Current );
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
                    ActiveHUD.BoundText.Colour = System.Drawing.Color.FromArgb( 255, 0, 0 );
                    ActiveHUD.BoundText.Caption = "You have " + Math.Round( secondsLeft / 1000 ) + " seconds to return or you will die.";
                    ActiveHUD.BoundText.Draw();

                }
            }

            RemoveWeaponCheck();

            if( isTimed && GameTime != 0 ) {
                if( GameTime - GetGameTimer() < 0 ) {
                    isTimed = false;
                }
            }

            
        }


        public void CantEnterVehichles() {
            SetPlayerMayNotEnterAnyVehicle( PlayerId() );
            DisableControlAction( 0, 75, true );
        }

        public void CantExitVehichles() {
            DisableControlAction( 0, 75, true );
        }


        private Vector3 noclipPos;

        public virtual void SetNoClip( bool toggle ) {
            if( noclipSpawnPos != Vector3.Zero && toggle ) {
                Game.PlayerPed.Position = noclipSpawnPos;
                noclipSpawnPos = Vector3.Zero;
            } else {
                noclipPos = Game.PlayerPed.Position;
            }
            deathTimer = 0;
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
                DisableControlAction( 0, 0, true );
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

        public void WriteChat( string prefix, string str, int r, int g, int b ) {
            TriggerEvent( "chat:addMessage", new {
                color = new[] { r, g, b },
                args = new[] { prefix, str }
            } );
        }

    }
}
