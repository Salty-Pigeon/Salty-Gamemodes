using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    class BaseGamemode : BaseScript {

        public Map GameMap;
        public PlayerList players;
        public int ID;

        public BaseGamemode( int ID ) {
            this.ID = ID;
        }

        public virtual void Start() {

        }

        public virtual void End() {

        }

        public virtual void Update() {

        }

        public virtual bool IsBase() {
            return !(GetType().IsSubclassOf( typeof( BaseGamemode ) ));
        }

    }
}
