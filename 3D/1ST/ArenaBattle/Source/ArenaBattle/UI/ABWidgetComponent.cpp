// Fill out your copyright notice in the Description page of Project Settings.


#include "UI/ABWidgetComponent.h"
#include "ABUserWIdget.h"

void UABWidgetComponent::InitWidget()
{
	Super::InitWidget();

	UABUserWIdget* ABUserWidget = Cast<UABUserWIdget>(GetWidget());

	if (ABUserWidget)
	{
		ABUserWidget->SetOwningActor(GetOwner());
	}
}
