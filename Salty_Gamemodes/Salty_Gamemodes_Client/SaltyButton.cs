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

        SaltyText ButtonText;
        SaltyMenu Parent;
        Vector2 Position;
        Vector2 Size;
        float Rotation;
        System.Drawing.Color Colour = System.Drawing.Color.FromArgb(255,255,255,255);
        string Texture = "";
        public Action Action;

        public SaltyButton( SaltyMenu parent, string caption, float x, float y, float width, float height, System.Drawing.Color colour, Action action ) {
            Parent = parent;
            Position = new Vector2( x, y );
            Size = new Vector2(width, height);
            Colour = colour;
            ButtonText = new SaltyText(x, y - (height/2), 0, 0, 0.2f, caption, 255, 255, 255, 255, false, true, 0, true );
            Action = action;
        }

        public SaltyButton( SaltyMenu parent, string texture, float x, float y, float width, float height, float rotation, Action action ) {
            Parent = parent;
            Position = new Vector2(x, y);
            Size = new Vector2(width, height);
            Texture = texture;
            Rotation = rotation;
            Action = action;
        }

        public bool MouseIntersecting() {
            int mouseX = 0, mouseY = 0;
            
            GetNuiCursorPosition(ref mouseX, ref mouseY);

            float sizeX = Init.ScreenWidth * (Position.X * Size.X);
            float sizeY = Init.ScreenHeight * (Position.Y * Size.Y);
            float x = (Init.ScreenWidth * Position.X) - (sizeX);
            float y = (Init.ScreenHeight * Position.Y) - (sizeY);
            return (mouseX > x && mouseX < x + (sizeX*2) && mouseY > y && mouseY < y + (sizeY*2));
        }

        public void Draw() {
            if( Texture == "") {
                DrawRect(Position.X, Position.Y, Size.X, Size.Y, Colour.R, Colour.G, Colour.B, Colour.A);
                ButtonText.Draw();
            }
            else {
                if( HasStreamedTextureDictLoaded( "saltyTextures" ) )
                    DrawSprite( "saltyTextures", Texture, Position.X, Position.Y, Size.X, Size.Y, Rotation, Colour.R, Colour.G, Colour.B, Colour.A);
            }
        }

        public void Delete() {

        }

    }
}
