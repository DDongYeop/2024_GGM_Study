/*
 * 서버-클라이언트 패킷 통신 간에 변환해야하는 코드들을 정의합니다.
 */

using Server;
using Server.C2SInGame;
using UnityEngine;

public static class PacketHelper
{
    public static Vector3 FLocationToVector3(this FLocation fLocation)
    {
        return new Vector3(fLocation.X, fLocation.Y, fLocation.Z);
    }
    
    public static FLocation Vector3ToFLocation(this Vector3 vector3)
    {
        var playerFLocation = new FLocation();
        playerFLocation.X = vector3.x;
        playerFLocation.Y = vector3.y;
        playerFLocation.Z = vector3.z;
        return playerFLocation;
    }
}
