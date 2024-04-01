// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Character/ABCharacterBase.h"
#include "ABChracterNonPlayer.generated.h"

/**
 * 
 */
UCLASS()
class ARENABATTLE_API AABChracterNonPlayer : public AABCharacterBase
{
	GENERATED_BODY()
	
protected:
	AABChracterNonPlayer();

protected:
	virtual void SetDead() override;
};
