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


        public SaltyText TeamText;

        public SaltyMenu TTTMenu;

        public Vector3 SavedTeleport;
        public bool CanTeleport = false;
        public bool CanDisguise = false;

        public List<DeadBody> DeadBodies = new List<DeadBody>();

        public float RadarTime = 0f;
        public float RadarScanTime = 30 * 1000;
        public bool isRadarActive = false;
        public List<Vector3> RadarPositions = new List<Vector3>();
        uint pedModel = 0;


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

            TeamText = new SaltyText( 0.033f, 0.855f, 0, 0, 0.5f, "Spectator", 255, 255, 255, 255, false, false, 0, true );

            SetTeam( team );
        }

        public override void Start() {

            foreach( var player in new PlayerList() ) {
                uint model = (uint)player.Character.Model.Hash;
                if( !HasModelLoaded( model ) ) {
                    RequestModel( model );
                }
            }

            Game.Player.Character.MaxHealth = 100;
            Game.Player.Character.Health = 100;
            GameMap.SpawnWeapons();
            SetPlayerMayNotEnterAnyVehicle( PlayerId() );
            WriteChat( "Game starting" );
            GameMap.CreateBlip();
            RadarPositions.Add( PlayerSpawn );
            base.Start();
        }

        public override void End() {
            WriteChat( "Game ending" );
            GameMap.ClearWeapons();
            base.End();

        }

        public void CloseTTTMenu() {
            TTTMenu = null;
        }


        public override void Controls() {

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

            if( IsControlJustPressed( 0, 244 ) ) {
                if( Team == (int)Teams.Traitors) {
                    TTTMenu = TTTMenu == null ? new TTT_TraitorMenu(this, 0.5f, 0.8f, 0.2f, 0.15f, System.Drawing.Color.FromArgb(44, 62, 80), CloseTTTMenu) : null;
                }
                if( Team == (int)Teams.Detectives) {
                    TTTMenu = TTTMenu == null ? new TTT_DetectiveMenu(this, 0.5f, 0.8f, 0.2f, 0.15f, System.Drawing.Color.FromArgb(44, 62, 80), CloseTTTMenu) : null;
                }
            }

            if( IsControlJustPressed(0, 243)) { // Tilde
                if( CanDisguise )
                    TriggerServerEvent("salty::netUpdatePlayerBool", "disguised");
            }

            if ( IsControlJustPressed(0, 19)) { // Left alt
                if (CanTeleport)
                    TeleportToSaved(3 * 1000);
            }

            if( IsControlJustPressed(0, 121)) { // Insert
                if( CanTeleport )
                    SavedTeleport = Game.PlayerPed.Position;
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
            //ApplyDamageToPed( ped, 10000, false );

            base.PlayerDied( killerType, deathcords );
        }

        float teleportLength;
        float teleportTime = 0;
        bool hasTeleported = false;
        bool isTeleporting = false;
        public void TeleportToSaved( float time ) {
            if (SavedTeleport == null) {
                WriteChat("No destination set");
                return;
            }
            teleportLength = time;
            teleportTime = GetGameTimer() + teleportLength;
        }

        public void DoTeleport() {
            float gameTime = GetGameTimer();
            if ( teleportTime > gameTime ) {
                DisableControlAction(0, 30, true); // Disable movement
                DisableControlAction(0, 31, true);
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
            }
        }


        public override void SpawnDeadBody( Vector3 position, uint model ) {
            DeadBodies.Add( new DeadBody( position, model ) );
        }

        public override void PlayerSpawned( ExpandoObject spawnInfo ) {
            Game.Player.Character.MaxHealth = 100;
            Game.Player.Character.Health = 100;
            base.PlayerSpawned( spawnInfo );
        }

        public override void ShowNames() {
            Vector3 position = Game.PlayerPed.ForwardVector;

            RaycastResult result = Raycast(Game.PlayerPed.Position, position, 75, IntersectOptions.Peds1, null);
            if (result.DitHitEntity) {
                if( result.HitEntity != Game.PlayerPed) {
                    int ent = result.HitEntity.Handle;
                    ShowDeadName( result, ent );
                    if( GetPlayerBool( NetworkGetPlayerIndexFromPed( ent ), "disguised" ) ) {
                        return;
                    }
                    if (IsPedAPlayer(ent)) {
                        HUDText.Caption = GetPlayerName(GetPlayerPed(ent)).ToString();
                        lastLooked = GetGameTimer();
                    }
                }
            }

        }

        public void ShowDeadName(RaycastResult result, int ent) {
            Debug.WriteLine( result.HitEntity.IsDead.ToString() );
            Debug.WriteLine( result.HitEntity.Model.GetHashCode().ToString() );
        }

        public void SetRadarActive( bool active ) {
            RadarTime = GetGameTimer() + RadarScanTime;
            isRadarActive = active;
        }

        public void ShowRadar() {

            if( RadarTime < GetGameTimer() ) {
                RadarPositions = new List<Vector3>();
                RadarTime += RadarScanTime;
                foreach( var ply in new PlayerList() ) {
                    RadarPositions.Add( ply.Character.Position );
                }
            }

            foreach( var pos in RadarPositions ) {
                float x = 0, y = 0;
                bool offScreen = Get_2dCoordFrom_3dCoord( pos.X, pos.Y, pos.Z, ref x, ref y );

                if( offScreen )
                    continue;

                Vector3 camPos = GetGameplayCamCoords();
                float dist = GetDistanceBetweenCoords( pos.X, pos.Y, pos.Z, camPos.X, camPos.Y, camPos.Z, true );
                SetTextScale( 0.3f, 0.3f );
                SetTextFont( 0 );
                SetTextProportional( true );
                SetTextColour( 255, 255, 255, 255 );
                SetTextDropshadow( 0, 0, 0, 0, 55 );
                SetTextEdge( 2, 0, 0, 0, 150 );
                SetTextDropShadow();
                SetTextOutline();
                SetTextEntry( "STRING" );
                SetTextCentre( true );
                AddTextComponentString( Math.Round( dist, 1 ) + "m" );
                DrawText( x, y );
                Get_2dCoordFrom_3dCoord( pos.X, pos.Y, pos.Z - 0.08f, ref x, ref y );
                DrawRect( x, y, 0.02f, 0.02f, 0, 200, 0, 255 );
            }

            
        }

        public override void HUD() {

            if( TTTMenu != null ) {
                TTTMenu.Draw();
            }

            HideReticle();
            if( isRadarActive )
                ShowRadar();

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

            if( CanDisguise) {
                if (GetPlayerBool(NetworkGetPlayerIndexFromPed(PlayerPedId()), "disguised")) {
                    Debug.WriteLine("Disguised");
                }
            }
            

            base.HUD();
        }



        
        public void DrawWeaponHUD() {
            if( lastScroll + (2 * 1000) > GetGameTimer() ) {
                int index = 0;
                foreach( var weapon in WeaponSlots.OrderBy( x => x.Value ) ) {
                    if( !PlayerWeapons.ContainsValue( weapon.Key ) )
                        continue;
                    if( WeaponTexts.Count <= weapon.Value ) {
                        WeaponTexts.Add( new SaltyText( 0.85f, 0.85f + (index * 0.4f), 0, 0, 0.3f, weapon.Key, 255, 255, 255, 255, false, false, 0, true ) );
                    }

                    if( (uint)Game.PlayerPed.Weapons.Current.Hash.GetHashCode() == (uint)GetHashKey( weapon.Key ) ) {
                        DrawRectangle( 0.85f, 0.85f + (0.04f * index), 0.1f, 0.03f, 200, 200, 0, 200 );
                    }
                    else {
                        DrawRectangle( 0.85f, 0.85f + (0.04f * index), 0.1f, 0.03f, 0, 0, 0, 200 );
                    }

                    WeaponTexts[index].Caption = GameWeapons[weapon.Key];
                    WeaponTexts[index].Position = new Vector2( 0.85f, 0.85f + (index * 0.04f) );
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
                    TeamText.Colour = System.Drawing.Color.FromArgb( 150, 150, 0 );
                    break;
                case ((int)Teams.Traitors):
                    TeamText.Caption = "Traitor";
                    TeamText.Colour = System.Drawing.Color.FromArgb( 255, 255, 255 );
                    break;
                case ((int)Teams.Innocents):
                    TeamText.Caption = "Innocent";
                    TeamText.Colour = System.Drawing.Color.FromArgb( 255, 255, 255 );
                    break;
                case ((int)Teams.Detectives):
                    TeamText.Caption = "Detective";
                    TeamText.Colour = System.Drawing.Color.FromArgb( 255, 255, 255 );
                    break;
            }
            base.SetTeam( team );
        }

        public override void Update() {

            FirstPersonForAlive();
            SetPlayerMayNotEnterAnyVehicle( PlayerId() );

            foreach( var body in DeadBodies ) {
                if( !IsPedRagdoll( body.ID ) )
                    body.Update();
            }

            if (isTeleporting) {
                DoTeleport();
            }

            base.Update();

        }
    }
}
