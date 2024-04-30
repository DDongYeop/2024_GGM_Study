// Fill out your copyright notice in the Description page of Project Settings.


#include "Character/ABCharacterNonPlayer.h"
#include "Components/CapsuleComponent.h"
#include "Physics/ABCollision.h"
#include <Engine/AssetManager.h>
#include "AI/ABAIController.h"

AABCharacterNonPlayer::AABCharacterNonPlayer()
{
	// Capsule
	GetCapsuleComponent()->SetCollisionProfileName(CPROFILE_ABCAPSULE);

	// Mesh
	GetMesh()->SetCollisionProfileName(TEXT("NoCollision"));
	GetMesh()->SetHiddenInGame(true);

	// AI
	AIControllerClass = AABAIController::StaticClass();
	AutoPossessAI = EAutoPossessAI::PlacedInWorldOrSpawned;
}

void AABCharacterNonPlayer::PostInitializeComponents()
{
	Super::PostInitializeComponents();

	ensure(NPCMeshes.Num() > 0);
	int32 RandIndex = FMath::RandRange(0, NPCMeshes.Num() - 1);
	NPCMeshHandle = UAssetManager::Get().GetStreamableManager().RequestAsyncLoad(NPCMeshes[RandIndex], FStreamableDelegate::CreateUObject(this, &AABCharacterNonPlayer::NPCMeshLoadCompleted));
}

float AABCharacterNonPlayer::GetAIPatrolRadius()
{
	return 800.0f;
}

float AABCharacterNonPlayer::GetAIDetectRange()
{
	return 0.0f;
}

float AABCharacterNonPlayer::GetAIAttackRange()
{
	return 0.0f;
}

float AABCharacterNonPlayer::GetAITurnSpeed()
{
	return 0.0f;
}

void AABCharacterNonPlayer::SetDead()
{
	Super::SetDead();

	FTimerHandle DeadTimerHandle;
	GetWorld()->GetTimerManager().SetTimer(DeadTimerHandle, FTimerDelegate::CreateLambda(
		[&]()
		{
			Destroy();
		}), 
		DeadEventDelayTime, false);
}

void AABCharacterNonPlayer::NPCMeshLoadCompleted()
{
	if (NPCMeshHandle.IsValid())
	{
		USkeletalMesh* NPCMesh = Cast<USkeletalMesh>(NPCMeshHandle->GetLoadedAsset());
		if (NPCMesh)
		{
			GetMesh()->SetSkeletalMesh(NPCMesh);
			GetMesh()->SetHiddenInGame(false);
		}
	}
	NPCMeshHandle->ReleaseHandle();
}
