/*
 * 하위 매니저들을 종합적으로 관리하는 매니저 클래스입니다.
 * 게임의 주요 매니저들(Data, Network, Character)을 초기화하고 전역 접근을 제공합니다.
 * 싱글톤 패턴을 활용하여 정적 프로퍼티를 통해 각 매니저에 접근할 수 있습니다.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임의 모든 매니저 시스템을 총괄하는 메인 매니저 클래스
/// </summary>
public class Manager : MonoBehaviour
{
    /// <summary>
    /// 게임 시작 시 모든 하위 매니저들을 순차적으로 초기화합니다.
    /// </summary>
    private void Start()
    {
        // 데이터 매니저 초기화 (프리팹, 에셋 등의 데이터 관리)
        SetDataManager();
        
        // 캐릭터 매니저 초기화 (PC, NPC 관리)
        SetCharacterManager();
        
        // 네트워크 매니저 초기화 (서버 통신 관리)
        SetNetworkManager();
    }
    
    /// <summary>
    /// 게임 데이터(프리팹, 에셋 등)를 관리하는 매니저에 대한 전역 접근 프로퍼티
    /// </summary>
    public static DataManager Data { get; private set; }

    /// <summary>
    /// DataManager 컴포넌트를 찾아 초기화합니다.
    /// </summary>
    private void SetDataManager()
    {
        Data = transform.Find("DataManager").GetComponent<DataManager>();
    }
    
    /// <summary>
    /// 서버 통신을 관리하는 매니저에 대한 전역 접근 프로퍼티
    /// </summary>
    public static NetworkManager Net { get; private set; }

    /// <summary>
    /// NetworkManager 컴포넌트를 찾아 초기화하고 필요한 설정을 수행합니다.
    /// </summary>
    private void SetNetworkManager()
    {
        Net = transform.Find("NetworkManager").GetComponent<NetworkManager>();
        Net.Initialize();  // 네트워크 매니저의 추가 초기화 작업 수행
    }
    
    /// <summary>
    /// 캐릭터 생성 및 관리를 담당하는 매니저에 대한 전역 접근 프로퍼티
    /// </summary>
    public static CharacterManager Char { get; private set; }

    /// <summary>
    /// CharacterManager 컴포넌트를 찾아 초기화합니다.
    /// </summary>
    private void SetCharacterManager()
    {
        Char = transform.Find("CharacterManager").GetComponent<CharacterManager>();
    }
}