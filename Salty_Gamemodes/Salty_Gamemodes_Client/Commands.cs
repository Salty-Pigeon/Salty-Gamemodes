using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {
    class Commands : BaseScript {

        Init Init;


        public Commands( Init init ) {
            Init = init;
        }

        public void Load() {



            RegisterCommand( "respawn", new Action<int, List<object>, string>( ( source, args, raw ) => {
                if( Init.Salty.ActiveGame is IceCreamMan ) {
                    (Init.Salty.ActiveGame as IceCreamMan).Respawn();
                }
            } ), false );

            
            RegisterCommand("noclip", new Action<int, List<object>, string>(( source, args, raw ) => {
                if( Init.Salty.isInRoom ) {
                    Init.Salty.ActiveGame.SetNoClip( !Init.Salty.ActiveGame.isNoclip );
                } else {
                    Init.Salty.SaltyGame.SetNoClip( !Init.Salty.SaltyGame.isNoclip );
                }
            } ), false);
            /*
            

            RegisterCommand("spawnPoints", new Action<int, List<object>, string>(( source, args, raw ) => {
                TriggerServerEvent("salty::netSpawnPointGUI");
            }), false);

            RegisterCommand("addspawnpoint", new Action<int, List<object>, string>(( source, args, raw ) => {
                TriggerServerEvent("salty::netModifyMapPos", "add", "AUTO", Convert.ToInt32(args[0]), Game.Player.Character.Position);
            }), false);

            RegisterCommand("addweaponpoint", new Action<int, List<object>, string>(( source, args, raw ) => {
                TriggerServerEvent("salty::netModifyWeaponPos", "add", "AUTO", args[0], Game.Player.Character.Position);
            }), false);

            RegisterCommand("kill", new Action<int, List<object>, string>(( source, args, raw ) => {
                Game.PlayerPed.Kill();
            }), false);

            RegisterCommand("createmap", new Action<int, List<object>, string>(( source, args, raw ) => {
                if (args[2] == null)
                    return;
                TriggerServerEvent("salty::netModifyMap", "add", args[0], 0, Game.Player.Character.Position, new Vector3(float.Parse(args[1].ToString()), float.Parse(args[2].ToString()), 0));
            }), false);

            RegisterCommand("weapon", new Action<int, List<object>, string>(( source, args, raw ) => {
                GiveWeaponToPed(Game.PlayerPed.Handle, (uint)GetHashKey(args[0].ToString()), 999, false, true);
            }), false);

            RegisterCommand("car", new Action<int, List<object>, string>(async ( source, args, raw ) => {
                // account for the argument not being passed
                var model = "adder";
                if (args.Count > 0) {
                    model = args[0].ToString();
                }

                // check if the model actually exists
                // assumes the directive `using static CitizenFX.Core.Native.API;`
                var hash = (uint)GetHashKey(model);
                if (!IsModelInCdimage(hash) || !IsModelAVehicle(hash)) {
                    TriggerEvent("chat:addMessage", new {
                        color = new[] { 255, 0, 0 },
                        args = new[] { "[CarSpawner]", $"It might have been a good thing that you tried to spawn a {model}. Who even wants their spawning to actually ^*succeed?" }
                    });
                    return;
                }

                // create the vehicle
                var vehicle = await World.CreateVehicle(model, Game.PlayerPed.Position, Game.PlayerPed.Heading);
                vehicle.CanBeVisiblyDamaged = false;
                vehicle.CanEngineDegrade = false;
                vehicle.CanTiresBurst = false;
                vehicle.CanWheelsBreak = false;
                vehicle.EngineHealth = 999999;
                vehicle.MaxHealth = 999999;
                vehicle.Health = 999999;
                vehicle.EnginePowerMultiplier = 100;
                vehicle.Gravity = 50;


                SetVehicleHandlingFloat(NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fCamberStiffnesss", 0.1f);
                SetVehicleHandlingFloat(NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fInitialDragCoeff ", 10f);
                SetVehicleHandlingFloat(NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fMass", 10000f);
                SetVehicleHandlingFloat(NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fInitialDriveForce", 2f);
                SetVehicleHandlingFloat(NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "FTRACTIONSPRINGDELTAMAX", 100f);
                SetVehicleHandlingFloat(NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fSteeringLock", 40f);
                SetVehicleHandlingFloat(NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fDownForceModifier", 100f);
                SetVehicleHandlingFloat(NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fDriveInertia", 1f);
                SetVehicleHandlingFloat(NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fDriveBiasFront", 0.5f);
                SetVehicleHandlingFloat(NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fTractionCurveLateral", 25f);
                SetVehicleHandlingFloat(NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fTractionCurveMax", 5f);
                SetVehicleHandlingFloat(NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fTractionCurveMin", 5f);
                SetVehicleHandlingFloat(NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fTractionBiasFront", 0.5f);
                SetVehicleHandlingFloat(NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fTractionLossMult", 0.1f);
                SetVehicleHandlingFloat(NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fSuspensionReboundDamp", 2f);
                SetVehicleHandlingFloat(NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fSuspensionCompDamp", 2f);
                SetVehicleHandlingFloat(NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fSuspensionForce", 3f);
                SetVehicleHandlingFloat(NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "FCOLLISIONDAMAGEMULT", 0f);
                SetVehicleHasStrongAxles(NetworkGetEntityFromNetworkId(vehicle.NetworkId), true);
                SetVehicleHighGear(NetworkGetEntityFromNetworkId(vehicle.NetworkId), 1);


                Game.PlayerPed.SetIntoVehicle(vehicle, VehicleSeat.Driver);

                // tell the player
                TriggerEvent("chat:addMessage", new {
                    color = new[] { 255, 0, 0 },
                    args = new[] { "[CarSpawner]", $"Woohoo! Enjoy your new ^*{model}!" }
                });
            }), false);

    */
        }

    }
}
