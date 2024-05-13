// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/GameModeBase.h"
#include "Interface/ABGameModeInterface.h"
#include "ABGameMode.generated.h"

/**
 * 
 */
UCLASS()
class ARENABATTLE_API AABGameMode : public AGameModeBase, public IABGameModeInterface
{
	GENERATED_BODY()
	
public:
	AABGameMode();

public:
	virtual void OnPlayerScore(int32 NewPlayerScore) override;
	virtual void OnPlayerDead() override;
	virtual bool IsGameCleared() override;

private:
	UPROPERTY(EditDefaultsOnly, BlueprintReadWrite, Category = Game, Meta = (AllowPrivateAccess = "true"))
	int32 ClearScore;

	UPROPERTY(VisibleInstanceOnly, BlueprintReadOnly, Category = Game, Meta = (AllowPrivateAccess = "true"))
	int32 CurrentScore;

	UPROPERTY(VisibleInstanceOnly, BlueprintReadOnly, Category = Game, Meta = (AllowPrivateAccess = "true"))
	uint8 bIsCleared : 1;
};
