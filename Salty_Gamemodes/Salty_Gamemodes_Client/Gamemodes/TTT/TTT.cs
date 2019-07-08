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

        public SaltyMenu TTTMenu;

        public Vector3 SavedTeleport = Vector3.Zero;
        public bool CanTeleport = false;
        public bool CanDisguise = false;
        public bool isDisguised = false;

        float teleportLength;
        float teleportTime = 0;
        bool hasTeleported = false;
        bool isTeleporting = false;

        public Dictionary<int, DeadBody> DeadBodies = new Dictionary<int, DeadBody>();

        uint pedModel = 0;

        public int SpendPoints = 1;

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

            ActiveHUD = new TTT_HUD(this);

            pedModel = (uint)GetHashKey( "mp_m_shopkeep_01" );

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
                { "WEAPON_COMBATMG", 2 }
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

            SetTeam( team );
        }

        public override void Start() {

            NetworkSetVoiceChannel( 1 );
            Game.Player.Character.MaxHealth = 100;
            Game.Player.Character.Health = 100;

            foreach ( var player in new PlayerList() ) {
                uint model = (uint)player.Character.Model.Hash;
                if( !HasModelLoaded( model ) ) {
                    RequestModel( model );
                }
            }

            SetPlayerMayNotEnterAnyVehicle( PlayerId() );       
            GameMap.CreateBlip();
            GetHUD().RadarPositions.Add( PlayerSpawn );
            base.Start();
        }

        public TTT_HUD GetHUD() {
            return ActiveHUD as TTT_HUD;
        }

        public override void End() {
            GameMap.ClearWeapons();
            CloseTTTMenu();
            base.End();

        }

        public void CloseTTTMenu() {
            TTTMenu = null;
        }
        

        public bool CanBuy() {
            if( SpendPoints <= 0 ) {
                WriteChat( "TTT", "Not enough funds", 0, 0, 230 );
                return false;
            }
            SpendPoints--;
            return true;
        }

        public override void Controls() {

           if( Team == 0 )
                return;

            if( IsControlJustPressed( 0, 23 ) && Game.PlayerPed.Weapons.Current.Hash.ToString() != "Unarmed" ) {


                // Drop current weapon, basegameode handles everything weapon related, grab the name of weapon from current weapon that's all that is needed from weapons.
                
                foreach( WeaponPickup wep in GameMap.CreatedWeapons.Values.ToList() ) {
                    if( (wep.WorldModel == Game.PlayerPed.Weapons.Current.Model.GetHashCode()) ) {
                        WeaponPickup item = new WeaponPickup( GameMap, wep.WeaponModel, wep.WeaponHash, wep.WorldModel, Game.Player.Character.Position, true, 1500, Game.PlayerPed.Weapons.Current.Ammo, Game.PlayerPed.Weapons.Current.AmmoInClip );
                        item.Throw();
                        RemoveWeapon( wep.WeaponModel, true );
                        break;
                    }
                }
            }

            if( IsControlJustPressed( 0, 244 ) ) { // M 244
                if( Team == (int)Teams.Traitors) {
                    TTTMenu = TTTMenu == null ? new TTT_TraitorMenu(this, 0.5f, 0.8f, 0.3f, 0.15f, System.Drawing.Color.FromArgb(44, 62, 80), CloseTTTMenu, CanBuy ) : null;
                }
                if( Team == (int)Teams.Detectives) {
                    TTTMenu = TTTMenu == null ? new TTT_DetectiveMenu(this, 0.5f, 0.8f, 0.3f, 0.15f, System.Drawing.Color.FromArgb(44, 62, 80), CloseTTTMenu, CanBuy ) : null;
                }
            }

            if( IsControlJustPressed(0, 243)) { // Tilde
                if( CanDisguise) {
                    TriggerServerEvent("salty::netUpdatePlayerBool", "disguised");
                    isDisguised = !isDisguised;
                }
            }

            if ( IsControlJustPressed(0, 212) ) { // Up arrow
                if (CanTeleport)
                    TeleportToSaved(3 * 1000);
            }

            if( IsControlJustPressed(0, 121)) { // Insert
                if( CanTeleport ) {
                    WriteChat( "TTT", "Teleport position set.", 0, 0, 200 );
                    SavedTeleport = Game.PlayerPed.Position;

                }
            }

            if( IsControlJustPressed( 0, 38 ) ) { // E dead body
                foreach( var body in DeadBodies ) {
                    Vector3 myPos = Game.PlayerPed.Position;
                    float dist = GetDistanceBetweenCoords( myPos.X, myPos.Y, myPos.Z, body.Value.Position.X, body.Value.Position.Y, body.Value.Position.Z, true );
                    if( dist > 2 ) { continue; }
                    if( body.Value.isDiscovered ) {
                        if( Team == (int)Teams.Detectives ) {
                            WriteChat( "TTT", "Scanning DNA", 0, 0, 230 );
                            GetHUD().DetectiveTracing = body.Value.KillerPed;
                        } 
                    } else {
                        body.Value.View();
                        TriggerServerEvent( "salty::netBodyDiscovered", body.Key );
                    }
                }
            }

            if (IsControlJustPressed(2, 15)) {
                ChangeSelectedWeapon(-1);
            }

            if (IsControlJustPressed(2, 14)) {
                ChangeSelectedWeapon(+1);
            }

            base.Controls();
        }

        public override bool CanPickupWeapon( string weaponModel ) {
            if( PlayerWeapons.ContainsKey( WeaponSlots[weaponModel] ) )
                return false;
            return base.CanPickupWeapon( weaponModel );
        }

        public override void PlayerPickedUpWeapon( string wepName, int count ) {
            lastScroll = GetGameTimer();
            base.PlayerPickedUpWeapon( wepName, count );
        }

        public override void PlayerDied( int killerType, Vector3 deathcords ) {
            SetTeam( (int)Teams.Spectators );
            base.PlayerDied( killerType, deathcords );
        }

        public void TeleportToSaved( float time ) {
            if( isTeleporting) {
                return;
            }
            if( Game.PlayerPed.IsShooting || Game.PlayerPed.IsJumping || Game.PlayerPed.IsInAir || Game.PlayerPed.IsReloading || Game.PlayerPed.IsClimbing || Game.PlayerPed.IsGoingIntoCover || Game.PlayerPed.IsRagdoll || Game.PlayerPed.IsGettingUp  )
                return;

            if (SavedTeleport == Vector3.Zero) {
                WriteChat( "TTT", "No destination set", 0, 0, 200 );
                return;
            }
            teleportLength = time;
            teleportTime = GetGameTimer() + teleportLength;
            isTeleporting = true;
        }

        public void DoTeleport() {
            float gameTime = GetGameTimer();
            if ( teleportTime > gameTime ) {
                DisableControlAction(0, 30, true); // Disable movement
                DisableControlAction(0, 31, true);
                DisableControlAction(0, 24, true);
                DisableControlAction(0, 257, true);
                DisableControlAction( 0, 22, true );
                DisableControlAction( 0, 21, true );

                int alpha = (int)Math.Round(255 * ((teleportTime - gameTime) / (teleportLength / 2)));
                if ( teleportTime - gameTime <= (teleportLength/2)) {
                    if( !hasTeleported ) {
                        hasTeleported = true;
                        Game.PlayerPed.Position = SavedTeleport;
                    }
                    alpha = 255 - alpha;
                }
                SetEntityAlpha( PlayerPedId(), alpha, 0 );
            } else if ( teleportTime < gameTime) {
                isTeleporting = false;
                hasTeleported = false;
            }
        }

        public void BodyDiscovered( int body ) {
            if( !DeadBodies[body].isDiscovered ) {
                DeadBodies[body].Discovered();
                WriteChat( "TTT", DeadBodies[body].Name + "'s body has been discovered", 255, 0, 0 );
            }
        }

        public void SpawnDeadBody( Vector3 position, int ply, int killer ) {
            int player = GetPlayerFromServerId( ply );
            int kill = GetPlayerFromServerId( killer );
            DeadBodies.Add( ply, new DeadBody( position, player, kill ) );
        }

        public override void PlayerSpawned( ExpandoObject spawnInfo ) {
            Game.Player.Character.MaxHealth = 100;
            Game.Player.Character.Health = 100;
            base.PlayerSpawned( spawnInfo );
        }


        public override void SetTeam( int team ) {
            GetHUD().UpdateTeam(team);
            base.SetTeam( team );
        }

        public override void Update() {

            FirstPersonForAlive();
            CantEnterVehichles();

            foreach( var body in DeadBodies ) {
                if( !IsPedRagdoll( body.Value.ID ) )
                    body.Value.Update();
            }

            if (isTeleporting) {
                DoTeleport();
            }

            if (TTTMenu != null) {
                TTTMenu.Draw();
            }

            base.Update();

        }

    }
}
