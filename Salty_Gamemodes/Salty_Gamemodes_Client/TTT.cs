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

        public Dictionary<int, string> WeaponInv = new Dictionary<int, string>();

        public Dictionary<string, int> WeaponSlots = new Dictionary<string, int>() {
            { "WEAPON_PISTOL", 1  },
            { "WEAPON_COMBATPISTOL", 1 },
            { "WEAPON_SMG", 2 },
            { "WEAPON_CARBINERIFLE", 2  },
            { "WEAPON_ASSAULTRIFLE", 2  },
            { "WEAPON_SNIPERRIFLE", 2 },
            { "WEAPON_PUMPSHOTGUN", 2  },
            { "WEAPON_MICROSMG", 2 },
            { "WEAPON_SAWNOFFSHOTGUN", 2 }
        };



        public GameState CurrentState = GameState.None;

        public TTT( Map gameMap, int team ) {
            GameMap = gameMap;
            GameMap.Gamemode = this;
            GameMap.CreateBlip();
            SetTeam( team );
        }

        public override void Start() {
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

            base.Controls();
        }

        public void ChangeSelectedWeapon( int offset ) {

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
            return !WeaponInv.ContainsKey( WeaponSlots[weaponModel] );
        }

        public override void PlayerPickedUpWeapon( string wepName, int count ) {
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
            base.PlayerSpawned( spawnInfo );
        }

        public override void HUD() {

            HideHudAndRadarThisFrame();

            if( Team == (int)Teams.Traitors ) {
                TeamText.Color = System.Drawing.Color.FromArgb( 255, 255, 255 );
                TeamText.Scale = 0.5f;
                TeamText.Caption = "Traitor";
                DrawRectangle( 0.025f, 0.86f, 0.07f, 0.03f, 200, 0, 0, 200 );

            }
            if( Team == (int)Teams.Innocents ) {
                TeamText.Color = System.Drawing.Color.FromArgb( 0, 200, 0 );
                TeamText.Caption = "Innocent";
            }

            DrawRectangle( 0.025f, 0.9f, 0.1f, 0.03f, 0, 0, 0, 200 );
            float healthPercent = (float)Game.PlayerPed.Health / (float)Game.PlayerPed.MaxHealth;
            DrawRectangle( 0.025f, 0.9f, (healthPercent) * 0.1f, 0.03f, 200, 0, 0, 200 );


            base.HUD();
        }

        public override void Update() {

            FirstPersonForAlive();

            base.Update();

        }
    }
}
