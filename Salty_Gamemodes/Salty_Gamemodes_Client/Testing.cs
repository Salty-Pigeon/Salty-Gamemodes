using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {
    class Testing : BaseScript {

        Init Init;

        public SaltyMenu testMenu;


        public Testing( Init init ) {
            Init = init;
        }
        
        public void CloseMenu() {
            testMenu = null;
        }

        public void Update() {
            if( testMenu != null )
                testMenu.Draw();
        }

        public void LoadCommands() {
            RegisterCommand("menu", new Action<int, List<object>, string>(( source, args, raw ) => {
                testMenu = new TTT_Menu(0.5f, 0.5f, 0.2f, 0.2f, System.Drawing.Color.FromArgb(0, 0, 0), CloseMenu);
            }), false);

            RegisterCommand("delmenu", new Action<int, List<object>, string>(( source, args, raw ) => {
                CloseMenu();
            }), false);

            RegisterCommand("mouse", new Action<int, List<object>, string>(( source, args, raw ) => {
                Debug.WriteLine(string.Format("{0} {1} {2}", GetGameplayCamRot(0).X, GetGameplayCamRot(0).Y, GetGameplayCamRot(0).Z));
            }), false);

            RegisterCommand("heading", new Action<int, List<object>, string>(( source, args, raw ) => {
                Debug.WriteLine(Game.PlayerPed.Heading.ToString());
            }), false);

            RegisterCommand("score", new Action<int, List<object>, string>(( source, args, raw ) => {
                Init.ActiveGame.AddScore(1);
            }), false);

            RegisterCommand("street", new Action<int, List<object>, string>(( source, args, raw ) => {
                uint streetName = 0;
                uint crossingRoad = 0;
                GetStreetNameAtCoord(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, ref streetName, ref crossingRoad);
                Debug.WriteLine(streetName + " : " + GetStreetNameFromHashKey(streetName));
            }), false);
        }
    }
}
