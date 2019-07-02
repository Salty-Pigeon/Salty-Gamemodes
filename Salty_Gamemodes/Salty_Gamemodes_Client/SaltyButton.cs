using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {
    class SaltyButton {

        Text ButtonText;

        Vector2 Position;
        Vector2 Size;
        float Rotation;
        System.Drawing.Color Colour;
        string Texture = "";
        public Action Action;

        public SaltyButton( string caption, float x, float y, float width, float height, System.Drawing.Color colour, Action action ) {
            Position = new Vector2( x, y );
            Size = new Vector2(width, height);
            Colour = colour;
            ButtonText = new Text(caption, new System.Drawing.PointF(Screen.Width * (x+(width/2)), Screen.Height * (y+(width/2))), 0.5f, System.Drawing.Color.FromArgb(255,255,255));
            ButtonText.Centered = true;
            Action = action;
        }

        public SaltyButton( string texture, float x, float y, float width, float height, float rotation, Action action ) {
            Position = new Vector2(x, y);
            Size = new Vector2(width, height);
            Texture = texture;
            Rotation = rotation;
            Action = action;
        }

        public bool MouseIntersecting() {
            int mouseX = 0, mouseY = 0;
            GetNuiCursorPosition(ref mouseX, ref mouseY);
            return (mouseX > Position.X && mouseX < Position.X + Size.X && mouseY > Position.Y && mouseY < Position.Y + Size.Y);
        }

        public void Draw() {
            if( Texture == "") {
                DrawRect(Position.X, Position.Y, Size.X, Size.Y, Colour.R, Colour.G, Colour.B, Colour.A);
                ButtonText.Draw();
                
            } else {
                if( HasStreamedTextureDictLoaded("saltyTextures") )
                    DrawSprite("saltyTextures", Texture, Position.X, Position.Y, Size.X, Size.Y, Rotation, Colour.R, Colour.G, Colour.B, Colour.A);
            }
        }

        public void Delete() {

        }

    }
}
