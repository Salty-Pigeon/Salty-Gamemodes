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
                { "WEAPON_UNARMED", "Helpless" },
                { "WEAPON_PISTOL", "Blat blat" },
                { "WEAPON_KNIFE", "Stabby stabby" }
            };

            WeaponMaxAmmo = new Dictionary<string, int>() {
                { "WEAPON_UNARMED", 0 },
                { "WEAPON_PISTOL", 1  },
                { "WEAPON_KNIFE", 1 }

            };

            WeaponSlots = new Dictionary<string, int>() {
                { "WEAPON_UNARMED", 0 },
                { "WEAPON_PISTOL", 2  },
                { "WEAPON_KNIFE", 1 }
            };

            GameMap = gameMap;
            GameMap.Gamemode = this;
            GameMap.CreateBlip();

            SetTeam( team );
        }

        public override void Start() {
            NetworkSetVoiceChannel( 1 );
            SetPlayerMeleeWeaponDamageModifier( PlayerId(), 0 );
            SetPlayerWeaponDamageModifier( PlayerId(), 20 );

            ActiveHUD.SetGameTimePosition( 0, 0, false );

            Game.Player.Character.MaxHealth = 100;
            Game.Player.Character.Health = 100;
            if( Team == (int)Teams.Murderer ) {
                GiveWeaponToPed( PlayerPedId(), (uint)GetHashKey( "WEAPON_KNIFE" ), 1, false, false );
            }
            else if( Team == (int)Teams.Civilian ) {
                RemoveWeaponFromPed( PlayerPedId(), (uint)GetHashKey( "WEAPON_UNARMED" ) );
            }

            base.Start();


        }

        public override void Controls() {
            if( IsControlJustPressed( 2, 15 ) ) {
                ChangeSelectedWeapon( -1 );
            }

            if( IsControlJustPressed( 2, 14 ) ) {
                ChangeSelectedWeapon( +1 );
            }
            base.Controls();
        }

        public override void Update() {
            CantEnterVehichles();
            base.Update();
        }
        
        public override void End() {
            base.End();
        }

        public override void HUD() {
            HideHudAndRadarThisFrame();
            ActiveHUD.DrawWeaponSwitch();
            FirstPersonForAlive();
            DrawFootprints();
            base.HUD();
        }

        List<Vector3> footprints = new List<Vector3>();
        Dictionary<Player, Vector3> PlayerFootprints = new Dictionary<Player, Vector3>();
        bool leftFoot = false;
        float footprintDistance = 3f;
        public void DrawFootprints() {
            foreach( var player in new PlayerList() ) {
                if( PlayerFootprints.ContainsKey(player) ) {
                    if( player.Character.Position.DistanceToSquared(PlayerFootprints[player]) >= footprintDistance ) {
                        PlayerFootprints[player] = player.Character.Position;
                        footprints.Add( new Vector3( player.Character.Position.X, player.Character.Position.Y, player.Character.Position.Z - player.Character.HeightAboveGround ) );
                    }
                } else {
                    PlayerFootprints.Add( player, player.Character.Position );
                }
            }
            foreach( var print in footprints ) {
                if( leftFoot ) {
                    ActiveHUD.DrawSpriteOrigin( print, "leftfoot", 0.01f, 0.01f, 0, false );
                    
                }
                else {
                    ActiveHUD.DrawSpriteOrigin( print, "rightfoot", 0.01f, 0.01f, 0, false );
                }
                leftFoot = !leftFoot;
            }
        }

        public override bool CanPickupWeapon( string weaponModel ) {
            return base.CanPickupWeapon( weaponModel );
        }

        public override void PlayerPickedUpWeapon( string wepName, int count ) {

            base.PlayerPickedUpWeapon( wepName, count );
        }

        public override void PlayerDroppedWeapon( string wepName, int count ) {
            if( wepName == "WEAPON_PISTOL" ) {
                WeaponPickup item = new WeaponPickup( GameMap, "WEAPON_PISTOL", (uint)GetHashKey( "WEAPON_PISTOL" ), GetHashKey( "W_PI_PISTOL" ), Game.Player.Character.Position, true, 5000, 1, -1 );
                item.Throw();
            }
            
            base.PlayerDroppedWeapon( wepName, count );
        }

        public override void PlayerSpawned( ExpandoObject spawnInfo ) {
            SetTeam( (int)Teams.Spectators );
            base.PlayerSpawned( spawnInfo );
        }

    }
}
