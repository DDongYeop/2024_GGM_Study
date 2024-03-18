// Fill out your copyright notice in the Description page of Project Settings.


#include "Character/ABCharacterPlayer.h"
#include "Camera/CameraComponent.h"
#include "GameFramework/SpringArmComponent.h"
#include "InputMappingContext.h"
#include "EnhancedInputComponent.h"
#include "EnhancedInputSubsystems.h"
#include "ABCharacterControlData.h"

AABCharacterPlayer::AABCharacterPlayer()
{
	// Camera
	CameraBoom = CreateDefaultSubobject<USpringArmComponent>(TEXT("CameraBoom"));
	CameraBoom->SetupAttachment(RootComponent);
	CameraBoom->TargetArmLength = 400.0f; // 4m 
	CameraBoom->bUsePawnControlRotation = true;

	FollowCamera = CreateDefaultSubobject<UCameraComponent>(TEXT("FollowCamera"));
	FollowCamera->SetupAttachment(CameraBoom, USpringArmComponent::SocketName);
	FollowCamera->bUsePawnControlRotation = false;

	// Input
	static ConstructorHelpers::FObjectFinder<UInputAction> JumpActionRef(TEXT("/Game/ArenaBattle/Input/Actions/IA_Jump.IA_Jump"));
	if (JumpActionRef.Object)
		JumpAction = JumpActionRef.Object;

	static ConstructorHelpers::FObjectFinder<UInputAction> ShoulderMoveActionRef(TEXT("/Game/ArenaBattle/Input/Actions/IA_ShoulderMove.IA_ShoulderMove"));
	if (ShoulderMoveActionRef.Object)
		ShoulderMoveAction = ShoulderMoveActionRef.Object;

	static ConstructorHelpers::FObjectFinder<UInputAction> ShoulderLookActionRef(TEXT("/Game/ArenaBattle/Input/Actions/IA_ShoulderLook.IA_ShoulderLook"));
	if (ShoulderLookActionRef.Object)
		ShoulderLookAction = ShoulderLookActionRef.Object;

	static ConstructorHelpers::FObjectFinder<UInputAction> QuaterMoveActionRef(TEXT("/Game/ArenaBattle/Input/Actions/IA_QuaterMove.IA_QuaterMove"));
	if (QuaterMoveActionRef.Object)
		QuaterMoveAction = QuaterMoveActionRef.Object;

	static ConstructorHelpers::FObjectFinder<UInputAction> ChangeControlActionRef(TEXT("/Game/ArenaBattle/Input/Actions/IA_ChangeControl.IA_ChangeControl"));
	if (ChangeControlActionRef.Object)
		ChangeControlAction = ChangeControlActionRef.Object;
}

void AABCharacterPlayer::BeginPlay()
{
	Super::BeginPlay();
}

void AABCharacterPlayer::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent)
{
	Super::SetupPlayerInputComponent(PlayerInputComponent);

	UEnhancedInputComponent* EnhancedInputComponent = CastChecked<UEnhancedInputComponent>(PlayerInputComponent);
	EnhancedInputComponent->BindAction(JumpAction, ETriggerEvent::Triggered, this, &ACharacter::Jump);
	EnhancedInputComponent->BindAction(JumpAction, ETriggerEvent::Completed, this, &ACharacter::StopJumping);
	EnhancedInputComponent->BindAction(ShoulderMoveAction, ETriggerEvent::Triggered, this, &AABCharacterPlayer::ShoulderMove);
	EnhancedInputComponent->BindAction(ShoulderLookAction, ETriggerEvent::Triggered, this, &AABCharacterPlayer::ShoulderLook);
	EnhancedInputComponent->BindAction(QuaterMoveAction, ETriggerEvent::Triggered, this, &AABCharacterPlayer::QuaterMove);
	EnhancedInputComponent->BindAction(ChangeControlAction, ETriggerEvent::Triggered, this, &AABCharacterPlayer::ChangeCharacterControl);
}

void AABCharacterPlayer::SetCharacterControlData(const UABCharacterControlData* CharacterControlData)
{
	Super::SetCharacterControlData(CharacterControlData);

	CameraBoom->TargetArmLength = CharacterControlData->TargetArmLeghth;
	CameraBoom->SetRelativeRotation(CharacterControlData->RelativeRotation);
	CameraBoom->bUsePawnControlRotation = CharacterControlData->bUsePawnControlRotation;
	CameraBoom->bInheritPitch = CharacterControlData->bInheritPitch;
	CameraBoom->bInheritYaw = CharacterControlData->bInheritYaw;
	CameraBoom->bInheritRoll = CharacterControlData->bInheritRoll;
	CameraBoom->bDoCollisionTest = CharacterControlData->bDoCollisionTest;
}

void AABCharacterPlayer::ShoulderMove(const FInputActionValue& Value)
{
	FVector2D MovementVector = Value.Get<FVector2D>();

	const FRotator Rotation = Controller->GetControlRotation();
	const FRotator YawRotation(0, Rotation.Yaw, 0);

	const FVector ForwardDirction = FRotationMatrix(YawRotation).GetUnitAxis(EAxis::X);
	const FVector RightDirection = FRotationMatrix(YawRotation).GetUnitAxis(EAxis::Y);

	AddMovementInput(ForwardDirction, MovementVector.X);
	AddMovementInput(RightDirection, MovementVector.Y);
}

void AABCharacterPlayer::ShoulderLook(const FInputActionValue& Value)
{
	FVector2D LookAxisVector = Value.Get<FVector2D>();
	AddControllerYawInput(LookAxisVector.X);
	AddControllerPitchInput(LookAxisVector.Y);
}

void AABCharacterPlayer::QuaterMove(const FInputActionValue& Value)
{
}

void AABCharacterPlayer::ChangeCharacterControl()
{
}
