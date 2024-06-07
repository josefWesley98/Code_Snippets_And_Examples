// Fill out your copyright notice in the Description page of Project Settings.


#include "CustomARPawn.h"

#include <string>

#include "Runtime/Engine/Classes/Kismet/KismetSystemLibrary.h"
#include "Runtime/Engine/Classes/Kismet/KismetMathLibrary.h"
#include "Runtime/Engine/Classes/Kismet/GameplayStatics.h"
#include "Materials/MaterialInstanceDynamic.h"
#include "GameFramework/PhysicsVolume.h"
#include "PhysicsEngine/PhysicsConstraintComponent.h"
#include "PlaceableActor.h"
#include "Components/MeshComponent.h"
#include "CustomGameMode.h"
#include "ARPin.h"
#include "Kismet/KismetSystemLibrary.h"
#include "ProjectileActor.h"
#include "BackgroundActor.h"
#include "LegoEngineActor.h"
#include "LegoFuelTankActor.h"
#include "Engine.h"
#include "ARBlueprintLibrary.h"
#include "Camera/CameraComponent.h"
#include "Containers/Ticker.h"
#include "Containers/UnrealString.h"
#include "CustomGameMode.h"

// Sets default values
ACustomARPawn::ACustomARPawn()
{
 	// Set this pawn to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;
	SceneComponent = CreateDefaultSubobject<USceneComponent>(TEXT("SceneComponent"));
	SetRootComponent(SceneComponent);
	CameraComponent = CreateDefaultSubobject<UCameraComponent>(TEXT("CameraComponent"));
	CameraComponent->SetupAttachment(SceneComponent);

	static ConstructorHelpers::FObjectFinder<UMaterialInterface> MaterialAsset(TEXT("Material'/Game/Assets/Materials/SelectedMaterial.SelectedMaterial'"));
	CustomMaterial = MaterialAsset.Object;
	
	CustomActorConstraint = CreateDefaultSubobject<UPhysicsConstraintComponent>(TEXT("CustomActorConstraint"));
	CustomActorConstraint->SetupAttachment(RootComponent);
	CustomActorConstraint->SetActive(false);
	CustomActorConstraint->Deactivate();
	
	BackgroundMusic = CreateDefaultSubobject<UAudioComponent>(TEXT("BackgroundMusic"));
	BackgroundMusic->SetupAttachment(RootComponent);
	static ConstructorHelpers::FObjectFinder<USoundWave> SoundCueAsset(TEXT("SoundWave'/Game/Assets/Audio/BGMusic_mp4.BGMusic_mp4'"));
	if (SoundCueAsset.Succeeded())
	{
		BackgroundMusic->SetSound(SoundCueAsset.Object);
		BackgroundMusic->bAlwaysPlay = true;
		BackgroundMusic->bIsMusic = true;
		BackgroundMusic->bCanPlayMultipleInstances = true;
		BackgroundMusic->Play();
	}
}

// Called when the game starts or when spawned
void ACustomARPawn::BeginPlay()
{
	Super::BeginPlay();
	HeldObjectColour = "Red";
	OriginalXValue = 0;
	OriginalYValue = 0;
	OriginalZValue = 0;
	OnlyDoOnce = true;
	DistanceTraveled = 3000.0f;
	OverlapLocation = FVector(0);
	TargetPosition = FVector(0);
	if(ACustomGameMode::GetInstance())
	{
		MyGameMode = ACustomGameMode::GetInstance();
		// //GEngine->AddOnScreenDebugMessage(-1, 2.0f, FColor::Yellow, TEXT("Game Mode Reference Set"));
	}
	
	if(UHUDGameUserWidget::GetInstance())
	{
		UI = UHUDGameUserWidget::GetInstance();
		// //GEngine->AddOnScreenDebugMessage(-1, 2.0f, FColor::Yellow, TEXT("Reference for UI Set In AR Pawn"));
	}
	
	CanBePlaced = false;
	ObjectSelected = false;
	FinalLocation = FVector(0);
}

void ACustomARPawn::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);
	if(!MyGameMode)
	{
		if(ACustomGameMode::GetInstance())
		{
			MyGameMode = ACustomGameMode::GetInstance();
		}
	}
	if(!UI)
	{
		if(UHUDGameUserWidget::GetInstance())
		{
			UI = UHUDGameUserWidget::GetInstance();
		}
	}
	if(!ReadyForAnotherCheck)
	{
		Timer += DeltaTime;
		if(Timer >= 0.5f)
		{
			ReadyForAnotherCheck = true;
			Timer = 0.0f;
		}
	}

	
	
	if(UI)
	{
		if(UI->GetDoLaunch() || UI->GetIsGameActive())
		{
			if(UI->GetDoLaunch())
			{
				TArray<ALegoEngineActor*> Actors;
				int i = 0;
				for (const auto& ConstrainedGroup : ConnectedActors)
				{
					if(i == ConnectedActors.Num())
					{
						Target = ConstrainedGroup.Key;
					}
					i++;
					if(ConstrainedGroup.Key->IsA<ALegoBrickActor>())
					{
						const ALegoBrickActor* LegoBrick = Cast<ALegoBrickActor>(ConstrainedGroup.Key);
						LegoBrick->StaticMeshComponent->SetMobility(EComponentMobility::Movable);
						LegoBrick->SceneComponent->SetMobility(EComponentMobility::Movable);
					}
					if(ConstrainedGroup.Key->IsA<ALegoEngineActor>())
					{
						ALegoEngineActor* LegoEngine = Cast<ALegoEngineActor>(ConstrainedGroup.Key);
						LegoEngine->StaticMeshComponent->SetMobility(EComponentMobility::Movable);
						LegoEngine->SceneComponent->SetMobility(EComponentMobility::Movable);
						if(!Actors.Contains(ConstrainedGroup.Key))
						{
							Actors.Add(LegoEngine);
						}
					}
					if(ConstrainedGroup.Value->IsA<ALegoBrickActor>())
					{
						const ALegoBrickActor* LegoBrick = Cast<ALegoBrickActor>(ConstrainedGroup.Value);
						LegoBrick->StaticMeshComponent->SetMobility(EComponentMobility::Movable);
						LegoBrick->SceneComponent->SetMobility(EComponentMobility::Movable);
					}
					if(ConstrainedGroup.Value->IsA<ALegoEngineActor>())
					{
						ALegoEngineActor* LegoEngine = Cast<ALegoEngineActor>(ConstrainedGroup.Value);
						LegoEngine->StaticMeshComponent->SetMobility(EComponentMobility::Movable);
						LegoEngine->SceneComponent->SetMobility(EComponentMobility::Movable);
						if(!Actors.Contains(ConstrainedGroup.Value))
						{
							Actors.Add(LegoEngine);
						}
					}
				}
				if(OnlyDoOnce)
				{
					if(Engines.Num() > 0)
					{
						for(ALegoEngineActor* Engine : Actors)
						{
							const FVector ImpulseDirection = FVector::UpVector * 100.0f;
							//Engine->StaticMeshComponent->AddImpulse(ImpulseDirection, NAME_None, false);
							//GEngine->AddOnScreenDebugMessage(0, 2.0f, FColor::Red, TEXT("Launching!"));
							Engine->StartJets();
							
						}
					}
					DoPrep = true;
					UI->SetDoLaunchForPlayer(false);
					OnlyDoOnce = false;
				}
			}
			if(UI->GetIsGameActive())
			{
				TArray<AActor*> ActorsToRemoveFromList;
				int i = 0;
				if(DoPrep)
				{
					for (AActor* ActorRef : ListToRemoveCollisions)
					{
						FVector Destination = FVector::ZeroVector;
						if (ActorRef->IsA<ALegoBrickActor>())
						{
							const ALegoBrickActor* Temp = Cast<ALegoBrickActor>(ActorRef);
							Destination = Temp->GetActorLocation() + (FVector::UpVector * 100.0f);
							NewPosOnLaunch.Add(ActorRef, Destination);
							StartPositions.Add(ActorRef, Temp->GetActorLocation());
						}
						else if (ActorRef->IsA<ALegoEngineActor>())
						{
							const ALegoEngineActor* Temp = Cast<ALegoEngineActor>(ActorRef);
							Destination = Temp->GetActorLocation() + (FVector::UpVector * 100.0f);
							NewPosOnLaunch.Add(ActorRef, Destination);
							StartPositions.Add(ActorRef, Temp->GetActorLocation());
						}
					}
					//GEngine->AddOnScreenDebugMessage(0, 2.0f, FColor::Red, TEXT("Prep Done!"));
					DoPrep = false;
				}
				for(AActor* Actor1: ListToRemoveCollisions)
				{
					const ALegoBrickActor* LegoBrick1 = nullptr;
					const ALegoEngineActor* LegoEngine1 = nullptr;
					if(Actor1->IsA<ALegoBrickActor>())
					{
						
						LegoBrick1 = Cast<ALegoBrickActor>(Actor1);
						if(i == 0)
						{
							TargetPosition = LegoBrick1->SceneComponent->GetComponentLocation();
						}
						i++;
						const float LerpFactor = FMath::Clamp(FMath::Sin(GetWorld()->GetTimeSeconds()), 0.0f, 1.0f) * 10.0f;
	
						const FVector LerpLocation = FMath::Lerp(StartPositions[Actor1], NewPosOnLaunch[Actor1], LerpFactor);
	
						// Set the location
						LegoBrick1->StaticMeshComponent->SetAllPhysicsPosition(LerpLocation);
						LegoBrick1->SceneComponent->SetRelativeLocation(LerpLocation);
						if(FVector::Dist(LegoBrick1->StaticMeshComponent->GetComponentLocation(), LerpLocation) < 0.5f)
						{
							LegoBrick1->StaticMeshComponent->SetMobility(EComponentMobility::Static);
							LegoBrick1->StaticMeshComponent->SetMobility(EComponentMobility::Static);
							ActorsToRemoveFromList.Add(Actor1);
						}
					}
					else if(Actor1->IsA<ALegoEngineActor>())
					{
						LegoEngine1 = Cast<ALegoEngineActor>(Actor1);
						if(i == 0)
						{
							TargetPosition = LegoEngine1->SceneComponent->GetComponentLocation();
						}
						i++;
						const float LerpFactor = FMath::Clamp(FMath::Sin(GetWorld()->GetTimeSeconds()), 0.0f, 1.0f) * 10.0f;
	
						const FVector LerpLocation = FMath::Lerp(StartPositions[Actor1], NewPosOnLaunch[Actor1], LerpFactor);
	
						// Set the location
						LegoEngine1->StaticMeshComponent->SetAllPhysicsPosition(LerpLocation);
						LegoEngine1->SceneComponent->SetRelativeLocation(LerpLocation);
						if(FVector::Dist(LegoEngine1->StaticMeshComponent->GetComponentLocation(), LerpLocation) < 0.5f)
						{
							LegoEngine1->StaticMeshComponent->SetMobility(EComponentMobility::Static);
							LegoEngine1->StaticMeshComponent->SetMobility(EComponentMobility::Static);
							ActorsToRemoveFromList.Add(Actor1);
						}
					}
				}
				if(ActorsToRemoveFromList.Num() > 0)
				{
					for(AActor* RemoveActor: ListToRemoveCollisions )
					{
						ListToRemoveCollisions.Remove(RemoveActor);
					}
					
					ActorsToRemoveFromList.Empty();
				}
			}
			if(UI->GetIsGameActive())
			{
				SpawningProjectileTimerLogic(DeltaTime);
	
				FuelTimer += DeltaTime;
				if(FuelTimer >= 10.0f)
				{
					UI->SetCurrentFuel(UI->GetCurrentFuel() - 1);
					FuelTimer = 0;
				}		}
		}
	 }
}

// Called to bind functionality to input
void ACustomARPawn::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent)
{
	Super::SetupPlayerInputComponent(PlayerInputComponent);

	//Bind various player inputs to functions
	PlayerInputComponent->BindTouch(IE_Pressed, this, &ACustomARPawn::OnScreenTouch);
	PlayerInputComponent->BindTouch(IE_Released, this, &ACustomARPawn::OnScreenRelease);
	PlayerInputComponent->BindTouch(IE_Repeat, this, &ACustomARPawn::OnScreenTouchMove);
}

void ACustomARPawn::OnScreenRelease(ETouchIndex::Type FingerIndex, FVector Location)
{
	if(!ObjectSelected || !UI) return;
	if (FingerIndex != ETouchIndex::Touch1) return;
	
	if(SelectedLegoBrick)
	{
		if (UI->GetDoMovement())
		{
			CanBePlaced = GetReferenceToBlocksBelow();
			if(CanBePlaced)
			{
				CanBePlaced = AreRotationsAligned(25.0f);
			}
			SelectedLegoBrick->EnableDisableCollisions(true);
			
			if (CanBePlaced)
			{
				// if(BlockToJoinLegoBrick1)
				// {
				if(BlockToJoinLegoBrick1)//->IsA<ALegoBrickActor>())
				{
					//ALegoBrickActor* LegoBlock = Cast<ALegoBrickActor>(BlockToJoinOne);
					if(!BlockToJoinLegoBrick1) return;
					
					// if(ConnectedActors.Contains(MakeTuple(Cast<AActor>(SelectedLegoBrick), Cast<AActor>(Brick)))) return;
					// if(ActiveConstraints.Contains(MakeTuple(Cast<AActor>(SelectedLegoBrick), Cast<AActor>(Brick)))) return;
					// if(SelectedLegoBrick->GetAttachedBricks().Contains(Cast<AActor>(Brick))) return;
					// if(LegoBlock->GetAttachedBricks().Contains(Cast<AActor>(SelectedLegoBrick))) return;
					
					FConstraintInstance ConstraintInstance1;

					ConstraintInstance1.SetLinearLimits(
					ELinearConstraintMotion::LCM_Locked,
					ELinearConstraintMotion::LCM_Locked,
					ELinearConstraintMotion::LCM_Locked,
					0.01f
						);
					ConstraintInstance1.SetAngularTwistLimit(
						ACM_Locked,
						0.01f
						);
					ConstraintInstance1.SetAngularSwing1Limit(
						ACM_Locked,
						0.01f
						);
					ConstraintInstance1.SetAngularSwing2Limit(
						ACM_Locked,
						0.01f
						);
				
					BlockToJoinLegoBrick1->SetupAttachment(SelectedLegoBrick);
				
					
					ConnectedActors.Add(MakeTuple(SelectedLegoBrick, BlockToJoinLegoBrick1));
					ActiveConstraints.Add(MakeTuple(SelectedLegoBrick,BlockToJoinLegoBrick1), BlockToJoinLegoBrick1->PhysicsConstraintComponent);
					
					ConnectedActors.Add(MakeTuple(BlockToJoinLegoBrick1,SelectedLegoBrick));
					ActiveConstraints.Add(MakeTuple(BlockToJoinLegoBrick1,SelectedLegoBrick), BlockToJoinLegoBrick1->PhysicsConstraintComponent);
					
					SelectedLegoBrick->AddAttachedBrick(BlockToJoinLegoBrick1);
					BlockToJoinLegoBrick1->AddAttachedBrick(SelectedLegoBrick);
					
					SelectedLegoBrick->HasConnections = true;
					BlockToJoinLegoBrick1->HasConnections = true;
					if(!ListToRemoveCollisions.Contains(SelectedLegoBrick))
					{
						ListToRemoveCollisions.Add(SelectedLegoBrick);
					}
					if(!ListToRemoveCollisions.Contains(BlockToJoinLegoBrick1))
					{
						ListToRemoveCollisions.Add(BlockToJoinLegoBrick1);
					}
					
					
					//
					BlockToJoinLegoBrick1->StaticMeshComponent->SetMobility(EComponentMobility::Static);
					BlockToJoinLegoBrick1->SceneComponent->SetMobility(EComponentMobility::Static);
					
					SelectedLegoBrick->StaticMeshComponent->SetMobility(EComponentMobility::Static);
					SelectedLegoBrick->SceneComponent->SetMobility(EComponentMobility::Static);
					RemoveCollisions = true;
					//GEngine->AddOnScreenDebugMessage(0, 2.0f, FColor::Red, TEXT("Block Connected"));
				}
				if(BlockToJoinLegoBrick2)//->IsA<ALegoEngineActor>())
				{
					
					if(!BlockToJoinLegoBrick2)return;
				
					
					FConstraintInstance ConstraintInstance;

					ConstraintInstance.SetLinearLimits(
					ELinearConstraintMotion::LCM_Locked,
					ELinearConstraintMotion::LCM_Locked,
					ELinearConstraintMotion::LCM_Locked,
					0
						);
					ConstraintInstance.SetAngularTwistLimit(
						ACM_Locked,
						0
						);
					ConstraintInstance.SetAngularSwing1Limit(
						ACM_Locked,
						0
						);
					ConstraintInstance.SetAngularSwing2Limit(
						ACM_Locked,
						0
						);
				
					BlockToJoinLegoBrick2->SetupAttachment(SelectedLegoBrick);
					
				
					ConnectedActors.Add(MakeTuple(SelectedLegoBrick,BlockToJoinLegoBrick2));
					ActiveConstraints.Add(MakeTuple(SelectedLegoBrick, BlockToJoinLegoBrick2), BlockToJoinLegoBrick2->PhysicsConstraintComponent);
					
					ConnectedActors.Add(MakeTuple(BlockToJoinLegoBrick2, SelectedLegoBrick));
					ActiveConstraints.Add(MakeTuple(BlockToJoinLegoBrick2, SelectedLegoBrick), BlockToJoinLegoBrick2->PhysicsConstraintComponent);
					
					SelectedLegoBrick->AddAttachedBrick(BlockToJoinLegoBrick2);
					BlockToJoinLegoBrick2->AddAttachedBrick(SelectedLegoBrick);
					
					SelectedLegoBrick->HasConnections = true;
					BlockToJoinLegoBrick2->HasConnections = true;
					if(!ListToRemoveCollisions.Contains(SelectedLegoBrick))
					{
						ListToRemoveCollisions.Add(SelectedLegoBrick);
					}
					if(!ListToRemoveCollisions.Contains(BlockToJoinLegoBrick2))
					{
						ListToRemoveCollisions.Add(BlockToJoinLegoBrick2);
					}
					//
					BlockToJoinLegoBrick2->StaticMeshComponent->SetMobility(EComponentMobility::Static);
					BlockToJoinLegoBrick2->SceneComponent->SetMobility(EComponentMobility::Static);
					
					SelectedLegoBrick->StaticMeshComponent->SetMobility(EComponentMobility::Static);
					SelectedLegoBrick->SceneComponent->SetMobility(EComponentMobility::Static);
					RemoveCollisions = true;
					//GEngine->AddOnScreenDebugMessage(0, 2.0f, FColor::Red, TEXT("Block Connected"));
				}
				if(BlockToJoinLegoEngine1)
				{
					if(!BlockToJoinLegoEngine1)return;
					
						
						FConstraintInstance ConstraintInstance;

						ConstraintInstance.SetLinearLimits(
						ELinearConstraintMotion::LCM_Locked,
						ELinearConstraintMotion::LCM_Locked,
						ELinearConstraintMotion::LCM_Locked,
						0
							);
						ConstraintInstance.SetAngularTwistLimit(
							ACM_Locked,
							0
							);
						ConstraintInstance.SetAngularSwing1Limit(
							ACM_Locked,
							0
							);
						ConstraintInstance.SetAngularSwing2Limit(
							ACM_Locked,
							0
							);
					
						BlockToJoinLegoEngine1->SetupAttachment(SelectedLegoBrick);
					
					ConnectedActors.Add(MakeTuple(SelectedLegoBrick, BlockToJoinLegoEngine1));
					ActiveConstraints.Add(MakeTuple(SelectedLegoBrick, BlockToJoinLegoEngine1), BlockToJoinLegoEngine1->PhysicsConstraintComponent);
					
					ConnectedActors.Add(MakeTuple(BlockToJoinLegoEngine1,SelectedLegoBrick));
					ActiveConstraints.Add(MakeTuple(BlockToJoinLegoEngine1, SelectedLegoBrick), BlockToJoinLegoEngine1->PhysicsConstraintComponent);
					
					SelectedLegoBrick->AddAttachedBrick(BlockToJoinLegoEngine1);
					BlockToJoinLegoEngine1->AddAttachedBrick(SelectedLegoBrick);
					
					SelectedLegoBrick->HasConnections = true;
					BlockToJoinLegoEngine1->HasConnections = true;
					if(!ListToRemoveCollisions.Contains(SelectedLegoBrick))
					{
						ListToRemoveCollisions.Add(SelectedLegoBrick);
					}
					if(!ListToRemoveCollisions.Contains(BlockToJoinLegoEngine1))
					{
						ListToRemoveCollisions.Add(BlockToJoinLegoEngine1);
					}
					//
					if(!Engines.Contains(BlockToJoinLegoEngine1))
					{
						Engines.Add(BlockToJoinLegoEngine1);
					}
					BlockToJoinLegoEngine1->StaticMeshComponent->SetMobility(EComponentMobility::Static);
					BlockToJoinLegoEngine1->SceneComponent->SetMobility(EComponentMobility::Static);
					
					SelectedLegoBrick->StaticMeshComponent->SetMobility(EComponentMobility::Static);
					SelectedLegoBrick->SceneComponent->SetMobility(EComponentMobility::Static);
					RemoveCollisions = true;
					//GEngine->AddOnScreenDebugMessage(0, 2.0f, FColor::Red, TEXT("Block Connected"));
					
				}
				
			}
			else
			{
				//GEngine->AddOnScreenDebugMessage(-1, 2.0f, FColor::Yellow, TEXT("failed the check for dropping on another"));
				ResetValues(true ,true, true);
			}
		}
		else if(UI->GetDoRotation())
		{
			SelectedLegoBrick->EndRotation();
			ResetValues(true ,true, true);
		}
		else
		{
			ResetValues(true ,true, true);
		}
	}
	if(SelectedEngineBrick)
	{
		if (UI->GetDoMovement())
		{
			CanBePlaced = GetReferenceToBlocksBelow();
			if(CanBePlaced)
			{
				CanBePlaced = AreRotationsAligned(25.0f);
			}
	
			SelectedEngineBrick->EnableDisableCollisions(true);
		
			if(CanBePlaced)
			{
			
					if(BlockToJoinLegoBrick1)//(BlockToJoinOne->IsA<ALegoBrickActor>())
					{
					
	
						if(!BlockToJoinLegoBrick1)return;
				
						FConstraintInstance ConstraintInstance4;
	
						ConstraintInstance4.SetLinearLimits(
						ELinearConstraintMotion::LCM_Locked,
						ELinearConstraintMotion::LCM_Locked,
						ELinearConstraintMotion::LCM_Locked,
						0.01f
							);
						ConstraintInstance4.SetAngularTwistLimit(
							ACM_Locked,
							0.01f
							);
						ConstraintInstance4.SetAngularSwing1Limit(
							ACM_Locked,
							0.01f
							);
						ConstraintInstance4.SetAngularSwing2Limit(
							ACM_Locked,
							0.01f
							);
				
						BlockToJoinLegoBrick1->SetupAttachment(SelectedEngineBrick);
						
						ConnectedActors.Add(MakeTuple(SelectedEngineBrick, BlockToJoinLegoBrick1));
						ActiveConstraints.Add(MakeTuple(SelectedEngineBrick,BlockToJoinLegoBrick1), BlockToJoinLegoBrick1->PhysicsConstraintComponent);
	
						ConnectedActors.Add(MakeTuple(BlockToJoinLegoBrick1, SelectedEngineBrick));
						ActiveConstraints.Add(MakeTuple(BlockToJoinLegoBrick1, SelectedEngineBrick), BlockToJoinLegoBrick1->PhysicsConstraintComponent);
							
						SelectedEngineBrick->AddAttachedBrick(BlockToJoinLegoBrick1);
						BlockToJoinLegoBrick1->AddAttachedBrick(SelectedEngineBrick);
						
						SelectedEngineBrick->HasConnections = true;
						BlockToJoinLegoBrick1->HasConnections = true;
						if(!ListToRemoveCollisions.Contains(SelectedEngineBrick))
						{
							ListToRemoveCollisions.Add(SelectedEngineBrick);
						}
						if(!ListToRemoveCollisions.Contains(BlockToJoinLegoBrick1))
						{
							ListToRemoveCollisions.Add(BlockToJoinLegoBrick1);
						}
						if(!Engines.Contains(SelectedEngineBrick))
						{
							Engines.Add(SelectedEngineBrick);
						}
						//GEngine->AddOnScreenDebugMessage(0, 2.0f, FColor::Red, TEXT("Block Connected"));
						RemoveCollisions = true;
						BlockToJoinLegoBrick1->StaticMeshComponent->SetMobility(EComponentMobility::Static);
						BlockToJoinLegoBrick1->SceneComponent->SetMobility(EComponentMobility::Static);
						
						SelectedEngineBrick->StaticMeshComponent->SetMobility(EComponentMobility::Static);
						SelectedEngineBrick->SceneComponent->SetMobility(EComponentMobility::Static);
					}
					if(BlockToJoinLegoBrick2)
					{
						if(!BlockToJoinLegoBrick2) return;
					
						
						FConstraintInstance ConstraintInstance5;
	
						ConstraintInstance5.SetLinearLimits(
						ELinearConstraintMotion::LCM_Locked,
						ELinearConstraintMotion::LCM_Locked,
						ELinearConstraintMotion::LCM_Locked,
						0.01f
							);
						ConstraintInstance5.SetAngularTwistLimit(
							ACM_Locked,
							0.01f
							);
						ConstraintInstance5.SetAngularSwing1Limit(
							ACM_Locked,
							0.01f
							);
						ConstraintInstance5.SetAngularSwing2Limit(
							ACM_Locked,
							0.01f
							);
				
						BlockToJoinLegoBrick2->SetupAttachment(SelectedEngineBrick);
						
						ConnectedActors.Add(MakeTuple(SelectedEngineBrick, BlockToJoinLegoBrick2));
						ActiveConstraints.Add(MakeTuple(SelectedEngineBrick, BlockToJoinLegoBrick2), BlockToJoinLegoBrick2->PhysicsConstraintComponent);
	
						ConnectedActors.Add(MakeTuple(BlockToJoinLegoBrick2,SelectedEngineBrick));
						ActiveConstraints.Add(MakeTuple(BlockToJoinLegoBrick2, SelectedEngineBrick), BlockToJoinLegoBrick2->PhysicsConstraintComponent);
						
						SelectedEngineBrick->AddAttachedBrick(BlockToJoinLegoBrick2);
						BlockToJoinLegoBrick2->AddAttachedBrick(SelectedEngineBrick);
						if(!Engines.Contains(SelectedEngineBrick))
						{
							Engines.Add(SelectedEngineBrick);
						}
						SelectedEngineBrick->HasConnections = true;
						BlockToJoinLegoBrick2->HasConnections = true;

						if(!ListToRemoveCollisions.Contains(SelectedEngineBrick))
						{
							ListToRemoveCollisions.Add(SelectedEngineBrick);
						}
						if(!ListToRemoveCollisions.Contains(BlockToJoinLegoBrick2))
						{
							ListToRemoveCollisions.Add(BlockToJoinLegoBrick2);
						}
						//GEngine->AddOnScreenDebugMessage(0, 2.0f, FColor::Red, TEXT("Block Connected"));
						RemoveCollisions = true;
						BlockToJoinLegoBrick2->StaticMeshComponent->SetMobility(EComponentMobility::Static);
						BlockToJoinLegoBrick2->SceneComponent->SetMobility(EComponentMobility::Static);
						SelectedEngineBrick->StaticMeshComponent->SetMobility(EComponentMobility::Static);
						SelectedEngineBrick->SceneComponent->SetMobility(EComponentMobility::Static);
					}
				//}
	
			
					if(BlockToJoinLegoEngine1)//(BlockToJoinTwo->IsA<ALegoBrickActor>())
					{
						//ALegoBrickActor* LegoBlock = Cast<ALegoBrickActor>(BlockToJoinTwo);
	
						if(!BlockToJoinLegoEngine1)return;
					
						FConstraintInstance ConstraintInstance6;
	
						ConstraintInstance6.SetLinearLimits(
						ELinearConstraintMotion::LCM_Locked,
						ELinearConstraintMotion::LCM_Locked,
						ELinearConstraintMotion::LCM_Locked,
						0.01f
							);
						ConstraintInstance6.SetAngularTwistLimit(
							ACM_Locked,
							0.01f
							);
						ConstraintInstance6.SetAngularSwing1Limit(
							ACM_Locked,
							0.01f
							);
						ConstraintInstance6.SetAngularSwing2Limit(
							ACM_Locked,
							0.01f
							);
						//New Object
						
						BlockToJoinLegoEngine1->SetupAttachment(SelectedEngineBrick);
						BlockToJoinLegoEngine1->AddAttachedBrick(SelectedEngineBrick);
						SelectedEngineBrick->AddAttachedBrick(BlockToJoinLegoEngine1);
						if(!Engines.Contains(SelectedEngineBrick))
						{
							Engines.Add(SelectedEngineBrick);
						}
						
						// if(LegoBlock->PhysicsConstraintComponent->IsValidLowLevel())
						// {
						// 	//GEngine->AddOnScreenDebugMessage(0, 2.0f, FColor::Red, TEXT("Is Valid!"));
						// }
						
						ConnectedActors.Add(MakeTuple(SelectedEngineBrick,BlockToJoinLegoEngine1));
						ActiveConstraints.Add(MakeTuple(SelectedEngineBrick,BlockToJoinLegoEngine1), BlockToJoinLegoEngine1->PhysicsConstraintComponent);
	
						ConnectedActors.Add(MakeTuple(BlockToJoinLegoEngine1,SelectedEngineBrick));
						ActiveConstraints.Add(MakeTuple(BlockToJoinLegoEngine1, SelectedEngineBrick), BlockToJoinLegoEngine1->PhysicsConstraintComponent);
							
						SelectedEngineBrick->AddAttachedBrick(BlockToJoinLegoEngine1);
						BlockToJoinLegoEngine1->AddAttachedBrick(SelectedEngineBrick);
						
						SelectedEngineBrick->HasConnections = true;
						BlockToJoinLegoEngine1->HasConnections = true;

						if(!ListToRemoveCollisions.Contains(SelectedEngineBrick))
						{
							ListToRemoveCollisions.Add(SelectedEngineBrick);
						}
						if(!ListToRemoveCollisions.Contains(BlockToJoinLegoEngine1))
						{
							ListToRemoveCollisions.Add(BlockToJoinLegoEngine1);
						}
						RemoveCollisions = true;
						//GEngine->AddOnScreenDebugMessage(0, 2.0f, FColor::Red, TEXT("Block Connected"));
	
						BlockToJoinLegoEngine1->StaticMeshComponent->SetMobility(EComponentMobility::Static);
						BlockToJoinLegoEngine1->SceneComponent->SetMobility(EComponentMobility::Static);
						
						SelectedEngineBrick->StaticMeshComponent->SetMobility(EComponentMobility::Static);
						SelectedEngineBrick->SceneComponent->SetMobility(EComponentMobility::Static);
					}
					
			}
			else
			{
				//GEngine->AddOnScreenDebugMessage(-1, 2.0f, FColor::Yellow, TEXT("failed the check for dropping on another"));
				ResetValues(true ,true, true);
			}
		}
		else if(UI->GetDoRotation())
		{
			SelectedEngineBrick->EndRotation();
			ResetValues(true ,true, true);
		}
		else
		{
			ResetValues(true ,true, true);
		}
	}
	ResetValues(true,true,true);
	
}

void ACustomARPawn::OnScreenTouchMove(ETouchIndex::Type FingerIndex, FVector Location)
{
	if(!ObjectSelected|| !UI) return;
		
	if (FingerIndex != ETouchIndex::Touch1) return;
        
    if (UI->GetDoMovement() && ObjectSelected && SelectedLegoBrick)
    {
        
        SelectedLegoBrick->EnableDisableCollisions(false);
        // Deproject screen space touch location to world space
        FVector WorldLocation, WorldDirection;
        UGameplayStatics::DeprojectScreenToWorld(GetWorld()->GetFirstPlayerController(), FVector2d(Location), WorldLocation, WorldDirection);
 
        // Perform a line trace from the player's location in the direction of the touch
        FHitResult HR;
        FVector EndLocation = WorldLocation + WorldDirection * 1000.0f; // Adjust the trace distance as needed
        FCollisionQueryParams CollisionParams;
        CollisionParams.AddIgnoredActor(SelectedLegoBrick); // Ignore the player pawn in the trace
 
        if (GetWorld()->LineTraceSingleByChannel(HR, WorldLocation, EndLocation, ECC_Visibility, CollisionParams))
        {
         	// Check if the hit actor is an AARGroundActor
         	if(HR.GetActor()->IsA<AARGroundActor>())
         	{
         		AARGroundActor* GroundActor = Cast<AARGroundActor>(HR.GetActor());
         		if (!GroundActor) return;
	            
         		// Use the hit location as the new target location
         		NewLocation = HR.ImpactPoint;
 
         		// Ensure the Lego brick doesn't fall below its starting Z position
         		float StartingZ = SelectedLegoBrick->GetStartZValue();
         		NewLocation.Z = FMath::Max(NewLocation.Z, StartingZ);
         		SelectedLegoBrick->StaticMeshComponent->SetAllPhysicsPosition(NewLocation);
         		SelectedLegoBrick->SetActorLocation(NewLocation);
         		FTransform YourTransform;
         		YourTransform.SetTranslation(NewLocation);
         		SelectedLegoBrick->SetActorTransform(YourTransform);
         		SelectedLegoBrick->SceneComponent->SetRelativeLocation(NewLocation);
         	}
        	else if(HR.GetActor()->IsA<ALegoBrickActor>())
         	{
         		ALegoBrickActor* LegoActor = Cast<ALegoBrickActor>(HR.GetActor());

         		if(!LegoActor) return;
         		
         		NewLocation = HR.ImpactPoint;
         		
         		FVector AdjustedLocation = FVector(
         			 HR.ImpactPoint.X,
					 HR.ImpactPoint.Y,
					HR.ImpactPoint.Z
				 );
         		
			 	NewLocation = AdjustedLocation;
         		//NewLocation.Z = FMath::Max(NewLocation.Z, SelectedLegoBrick->GetStartZValue());
         		SelectedLegoBrick->StaticMeshComponent->SetAllPhysicsPosition(NewLocation);
         		SelectedLegoBrick->SetActorLocation(NewLocation);
         		SelectedLegoBrick->SceneComponent->SetRelativeLocation(NewLocation);

         		if(ReadyForAnotherCheck)
         		{
         			CanBePlaced = GetReferenceToBlocksBelow();
         			if(CanBePlaced)
         			{
         				CanBePlaced = AreRotationsAligned(25.0f);
         				return;
         			}
         		}
	            else
	            {
	            	CanBePlaced = false;
	            }
         	}
        	else if(HR.GetActor()->IsA<ALegoEngineActor>())
         	{
         		ALegoEngineActor* LegoEngine = Cast<ALegoEngineActor>(HR.GetActor());

         		if(!LegoEngine) return;
        		
         		NewLocation = HR.ImpactPoint;
         		
         		FVector AdjustedLocation = FVector(
         			 HR.ImpactPoint.X,
					 HR.ImpactPoint.Y,
					 HR.ImpactPoint.Z
				 );
         		
			 	NewLocation = AdjustedLocation;
         		//NewLocation.Z = FMath::Max(NewLocation.Z, SelectedLegoBrick->GetStartZValue());
         		SelectedLegoBrick->StaticMeshComponent->SetAllPhysicsPosition(NewLocation);
         		SelectedLegoBrick->SetActorLocation(NewLocation);
         		SelectedLegoBrick->SceneComponent->SetRelativeLocation(NewLocation);

         		if(ReadyForAnotherCheck)
         		{
         			CanBePlaced = GetReferenceToBlocksBelow();
         			if(CanBePlaced)
         			{
         				CanBePlaced = AreRotationsAligned(25.0f);
         				return;
         			}
         		}
         		else
         		{
         			CanBePlaced = false;
         		}
         	}
        	else if(HR.GetActor()->IsA<ALegoFuelTankActor>())
         	{
         		ALegoFuelTankActor* LegoFuelTank = Cast<ALegoFuelTankActor>(HR.GetActor());

         		if(!LegoFuelTank) return;
         		NewLocation = HR.ImpactPoint;
         		
         		FVector AdjustedLocation = FVector(
         			 HR.ImpactPoint.X,
					 HR.ImpactPoint.Y,
					 HR.ImpactPoint.Z 
				 );
         		
			 	NewLocation = AdjustedLocation;
         		//NewLocation.Z = FMath::Max(NewLocation.Z, SelectedLegoBrick->GetStartZValue());
         		SelectedLegoBrick->StaticMeshComponent->SetAllPhysicsPosition(NewLocation);
         		SelectedLegoBrick->SetActorLocation(NewLocation);
         		SelectedLegoBrick->SceneComponent->SetRelativeLocation(NewLocation);

         		if(ReadyForAnotherCheck)
         		{
         			CanBePlaced = GetReferenceToBlocksBelow();
         			if(CanBePlaced)
         			{
         				CanBePlaced = AreRotationsAligned(25.0f);
         				return;
         			}
         		}
         		else
         		{
         			CanBePlaced = false;
         		}
         	}
		}
	}
	else if(UI->GetDoRotation() && SelectedLegoBrick)
	{
		SelectedLegoBrick->UpdateRotation(FVector2D(Location));
	}
	
	if (UI->GetDoMovement() && ObjectSelected && SelectedEngineBrick)
    {
        SelectedEngineBrick->EnableDisableCollisions(false);
        // Deproject screen space touch location to world space
        FVector WorldLocation, WorldDirection;
        UGameplayStatics::DeprojectScreenToWorld(GetWorld()->GetFirstPlayerController(), FVector2d(Location), WorldLocation, WorldDirection);
 
        // Perform a line trace from the player's location in the direction of the touch
        FHitResult HR;
        FVector EndLocation = WorldLocation + WorldDirection * 1000.0f; // Adjust the trace distance as needed
        FCollisionQueryParams CollisionParams;
        CollisionParams.AddIgnoredActor(SelectedEngineBrick); // Ignore the player pawn in the trace
 
        if (GetWorld()->LineTraceSingleByChannel(HR, WorldLocation, EndLocation, ECC_Visibility, CollisionParams))
        {
         	// Check if the hit actor is an AARGroundActor
         	if(HR.GetActor()->IsA<AARGroundActor>())
         	{
         		AARGroundActor* GroundActor = Cast<AARGroundActor>(HR.GetActor());
         		if (!GroundActor) return;
	            
         		// Use the hit location as the new target location
         		NewLocation = HR.ImpactPoint;
 
         		// Ensure the Lego brick doesn't fall below its starting Z position
         		float StartingZ = SelectedEngineBrick->GetStartZValue();
         		NewLocation.Z = FMath::Max(NewLocation.Z, StartingZ);
         		SelectedEngineBrick->StaticMeshComponent->SetAllPhysicsPosition(NewLocation);
         		SelectedEngineBrick->SetActorLocation(NewLocation);
         		FTransform YourTransform;
         		YourTransform.SetTranslation(NewLocation);
         		SelectedEngineBrick->SetActorTransform(YourTransform);
         		SelectedEngineBrick->SceneComponent->SetRelativeLocation(NewLocation);
         	}
         	else if(HR.GetActor()->IsA<ALegoBrickActor>())
         	{
         		ALegoBrickActor* LegoActor = Cast<ALegoBrickActor>(HR.GetActor());

         		if(!LegoActor) return;
         		
         		NewLocation = HR.ImpactPoint;
         		
         		FVector AdjustedLocation = FVector(
         			 HR.ImpactPoint.X,
					 HR.ImpactPoint.Y,
					 HR.ImpactPoint.Z
				 );
         		
			 	NewLocation = AdjustedLocation;
         		//NewLocation.Z = FMath::Max(NewLocation.Z, SelectedEngineBrick->GetStartZValue());
         		SelectedEngineBrick->StaticMeshComponent->SetAllPhysicsPosition(NewLocation);
         		SelectedEngineBrick->SetActorLocation(NewLocation);
         		SelectedEngineBrick->SceneComponent->SetRelativeLocation(NewLocation);

         		if(ReadyForAnotherCheck)
         		{
         			CanBePlaced = GetReferenceToBlocksBelow();
         			if(CanBePlaced)
         			{
         				CanBePlaced = AreRotationsAligned(25.0f);
         				return;
         			}
         		}
         		else
         		{
         			CanBePlaced = false;
         		}
         		
         	}
        	else if(HR.GetActor()->IsA<ALegoEngineActor>())
        	{
        		ALegoEngineActor* LegoEngine = Cast<ALegoEngineActor>(HR.GetActor());

        		if(!LegoEngine) return;
        		
        		NewLocation = HR.ImpactPoint;
         		
        		FVector AdjustedLocation = FVector(
        			HR.ImpactPoint.X,
					HR.ImpactPoint.Y,
					HR.ImpactPoint.Z
				);
         		
        		NewLocation = AdjustedLocation;
        		//NewLocation.Z = FMath::Max(NewLocation.Z, SelectedEngineBrick->GetStartZValue());
        		SelectedEngineBrick->StaticMeshComponent->SetAllPhysicsPosition(NewLocation);
        		SelectedEngineBrick->SetActorLocation(NewLocation);
        		SelectedEngineBrick->SceneComponent->SetRelativeLocation(NewLocation);

        		if(ReadyForAnotherCheck)
        		{
        			CanBePlaced = GetReferenceToBlocksBelow();
        			if(CanBePlaced)
        			{
        				CanBePlaced = AreRotationsAligned(25.0f);
        				return;
        			}
        		}
        		else
        		{
        			CanBePlaced = false;
        		}
        	}
        	else if(HR.GetActor()->IsA<ALegoFuelTankActor>())
        	{
        		ALegoFuelTankActor* LegoFuelTank = Cast<ALegoFuelTankActor>(HR.GetActor());

        		if(!LegoFuelTank) return;
        		
        		NewLocation = HR.ImpactPoint;
         		
        		FVector AdjustedLocation = FVector(
        			HR.ImpactPoint.X,
					HR.ImpactPoint.Y,
					HR.ImpactPoint.Z
				);
         		
        		NewLocation = AdjustedLocation;
        		//NewLocation.Z = FMath::Max(NewLocation.Z, SelectedEngineBrick->GetStartZValue());
        		SelectedEngineBrick->StaticMeshComponent->SetAllPhysicsPosition(NewLocation);
        		SelectedEngineBrick->SetActorLocation(NewLocation);
        		SelectedEngineBrick->SceneComponent->SetRelativeLocation(NewLocation);

        		if(ReadyForAnotherCheck)
        		{
        			CanBePlaced = GetReferenceToBlocksBelow();
        			if(CanBePlaced)
        			{
        				CanBePlaced = AreRotationsAligned(25.0f);
        				return;
        			}
        		}
        		else
        		{
        			CanBePlaced = false;
        		}
         		
        	}
		}
	}
	else if(UI->GetDoRotation() && SelectedEngineBrick)
	{
		SelectedEngineBrick->UpdateRotation(FVector2D(Location));
	}
	
	if (UI->GetDoMovement() && ObjectSelected && SelectedFuelTankBrick)
    {
        SelectedFuelTankBrick->EnableDisableCollisions(false);
        // Deproject screen space touch location to world space
        FVector WorldLocation, WorldDirection;
        UGameplayStatics::DeprojectScreenToWorld(GetWorld()->GetFirstPlayerController(), FVector2d(Location), WorldLocation, WorldDirection);
 
        // Perform a line trace from the player's location in the direction of the touch
        FHitResult HR;
        FVector EndLocation = WorldLocation + WorldDirection * 1000.0f; // Adjust the trace distance as needed
        FCollisionQueryParams CollisionParams;
        CollisionParams.AddIgnoredActor(SelectedFuelTankBrick); // Ignore the player pawn in the trace
 
        if (GetWorld()->LineTraceSingleByChannel(HR, WorldLocation, EndLocation, ECC_Visibility, CollisionParams))
        {
        	
         	// Move Over ground.
         	if(HR.GetActor()->IsA<AARGroundActor>())
         	{
         		AARGroundActor* GroundActor = Cast<AARGroundActor>(HR.GetActor());
         		if (!GroundActor) return;
         		NewLocation = HR.ImpactPoint;
         		
         		float StartingZ = SelectedFuelTankBrick->GetStartZValue();
         		NewLocation.Z = FMath::Max(NewLocation.Z, StartingZ);
         		SelectedFuelTankBrick->StaticMeshComponent->SetAllPhysicsPosition(NewLocation);
         		SelectedFuelTankBrick->SetActorLocation(NewLocation);
         		SelectedFuelTankBrick->SceneComponent->SetRelativeLocation(NewLocation);
         	}
        	// move over Lego Brick
        	if(HR.GetActor()->IsA<ALegoBrickActor>())
         	{
         		ALegoBrickActor* LegoActor = Cast<ALegoBrickActor>(HR.GetActor());

         		if(!LegoActor) return;
        		
         		NewLocation = HR.ImpactPoint;
         		
         		FVector AdjustedLocation = FVector(
         			 HR.ImpactPoint.X,
					 HR.ImpactPoint.Y,
					 HR.ImpactPoint.Z
				 );
         		
			 	NewLocation = AdjustedLocation;
         		///NewLocation.Z = FMath::Max(NewLocation.Z, SelectedFuelTankBrick->GetStartZValue());
         		SelectedFuelTankBrick->StaticMeshComponent->SetAllPhysicsPosition(NewLocation);
         		SelectedFuelTankBrick->SetActorLocation(NewLocation);
         		SelectedFuelTankBrick->SceneComponent->SetRelativeLocation(NewLocation);

         		if(ReadyForAnotherCheck)
         		{
         			CanBePlaced = GetReferenceToBlocksBelow();
         			if(CanBePlaced)
         			{
         				CanBePlaced = AreRotationsAligned(25.0f);
         				return;
         			}
         		}
         		else
         		{
         			CanBePlaced = false;
         		}
         	}
        	// Move OVer Lego Engine.
        	else if(HR.GetActor()->IsA<ALegoEngineActor>())
         	{
				ALegoEngineActor* ALegoEngine = Cast<ALegoEngineActor>(HR.GetActor());
        		
        		if(!ALegoEngine) return;
        		
         		NewLocation = HR.ImpactPoint;
         		
         		FVector AdjustedLocation = FVector(
         			 HR.ImpactPoint.X,
					 HR.ImpactPoint.Y,
					 HR.ImpactPoint.Z
				 );
         		
			 	NewLocation = AdjustedLocation;
         		//NewLocation.Z = FMath::Max(NewLocation.Z, SelectedFuelTankBrick->GetStartZValue());
         		SelectedFuelTankBrick->StaticMeshComponent->SetAllPhysicsPosition(NewLocation);
         		SelectedFuelTankBrick->SetActorLocation(NewLocation);
         		SelectedFuelTankBrick->SceneComponent->SetRelativeLocation(NewLocation);

         		if(ReadyForAnotherCheck)
         		{
         			CanBePlaced = GetReferenceToBlocksBelow();
         			if(CanBePlaced)
         			{
         				CanBePlaced = AreRotationsAligned(25.0f);
         				return;
         			}
         		}
         		else
         		{
         			CanBePlaced = false;
         		}
         	}
        	// Move over lego fuel tanl.
        	else if(HR.GetActor()->IsA<ALegoFuelTankActor>())
         	{
         		ALegoFuelTankActor* ALegoFuelTank = Cast<ALegoFuelTankActor>(HR.GetActor());

         		if(!ALegoFuelTank) return;
        		
         		NewLocation = HR.ImpactPoint;
         		
         		FVector AdjustedLocation = FVector(
         			 HR.ImpactPoint.X,
					 HR.ImpactPoint.Y,
					 HR.ImpactPoint.Z
				 );
         		
			 	NewLocation = AdjustedLocation;
         		SelectedFuelTankBrick->StaticMeshComponent->SetAllPhysicsPosition(NewLocation);
         		SelectedFuelTankBrick->SetActorLocation(NewLocation);
         		SelectedFuelTankBrick->SceneComponent->SetRelativeLocation(NewLocation);

         		if(ReadyForAnotherCheck)
         		{
         			CanBePlaced = GetReferenceToBlocksBelow();
         			if(CanBePlaced)
         			{
         				CanBePlaced = AreRotationsAligned(25.0f);
         			}
         		}
         		else
         		{
         			CanBePlaced = false;
         		}
         	}
		}
	}
	else if(UI->GetDoRotation() && SelectedFuelTankBrick)
	{
		SelectedFuelTankBrick->UpdateRotation(FVector2D(Location));
	}
}

void ACustomARPawn::OnScreenTouch(const ETouchIndex::Type FingerIndex, const FVector ScreenPos)
{
	auto Temp = GetWorld()->GetAuthGameMode();
	auto GM = Cast<ACustomGameMode>(Temp);
	
	if(UI)
	{
		if(UI->GetDoMovement() && ObjectSelected) return;
		if(MyGameMode)
		{
			if(UI->GetDoSpawning())
			{
				//GEngine->AddOnScreenDebugMessage(-1, 2.0f, FColor::Yellow, TEXT("Spawning Lego Brick."));

				if(AActor* SpawnedActor = MyGameMode->LineTraceSpawnActor(ScreenPos, UI->GetColour(), UI->GetBlockType()))
				{
					if(SpawnedActor->IsA<ALegoBrickActor>())
					{
						ALegoBrickActor* SpawnedLegoBlock = Cast<ALegoBrickActor>(SpawnedActor);
						SpawnedLegoBlock->SetBlockType(UI->GetBlockType());
						SpawnedLegoBlock->SetColour(UI->GetColour());
						SpawnedLegoBlock = nullptr;
					}
					else if(SpawnedActor->IsA<ALegoEngineActor>())
					{
						ALegoEngineActor* SpawnedLegoEngine = Cast<ALegoEngineActor>(SpawnedActor);
						SpawnedLegoEngine->SetColour(UI->GetColour());
						SpawnedLegoEngine = nullptr;
					}
					else if(SpawnedActor->IsA<ALegoFuelTankActor>())
					{
						ALegoFuelTankActor* SpawnedLegoFuelTank = Cast<ALegoFuelTankActor>(SpawnedActor);
						SpawnedLegoFuelTank->SetColour(UI->GetColour());
						SpawnedLegoFuelTank = nullptr;
					}
				}

				UI->SetSpawningFromPlayer();
				return;
			}
		}
	
		if(WorldHitTest(FVector2d(ScreenPos),HitResult))
		{
	 	
			AActor* HitActor = HitResult.GetActor();
			if (SelectedLegoBrick)
			{
				if(SelectedLegoBrick != HitActor)
				{
					ResetValues(true ,true, true);
					//GEngine->AddOnScreenDebugMessage(-1, 2.0f, FColor::Yellow, TEXT("Deselecting."));
				}
			}
			else if(SelectedEngineBrick)
			{
				if(SelectedEngineBrick != HitActor)
				{
					ResetValues(true ,true, true);
					//GEngine->AddOnScreenDebugMessage(-1, 2.0f, FColor::Yellow, TEXT("Deselecting."));
				}
			}
			else if(SelectedFuelTankBrick)
			{
				if(SelectedFuelTankBrick != HitActor)
				{
					ResetValues(true ,true, true);
					//GEngine->AddOnScreenDebugMessage(-1, 2.0f, FColor::Yellow, TEXT("Deselecting."));
				}
			}
	 	
			if (HitActor && !UI->GetDoSpawning())
			{
				if(HitActor->IsA<ALegoBrickActor>() || HitActor->IsA<ALegoEngineActor>() || HitActor->IsA<ALegoFuelTankActor>())
				{
					// handle selecting Actor.
					HandleSelection(HitActor);
					if(UI->GetDoRotation() && ObjectSelected)
					{
						HandleRotation(FingerIndex, ScreenPos);
					}
				}
				else
				{
					ResetValues(true ,true, true);
					//GEngine->AddOnScreenDebugMessage(-1, 2.0f, FColor::Yellow, TEXT("Deselecting."));
				}
			}
			// catch for tapping nothing and also handles deselection.
			else
			{
				ResetValues(true ,true, true);
				//GEngine->AddOnScreenDebugMessage(-1, 2.0f, FColor::Yellow, TEXT("Deselecting."));
			}
		}
		else
		{
			if(SelectedLegoBrick && ObjectSelected || SelectedEngineBrick && ObjectSelected || SelectedFuelTankBrick && ObjectSelected)
			{
				ResetValues(true ,true, true);
			}
		}
	}
}

bool ACustomARPawn::WorldHitTest(FVector2d ScreenPos, FHitResult& newHitResult)
{
	// Get player controller
	APlayerController* PlayerController = UGameplayStatics::GetPlayerController(this, 0);

	// Perform deprojection to get a 3D world position from the 2D screen position
	FVector WorldPosition, WorldDirection;
	bool DeprojectSuccess = UGameplayStatics::DeprojectScreenToWorld(PlayerController, FVector2D(ScreenPos), WorldPosition, WorldDirection);

	if (DeprojectSuccess)
	{
		auto TraceResult = UARBlueprintLibrary::LineTraceTrackedObjects(FVector2D(ScreenPos), false, false, false, true);
		if(TraceResult.IsValidIndex(0))
		{
			auto TrackedTF = TraceResult[0].GetLocalToWorldTransform();
		
			if (FVector::DotProduct(TrackedTF.GetRotation().GetUpVector(), WorldDirection) < 0)
			{
				//Spawn the actor pin and get the transform
				UARPin*  ActorPinTest = UARBlueprintLibrary::PinComponent(nullptr, TraceResult[0].GetLocalToWorldTransform(), TraceResult[0].GetTrackedGeometry());

				// Check if ARPins are available on your current device. ARPins are currently not supported locally by ARKit, so on iOS, this will always be "FALSE" 
				if (ActorPinTest)
				{
					// If the pin is valid 
					auto PinTF = ActorPinTest->GetLocalToWorldTransform();
				}
			}
		}
		// Perform a line trace to detect the hit point in the world
		FVector TraceStart = WorldPosition;
		FVector TraceEnd = TraceStart + (WorldDirection * 1000.0f); // Adjust TraceDistance as needed

		FCollisionQueryParams CollisionParams;
		CollisionParams.AddIgnoredActor(this); // Ignore the pawn itself

		// Perform line trace
		return GetWorld()->LineTraceSingleByChannel(newHitResult, TraceStart, TraceEnd, ECC_Visibility, CollisionParams);
	}

	return false;
}

void ACustomARPawn::HandleRotation(const ETouchIndex::Type FingerIndex, const FVector ScreenPos)
{
	if (FingerIndex == ETouchIndex::Touch1)
	{
		if (SelectedLegoBrick)
		{
			SelectedLegoBrick->StartRotation(FVector2d(ScreenPos));
		}
		else if(SelectedEngineBrick)
		{
			SelectedEngineBrick->StartRotation(FVector2d(ScreenPos));
		}
		else if(SelectedFuelTankBrick)
		{
			SelectedFuelTankBrick->StartRotation(FVector2d(ScreenPos));
		}
	}
}

void ACustomARPawn::HandleSelection(AActor* hitActor)
{
	if(hitActor->IsA<ALegoBrickActor>())
	{
		SelectedLegoBrick = Cast<ALegoBrickActor>(hitActor);
		// handles unattaching from blocks when picked back up after connection.
		if(SelectedLegoBrick->HasConnections)
		{
			bool AnyActive = true;
			for(const FName ConnectionName: SelectedLegoBrick->GetConnectorsName())
			{
				AnyActive = SelectedLegoBrick->GetIsConnectionPointIsAvailable(ConnectionName);
			}
			if(AnyActive)
			{
				TArray<AActor*> BricksToUnAttach = SelectedLegoBrick->GetAttachedBricks();
				for(AActor* Brick : BricksToUnAttach)
				{
					ConnectedActors.Remove(MakeTuple(Cast<AActor>(SelectedLegoBrick), Cast<AActor>(Brick)));
					ActiveConstraints[MakeTuple(Cast<AActor>(SelectedLegoBrick), Cast<AActor>(Brick))]->DestroyComponent();
					ActiveConstraints.Remove(MakeTuple(Cast<AActor>(SelectedLegoBrick),  Cast<AActor>(Brick)));
					SelectedLegoBrick->RemoveAttachedBrick(Cast<AActor>(Brick));
					if(Brick->IsA<ALegoBrickActor>())
					{
						ALegoBrickActor* RemoveBrick = Cast<ALegoBrickActor>(Brick);
						RemoveBrick->RemoveAttachedBrick(Cast<AActor>(SelectedLegoBrick));
					}
					else if(Brick->IsA<ALegoEngineActor>())
					{
						ALegoEngineActor* RemoveBrick = Cast<ALegoEngineActor>(Brick);
						RemoveBrick->RemoveAttachedBrick(Cast<AActor>(SelectedLegoBrick));
					}
					else if(Brick->IsA<ALegoFuelTankActor>())
					{
						ALegoFuelTankActor* RemoveBrick = Cast<ALegoFuelTankActor>(Brick);
						RemoveBrick->RemoveAttachedBrick(Cast<AActor>(SelectedLegoBrick));
					}
				}
			}
			else
			{
				ResetValues(true, true, true);
				return;
			}
		}
	}
	if(hitActor->IsA<ALegoEngineActor>())
	{
		SelectedEngineBrick = Cast<ALegoEngineActor>(hitActor);
		// handles unattaching from blocks when picked back up after connection.
		if(SelectedEngineBrick->HasConnections)
		{
			bool AnyActive = true;
			for(const FName ConnectionName: SelectedEngineBrick->GetConnectorsName())
			{
				AnyActive = SelectedEngineBrick->GetIsConnectionPointIsAvailable(ConnectionName);
			}
			if(AnyActive)
			{
				TArray<AActor*> BricksToUnAttach = SelectedEngineBrick->GetAttachedBricks();
				for(AActor* Brick : BricksToUnAttach)
				{
					ConnectedActors.Remove(MakeTuple(Cast<AActor>(SelectedEngineBrick), Cast<AActor>(Brick)));
					ActiveConstraints[MakeTuple(Cast<AActor>(SelectedEngineBrick), Cast<AActor>(Brick))]->DestroyComponent();
					ActiveConstraints.Remove(MakeTuple(Cast<AActor>(SelectedEngineBrick),  Cast<AActor>(Brick)));
					SelectedEngineBrick->RemoveAttachedBrick(Cast<AActor>(Brick));
					if(Brick->IsA<ALegoBrickActor>())
					{
						ALegoBrickActor* RemoveBrick = Cast<ALegoBrickActor>(Brick);
						RemoveBrick->RemoveAttachedBrick(Cast<AActor>(SelectedEngineBrick));
					}
					else if(Brick->IsA<ALegoEngineActor>())
					{
						ALegoEngineActor* RemoveBrick = Cast<ALegoEngineActor>(Brick);
						RemoveBrick->RemoveAttachedBrick(Cast<AActor>(SelectedEngineBrick));
					}
					else if(Brick->IsA<ALegoFuelTankActor>())
					{
						ALegoFuelTankActor* RemoveBrick = Cast<ALegoFuelTankActor>(Brick);
						RemoveBrick->RemoveAttachedBrick(Cast<AActor>(SelectedEngineBrick));
					}
				}
			}
			else
			{
				ResetValues(true, true, true);
				return;
			}
		}
		
	}
	
	if (SelectedLegoBrick)
	{
		HeldObjectStaticMesh = SelectedLegoBrick->StaticMeshComponent;
		if (!HeldObjectStaticMesh) return;
	
	
		if(!CustomMaterial) return;
	
		ObjectSelected = true;
		HeldObjectStaticMesh->SetMaterial(0, CustomMaterial);
		HeldObjectColour = SelectedLegoBrick->GetColour();
	}
	else if(SelectedEngineBrick)
	{
		HeldObjectStaticMesh = SelectedEngineBrick->StaticMeshComponent;
		if (!HeldObjectStaticMesh) return;
	
	
		if(!CustomMaterial) return;
	
		ObjectSelected = true;
		HeldObjectStaticMesh->SetMaterial(0, CustomMaterial);
		HeldObjectColour = SelectedEngineBrick->GetColour();
	}
	else
	{
		return;
	}
	
}

void ACustomARPawn::ResetValues(bool resetStaticMesh, bool resetCustomActor, bool ResetBlocksBelow)
{
	
	if(SelectedLegoBrick && resetCustomActor)
	{
		if(HeldObjectStaticMesh && resetStaticMesh)
		{
			SelectedLegoBrick->SetColour(HeldObjectColour);
			//HeldObjectStaticMesh->SetMaterial(0, OriginalMaterial);
			HeldObjectStaticMesh = nullptr;
		}
		ObjectSelected = false;
		SelectedLegoBrick = nullptr;
		if(BlocksBelow.Num() > 0 && ResetBlocksBelow)
		{
			BlocksBelow.Empty();
		}
		if (BlockToJoinLegoBrick1)
		{
			BlockToJoinLegoBrick1 = nullptr;
		}
		if(BlockToJoinLegoBrick2)
		{
			BlockToJoinLegoBrick2 = nullptr;
		}
	}
	else if(SelectedEngineBrick && resetCustomActor)
	{
		if(HeldObjectStaticMesh && resetStaticMesh)
		{
			SelectedEngineBrick->SetColour(HeldObjectColour);
			//HeldObjectStaticMesh->SetMaterial(0, OriginalMaterial);
			HeldObjectStaticMesh = nullptr;
		}
		ObjectSelected = false;
		SelectedEngineBrick = nullptr;
		if(BlocksBelow.Num() > 0 && ResetBlocksBelow)
		{
			BlocksBelow.Empty();
		}
		if (BlockToJoinLegoBrick1)
		{
			BlockToJoinLegoBrick1 = nullptr;
		}
		if(BlockToJoinLegoBrick2)
		{
			BlockToJoinLegoBrick2 = nullptr;
		}
	}
}

void ACustomARPawn::RemoveHeart() const
{
	if(UI)
	{
		if(UI->GetCurrentHearts() == 1)
		{
			UI->SetCurrentHeart(0);
			UI->SetGameOver();
		}
		else
		{
			const int NewHeartAmount = UI->GetCurrentHearts() - 1;
			UI->SetCurrentHeart(NewHeartAmount);
		}
	}
}

void ACustomARPawn::SpawningProjectileTimerLogic(float dt)
{
	if(DistanceTraveled >= 1000 && DistanceTraveled < 2000)
	{
		CurrentActiveTimers = 2;
	}
	else if(DistanceTraveled >= 2000 && DistanceTraveled < 3000)
	{
		CurrentActiveTimers = 3;
	}
	else if(DistanceTraveled >= 3000 && DistanceTraveled < 4000)
	{
		CurrentActiveTimers = 4;
	}
	else if(DistanceTraveled >= 4000 && DistanceTraveled < 5000)
	{
		CurrentActiveTimers = 5;
	}
	if(DistanceTraveled  > 1000.0f)
	{
		MinTimeToSpawn = (2500.0f / DistanceTraveled) * 2.0f;
		 MaxTimeToSpawn = (5000.0f / DistanceTraveled) * 2.0f;
	}
	else
	{
		MinTimeToSpawn = 7.5f;
		MaxTimeToSpawn = 15.0f;
	}
	
	if(CurrentActiveTimers == 1)
	{
		ProjectileTimer1 += dt;
		if(ProjectileTimer1 >= ProjectileEndTime1)
		{
			SpawnNewProjectile();
			ProjectileTimer1 = 0;
			ProjectileEndTime1 = FMath::FRandRange(MinTimeToSpawn, MaxTimeToSpawn);
		}
	}
	if(CurrentActiveTimers == 2)
	{
		ProjectileTimer2 += dt;
		if(ProjectileTimer2 >= ProjectileEndTime2)
		{
			SpawnNewProjectile();
			ProjectileTimer2 = 0;
			ProjectileEndTime2 = FMath::FRandRange(MinTimeToSpawn, MaxTimeToSpawn);
		}
	}
	if(CurrentActiveTimers == 3)
	{
		ProjectileTimer3 += dt;
		if(ProjectileTimer3 >= ProjectileEndTime3)
		{
			SpawnNewProjectile();
			ProjectileTimer3 = 0;
			ProjectileEndTime3 = FMath::FRandRange(MinTimeToSpawn, MaxTimeToSpawn);
		}
	}
	if(CurrentActiveTimers == 4)
	{
		ProjectileTimer4 += dt;
		if(ProjectileTimer4 >= ProjectileEndTime4)
		{
			SpawnNewProjectile();
			ProjectileTimer4 = 0;
			ProjectileEndTime4 = FMath::FRandRange(MinTimeToSpawn, MaxTimeToSpawn);
		}
	}
	if(CurrentActiveTimers == 5)
	{
		ProjectileTimer5 += dt;
		if(ProjectileTimer5 >= ProjectileEndTime5)
		{
			SpawnNewProjectile();
			ProjectileTimer5 = 0;
			ProjectileEndTime5 = FMath::FRandRange(MinTimeToSpawn, MaxTimeToSpawn);
		}
	}
	
}

void ACustomARPawn::SpawnNewProjectile()
{
	// Step 1: Generate a random angle in radians
	const float RandomAngle = FMath::FRandRange(0, 2 * PI);
	const int RandomInteger = FMath::RandRange(0, 6);

	// Step 2: Calculate spawn position at a distance of 25.0f from the target
	constexpr float SpawnDistance = 75.0f;
	const FVector SpawnOffset = FVector(FMath::Cos(RandomAngle), FMath::Sin(RandomAngle), 0) * SpawnDistance;
	const FVector SpawnPosition = TargetPosition + SpawnOffset;

	// Step 3: Calculate the destination point on the opposite side of the target
	const FVector DestinationPosition = TargetPosition - TargetPosition.GetSafeNormal() * SpawnDistance;
	const FRotator LookAtRotation = FRotationMatrix::MakeFromX(DestinationPosition - SpawnPosition).Rotator();
	// Spawn the actor
	const FActorSpawnParameters SpawnInfo;
	const FRotator MyRot(0, 0, 0);
	AProjectileActor* SpawnedActor = GetWorld()->SpawnActor<AProjectileActor>(SpawnPosition, LookAtRotation, SpawnInfo);

	// Set the block type based on the random condition
	if(RandomInteger < 3)
	{
		SpawnedActor->SetProjectileType("Meteor");
	}
	else
	{
		SpawnedActor->SetProjectileType("Comet");
	}
	
	SpawnedActor->SetCustomARPawnRef(this);
	SpawnedActor->SetStartEndPos(SpawnPosition, DestinationPosition);
}

bool ACustomARPawn::AreRotationsAligned(float ErrorMargin)
{
	if(BlocksBelow.Num() == 0) return false;

	float FinalRotationPitch = 0;
	float FinalRotationYaw = 0;
	float FinalRotationRoll = 0;
	
	for(AActor* Block : BlocksBelow)
	{
		FRotator Rotation1 = FRotator(0);
		FRotator Rotation2 = Block->GetActorRotation();
		
		
		if(Block->IsA<ALegoBrickActor>())
		{
			ALegoBrickActor* BrickRef = Cast<ALegoBrickActor>(Block);
			Rotation2 = BrickRef->SceneComponent->GetComponentRotation();
		}
		else if(Block->IsA<ALegoEngineActor>())
		{
			ALegoEngineActor* BrickRef = Cast<ALegoEngineActor>(Block);
			Rotation2 = BrickRef->SceneComponent->GetComponentRotation();
		}
		else if(Block->IsA<ALegoFuelTankActor>())
		{
			ALegoFuelTankActor* BrickRef = Cast<ALegoFuelTankActor>(Block);
			Rotation2 = BrickRef->SceneComponent->GetComponentRotation();
		}
		
		if(SelectedLegoBrick)
		{
			Rotation1 = SelectedLegoBrick->SceneComponent->GetComponentRotation();
		}
		else if(SelectedEngineBrick)
		{
			Rotation1 = SelectedEngineBrick->SceneComponent->GetComponentRotation();
		}
		else if(SelectedFuelTankBrick)
		{
			Rotation1 = SelectedFuelTankBrick->SceneComponent->GetComponentRotation();
		}
		
		Rotation1.Normalize();
		Rotation2.Normalize();

		float DotProduct = FVector::DotProduct(Rotation1.Vector(), Rotation2.Vector());
		float AngleDifference = FMath::Acos(FMath::Clamp(DotProduct, -1.0f, 1.0f));

		if (FMath::Abs(AngleDifference) <= FMath::DegreesToRadians(ErrorMargin))
		{
			FRotator CorrectedRotator = FRotator(Rotation2);
			FinalRotationPitch += CorrectedRotator.Pitch;
			FinalRotationYaw += CorrectedRotator.Yaw;
			FinalRotationRoll += CorrectedRotator.Roll;
		
		}
		else if (FMath::Abs(AngleDifference) <= FMath::DegreesToRadians(180.0f - ErrorMargin))
		{
			const float TargetAngle = FMath::RoundToFloat(Rotation1.Yaw / 180.0f) * 180.0f + 180.0f;
			const FRotator CorrectedRotator = FRotator(Rotation2.Pitch, TargetAngle, Rotation2.Roll);
			FinalRotationPitch += CorrectedRotator.Pitch;
			FinalRotationYaw += CorrectedRotator.Yaw;
			FinalRotationRoll += CorrectedRotator.Roll;
		}
		else
		{
			BlocksBelow.Remove(Block);
		}
	}
	const int NumLeft = BlocksBelow.Num();

	if(NumLeft > 0)
	{
		FinalRotationPitch /= NumLeft;
		FinalRotationYaw /= NumLeft;
		FinalRotationRoll /= NumLeft;
		const FRotator FinalRotation = FRotator(FinalRotationPitch, FinalRotationYaw, FinalRotationRoll);
		
		if(SelectedLegoBrick)
		{
			SelectedLegoBrick->StaticMeshComponent->SetAllPhysicsRotation(FinalRotation);
			SelectedLegoBrick->SceneComponent->SetRelativeRotation(FinalRotation);
		}
		else if(SelectedEngineBrick)
		{
			SelectedEngineBrick->StaticMeshComponent->SetAllPhysicsRotation(FinalRotation);
			SelectedEngineBrick->SceneComponent->SetRelativeRotation(FinalRotation);
		}
		else if(SelectedFuelTankBrick)
		{
			SelectedFuelTankBrick->StaticMeshComponent->SetAllPhysicsRotation(FinalRotation);
			SelectedFuelTankBrick->SceneComponent->SetRelativeRotation(FinalRotation);
		}
		//GEngine->AddOnScreenDebugMessage(0, 5.0f, FColor::Red, TEXT("Rotation Worked Out."));
		return true;
	}
	else
	{
		//GEngine->AddOnScreenDebugMessage(0, 5.0f, FColor::Red, TEXT("Rotation Didnt Work Out."));
		return false;
	}
}

bool ACustomARPawn::GetReferenceToBlocksBelow()
{
    TArray<AActor*> Bricks = MyGameMode->GetActiveLegoBricks();
	FVector ReferenceLocation;
	BlocksBelow.Empty();
	
    float NearestDistanceSquared = 10000.0f;
	AActor* NewNearestActor = nullptr;

	
    for (AActor* brick : Bricks)
    {
    	if(SelectedLegoBrick)
    	{
    		ReferenceLocation = SelectedLegoBrick->StaticMeshComponent->GetComponentLocation();
    	}
    	else if(SelectedEngineBrick)
    	{
    		ReferenceLocation = SelectedEngineBrick->StaticMeshComponent->GetComponentLocation();
    	}
    	else if(SelectedFuelTankBrick)
    	{
    		ReferenceLocation = SelectedFuelTankBrick->StaticMeshComponent->GetComponentLocation();
    	}
    	
        if (brick)
        {
            float DistanceSquared = 0;

        	if(brick->IsA<ALegoBrickActor>())
        	{
		        const ALegoBrickActor* LegoBrick = Cast<ALegoBrickActor>(brick);
        		DistanceSquared = FVector::DistSquared(LegoBrick->StaticMeshComponent->GetComponentLocation(), ReferenceLocation);
        	}
        	else if(brick->IsA<ALegoEngineActor>())
        	{
		        const ALegoEngineActor* LegoEngine = Cast<ALegoEngineActor>(brick);
        		DistanceSquared = FVector::DistSquared(LegoEngine->StaticMeshComponent->GetComponentLocation(), ReferenceLocation);
        	}
        	
            if (DistanceSquared < NearestDistanceSquared)
            {
                NearestDistanceSquared = DistanceSquared;
                NewNearestActor = brick;
            	
            }
        }
    }
	BlocksBelow.Add(NewNearestActor);
	
	if (BlocksBelow.Num() == 0)
	{
		//GEngine->AddOnScreenDebugMessage(0, 2.0f, FColor::Red, TEXT("Found no references to some blocks below!"));
		ReadyForAnotherCheck = false;
		return false;
	}
	
    for (AActor* Brick : Bricks)
    {
    	if(SelectedLegoBrick)
    	{
    		if(Brick->IsA<ALegoBrickActor>())
    		{
    			const ALegoBrickActor* BlockRef = Cast<ALegoBrickActor>(Brick);
    			if(FVector::Dist(SelectedLegoBrick->SceneComponent->GetComponentLocation(), BlockRef->SceneComponent->GetComponentLocation()) <= 3.0f)
    			{
    				if(!BlocksBelow.Contains(Brick))
    				{
    					BlocksBelow.Add(Brick);
    				}
    			}
    		}
    		else if(Brick->IsA<ALegoEngineActor>())
    		{
    			const ALegoEngineActor* BlockRef = Cast<ALegoEngineActor>(Brick);
    			if(FVector::Dist(SelectedLegoBrick->SceneComponent->GetComponentLocation(),  BlockRef->SceneComponent->GetComponentLocation()) <= 3.0f) 
    			{
    				if(!BlocksBelow.Contains(Brick))
    				{
    					BlocksBelow.Add(Brick);
    				}
    			}
    		}
    	}
    	else if(SelectedEngineBrick)
    	{
    		if(Brick->IsA<ALegoBrickActor>())
    		{
    			const ALegoBrickActor* BlockRef = Cast<ALegoBrickActor>(Brick);
    			if(FVector::Dist(SelectedEngineBrick->SceneComponent->GetComponentLocation(), BlockRef->SceneComponent->GetComponentLocation()) <= 3.0f)
    			{
    				if(!BlocksBelow.Contains(Brick))
    				{
    					BlocksBelow.Add(Brick);
    				}
    			}
    		}
    		else if(Brick->IsA<ALegoEngineActor>())
    		{
    			const ALegoEngineActor* BlockRef = Cast<ALegoEngineActor>(Brick);
    			if(FVector::Dist(SelectedEngineBrick->SceneComponent->GetComponentLocation(), BlockRef->SceneComponent->GetComponentLocation()) <= 3.0f) 
    			{
    				if(!BlocksBelow.Contains(Brick))
    				{
    					BlocksBelow.Add(Brick);
    				}
    			}
    		}
    	}
    }
	
    if (BlocksBelow.Num() > 0)
    {
        //GEngine->AddOnScreenDebugMessage(0, 2.0f, FColor::Red, TEXT("Found references to some blocks below!"));
        if(CheckLinedUpConnectorsAndConnections())
        {
            //GEngine->AddOnScreenDebugMessage(0, 2.0f, FColor::Red, TEXT("Lined Up Some Connection Points."));
        	ReadyForAnotherCheck = false;
            return true;
        }
        else
        {
            //GEngine->AddOnScreenDebugMessage(0, 2.0f, FColor::Red, TEXT("Failed to find references to some blocks below!"));
        	ReadyForAnotherCheck = false;
            return false;
        }
    }
    else
    {
        //GEngine->AddOnScreenDebugMessage(0, 2.0f, FColor::Red, TEXT("Found no references to some blocks below!"));
    	ReadyForAnotherCheck = false;
        return false;
    }
}

bool ACustomARPawn::CheckLinedUpConnectorsAndConnections()
{
    if (BlocksBelow.Num() == 0)
        return false;
	
	TMap<AActor*, FVector> LinedUpPositions;
	
    TArray<FName> CurrentConnectionsName;

	if(SelectedLegoBrick)
	{
		CurrentConnectionsName = SelectedLegoBrick->GetConnectionsName();
	}
	if(SelectedEngineBrick)
	{
		CurrentConnectionsName = SelectedEngineBrick->GetConnectionsName();
	}
	TMap<AActor*, int> CountOfAvailableConnections;

    TArray<AActor*> BlocksToRemove;

    for (AActor* Block : BlocksBelow)
    {
	    constexpr int MinLinedUpConnectors = 2;
	    constexpr float DistanceThreshold = 0.65f;
	    int LinedUpConnectors = 0;
    	
		if(Block->IsA<ALegoBrickActor>())
		{
			ALegoBrickActor* LegoBlock = Cast<ALegoBrickActor>(Block);
			TArray<FName> BelowConnectorsName = LegoBlock->GetConnectorsName();

			if (BelowConnectorsName.Num() == 0 || CurrentConnectionsName.Num() == 0) continue;

			// Loop through all connections
			for (const FName& BelowConnector : BelowConnectorsName)
			{
				for (const FName& CurrentConnector : CurrentConnectionsName)
				{
					float SelectedObjectX = 0;
    				float SelectedObjectY = 0;
    				if(SelectedLegoBrick)
    				{
    					SelectedObjectX = SelectedLegoBrick->GetConnectionSocketPosition(CurrentConnector).X;
    					SelectedObjectY = SelectedLegoBrick->GetConnectionSocketPosition(CurrentConnector).Y;
    					const float DistanceX = FMath::Abs(SelectedObjectX - LegoBlock->GetConnectorSocketPosition(BelowConnector).X);
    					const float DistanceY = FMath::Abs(SelectedObjectY - LegoBlock->GetConnectorSocketPosition(BelowConnector).Y);

    					if (DistanceX <= DistanceThreshold && DistanceY <= DistanceThreshold)
    					{
    						if (SelectedLegoBrick->GetIsConnectionPointIsAvailable(CurrentConnector) && LegoBlock->GetIsConnectorPointIsAvailable(BelowConnector))
    						{
    							if(!LinedUpPositions.Contains(Block))
    							{
    								LinedUpConnectors++;
    								LinedUpPositions.Add(Block, LegoBlock->GetConnectorSocketPosition(BelowConnector));
    							}
    						}
    					}
    				}
    				else if(SelectedEngineBrick)
    				{
    					SelectedObjectX = SelectedEngineBrick->GetConnectionSocketPosition(CurrentConnector).X;
    					SelectedObjectY = SelectedEngineBrick->GetConnectionSocketPosition(CurrentConnector).Y;
    					const float DistanceX = FMath::Abs(SelectedObjectX - LegoBlock->GetConnectorSocketPosition(BelowConnector).X);
    					const float DistanceY = FMath::Abs(SelectedObjectY - LegoBlock->GetConnectorSocketPosition(BelowConnector).Y);

    					if (DistanceX <= DistanceThreshold && DistanceY <= DistanceThreshold)
    					{
    						if (SelectedEngineBrick->GetIsConnectionPointIsAvailable(CurrentConnector) && LegoBlock->GetIsConnectorPointIsAvailable(BelowConnector))
    						{
    							if(!LinedUpPositions.Contains(Block))
    							{
    								LinedUpConnectors++;
    								LinedUpPositions.Add(Block, LegoBlock->GetConnectorSocketPosition(BelowConnector));
    							}
    						}
    					}
    				}
				}
			}

			if (LinedUpConnectors <= MinLinedUpConnectors)
			{
				BlocksToRemove.Add(Block);
			}
			else
			{
				CountOfAvailableConnections.Add(Block, LinedUpConnectors);
			}
		}
    	if(Block->IsA<ALegoEngineActor>())
    	{
    		ALegoEngineActor* LegoEngine = Cast<ALegoEngineActor>(Block);
    		TArray<FName> BelowConnectorsName = LegoEngine->GetConnectorsName();

    		if (BelowConnectorsName.Num() == 0 || CurrentConnectionsName.Num() == 0)
    			continue;  // Skip blocks without connectors or connections

    		// Loop through all connections
    		for (const FName& BelowConnector : BelowConnectorsName)
    		{
    			for (const FName& CurrentConnector : CurrentConnectionsName)
    			{
    				float SelectedObjectX = 0;
    				float SelectedObjectY = 0;
    				if(SelectedLegoBrick)
    				{
    					SelectedObjectX = SelectedLegoBrick->GetConnectionSocketPosition(CurrentConnector).X;
    					SelectedObjectY = SelectedLegoBrick->GetConnectionSocketPosition(CurrentConnector).Y;
    					const float DistanceX = FMath::Abs(SelectedObjectX - LegoEngine->GetConnectorSocketPosition(BelowConnector).X);
    					const float DistanceY = FMath::Abs(SelectedObjectY - LegoEngine->GetConnectorSocketPosition(BelowConnector).Y);

    					if (DistanceX <= DistanceThreshold && DistanceY <= DistanceThreshold)
    					{
    						if (SelectedLegoBrick->GetIsConnectionPointIsAvailable(CurrentConnector) && LegoEngine->GetIsConnectorPointIsAvailable(BelowConnector))
    						{
    							LinedUpConnectors++;
    							LinedUpPositions.Add(Block, LegoEngine->GetConnectorSocketPosition(BelowConnector));
    						}
    					}
    				}
    			}
    		}

    		if (LinedUpConnectors <= MinLinedUpConnectors)
    		{
    			BlocksToRemove.Add(Block);
    		}
    		else
    		{
    			CountOfAvailableConnections.Add(Block, LinedUpConnectors);
    		}
    	}
    }
	const FVector& TargetPos = NewLocation;
	int NumLeft = BlocksBelow.Num();
	auto SortByDistanceToTarget = [&TargetPos](const AActor& A, const AActor& B)
	{
		
		// Calculate squared distances to the target
		const float DistSquaredA = FVector::DistSquared(TargetPos, A.GetRootComponent()->GetComponentLocation());
		const float DistSquaredB = FVector::DistSquared(TargetPos, A.GetRootComponent()->GetComponentLocation());

		// Sort in ascending order based on distance
		return DistSquaredA < DistSquaredB;
	};

	// Use TArray::Sort with the custom predicate
	BlocksBelow.Sort(SortByDistanceToTarget);
	
	// if(NumLeft > 2)
	// {
	// 	CountOfAvailableConnections.ValueSort([](const int& A, const int& B) {
	// 		return A > B;
	// 	});
	//
	// 	// Add all but the two highest entries to BlocksToRemove
	// 	const int NumBlocksToRemove = CountOfAvailableConnections.Num() - 2;
	// 	int BlocksAdded = 0;
	//
	// 	for (const auto& Entry : CountOfAvailableConnections)
	// 	{
	// 		BlocksToRemove.Add(Entry.Key);
	//
	// 		// Check if we have added enough blocks
	// 		BlocksAdded++;
	// 		if (BlocksAdded >= NumBlocksToRemove)
	// 		{
	// 			break;
	// 		}
	// 	}
	// }
	//
	//
	// if(SelectedLegoBrick)
	// {
	// 	for (AActor* actor : BlocksBelow)
	// 	{
	// 		if(actor->IsA<ALegoBrickActor>())
	// 		{
	// 			ALegoBrickActor* BrickRef = Cast<ALegoBrickActor>(actor);
	// 			if(FVector::Dist(BrickRef->SceneComponent->GetComponentLocation(), SelectedLegoBrick->SceneComponent->GetComponentLocation()) < 5.0f)
	// 			{
	// 				if(!BlocksToRemove.Contains(actor))
	// 				{
	// 					BlocksToRemove.Add(actor);
	// 				}
	// 			}
	// 		}
	// 		else if(actor->IsA<ALegoEngineActor>())
	// 		{
	// 			ALegoEngineActor* BrickRef = Cast<ALegoEngineActor>(actor);
	// 			if(FVector::Dist(BrickRef->SceneComponent->GetComponentLocation(), SelectedLegoBrick->SceneComponent->GetComponentLocation()) < 5.0f)
	// 			{
	// 				if(!BlocksToRemove.Contains(actor))
	// 				{
	// 					BlocksToRemove.Add(actor);
	// 				}
	// 			}
	// 		}
	// 	}
	// }
	// else if(SelectedEngineBrick)
	// {
	// 	for (AActor* actor : BlocksBelow)
	// 	{
	// 		if(actor->IsA<ALegoBrickActor>())
	// 		{
	// 			ALegoBrickActor* BrickRef = Cast<ALegoBrickActor>(actor);
	// 			if(FVector::Dist(BrickRef->SceneComponent->GetComponentLocation(), SelectedEngineBrick->SceneComponent->GetComponentLocation()) < 3.0f)
	// 			{
	// 				if(!BlocksToRemove.Contains(actor))
	// 				{
	// 					BlocksToRemove.Add(actor);
	// 				}
	// 			}
	// 		}
	// 		else if(actor->IsA<ALegoEngineActor>())
	// 		{
	// 			ALegoEngineActor* BrickRef = Cast<ALegoEngineActor>(actor);
	// 			if(FVector::Dist(BrickRef->SceneComponent->GetComponentLocation(), SelectedEngineBrick->SceneComponent->GetComponentLocation()) < 3.0f)
	// 			{
	// 				if(!BlocksToRemove.Contains(actor))
	// 				{
	// 					BlocksToRemove.Add(actor);
	// 				}
	// 			}
	// 		}
	// 	}
	// }
	
	for (AActor* BlockToRemove : BlocksToRemove)
	{
		BlocksBelow.Remove(BlockToRemove);
		LinedUpPositions.Remove(BlockToRemove);
	}
    AlignedToBlocksBelowPos = FVector(0, 0, 0);
	//GEngine->AddOnScreenDebugMessage(-1, 2.0f, FColor::Yellow, FString::Printf(TEXT("Blocks In BlocksBelow Left: %d"), NumLeft));
    if (BlocksBelow.Num() > 0)
    {
    	AActor* Block1 = nullptr;
    	AActor* Block2 = nullptr;
		int i = 0;
    	for(AActor* Block : BlocksBelow)
    	{
    		if(i == 0)
    		{
    			if(Block->IsA<ALegoBrickActor>())
    			{
    				BlockToJoinLegoBrick1 = Cast<ALegoBrickActor>(Block);
    				Block1 = Block;
    			}
    			else if(Block->IsA<ALegoEngineActor>())
    			{
    				BlockToJoinLegoEngine1 = Cast<ALegoEngineActor>(Block);
    			}
    		}
    		if(i == 1)
    		{
    			if(Block->IsA<ALegoBrickActor>())
    			{
    				BlockToJoinLegoBrick2 = Cast<ALegoBrickActor>(Block);
    				Block2 = Block;
    			}
    			else if(Block->IsA<ALegoEngineActor>())
    			{
    				BlockToJoinLegoEngine1 = Cast<ALegoEngineActor>(Block);
    			}
    		}
    		// if(i > 2)
    		// {
    		// 	break;	
    		// }
    		i++;
    	}
    	if(i == 1)
    	{
    		BlockToJoinLegoBrick2 = nullptr;
    		TArray<AActor*> FinalBlocksToRemove;
    		for (const auto&  Connection : LinedUpPositions)
    		{
    			if(Connection.Key != Block1)
    			{
    				FinalBlocksToRemove.Add(Connection.Key);
    			}
    		}
    		if(FinalBlocksToRemove.Num() > 0)
    		{
    			for(AActor* RemoveBlock : FinalBlocksToRemove)
    			{
    				LinedUpPositions.Remove(RemoveBlock);
    			}
    		}
    		if(LinedUpPositions.Num() > 0)
    		{
    			for (const auto&  Connection : LinedUpPositions)
    			{
    			    AlignedToBlocksBelowPos += Connection.Value;
    			}
    			//AlignedToBlocksBelowPos.Z = NewLocation.Z;
    			AlignedToBlocksBelowPos /=  LinedUpPositions.Num();
    			
    			if(SelectedLegoBrick)
    			{
    				SelectedLegoBrick->SceneComponent->SetRelativeLocation(AlignedToBlocksBelowPos);
    				SelectedLegoBrick->StaticMeshComponent->SetAllPhysicsPosition(AlignedToBlocksBelowPos);
    				//GEngine->AddOnScreenDebugMessage(-1, 2.0f, FColor::Yellow, TEXT("SetPosition From Socket Positions."));
    			}
    			else if(SelectedEngineBrick)
    			{
    				SelectedEngineBrick->SceneComponent->SetRelativeLocation(AlignedToBlocksBelowPos);
    				SelectedEngineBrick->StaticMeshComponent->SetAllPhysicsPosition(AlignedToBlocksBelowPos);
    				//GEngine->AddOnScreenDebugMessage(-1, 2.0f, FColor::Yellow, TEXT("SetPosition From Socket Positions."));
    			}
    		}
    	}
    	else if(i == 2)
    	{
    		TArray<AActor*> FinalBlocksToRemove;
    		for (const auto&  Connection : LinedUpPositions)
    		{
    			if(Connection.Key != Block1 && Connection.Key != Block2)
    			{
    				FinalBlocksToRemove.Add(Connection.Key);
    			}
    		}
    		if(FinalBlocksToRemove.Num() > 0)
    		{
    			for(AActor* RemoveBlock : FinalBlocksToRemove)
    			{
    				LinedUpPositions.Remove(RemoveBlock);
    			}
    		}
    		if(LinedUpPositions.Num() > 0)
    		{
    			for (const auto&  Connection : LinedUpPositions)
    			{
    			    AlignedToBlocksBelowPos += Connection.Value;
    			}
    			AlignedToBlocksBelowPos /= LinedUpPositions.Num();
    			//AlignedToBlocksBelowPos.Z = NewLocation.Z;
    			if(SelectedLegoBrick)
    			{
    				SelectedLegoBrick->SceneComponent->SetRelativeLocation(AlignedToBlocksBelowPos);
    				SelectedLegoBrick->StaticMeshComponent->SetAllPhysicsPosition(AlignedToBlocksBelowPos);
    				//GEngine->AddOnScreenDebugMessage(-1, 2.0f, FColor::Yellow, TEXT("SetPosition From Socket Positions."));
    			}
    			else if(SelectedEngineBrick)
    			{
    				SelectedEngineBrick->SceneComponent->SetRelativeLocation(AlignedToBlocksBelowPos);
    				SelectedEngineBrick->StaticMeshComponent->SetAllPhysicsPosition(AlignedToBlocksBelowPos);
    				//GEngine->AddOnScreenDebugMessage(-1, 2.0f, FColor::Yellow, TEXT("SetPosition From Socket Positions."));
    			}
    		}
    	}
        return true;
    }
    else
    {
        return false;
    }
}
