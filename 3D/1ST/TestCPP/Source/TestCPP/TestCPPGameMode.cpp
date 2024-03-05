// Copyright Epic Games, Inc. All Rights Reserved.

#include "TestCPPGameMode.h"
#include "TestCPPCharacter.h"
#include "UObject/ConstructorHelpers.h"

ATestCPPGameMode::ATestCPPGameMode()
{
	// set default pawn class to our Blueprinted character
	static ConstructorHelpers::FClassFinder<APawn> PlayerPawnBPClass(TEXT("/Game/ThirdPerson/Blueprints/BP_ThirdPersonCharacter"));
	if (PlayerPawnBPClass.Class != NULL)
	{
		DefaultPawnClass = PlayerPawnBPClass.Class;
	}
}
