// Fill out your copyright notice in the Description page of Project Settings.


#include "MyGameInstance.h"

void UMyGameInstance::Init()
{
	//부모의 Init 호출 
	Super::Init();

	UE_LOG(LogTemp, Log, TEXT("%s"), TEXT("Hello Unreal!"));
	UE_LOG(LogTemp, Log, TEXT("%s"), TEXT("Hello Live Coding!"));
}
