using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Dynamic;
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

        public virtual void PlayerKilled( Player player, int killerID, ExpandoObject deathData ) {
            Debug.WriteLine( "Player killed" );
        }

        public virtual void PlayerDied( Player player, int killerType, Vector3 deathcords ) {
            Debug.WriteLine( "Player died" );
        }

        public virtual bool IsBase() {
            return !(GetType().IsSubclassOf( typeof( BaseGamemode ) ));
        }

        public void WriteChat( string str ) {
            TriggerClientEvent( "chat:addMessage", new {
                color = new[] { 255, 0, 0 },
                args = new[] { GetType().ToString(), str }
            } );
        }

    }
}
