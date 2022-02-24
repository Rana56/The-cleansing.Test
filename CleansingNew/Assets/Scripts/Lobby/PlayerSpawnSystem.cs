using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheCleansing.Lobby
{
    public class PlayerSpawnSystem : NetworkBehaviour                   //spawn system created by the server, when spawning to a scene a game object with this scripte will be spawned
    {
        [SerializeField] private GameObject playerPrefab = null;        //the prefab that will spawn, assigned to a connection

        private static List<Transform> spawnPoints = new List<Transform>();         //stores the posisitions in the scene, transform - position, rotation, scale

        private int nextIndex = 0;          //when player spawns in, this allows the server to know where to spawn next player

        public static void AddSpawnPoint(Transform transform)               //spawns system doesn't exist during desgin time, spwaned later, method called by other objects
        {
            spawnPoints.Add(transform);             //adds to the list

            spawnPoints = spawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();           //makes sure the order is correct
        }

        public static void RemoveSpawnPoint(Transform transform) => spawnPoints.Remove(transform);          //removes spawn point

        public override void OnStartServer() => NetworkManagerCleansingLobby.OnServerReadied += SpawnPlayer;         //when this game object starts existing on the server, the object is subscribed to the onserverReadied event

        [ServerCallback]
        private void OnDestroy() => NetworkManagerCleansingLobby.OnServerReadied -= SpawnPlayer;             //when the object is destroyed, it is unsubscribed from the event

        [Server]
        public void SpawnPlayer(NetworkConnection conn)             //takes the connection of player as a parameter
        {
            Transform spawnPoint = spawnPoints.ElementAtOrDefault(nextIndex);           //gets the next spawn point based on the next index

            if (spawnPoint == null)             //exception handling, incase spawn is empty
            {
                Debug.LogError($"Missing spawn point for player {nextIndex}");
                return;
            }

            GameObject playerInstance = Instantiate(playerPrefab, spawnPoints[nextIndex].position, spawnPoints[nextIndex].rotation);           //spawns in the player, instantiates the prefab, spawns it at the position from spawnPoints facing a certian direction (rotation)
            //NetworkServer.Spawn(playerInstance, conn);          //spwans it for the other clients, connection also passed a parameter to show the connection belongs to the player object that is spawned in - the user has authority over it
            NetworkServer.AddPlayerForConnection(conn, playerInstance);

            nextIndex++;        //increases the index, so when a new player joins they get a new spawn point
        }
    }
}