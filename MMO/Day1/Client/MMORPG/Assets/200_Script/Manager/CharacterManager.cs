/*
 * PC 및 NPC 등 모든 캐릭터의 생성 및 관리를 담당합니다.
 * 게임 내 캐릭터들의 생성, 초기화, 관리 기능을 제공합니다.
 */

using System.Collections;
using System.Collections.Generic;
using Server;
using UnityEngine;

/// <summary>
/// 게임 내 모든 캐릭터의 생성과 관리를 담당하는 매니저 클래스
/// </summary>
public class CharacterManager : MonoBehaviour
{
    /// <summary>
    /// 플레이어가 직접 조종하는 PC(Player Character) 인스턴스
    /// </summary>
    public PC MyPC;
    
    /// <summary>
    /// 서버로부터 받은 정보를 기반으로 플레이어 캐릭터를 생성하고 초기화합니다.
    /// </summary>
    /// <param name="pcInfo">서버로부터 수신한 PC 정보 (위치, 인덱스 등)</param>
    public void CreateMyPC(PcInfoBr pcInfo)
    {
        //TODO: 2
        return;

        // 플레이어 캐릭터의 루트 오브젝트 생성
        GameObject pcRoot = Instantiate(Manager.Data.PlayerRoot);
        
        // 플레이어 캐릭터의 3D 모델(기사) 생성 및 루트 오브젝트의 자식으로 설정
        GameObject knightModel = Instantiate(Manager.Data.PlayerModel, pcRoot.transform);
        
        // PC 컴포넌트 참조 가져오기
        MyPC = pcRoot.GetComponent<PC>();
        
        // PC 초기 설정
        MyPC.ModelTransform = knightModel.transform;  // 모델의 Transform 설정
        MyPC.Index = pcInfo.Index;                   // 서버에서 할당받은 고유 인덱스 설정
        
        // 서버에서 받은 위치 정보로 PC의 초기 위치 설정
        MyPC.SetPosition(pcInfo.Pos.FLocationToVector3());
        
        // PC 컴포넌트 초기화 (위치 전송 등 시작)
        MyPC.Initialize();
    }
}