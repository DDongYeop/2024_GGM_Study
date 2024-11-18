using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Server;


public class SocketManager
{
    private static SpinLock connectedClientsLock = new SpinLock();
    private static readonly Dictionary<int, Socket> _connectedClients = new Dictionary<int, Socket>();
    private static int _nextClientId = 1;

    public static int AddClient(Socket clientSocket)
    {
        int clientId = _nextClientId++;
        _connectedClients[clientId] = clientSocket;
        return clientId;
    }

    public static void RemoveClient(int clientId)
    {
        if (_connectedClients.ContainsKey(clientId))
        {
            _connectedClients.Remove(clientId);
        }
    }

    public static void BroadcastToAll(PacketType type, byte[] data, int excludeClientId = -1)
    {
#if TimeLogger
        var timer = TimeLogger.Instance;
        timer.Start(TimeLogger.TimerId.BroadcastToAll);
#endif
        List<int> clientsToRemove = new List<int>();
        List<KeyValuePair<int, Socket>> clientsSnapshot;

        // _connectedClients의 스냅샷 생성
        try
        {
            connectedClientsLock.Lock();
            clientsSnapshot = new List<KeyValuePair<int, Socket>>(_connectedClients);
        }
        finally
        {
            connectedClientsLock.Unlock();
        }

        foreach (var client in clientsSnapshot)
        {
            if (client.Key != excludeClientId)
            {
                try
                {
                    Program.SendPacket(client.Value, type, data);
                    Console.WriteLine($"Sent {type} packet to client {client.Key}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending packet to client {client.Key}: {ex.Message}");
                    clientsToRemove.Add(client.Key);
                }
            }
        }
        try
        {
            connectedClientsLock.Lock();
            // 유효하지 않은 클라이언트를 목록에서 제거
            foreach (var clientId in clientsToRemove)
            {
                _connectedClients.Remove(clientId);

                Console.WriteLine($"Removed client {clientId} due to disconnected socket.");
            }
        }
        finally
        {
            connectedClientsLock.Unlock();
        }
#if TimeLogger
        timer.Start(TimeLogger.TimerId.BroadcastToAll);
#endif
    }



    public static int GetClientId(Socket clientSocket)
    {
        foreach (var client in _connectedClients)
        {
            if (client.Value == clientSocket)
            {
                return client.Key;
            }
        }
        return -1;
    }
}
