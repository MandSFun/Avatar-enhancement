using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using static UnityEngine.Rendering.PostProcessing.SubpixelMorphologicalAntialiasing;

public class DatabaseScript : MonoBehaviour
{

    public Item[] startingItems;
    public static DatabaseScript instance;
    private int playerID = 0;
    [SerializeField] private Item Ak47Item;
    [SerializeField] private Item dynamiteItem;
    [SerializeField] private Item M4Item;
    [SerializeField] private Item SMGItem;
    [SerializeField] private Item smokeGrenadeItem;
    [SerializeField] private Item grenadeItem;
    [SerializeField] private Item backpackItem;


    private void Awake()
    {
        instance = this;
    }

    private string dbName = "URI=file:databasefiles.db";
    // Start is called before the first frame update
    void Start()
    {

        //create the table
        CreateDB();


        //display records to the console
       // DisplayWeapons();
        
    }

    // Update is called once per frame

    public void CreateDB()
    {

        //create the db connection
        using (var connection = new SqliteConnection(dbName))
        {

            connection.Open();

            //set up objeect called command to allow db control
            using (var command = connection.CreateCommand())
            {

                //sql statements to execute
                command.CommandText = "CREATE TABLE IF NOT EXISTS userdata (playerid INT, name VARCHAR(20));";
                command.ExecuteNonQuery();
                command.CommandText = "CREATE TABLE IF NOT EXISTS weapons (playerid INT, weaponid INT, quantity INT);";
                command.ExecuteNonQuery();
                command.CommandText = "CREATE TABLE IF NOT EXISTS playerlocation (playerid INT, x INT, y INT, z INT);";
                command.ExecuteNonQuery();
            }

            connection.Close();
        }
    }


    public void AddWeapon(int playerID, int weaponid, int quantity)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {

                //correct format is " INSERT INTO weapons (playerid,weponid,quantity) VALUES (playerid, weaponid, quantity); "
                command.CommandText = "INSERT INTO weapons (playerid,weaponid,quantity) VALUES (" + playerID + "," + weaponid + "," + quantity + ");";
                command.ExecuteNonQuery();
                Debug.Log("Weapon added with id: " + weaponid + " to player with ID = " + playerID);
            }

            connection.Close();
        }
        
    }

    public void RemoveWeapon(int playerID, int weaponid, int quantity)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {

                //correct format is " DELETE FROM weapons WHERE playerid = player ANS weaponid = id; "
                command.CommandText = "DELETE FROM weapons WHERE playerid = " + playerID + " AND weaponid = " + weaponid  + " ;";
                command.ExecuteNonQuery();

            }

            connection.Close();
        }
        Debug.Log("Weapon added with id: " + weaponid);
    }

    //prints out all items stored in the DB for all users
    public void DisplayWeapons()
    {
        using (var connection = new SqliteConnection(dbName))
        {

            connection.Open();


            using (var command = connection.CreateCommand())
            {

                command.CommandText = "SELECT * FROM weapons;";

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Debug.Log("Player ID: " + reader["playerid"] + " \tWeapon ID: " + reader["weaponid"] + " \tQuanitity: " + reader["quantity"]);
                    }
                    reader.Close();
                }
            }

            connection.Close();


        }
    }



    public void UpdatePlayerLocation(int playerID, int x, int y, int z)
    {
        bool dataFound = false;
        using (var connection = new SqliteConnection(dbName))
        {

            connection.Open();


            using (var command = connection.CreateCommand())
            {

                command.CommandText = "SELECT * FROM playerlocation WHERE playerid = " + playerID + ";";

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["playerID"] == null)
                        {
                            Debug.Log("no prior location data found");
                            dataFound = false;

                        }
                        else
                        {
                            dataFound = true;
                        }
                    }
                    reader.Close();
                }

                if (dataFound)
                {

                    //update the players location while ensuring playerid is correct
                    //  UPDATE userdata SET x = x, SET y = y, SET z = z WHERE playerid = playerid;
                    command.CommandText = "UPDATE playerlocation SET x = " + x + ", y = " + y + ", Z = " + z + " WHERE playerid = " + playerID + ";";
                    command.ExecuteNonQuery();
                    //Debug.Log("player location is now : x=" + x + " y= " + y + " z= " + z);


                }
                else if (!dataFound)
                {
                    command.CommandText = "INSERT INTO playerlocation (playerid,x,y,z) VALUES (" + playerID + "," + x + "," + y + "," + z + ");";
                    command.ExecuteNonQuery();
                }
            }
            connection.Close();


        }
    }

    public Item[] GetStartingItems(int playerID)
    {

        List<Item> items = new List<Item>();
        using (var connection = new SqliteConnection(dbName))
        {

            connection.Open();
            /*
            using (var command = connection.CreateCommand())
            {

                
                command.CommandText = "SELECT * FROM weapons WHERE playerid = " + playerID + "AND weaponid = 7;";

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["playerID"] == null)
                        {
                            AddWeapon(playerID, 7, 1);
                        }

                    }
                    reader.Close();
                }
            }
            */


            using (var command = connection.CreateCommand())
            {
                
                command.CommandText = "SELECT * FROM weapons WHERE playerid =" + playerID + ";";

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Debug.Log("Item found with id " + reader["weaponid"]);
                        Item newItem = SQLConnection.instance.HashToItem((int)reader["weaponid"]);
                        Debug.Log("Item found with name " + newItem.name);
                        items.Add(newItem);
                    }
                    reader.Close();
                }
            }

            connection.Close();


        }

        Item[] startingItems = items.ToArray();
        return startingItems;
    }

    public int[] getStartingLocation(int playerID)
    {

        int[] startingCoordinates = new int[3];
        using (var connection = new SqliteConnection(dbName))
        {

            connection.Open();


            using (var command = connection.CreateCommand())
            {

                command.CommandText = "SELECT * FROM playerlocation WHERE playerid =" + playerID + ";";

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        startingCoordinates[0] = (int)reader["x"];
                        startingCoordinates[1] = (int)reader["y"];
                        startingCoordinates[2] = (int)reader["z"];
                        
                    }
                    reader.Close();
                }
            }

            connection.Close();


        }

        
        return startingCoordinates;
    }

    

}
