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
    class TTT : BaseGamemode {


        public Text TeamText;
        

        public enum Teams {
            Spectators,
            Traitors,
            Innocents,
            Detectives
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
            GameMap.Gamemode = this;

            GameMap.WeaponWeights = new Dictionary<string, int>() {
                { "WEAPON_PISTOL", 8 },
                { "WEAPON_COMBATPISTOL", 7  },
                { "WEAPON_SMG", 6  },
                { "WEAPON_CARBINERIFLE", 5  },
                { "WEAPON_ASSAULTRIFLE", 4  },
                //{ "WEAPON_SNIPERRIFLE", 2 },
                { "WEAPON_PUMPSHOTGUN", 4  },
                { "WEAPON_MICROSMG", 6 },
                { "WEAPON_COMBATMG", 3 }
            };

            GameWeapons = new Dictionary<string, string>() {
                { "WEAPON_UNARMED", "Fists" },
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

            WeaponSlots = new Dictionary<string, int>() {
                { "WEAPON_UNARMED", 0 },
                { "WEAPON_PISTOL", 1  },
                { "WEAPON_COMBATPISTOL", 1 },
                { "WEAPON_MICROSMG", 1 },
                { "WEAPON_SMG", 2 },
                { "WEAPON_CARBINERIFLE", 2  },
                { "WEAPON_ASSAULTRIFLE", 2  },
                //{ "WEAPON_SNIPERRIFLE", 1 },
                { "WEAPON_PUMPSHOTGUN", 2  },
                { "WEAPON_COMBATMG", 2 },
            };

            WeaponMaxAmmo = new Dictionary<string, int>() {
                { "WEAPON_UNARMED", 0 },
                { "WEAPON_PISTOL", 38  },
                { "WEAPON_COMBATPISTOL", 38 },
                { "WEAPON_MICROSMG", 96 },
                { "WEAPON_SMG", 180 },
                { "WEAPON_CARBINERIFLE", 180  },
                { "WEAPON_ASSAULTRIFLE", 180  },
                //{ "WEAPON_SNIPERRIFLE", 1 },
                { "WEAPON_PUMPSHOTGUN", 24  },
                { "WEAPON_COMBATMG", 300 },
            };

            TeamText = new Text( "Spectator", new System.Drawing.PointF( Screen.Width * 0.033f, Screen.Height * 0.855f ), 0.5f );

            SetTeam( team );
        }

        public override void Start() {
            Game.Player.Character.MaxHealth = 100;
            Game.Player.Character.Health = 100;
            GameMap.SpawnWeapons();
            SetPlayerMayNotEnterAnyVehicle( PlayerId() );
            WriteChat( "Game starting" );
            GameMap.CreateBlip();
            base.Start();
        }

        public override void End() {
            WriteChat( "Game ending" );
            GameMap.ClearWeapons();
            base.End();

        }


        public override void Controls() {

            if( IsControlJustPressed( 0, 23 ) && Game.PlayerPed.Weapons.Current.Hash.ToString() != "Unarmed" ) {


                // Drop current weapon, basegameode handles everything weapon related, grab the name of weapon from current weapon that's all that is needed from weapons.
                
                foreach( WeaponPickup wep in GameMap.CreatedWeapons.Values.ToList() ) {
                    if( (wep.WorldModel == Game.PlayerPed.Weapons.Current.Model.GetHashCode()) ) {
                        WeaponPickup item = new WeaponPickup( GameMap, wep.WeaponModel, wep.WeaponHash, wep.WorldModel, Game.Player.Character.Position, true, 1500, Game.PlayerPed.Weapons.Current.Ammo );
                        item.Throw();
                        Game.PlayerPed.Weapons.Remove( Game.PlayerPed.Weapons.Current );
                        break;
                    }
                }
            }

            base.Controls();
        }

        public override bool CanPickupWeapon( string weaponModel ) {
            bool canPickup = true;
            if( PlayerWeapons.ContainsKey( WeaponSlots[weaponModel] ) )
                canPickup = false;
           
            return canPickup;
        }

        public override void PlayerPickedUpWeapon( string wepName, int count ) {
            lastScroll = GetGameTimer();
            PlayerWeapons[WeaponSlots[wepName]] = wepName;
            base.PlayerPickedUpWeapon( wepName, count );
        }

        public override void PlayerDroppedWeapon( string wepName, int count ) {
            PlayerWeapons.Remove( WeaponSlots[wepName] );
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

            HideReticle();

            if( Team == (int)Teams.Traitors ) {
                DrawRectangle( 0.025f, 0.86f, 0.07f, 0.03f, 200, 0, 0, 200 );
            }
            if( Team == (int)Teams.Detectives) {
                DrawRectangle(0.025f, 0.86f, 0.07f, 0.03f, 0, 0, 200, 200);
            }
            if( Team == (int)Teams.Innocents) {
                DrawRectangle(0.025f, 0.86f, 0.07f, 0.03f, 0, 200, 0, 200);
            }

            TeamText.Draw();

            DrawBaseHealthHUD();
            DrawWeaponHUD();
            ShowNames();


            base.HUD();
        }

        
        public void DrawWeaponHUD() {
            if( lastScroll + (2 * 1000) > GetGameTimer() ) {
                int index = 0;
                foreach( var weapon in WeaponSlots ) {
                    int offset = weapon.Value;
                    if( PlayerWeapons.Count-1 < offset ) {
                        offset--;
                    }
                    if( !PlayerWeapons.ContainsValue( weapon.Key ) )
                        continue;
                    if( WeaponTexts.Count <= weapon.Value ) {
                        WeaponTexts.Add( new Text( weapon.Key, new System.Drawing.PointF( Screen.Width * 0.85f, Screen.Height * 0.85f + (offset * 0.4f) ), 0.3f ) );
                    }

                    if( Game.PlayerPed.Weapons.Current.Hash.GetHashCode() == GetHashKey( weapon.Key ) ) {
                        DrawRectangle( 0.85f, 0.85f + (0.04f * offset), 0.1f, 0.03f, 200, 200, 0, 200 );
                    }
                    else {
                        DrawRectangle( 0.85f, 0.85f + (0.04f * offset), 0.1f, 0.03f, 0, 0, 0, 200 );
                    }

                    WeaponTexts[index].Caption = GameWeapons[weapon.Key];
                    WeaponTexts[index].Position = new System.Drawing.PointF( Screen.Width * 0.85f, Screen.Height * (0.85f + (offset * 0.04f)) );
                    WeaponTexts[index].Draw();
                    index++;
                }
            }
            if( IsControlJustPressed( 2, 15 ) ) {
                ChangeSelectedWeapon( -1 );
            }

            if( IsControlJustPressed( 2, 14 ) ) {
                ChangeSelectedWeapon( +1 );
            }
        }


        public override void SetTeam( int team ) {
            switch( team ) {
                case ((int)Teams.Spectators):
                    TeamText.Caption = "Spectate";
                    TeamText.Color = System.Drawing.Color.FromArgb( 150, 150, 0 );
                    break;
                case ((int)Teams.Traitors):
                    TeamText.Caption = "Traitor";
                    TeamText.Color = System.Drawing.Color.FromArgb( 255, 255, 255 );
                    break;
                case ((int)Teams.Innocents):
                    TeamText.Caption = "Innocent";
                    TeamText.Color = System.Drawing.Color.FromArgb( 255, 255, 255 );
                    break;
                case ((int)Teams.Detectives):
                    TeamText.Caption = "Detective";
                    TeamText.Color = System.Drawing.Color.FromArgb( 255, 255, 255 );
                    break;
            }
            base.SetTeam( team );
        }

        public override void Update() {

            FirstPersonForAlive();
            SetPlayerMayNotEnterAnyVehicle( PlayerId() );

            base.Update();

        }
    }
}
