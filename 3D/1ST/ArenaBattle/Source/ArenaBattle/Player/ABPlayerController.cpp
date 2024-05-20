// Fill out your copyright notice in the Description page of Project Settings.


#include "Player/ABPlayerController.h"
#include <Kismet/GameplayStatics.h>
#include "UI/ABHUDWidget.h"
#include "ABSaveGame.h"

AABPlayerController::AABPlayerController()
{
	static ConstructorHelpers::FClassFinder<UABHUDWidget> ABHUDWidgetRef(TEXT("/Game/ArenaBattle/UI/WBP_ABHUD.WBP_ABHUD_C"));
	if (ABHUDWidgetRef.Class)
	{
		ABHUDWidgetClass = ABHUDWidgetRef.Class;
	}

	SaveGameInstance = Cast<UABSaveGame>(UGameplayStatics::LoadGameFromSlot(TEXT("Player0"), 0));
	if (!SaveGameInstance)
	{
		SaveGameInstance = NewObject<UABSaveGame>();
		SaveGameInstance->RetryCount = 0;

		if (!UGameplayStatics::SaveGameToSlot(SaveGameInstance, TEXT("Player0"), 0))
		{
			UE_LOG(LogTemp, Error, TEXT("Save Game Error!"));
		}
	}
}

void AABPlayerController::GameScoreChanged(int32 NewScore)
{
	BP_OnScoreChanged(NewScore);
}

void AABPlayerController::GameClear()
{
	BP_OnGameClear();
}

void AABPlayerController::GameOver()
{
	BP_OnGameOver();

	if (!UGameplayStatics::SaveGameToSlot(SaveGameInstance, TEXT("Player0"), 0))
	{
		UE_LOG(LogTemp, Error, TEXT("Save Game Error!"));
	}

	BP_OnGameRetryCount(SaveGameInstance->RetryCount);
}

void AABPlayerController::BeginPlay()
{
	Super::BeginPlay();

	FInputModeGameOnly GameOnlyInputMode;
	SetInputMode(GameOnlyInputMode);

	SaveGameInstance = Cast<UABSaveGame>(UGameplayStatics::LoadGameFromSlot(TEXT("Player0"), 0));
	if (!SaveGameInstance)
	{
		SaveGameInstance = NewObject<UABSaveGame>();
		SaveGameInstance->RetryCount = 0;
	}

	SaveGameInstance->RetryCount++;

	BP_OnGameRetryCount(SaveGameInstance->RetryCount);
}
