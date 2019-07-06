using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace Salty_Gamemodes_Server {
    class Database {
        
        MySqlConnection Connection;

        public Database() {
            MySqlConnectionStringBuilder conn_string = new MySqlConnectionStringBuilder();
            conn_string.Server = "mysql-mariadb-dal01-9-101.zap-hosting.com";
            conn_string.UserID = "zap429233-1";
            conn_string.Password = "8elLq3P3BUgFE7I3";
            conn_string.Database = "zap429233-1";

            Connection = new MySqlConnection( conn_string.ToString() );
            Connection.Open();
        }

        public Dictionary<string, Map> Load() {
            Dictionary<string, Map> Maps = new Dictionary<string, Map>();
            if( Connection.State == System.Data.ConnectionState.Open ) {
                MySqlCommand comm = new MySqlCommand( "", Connection );
                comm.CommandText = "SELECT * FROM maps";
                MySqlDataReader MyDataReader = comm.ExecuteReader();
                while( MyDataReader.Read() ) {
                    string name = MyDataReader.GetString( 1 );
                    float x = MyDataReader.GetFloat( 2 );
                    float y = MyDataReader.GetFloat( 3 );
                    float z = MyDataReader.GetFloat( 4 );
                    float width = MyDataReader.GetFloat( 5 );
                    float height = MyDataReader.GetFloat( 6 );
                    Map map = new Map( new Vector3( x, y, z ), new Vector3( width, height, 0 ), name );
                    string spawnPoints = MyDataReader.GetString( 7 );
                    if( spawnPoints.Length > 2 ) {
                        var points = spawnPoints.Split( ':' );
                        for( var i = 0; i < points.Length; i++ ) {
                            var vector = points[ i ].Split( ',' );
                            //map.AddSpawnPoint( Convert.ToInt32(vector[0]), new Vector3( float.Parse( vector[1] ), float.Parse( vector[2] ), float.Parse( vector[3] ) ) );
                            map.AddSpawnPoint( Convert.ToInt32(vector[0]), new Vector3( float.Parse( vector[1] ), float.Parse( vector[2] ), float.Parse( vector[3] ) ) );
                        }
                    }

                    string gunSpawns = MyDataReader.GetString( 8 );
                    if( gunSpawns.Length > 2 ) {
                        var points = gunSpawns.Split( ':' );
                        for( var i = 0; i < points.Length; i++ ) {
                            var vector = points[i].Split( ',' );
                            map.AddWeaponSpawn( vector[0], new Vector3( float.Parse( vector[1] ), float.Parse( vector[2] ), float.Parse( vector[3] ) ) );
                        }
                    }
                    
                    Maps.Add( name, map );
                }
                MyDataReader.Close();
            }
            return Maps;
        }


        public void SaveAll( Dictionary<string, Map> Maps ) {
            if( Connection.State == System.Data.ConnectionState.Open ) {
                foreach( var map in Maps ) {
                    MySqlCommand comm = new MySqlCommand( "", Connection );
                    comm.CommandText = "REPLACE INTO maps(name,x,y,z,width,height,spawnPoints,gunSpawns) VALUES(?name, ?x, ?y, ?z, ?width, ?height, ?spawnPoints, ?gunSpawns)";
                    comm.Parameters.AddWithValue( "name", map.Key );
                    comm.Parameters.AddWithValue( "x", map.Value.Position.X );
                    comm.Parameters.AddWithValue( "y", map.Value.Position.Y );
                    comm.Parameters.AddWithValue( "z", map.Value.Position.Z );
                    comm.Parameters.AddWithValue( "width", map.Value.Size.X );
                    comm.Parameters.AddWithValue( "height", map.Value.Size.Y );
                    comm.Parameters.AddWithValue( "spawnPoints", map.Value.SpawnPointsAsString( ) );
                    comm.Parameters.AddWithValue( "gunSpawns", map.Value.GunSpawnsAsString( ) );
                    comm.ExecuteNonQuery();
                }
            }
        }

        public void SaveMap( Map map ) {
            if( Connection.State == System.Data.ConnectionState.Open ) {
                MySqlCommand comm = new MySqlCommand( "", Connection );
                comm.CommandText = "REPLACE INTO maps(name,x,y,z,width,height,spawnPoints,gunSpawns) VALUES(?name, ?x, ?y, ?z, ?width, ?height, ?spawnPoints, ?gunSpawns)";
                comm.Parameters.AddWithValue( "name", map.Name );
                comm.Parameters.AddWithValue( "x", map.Position.X );
                comm.Parameters.AddWithValue( "y", map.Position.Y );
                comm.Parameters.AddWithValue( "z", map.Position.Z ); 
                comm.Parameters.AddWithValue( "width", map.Size.X );
                comm.Parameters.AddWithValue( "height", map.Size.Y );
                comm.Parameters.AddWithValue( "spawnPoints", map.SpawnPointsAsString() );
                comm.Parameters.AddWithValue( "gunSpawns", map.GunSpawnsAsString() );
                comm.ExecuteNonQuery();
            }
        }

        public void Remove( string name ) {
            MySqlCommand comm = new MySqlCommand( "", Connection );
            comm.CommandText = "DELETE FROM maps WHERE name = ?name";
            comm.Parameters.AddWithValue( "name", name );
            comm.ExecuteNonQuery();
        }

    }
}
