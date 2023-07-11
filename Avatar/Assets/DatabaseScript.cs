using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;

public class DatabaseScript : MonoBehaviour
{

    public Item[] startingItems;
    public static DatabaseScript instance;
    private int playerID;
    [SerializeField] private Item Ak47Item;
    [SerializeField] private Item dynamiteItem;
    [SerializeField] private Item M4Item;
    [SerializeField] private Item SMGItem;

    private void Awake()
    {
        instance = this;
    }

    private string dbName = "URI=file:usersdata1.db";
    // Start is called before the first frame update
    void Start()
    {

        //create the table
        CreateDB();


        //display records to the console
        DisplayWeapons();
        startingItems = getStartingItems(playerID);
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
                command.CommandText = "CREATE TABLE IF NOT EXISTS userlocation (playerid INT, x INT, y INT, z INT);";
                command.ExecuteNonQuery();
            }

            connection.Close();
        }
    }


    public void AddWeapon(int playerid, int weaponid, int quantity)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {

                //correct format is " INSERT INTO weapons (playerid,weponid,quantity) VALUES (playerid, weaponid, quantity); "
                command.CommandText = "INSERT INTO weapons (playerid,weaponid,quantity) VALUES (" + playerid + "," + weaponid + "," + quantity + ");";
                command.ExecuteNonQuery();

            }

            connection.Close();
        }
        Debug.Log("Weapon added with id: " +  weaponid);
    }


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


        }
    }



    public void UpdatePlayerLocation(int playerID, int x, int y, int z)
    {
        using (var connection = new SqliteConnection(dbName))
        {

            connection.Open();


            using (var command = connection.CreateCommand())
            {
                //update the players location while ensuring playerid is correct
                //  UPDATE userdata SET x = x, SET y = y, SET z = z WHERE playerid = playerid;
                command.CommandText = "UPDATE userlocation SET x = " + x + ", y = " + y + ", Z = " + z + " WHERE playerid = " + playerID + ";";
                command.ExecuteNonQuery();
                //Debug.Log("player location is now : x=" + x + " y= " + y + " z= " + z);

            }

            connection.Close();


        }
    }

    public Item[] getStartingItems(int playerID)
    {

        List<Item> items = new List<Item>();
        using (var connection = new SqliteConnection(dbName))
        {

            connection.Open();


            using (var command = connection.CreateCommand())
            {

                command.CommandText = "SELECT * FROM weapons WHERE playerid ="+ playerID + ";";

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int weaponid = (int)reader["weaponid"];
                        Item newItem = new Item();
                        switch (weaponid)
                        {
                            case 1: newItem = Ak47Item;
                            break;
                            case 2: newItem = dynamiteItem;
                            break;
                            case 3: newItem = M4Item;
                            break;
                            case 4: newItem = SMGItem;
                            break;
                        }
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

}
