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


        public Text TeamText;
        public Text HealthText;
        public Text AmmoText;

        public Text PrimaryGunText;
        public Text SecondaryGunText;
        public Text SpecialGunText;

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

        public float lastScroll = 0;

        public Dictionary<int, string> WeaponInv = new Dictionary<int, string>();

        public Dictionary<string, int> WeaponSlots = new Dictionary<string, int>() {
            { "WEAPON_PISTOL", 2  },
            { "WEAPON_COMBATPISTOL", 2 },
            { "WEAPON_MICROSMG", 2 },
            { "WEAPON_SMG", 1 },
            { "WEAPON_CARBINERIFLE", 1  },
            { "WEAPON_ASSAULTRIFLE", 1  },
            //{ "WEAPON_SNIPERRIFLE", 1 },
            { "WEAPON_PUMPSHOTGUN", 1  },
            { "WEAPON_COMBATMG", 1 }
        };


        public GameState CurrentState = GameState.None;

        public TTT( Map gameMap, int team ) {

            GameWeapons = new Dictionary<string, string>() {
                { "WEAPON_PISTOL", "Pistol"  },
                { "WEAPON_COMBATPISTOL", "Combat Pistol" },
                { "WEAPON_SMG", "SMG" },
                { "WEAPON_CARBINERIFLE", "Carbine"  },
                { "WEAPON_ASSAULTRIFLE", "AK47"  },
                //{ "WEAPON_SNIPERRIFLE", "Sniper" },
                { "WEAPON_PUMPSHOTGUN", "Pump Shotgun"  },
                { "WEAPON_MICROSMG", "Micro-SMG" },
                { "WEAPON_COMBATMG", "Light Machine Gun" }
            };

            TeamText = new Text( "Spectator", new System.Drawing.PointF( Screen.Width * 0.033f, Screen.Height * 0.855f ), 0.5f );
            HealthText = new Text( "Health: ", new System.Drawing.PointF( Screen.Width * 0.033f, Screen.Height * 0.895f ), 0.5f );
            AmmoText = new Text( "Ammo: ", new System.Drawing.PointF( Screen.Width * 0.033f, Screen.Height * 0.935f ), 0.5f );

            PrimaryGunText = new Text( "", new System.Drawing.PointF( Screen.Width * 0.8f, Screen.Height * 0.935f ), 0.3f );
            SecondaryGunText = new Text( "", new System.Drawing.PointF( Screen.Width * 0.8f, Screen.Height * 0.895f ), 0.3f );
            SpecialGunText = new Text( "", new System.Drawing.PointF( Screen.Width * 0.8f, Screen.Height * 0.855f ), 0.3f );


            GameMap = gameMap;
            GameMap.Gamemode = this;
            GameMap.CreateBlip();
            SetTeam( team );
        }

        public override void Start() {
            Game.Player.Character.MaxHealth = 100;
            Game.Player.Character.Health = 100;
            WriteChat( "Game starting" );
            base.Start();
        }

        public override void End() {
            WriteChat( "Game ending" );
            base.End();

        }


        public override void Controls() {

            if( IsControlJustPressed(2, 15) ) {
                ChangeSelectedWeapon( +1 );
            }

            if( IsControlJustPressed( 2, 14 ) ) {
                ChangeSelectedWeapon( -1 );
            }

            if( IsControlJustPressed( 0, 23 ) && Game.PlayerPed.Weapons.Current.Hash.ToString() != "Unarmed" ) {


                // Drop current weapon, basegameode handles everything weapon related, grab the name of weapon from current weapon that's all that is needed from weapons.
                
                foreach( WeaponPickup wep in GameMap.CreatedWeapons.Values.ToList() ) {
                    if( (wep.WorldModel == Game.PlayerPed.Weapons.Current.Model.GetHashCode()) ) {
                        WeaponPickup item = new WeaponPickup( GameMap, wep.WeaponModel, wep.WeaponHash, wep.WorldModel, Game.Player.Character.Position, true, Game.PlayerPed.Weapons.Current.Ammo );
                        item.Throw();
                        GameMap.SpawnedWeapons.Add( item );
                        Game.PlayerPed.Weapons.Remove( Game.PlayerPed.Weapons.Current );
                        break;
                    }
                }
            }

            base.Controls();
        }

        public void ChangeSelectedWeapon( int offset ) {
            lastScroll = GetGameTimer();
            int index = 0;
            Weapon wepon = Game.PlayerPed.Weapons.Current;

            foreach( var wep in WeaponInv ) {
                if( GetHashKey( wep.Value ) == wepon.Hash.GetHashCode() ) {
                    if( index + offset < 0 ) {
                        SetCurrentPedWeapon( PlayerPedId(), (uint)GetHashKey(WeaponInv.ElementAt(WeaponInv.Count-1).Value), true );
                    } else if ( index + offset >= WeaponInv.Count ) {
                        SetCurrentPedWeapon( PlayerPedId(), (uint)GetHashKey( WeaponInv.ElementAt( 0 ).Value ), true );
                    } else {
                        SetCurrentPedWeapon( PlayerPedId(), (uint)GetHashKey( WeaponInv.ElementAt( index + offset ).Value ), true );
                    }
                    break;
                }
                index++;
            }
        }

        public override bool CanPickupWeapon( string weaponModel ) {
            bool canPickup = true;
            if( WeaponInv.ContainsKey( WeaponSlots[weaponModel] ) )
                canPickup = false;
           
            return canPickup;
        }

        public override void PlayerPickedUpWeapon( string wepName, int count ) {
            lastScroll = GetGameTimer();
            WeaponInv[WeaponSlots[wepName]] = wepName;
            base.PlayerPickedUpWeapon( wepName, count );
        }

        public override void PlayerDroppedWeapon( string wepName, int count ) {
            WeaponInv.Remove( WeaponSlots[wepName] );
            base.PlayerDroppedWeapon( wepName, count );
        }

        public override void PlayerDied( int killerType, Vector3 deathcords ) {
            SetTeam( (int)Teams.Spectators );
            base.PlayerDied( killerType, deathcords );
        }

        public override void PlayerSpawned( ExpandoObject spawnInfo ) {
            Game.Player.Character.MaxHealth = 100;
            Game.Player.Character.Health = 100;
            base.PlayerSpawned( spawnInfo );
        }

        public override void HUD() {
            
            if( Game.PlayerPed.Weapons.Current.Hash.ToString() == "SniperRifle" && isScoped ) {

            } else {
                HideHudAndRadarThisFrame();
            }
            HideReticle();

            if( Team == (int)Teams.Traitors ) {
                DrawRectangle( 0.025f, 0.86f, 0.07f, 0.03f, 200, 0, 0, 200 );
            }

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


            TeamText.Draw();
            HealthText.Draw();
            AmmoText.Draw();

            DrawWeaponHUD();

            base.HUD();
        }

        public void DrawWeaponHUD() {
            if( lastScroll + (2 * 1000) > GetGameTimer() ) {
                var resolution = Screen.Resolution;
                foreach( var weapon in PlayerWeapons ) {
                    int boxIndex = WeaponSlots[weapon] - 1;
                    if( Game.PlayerPed.Weapons.Current.Hash.GetHashCode() == GetHashKey(weapon) ) {
                        DrawRectangle( 0.85f, 0.9f + (0.04f * boxIndex), 0.1f, 0.03f, 200, 200, 0, 200 );
                    }
                    else {
                        DrawRectangle( 0.85f, 0.9f + (0.04f * boxIndex), 0.1f, 0.03f, 0, 0, 0, 200 );
                    }

                    if( WeaponSlots[weapon] == 1 ) {
                        PrimaryGunText.Caption = GameWeapons[weapon];
                        PrimaryGunText.Position = new System.Drawing.PointF( Screen.Width * 0.85f, Screen.Height * 0.9f );
                        PrimaryGunText.Draw();
                    } else if ( WeaponSlots[weapon] == 2 ) {
                        SecondaryGunText.Caption = GameWeapons[weapon]; 
                        SecondaryGunText.Position = new System.Drawing.PointF( Screen.Width * 0.85f, Screen.Height * 0.94f );
                        SecondaryGunText.Draw();
                    }
                    else if( WeaponSlots[weapon] == 3 ) {

                    }
                }

                
                SpecialGunText.Draw();

            }

        }

        public override void SetTeam( int team ) {
            switch( team ) {
                case (0):
                    TeamText.Caption = "Spectator";
                    TeamText.Color = System.Drawing.Color.FromArgb( 150, 150, 0 );
                    break;
                case (1):
                    TeamText.Caption = "Traitor";
                    TeamText.Color = System.Drawing.Color.FromArgb( 255, 255, 255 );
                    break;
                case (2):
                    TeamText.Caption = "Innocent";
                    TeamText.Color = System.Drawing.Color.FromArgb( 255, 255, 255 );
                    break;
            }
            base.SetTeam( team );
        }

        public override void Update() {

            FirstPersonForAlive();

            base.Update();

        }
    }
}
