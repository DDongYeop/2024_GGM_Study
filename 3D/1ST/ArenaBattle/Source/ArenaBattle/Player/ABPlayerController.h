// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/PlayerController.h"
#include "ABPlayerController.generated.h"

/**
 * 
 */
UCLASS()
class ARENABATTLE_API AABPlayerController : public APlayerController
{
	GENERATED_BODY()
	
public:
	AABPlayerController();

public:
	UFUNCTION(BlueprintImplementableEvent, Category = Game, Meta=(DisplayName="OnScoreChangedCpp"))
	void BP_OnScoreChanged(int32 NewScore);
	UFUNCTION(BlueprintImplementableEvent, Category = Game, Meta = (DisplayName = "OnGameClearCpp"))
	void BP_OnGameClear();
	UFUNCTION(BlueprintImplementableEvent, Category = Game, Meta = (DisplayName = "OnGameOverCpp"))
	void BP_OnGameOver();
	UFUNCTION(BlueprintImplementableEvent, Category = Game, Meta = (DisplayName = "OnGameRetryCountCpp"))
	void BP_OnGameRetryCount(int32 NewRetryCount);

	void GameScoreChanged(int32 NewScore);
	void GameClear();
	void GameOver();

protected:
	virtual void BeginPlay() override;

// HUD Section
protected:
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category=HUD)
	TSubclassOf<class UABHUDWidget> ABHUDWidgetClass;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = HUD)
	TObjectPtr<class UABHUDWidget> ABHUDWidget;

// Save Game Section
protected:
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category=SaveGame)
	TObjectPtr<class UABSaveGame> SaveGameInstance;
};
