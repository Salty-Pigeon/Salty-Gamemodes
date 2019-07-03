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

        public Vector2 Position;
        public Vector2 Size;
        System.Drawing.Color Colour;
        Vector3 heading; 

        List<SaltyButton> Buttons = new List<SaltyButton>();

        public SaltyMenu( float x, float y, float width, float height, System.Drawing.Color colour ) {
            heading.X = GetGameplayCamRelativeHeading();
            heading.Y = GetGameplayCamRelativePitch();

            GetScreenActiveResolution( ref Init.ScreenWidth, ref Init.ScreenHeight );
            Position = new Vector2(x, y);
            Size = new Vector2(width, height);
            Colour = colour;
        }

        public void AddSpriteButton( string texture, float x, float y, float width, float height, float rotation, Action action ) {
            float buttonX = (Position.X - Size.X / 2) + (width * Size.X / 2) + (x * Size.X);
            float buttonY = (Position.Y - Size.Y / 2) + (height * Size.Y / 2) + (y * Size.Y);
            SaltyButton button = new SaltyButton(this, texture, buttonX, buttonY, width * Size.X, height * Size.Y, rotation, action);

            Buttons.Add(button);
        }

        public void AddTextButton( string caption, float x, float y, float width, float height, System.Drawing.Color colour, Action action ) {

            float buttonX = (Position.X - Size.X / 2) + (width * Size.X / 2) + (x * Size.X);
            float buttonY = (Position.Y - Size.Y / 2) + (height * Size.Y / 2) + (y * Size.Y);
            SaltyButton button = new SaltyButton( this, caption, buttonX, buttonY, width * Size.X, height * Size.Y, colour, action );

            Buttons.Add(button);
        }

        public void Draw() {
            ShowCursorThisFrame();
            SetGameplayCamRelativeHeading( heading.X );

            SetGameplayCamRelativePitch( heading.Y, 1 );

            DrawRect( Position.X, Position.Y, Size.X, Size.Y, Colour.R, Colour.G, Colour.B, 255);

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
