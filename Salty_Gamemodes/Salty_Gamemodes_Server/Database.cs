﻿using System;
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
            conn_string.Server = "localhost";
            conn_string.UserID = "saltypigeon";
            conn_string.Password = "SoccerAnon2";
            conn_string.Database = "saltygamemodes";

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
                    Map map = new Map( new Vector3( x, y, z ), new Vector2( width, height ), name );
                    string spawnPoints = MyDataReader.GetString( 7 );
                    if( spawnPoints.Length > 2 ) {
                        var points = spawnPoints.Split( ':' );
                        for( var i = 0; i < points.Length; i++ ) {
                            var vector = points[ i ].Split( ',' );
                            map.AddSpawnPoint( new Vector3( float.Parse( vector[ 0 ] ), float.Parse( vector[ 1 ] ), float.Parse( vector[ 2 ] ) ) );
                        }
                    }
                    Maps.Add( name, map );
                    Debug.WriteLine( name + " loaded" );
                }
                MyDataReader.Close();
            }
            return Maps;
        }

        private void AddSpawnPoint( Vector3 pos, string name, string spawnPoints ) {
            MySqlCommand comm = new MySqlCommand( "", Connection );
            comm.CommandText = "UPDATE maps SET spawnPoints = ?spawnPoints WHERE name = ?name";
            comm.Parameters.AddWithValue( "spawnPoints", spawnPoints );
            comm.Parameters.AddWithValue( "name", name );
            comm.ExecuteNonQuery();
        }

        public string SpawnPointsAsString( List<Vector3> points ) {
            string spawnPoints = "";
            foreach( var vector in points ) {
                spawnPoints += string.Format( "{0},{1},{2}:", vector.X, vector.Y, vector.Z );
            }
            if( spawnPoints == "" ) {
                spawnPoints = "0,0,0:";
            }
            return spawnPoints.Substring( 0, spawnPoints.Length - 1 );
        }

        public void Save( Dictionary<string, Map> Maps ) {
            if( Connection.State == System.Data.ConnectionState.Open ) {
                foreach( var map in Maps ) {
                    MySqlCommand comm = new MySqlCommand( "", Connection );
                    comm.CommandText = "REPLACE INTO maps(name,x,y,z,width,height,spawnPoints) VALUES(?name, ?x, ?y, ?z, ?width, ?height, ?spawnPoints)";
                    comm.Parameters.AddWithValue( "name", map.Key );
                    comm.Parameters.AddWithValue( "x", map.Value.Position.X );
                    comm.Parameters.AddWithValue( "y", map.Value.Position.Y );
                    comm.Parameters.AddWithValue( "z", map.Value.Position.Z );
                    comm.Parameters.AddWithValue( "width", map.Value.Size.X );
                    comm.Parameters.AddWithValue( "height", map.Value.Size.Y );
                    comm.Parameters.AddWithValue( "spawnPoints", SpawnPointsAsString( map.Value.SpawnPoints ) );
                    comm.ExecuteNonQuery();
                }
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