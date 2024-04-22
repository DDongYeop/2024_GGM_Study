// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "ABStageGimmick.generated.h"

UENUM(BlueprintType)
enum class EStageState : uint8
{
	READY = 0,
	FIGHT,
	REWARD,
	NEXT
};

DECLARE_DELEGATE(FOnStageChangedDelegate);
USTRUCT(BlueprintType)
struct FStageChangedDelegateWrapper
{
	GENERATED_BODY()
	FStageChangedDelegateWrapper() {}
	FStageChangedDelegateWrapper(const FOnStageChangedDelegate& InDelegate) : StageDelegate(InDelegate){}

	FOnStageChangedDelegate StageDelegate;
};

UCLASS()
class ARENABATTLE_API AABStageGimmick : public AActor
{
	GENERATED_BODY()
	
public:	
	// Sets default values for this actor's properties
	AABStageGimmick();

protected:
	virtual void OnConstruction(const FTransform& Transform) override;

// Stage Section
protected:
	UPROPERTY(VisibleAnywhere, Category = Stage, Meta=(AllowPrivateAccess="true"))
	TObjectPtr<class UStaticMeshComponent> Stage;

	UPROPERTY(VisibleAnywhere, Category = Stage, Meta = (AllowPrivateAccess = "true"))
	TObjectPtr<class UBoxComponent> StageTrigger;

	UFUNCTION()
	void OnStageTriggerOverlap(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult);

// Gate Section
protected:
	UPROPERTY(VisibleAnywhere, Category = Gate, Meta = (AllowPrivateAccess = "true"))
	TMap<FName, TObjectPtr<class UStaticMeshComponent>> Gates;

	UPROPERTY(VisibleAnywhere, Category = Gate, Meta = (AllowPrivateAccess = "true"))
	TArray<TObjectPtr<class UBoxComponent>> GateTriggers;

	UFUNCTION()
	void OnGateTriggerOverlap(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult);

	void OpenAllGates();
	void CloseAllGates();

// State Section
protected:
	UPROPERTY(EditAnywhere, Category=Stage, Meta=(AllowPrivateAccess="true"))
	EStageState CurrentState;
	
	UPROPERTY()
	TMap<EStageState, FStageChangedDelegateWrapper> StateChangeActions;

	void SetState(EStageState InNewState);
	void SetReady();
	void SetFight();
	void SetChooseReward();
	void SetChooseNext();
};
