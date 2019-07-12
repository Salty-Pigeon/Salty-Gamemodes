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
    class TDM : BaseGamemode {

        public enum Teams {
            Spectators,
            Blue,
            Red
        }

        public enum GameState {
            None,
            PreRound,
            InGame,
            PostRound
        }

        public TDM( Map gameMap, int team ) {

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
                { "WEAPON_COMBATMG", "Light Machine Gun" },
                { "WEAPON_KNIFE", "Knife" }
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
                { "WEAPON_KNIFE", 3 },
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
                { "WEAPON_KNIFE", 1 },
            };

            GameMap = gameMap;
            GameMap.Gamemode = this;
            GameMap.CreateBlip();

            SetTeam( team );
        }

        public override void Controls() {
            if( IsControlJustPressed( 0, 23 ) ) {
                DropWeapon();
            }
            base.Controls();
        }

        public override void Start() {
            ActiveHUD.ScoreText.Position = new Vector2( 0.06f, 0.855f );
            ActiveHUD.ScoreText.Scale = 0.5f;
            base.Start();
        }

        public override void End() {
            GameMap.ClearWeapons();
            base.End();
        }

        public override bool CanPickupWeapon( string weaponModel ) {
            if( PlayerWeapons.ContainsKey( WeaponSlots[weaponModel] ) )
                return false;
            return base.CanPickupWeapon( weaponModel );
        }

        public override void PlayerSpawned( ExpandoObject spawnInfo ) {
            Game.PlayerPed.Position = PlayerSpawn;
            base.PlayerSpawned( spawnInfo );
        }

        public override void HUD() {
            ActiveHUD.DrawWeaponSwitch();
            ActiveHUD.DrawHealth();
            ActiveHUD.DrawScore();
            base.HUD();
        }
    }
}
