using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    class TDM : BaseGamemode {

        public enum Teams {
            Spectators,
            Blue,
            Red
        }

        public enum GameState {
            None,
            PreRound,
            InGame,
            PostRound
        }

        public TDM( int ID, Map map, List<Player> players ) : base( ID, map, players ) {
            GameMap.WeaponWeights = new Dictionary<string, int>() {
                { "WEAPON_PISTOL", 8 },
                { "WEAPON_COMBATPISTOL", 7  },
                { "WEAPON_SMG", 6  },
                { "WEAPON_CARBINERIFLE", 5  },
                { "WEAPON_ASSAULTRIFLE", 4  },
                { "WEAPON_PUMPSHOTGUN", 4  },
                { "WEAPON_MICROSMG", 6 },
                { "WEAPON_COMBATMG", 2 }
            };

            GameMap.GameWeapons = new Dictionary<string, string>() {
                { "WEAPON_UNARMED", "Fists" },
                { "WEAPON_PISTOL", "Pistol"  },
                { "WEAPON_COMBATPISTOL", "Combat Pistol" },
                { "WEAPON_SMG", "SMG" },
                { "WEAPON_CARBINERIFLE", "Carbine"  },
                { "WEAPON_ASSAULTRIFLE", "AK47"  },
                { "WEAPON_PUMPSHOTGUN", "Pump Shotgun"  },
                { "WEAPON_MICROSMG", "Micro-SMG" },
                { "WEAPON_COMBATMG", "Light Machine Gun" },
                { "WEAPON_KNIFE", "Knife" }
            };

            GameMap.WeaponSpawnAmmo = new Dictionary<string, int>() {
                { "WEAPON_UNARMED", 0 },
                { "WEAPON_PISTOL", 12  },
                { "WEAPON_COMBATPISTOL", 12 },
                { "WEAPON_MICROSMG", 32 },
                { "WEAPON_SMG", 60 },
                { "WEAPON_CARBINERIFLE", 60  },
                { "WEAPON_ASSAULTRIFLE", 60  },
                { "WEAPON_PUMPSHOTGUN", 8  },
                { "WEAPON_COMBATMG", 100 },
                { "WEAPON_KNIFE", 1 },
            };
        }
        public override void Start() {

            foreach( var ply in InGamePlayers ) {
                SpawnClient( ply, 1, (int)Teams.Blue );
            }

            GameMap.SpawnWeapons();

            base.Start();
        }

        public override void End() {
            TriggerClientEvent( "salty::ClearWeapons", GameMap.Name );
            base.End();
        }

        public override void PlayerKilled( Player player, int killerID, Vector3 deathcords ) {
            if( killerID < 0 ) {
                return;
            }
            Player killer = Init.SourceToPlayer( killerID );
            AddScore( killer, 1 );
            Debug.WriteLine( killer.Name + " killed player " + player.Handle );
            base.PlayerKilled( player, killerID, deathcords );
        }

        public override void PlayerDied( Player player, int killerType, Vector3 deathcords ) {
            Debug.WriteLine( killerType + " died player " + player.Handle );
            base.PlayerDied( player, killerType, deathcords );
        }

    }
}
