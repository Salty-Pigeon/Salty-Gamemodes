using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {
    public class SaltyMenu : BaseScript {

        Vector2 Position;
        Vector2 Size;
        System.Drawing.Color Colour;

        List<SaltyButton> Buttons = new List<SaltyButton>();

        public SaltyMenu( float x, float y, float width, float height, System.Drawing.Color colour ) {
            Position = new Vector2(x, y);
            Size = new Vector2(width, height);
            Colour = colour;
        }

        public void AddSpriteButton( string texture, float x, float y, float width, float height, float rotation, Action action ) {
            SaltyButton button = new SaltyButton(texture, Position.X + (x * Size.X), Position.Y + (y * Size.Y), width * Size.X, height * Size.Y, rotation, action);
            Buttons.Add(button);
        }

        public void AddTextButton( string caption, float x, float y, float width, float height, System.Drawing.Color colour, Action action ) {
            SaltyButton button = new SaltyButton(caption, Position.X + (x * Size.X), Position.Y + (y * Size.Y), width * Size.X, height * Size.Y, colour, action);
            Buttons.Add(button);
        }

        public void Draw() {
            ShowCursorThisFrame();
            DrawRect(Position.X, Position.Y, Size.X, Size.Y, Colour.R, Colour.G, Colour.B, 255);

            foreach( var button in Buttons) {
                button.Draw();
            }

            if( IsControlJustPressed(0, 24) ) {
                MouseClick();
            }

        }

        public void Close() {
            Init.testMenu = null;
        }

        public void MouseClick() {
            foreach( var button in Buttons ) {
                if( button.MouseIntersecting()) {
                    Debug.WriteLine("Button clicked!");
                    button.Action();
                }
            }
        }

    }
}
