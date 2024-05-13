// Fill out your copyright notice in the Description page of Project Settings.


#include "Player/ABPlayerController.h"
#include "UI/ABHUDWidget.h"

AABPlayerController::AABPlayerController()
{
	static ConstructorHelpers::FClassFinder<UABHUDWidget> ABHUDWidgetRef(TEXT("/Game/ArenaBattle/UI/WBP_ABHUD.WBP_ABHUD_C"));
	if (ABHUDWidgetRef.Class)
		ABHUDWidgetClass = ABHUDWidgetRef.Class;
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
}

void AABPlayerController::BeginPlay()
{
	Super::BeginPlay();

	FInputModeGameOnly GameOnlyInputMode;
	SetInputMode(GameOnlyInputMode);

	ABHUDWidget = CreateWidget<UABHUDWidget>(this, ABHUDWidgetClass);
	if (ABHUDWidget)
		ABHUDWidget->AddToViewport();
}
