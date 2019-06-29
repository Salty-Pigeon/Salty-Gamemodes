using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {
    class Murder : BaseGamemode {
        public enum Teams {
            Spectators,
            Murderer,
            Civilian
        }

        public enum GameState {
            None,
            PreRound,
            InGame,
            PostRound
        }

        public Murder( Map gameMap, int team ) {

            GameWeapons = new Dictionary<string, string>() {
                { "WEAPON_UNARMED", "Paper Fists" },
                { "WEAPON_KNIVE", "Knife" },
                { "WEAPON_BAT", "Bat" },
                { "WEAPON_PISTOL", "Pistol" }
            };

            GameMap = gameMap;
            GameMap.Gamemode = this;
            GameMap.CreateBlip();

            SetTeam( team );
        }

        public override void Start() {
            base.Start();

            Game.Player.Character.MaxHealth = 100;
            Game.Player.Character.Health = 100;
            if( Team == (int)Teams.Murderer ) {
                //GiveWeaponToPed( PlayerPedId(), (uint)GetHashKey("WEAPON_PISTOL"), 100, false, true );
            } else if( Team == (int)Teams.Civilian ) {

            }
            //RemoveWeaponFromPed( PlayerPedId(), (uint)GetHashKey( "WEAPON_UNARMED" ) );
            GiveWeaponToPed( PlayerPedId(), (uint)GetHashKey( "WEAPON_BAT" ), 100, false, true );
            Game.PlayerPed.Weapons.Select(Game.PlayerPed.Weapons.BestWeapon);
            SetCurrentPedWeapon( PlayerPedId(), (uint)GetHashKey( "WEAPON_BAT" ), true );

        }

        public override void End() {
            base.End();
        }

        public override void HUD() {

            DrawBaseHealthHUD();
            DrawBaseWeaponHUD();

            base.HUD();
        }

        public override bool CanPickupWeapon( string weaponModel ) {
            return base.CanPickupWeapon( weaponModel );
        }

        public override void PlayerDroppedWeapon( string wepName, int count ) {
            base.PlayerDroppedWeapon( wepName, count );
        }

        public override void PlayerSpawned( ExpandoObject spawnInfo ) {
            Game.Player.Character.MaxHealth = 100;
            Game.Player.Character.Health = 100;
            base.PlayerSpawned( spawnInfo );
        }

    }
}
