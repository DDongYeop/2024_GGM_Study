// Fill out your copyright notice in the Description page of Project Settings.


#include "MyGameInstance.h"

void UMyGameInstance::Init()
{
	//언리얼은 유니코드 체제를 쓴다

	Super::Init();

	// 내 작업
	TCHAR LogCharArray[] = TEXT("Hello Unreal!");
	UE_LOG(LogTemp, Log, LogCharArray);

	FString LogCharString = LogCharArray;
	UE_LOG(LogTemp, Log, TEXT("%s"), *LogCharString);

	const TCHAR* LogCharPtr = *LogCharString;
	TCHAR* LogCharDataPtr = LogCharString.GetCharArray().GetData();

	TCHAR LogCharArrayWithSize[100];
	FCString::Strcpy(LogCharArrayWithSize, LogCharString.Len(), *LogCharString);

	if (LogCharString.Contains(TEXT("unreal"), ESearchCase::IgnoreCase))
	{
		int32 Index = LogCharString.Find(TEXT("unreal"), ESearchCase::IgnoreCase);
		FString EndString = LogCharString.Mid(Index);
		UE_LOG(LogTemp, Log, TEXT("Find Test: %s"), *EndString);
	}

	FString Left, Right;
	if (LogCharString.Split(TEXT(" "), &Left, &Right))
	{
		UE_LOG(LogTemp, Log, TEXT("Split Test : %s 와 %s"), *Left, *Right); 
		// 한국어 나오도록 하는 법 : 파일 -> 다른이름으로 저장 -> 저장 옆 "인코딩하여 저장" -> 인코딩 - 유티코드-코드 페이지 1200
	}

	int32 IntValue = 32;
	float FloatValue = 3.141592;

	FString FloatIntString = FString::Printf(TEXT("Int:%d Float:%f"), IntValue, FloatValue);
	FString FloatString = FString::SanitizeFloat(FloatValue);
	FString IntString = FString::FromInt(IntValue);

	UE_LOG(LogTemp, Log, TEXT("%s"), *FloatIntString);
	UE_LOG(LogTemp, Log, TEXT("Int:%s FLoat:%s"), *IntString, *FloatString);

	int32 IntValueFromString = FCString::Atoi(*IntString);
	float FloatValueFromString = FCString::Atoi(*FloatString);
	FString FloatIntString2 = FString::Printf(TEXT("Int: %d, Float: %f"), IntValueFromString, FloatValueFromString);
	UE_LOG(LogTemp, Log, TEXT("%s"), *FloatIntString2);

	// FName: 에셋 관리를 위해 사용되는 문자욜 체계
	// - 대소문자 구분 없음
	FName Key1(TEXT("PELVIS"));
	FName Key2(TEXT("pelvis"));
	UE_LOG(LogTemp, Log, TEXT("FName 비교 결과 : %s"), Key1 == Key2 ? TEXT("같음") : TEXT("다름"));

	for (int i = 0; i < 10000; ++i)
	{
		FName SearchInNmaePool = FName(TEXT("pelvis"));
		const static FName StaticOnlyOnce(TEXT("pelvis"));
	}
	
	// FText: 다국어 지원을 위한 문자열 관리 체계
	// - 일종의 키로 작용
}
