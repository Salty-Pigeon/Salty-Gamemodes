﻿using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    public class MapManager : BaseScript {

        public Dictionary<string, Map> Maps;

        public Dictionary<string, int> MapVotes = new Dictionary<string, int>();
        public double VoteTime = 0;
        public bool VoteStarted = false;
        public List<Player> PlayerVoted = new List<Player>();

        public MapManager( Dictionary<string,Map> maps ) {
            Maps = maps;
        }

        public void AddMap( Map map ) {
            if( Maps.ContainsKey( map.Name ) )
                return;
            Maps.Add( map.Name, map );
        }

        public Map FindInsideMap( Vector3 pos ) {
            foreach( var map in Maps ) {
                if( pos.X > map.Value.Position.X - ( map.Value.Size.X / 2 ) && pos.X < map.Value.Position.X + ( map.Value.Size.X / 2 ) && pos.Y > map.Value.Position.Y - ( map.Value.Size.Y / 2 ) && pos.Y < map.Value.Position.Y + ( map.Value.Size.Y / 2 ) ) {
                    return map.Value;
                }
            }
            return null;
        }

        public List<Map> MapList(string mapTag) {
            if( mapTag == "*" ) {
                return Maps.Values.ToList();
            }
            return Maps.Values.Where( i => i.Name.Contains( mapTag ) ).ToList();
        }

        public Dictionary<string,Dictionary<int,List<Vector3>>> AllMapsSpawns() {
            Dictionary<string, Dictionary<int,List<Vector3>>> maps = new Dictionary<string, Dictionary<int,List<Vector3>>>();
            foreach( var map in Maps ) {
                maps.Add( map.Value.Name, map.Value.SpawnPoints );

            }
            return maps;
        }

        public Dictionary<string, List<Vector3>> AllMapsBounds() {
            Dictionary<string, List<Vector3>> maps = new Dictionary<string, List<Vector3>>();
            foreach( var map in Maps ) {
                maps.Add( map.Value.Name, new List<Vector3> { map.Value.Position, map.Value.Size } );
            }
            return maps;
        }

        public void Update() {

        }

    }
}
