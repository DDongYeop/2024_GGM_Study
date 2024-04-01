// Fill out your copyright notice in the Description page of Project Settings.


#include "Character/ABChracterNonPlayer.h"
#include "Components//CapsuleComponent.h"
#include "Physics//ABCollision.h"

AABChracterNonPlayer::AABChracterNonPlayer()
{
	//Capsule
	GetCapsuleComponent()->SetCollisionProfileName(CPROFILE_ABCAPSULE);

	//Mesh
	GetMesh()->SetCollisionProfileName(TEXT("NoCollision"));

	static ConstructorHelpers::FObjectFinder <USkeletalMesh> CharacterMeshRef(TEXT("/Game/InfinityBladeWarriors/Character/CompleteCharacters/SK_CharM_Ram.SK_CharM_Ram"));
	if (CharacterMeshRef.Object)
		GetMesh()->SetSkeletalMesh(CharacterMeshRef.Object);
}

void AABChracterNonPlayer::SetDead()
{
	Super::SetDead();

	FTimerHandle DeadTimerHandle;
	GetWorld()->GetTimerManager().SetTimer(DeadTimerHandle, FTimerDelegate::CreateLambda(
		[&]()
		{
			Destroy();
		}), DeadEventDelayTime, false);
}
