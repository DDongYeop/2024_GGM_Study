// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "ABUserWIdget.h"
#include "ABHpBarWidjet.generated.h"

/**
 * 
 */
UCLASS()
class ARENABATTLE_API UABHpBarWidjet : public UABUserWIdget
{
	GENERATED_BODY()
	
public:
	UABHpBarWidjet(const FObjectInitializer& ObjectInitializer);

protected:
	virtual void NativeConstruct();

public:
	FORCEINLINE void SetMaxHp(float NewMaxHp) { MaxHp = NewMaxHp; }
	void UpdateHpBar(float NewCurrentHp);

protected:
	UPROPERTY()
	TObjectPtr<class UProgressBar> HpProgressBar;
	
	UPROPERTY()
	float MaxHp;
};
