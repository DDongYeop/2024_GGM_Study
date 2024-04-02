// Fill out your copyright notice in the Description page of Project Settings.


#include "UI/ABHpBarWidjet.h"
#include "Components/ProgressBar.h"
#include "UI//ABCharacterWidgetInterface.h"

UABHpBarWidjet::UABHpBarWidjet(const FObjectInitializer& ObjectInitializer) : Super(ObjectInitializer)
{
	MaxHp = -1.0f;
}

void UABHpBarWidjet::NativeConstruct()
{
	Super::NativeConstruct();

	HpProgressBar = Cast<UProgressBar>(GetWidgetFromName(TEXT("PbHpBar")));
	ensure(HpProgressBar);

	IABCharacterWidgetInterface* CharacterWidget = Cast<IABCharacterWidgetInterface>(OwningActor);

	if (CharacterWidget)
	{
		CharacterWidget->SetupCharacterWidget(this);
	}
}

void UABHpBarWidjet::UpdateHpBar(float NewCurrentHp)
{
	ensure(MaxHp > 0.0f);

	if (HpProgressBar)
	{
		HpProgressBar->SetPercent(NewCurrentHp / MaxHp);
	}
}
