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
            TeamText = new Text( "Spectator", new System.Drawing.PointF( Screen.Width * 0.033f, Screen.Height * 0.855f ), 0.5f );
            HealthText = new Text( "Health: ", new System.Drawing.PointF( Screen.Width * 0.033f, Screen.Height * 0.895f ), 0.5f );
            AmmoText = new Text( "Ammo: ", new System.Drawing.PointF( Screen.Width * 0.033f, Screen.Height * 0.935f ), 0.5f );
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
                foreach( WeaponPickup wep in GameMap.SpawnedWeapons.ToList() ) {

                    if( (int)wep.WeaponHash == Game.PlayerPed.Weapons.Current.Hash.GetHashCode() ) {
                        WeaponPickup item = new WeaponPickup( GameMap, wep.WeaponModel, wep.WeaponHash, wep.WorldModel, Game.Player.Character.Position, true, Game.PlayerPed.Weapons.Current.Ammo );
                        item.Throw();
                        GameMap.SpawnedWeapons.Add( item );
                        break;
                    }
                }
                Game.PlayerPed.Weapons.Remove( Game.PlayerPed.Weapons.Current );
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
            Game.Player.Character.MaxHealth = 100;
            Game.Player.Character.Health = 100;
            base.PlayerSpawned( spawnInfo );
        }

        public override void HUD() {

            HideHudAndRadarThisFrame();

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

            base.HUD();
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
