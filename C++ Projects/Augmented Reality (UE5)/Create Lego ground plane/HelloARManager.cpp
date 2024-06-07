// Fill out your copyright notice in the Description page of Project Settings.


#include "HelloARManager.h"
#include "ARPlaneActor.h"
#include "ARPin.h"
#include "ARSessionConfig.h"
#include "Math/UnrealMathUtility.h"
#include "ARBlueprintLibrary.h"
#include "Runtime/Engine/Classes/Kismet/GameplayStatics.h"


// Sets default values
AHelloARManager::AHelloARManager()
{
 	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

	// This constructor helper is useful for a quick reference within UnrealEngine and if you're working alone. But if you're working in a team, this could be messy.
	// If the artist chooses to change the location of an art asset, this will throw errors and break the game.
	// Instead let unreal deal with this. Usually avoid this method of referencing.
	// For long term games, create a Data-Only blueprint (A blueprint without any script in it) and set references to the object using the blueprint editor.
	// This way, unreal will notify your artist if the asset is being used and what can be used to replace it.
	static ConstructorHelpers::FObjectFinder<UARSessionConfig> ConfigAsset(TEXT("ARSessionConfig'/Game/Blueprints/HelloARSessionConfig.HelloARSessionConfig'"));
	Config = ConfigAsset.Object;
	
	//Populate the plane colours array
	PlaneColors.Add(FColor::Blue);
	PlaneColors.Add(FColor::Red);
	PlaneColors.Add(FColor::Green);
	PlaneColors.Add(FColor::Cyan);
	PlaneColors.Add(FColor::Magenta);
	PlaneColors.Add(FColor::Emerald);
	PlaneColors.Add(FColor::Orange);
	PlaneColors.Add(FColor::Purple);
	PlaneColors.Add(FColor::Turquoise);
	PlaneColors.Add(FColor::White);
	PlaneColors.Add(FColor::Yellow);
}

// Called when the game starts or when spawned
void AHelloARManager::BeginPlay()
{
	Super::BeginPlay();

	//Start the AR Session
	UARBlueprintLibrary::StartARSession(Config);

	if(UHUDGameUserWidget::GetInstance())
	{
		UI = UHUDGameUserWidget::GetInstance();
	}
	DeleteOffScreenPlanes = true;
}

// Called every frame
void AHelloARManager::Tick(float DeltaTime)
{
    Super::Tick(DeltaTime);

    if (!UI)
    {
        if (UHUDGameUserWidget::GetInstance())
        {
            UI = UHUDGameUserWidget::GetInstance();
        }
    }

    switch (UARBlueprintLibrary::GetARSessionStatus().Status)
    {
    case EARSessionStatus::Running:
        UpdatePlaneActors();
        break;
    case EARSessionStatus::FatalError:

        ResetARCoreSession();
        UARBlueprintLibrary::StartARSession(Config);
        break;
    }

    if (UI && UI->GetDoDisablePlaneGeneration())
    {
        if (DeleteOffScreenPlanes)
        {
            // Get the player controller
        	APlayerController* PlayerController = UGameplayStatics::GetPlayerController(this, 0);
            // Calculate the average Z height of all AR planes
            float AverageZ = 0.0f;
            for (FVector PlanePos : ARPlanePositionsAll)
            {
                AverageZ += PlanePos.Z;
            }
            AverageZ /= ARPlanePositionsAll.Num();
        	//ArrangeInGrid(AverageZ - 2.5f);
        	
        	for (AActor* NewPlane : SpawnedInstances)
            {
            	FVector2D ScreenLocation;
            	if (PlayerController && PlayerController->ProjectWorldLocationToScreen(NewPlane->GetActorLocation(), ScreenLocation))
            	{
            		int32 ViewportWidth, ViewportHeight;
            		PlayerController->GetViewportSize(ViewportWidth, ViewportHeight);

            		if (ScreenLocation.X < 0 || ScreenLocation.X > ViewportWidth  || ScreenLocation.Y < 0.0f || ScreenLocation.Y > ViewportHeight )
            		{
            			// Object is outside the screen bounds, destroy it
            			SpawnedInstances.Remove(NewPlane);
            			NewPlane->Destroy();
            		}
            	}
        		NewPlane->SetActorLocation(FVector(NewPlane->GetActorLocation().X, NewPlane->GetActorLocation().Y, AverageZ - 2.5f));
        		
            	if(NewPlane->IsA<AARGroundActor>())
            	{
            		AARGroundActor* GroundActor = Cast<AARGroundActor>(NewPlane);
            		GroundActor->ActivateComponents();
            	}
        		
            	FBox CurrentObjectBox = NewPlane->GetComponentsBoundingBox();
            	if(AlreadySpawnedActors.Num() > 0)
            	{
            		// Iterate over already spawned objects
            		for (AActor* OtherInstance : SpawnedInstances)
            		{
            			if (NewPlane == OtherInstance)
            			{
            				// Skip self-comparison
            				continue;
            			}
            		
            			if (!OtherInstance)
            			{
            				// Handle the case where the other actor is invalid
            				continue;
            			}
            		
            			// Create bounding box for the other spawned object
            			FBox OtherObjectBox = OtherInstance->GetComponentsBoundingBox();
            		
            			// Check for intersection between bounding boxes
            			if (CurrentObjectBox.Intersect(OtherObjectBox))
            			{
            				// Intersection detected, destroy the other object
            				OtherInstance->Destroy();
            				AlreadySpawnedActors.Remove(OtherInstance);
            				SpawnedInstances.Remove(OtherInstance);
            				//GEngine->AddOnScreenDebugMessage(-1, 2.0f, FColor::Yellow, TEXT("Destroyed Intersecting Plane"));
            			}
            		}
            	}
            }
            DeleteOffScreenPlanes = false;
        }
    }
}

void AHelloARManager::ArrangeInGrid(float TargetZ)
{
	const float Spacing = 46.0f;
	TMap<FIntPoint, AActor*> Grid;

	SpawnedInstances.Sort([](const AActor& A, const AActor& B)
	{
		return A.GetActorLocation().X < B.GetActorLocation().X;
	});

	for (AActor* Instance : SpawnedInstances)
	{
		if (!IsValid(Instance))
		{
			continue;
		}

		FVector InstanceLocation = Instance->GetActorLocation();
		
		FIntPoint GridPosition = FIntPoint(
			FMath::RoundToInt(InstanceLocation.X / Spacing),
			FMath::RoundToInt(InstanceLocation.Y / Spacing)
		);

		while (Grid.Contains(GridPosition))
		{
			GridPosition.X++;
		}

		Grid.Add(GridPosition, Instance);

		FVector NewInstanceLocation = FVector(
			GridPosition.X * Spacing,
			GridPosition.Y * Spacing,
			InstanceLocation.Z
		);

		Instance->SetActorLocation(NewInstanceLocation);
	}
}


//Updates the geometry actors in the world
void AHelloARManager::UpdatePlaneActors()
{
	auto Geometries = UARBlueprintLibrary::GetAllGeometriesByClass<UARPlaneGeometry>();

	// Array to store the positions of AR planes
	TArray<FVector> ARPlanePositions;
	
	for (auto& It : Geometries)
	{
		if (PlaneActors.Contains(It))
		{
			AARPlaneActor* CurrentPActor = *PlaneActors.Find(It);

			if (It->GetSubsumedBy()->IsValidLowLevel())
			{
				CurrentPActor->Destroy();
				PlaneActors.Remove(It);
				
			}
			else
			{
				switch (It->GetTrackingState())
				{
				case EARTrackingState::Tracking:
					CurrentPActor->UpdatePlanePolygonMesh();
					ARPlanePositions.Add(It->GetLocalToWorldTransform().GetLocation());
					ARPlaneSizes.Add(It->GetExtent());
					ARPlanePositionsAll.Add(It->GetLocalToWorldTransform().GetLocation());
					SpawnCustomStaticMeshInstances();
					// if(CurrentGroundActors.Num() > 0)
					// {
					// 	for(const auto& GroundInstance : CurrentGroundActors)
					// 	{
					// 		
					// 		CurrentPActor->UpdateArGroundObjectPlanes(GroundInstance);
					// 	}
					// }
					
					break;
				case EARTrackingState::StoppedTracking:
                    	
					CurrentPActor->Destroy();
					PlaneActors.Remove(It);
					break;
				}
			}
		}
		else
		{
			switch (It->GetTrackingState())
			{
			case EARTrackingState::Tracking:
				if (!It->GetSubsumedBy()->IsValidLowLevel())
				{
					PlaneActor = SpawnPlaneActor();
					PlaneActor->SetColor(GetPlaneColor(PlaneIndex));
					PlaneActor->ARCorePlaneObject = It;
					PlaneActors.Add(It, PlaneActor);
					PlaneActor->UpdatePlanePolygonMesh();
					ARPlanePositions.Add(It->GetLocalToWorldTransform().GetLocation());
					//SpawnCustomStaticMeshInstances(ARPlanePositions, ARPlaneSizes);
					PlaneIndex++;
				}
				break;
			}
		}
	}
}
void AHelloARManager::SpawnCustomStaticMeshInstances()
{
    if (ARPlanePositionsAll.Num() == 0 || ARPlaneSizes.Num() == 0)
    {
        //GEngine->AddOnScreenDebugMessage(-1, 2.0f, FColor::Yellow, TEXT("ARPlanePositions or ARPlaneSizes is empty!"));
        return;
    }
	if(CurrentGroundActors.Num() > 0)
	{
		CurrentGroundActors.Empty();
	}
	
    FVector ObjectOffset(49.0f, 49.0f, 0.0f);
    FVector MeshSize(45.0f, 45.0f, 2.0f);
    float MinZOffset = -5.0f;
    float MaxZOffset = 5.0f;

    TArray<AActor*> InstancesToRemove;

    for (int newPlaneIndex = 0; newPlaneIndex < ARPlanePositionsAll.Num(); newPlaneIndex++)
    {
        FVector PlanePosition = ARPlanePositionsAll[newPlaneIndex];
        FVector PlaneSize = ARPlaneSizes[newPlaneIndex];

        int NumInstancesX = FMath::Max(1, FMath::RoundToInt(PlaneSize.X / ObjectOffset.X));
        int NumInstancesY = FMath::Max(1, FMath::RoundToInt(PlaneSize.Y / ObjectOffset.Y));

        for (int InstanceX = -1; InstanceX < NumInstancesX; InstanceX++)
        {
            for (int InstanceY = -1; InstanceY < NumInstancesY; InstanceY++)
            {
                FVector SpawnLocation = PlanePosition + FVector(InstanceX * ObjectOffset.X, InstanceY * ObjectOffset.Y, 0.0f);

                bool bOverlap = false;
                bool bWithinZRange = true;

                if (!SpawnedInstances.IsEmpty())
                {
                    for (AActor* ExistingInstance : SpawnedInstances)
                    {
                        if (!IsValid(ExistingInstance))
                        {
                            continue;
                        }

                        FBox ExistingInstanceBounds = ExistingInstance->GetComponentsBoundingBox();
                        FBox SpawnedInstanceBounds = FBox(SpawnLocation - MeshSize / 2, SpawnLocation + MeshSize / 2);

                        if (ExistingInstanceBounds.Intersect(SpawnedInstanceBounds))
                        {
                            bOverlap = true;
                            break;
                        }
                    }
                    bWithinZRange = FMath::IsWithinInclusive(SpawnLocation.Z, SpawnedInstances[0]->GetActorLocation().Z + MinZOffset, SpawnedInstances[0]->GetActorLocation().Z + MaxZOffset);
                }

                if (!bOverlap && bWithinZRange)
                {
                    if (!SpawnedInstances.IsEmpty())
                    {
                        SpawnLocation.Z = SpawnedInstances[0]->GetActorLocation().Z;
                    }

                    const FActorSpawnParameters SpawnInfo;
                    const FRotator MyRot(0, 0, 0);
                    AARGroundActor* newActor = GetWorld()->SpawnActor<AARGroundActor>(SpawnLocation, MyRot, SpawnInfo);

                    if (newActor)
                    {
                        UStaticMeshComponent* meshComp = newActor->FindComponentByClass<UStaticMeshComponent>();
                        if (meshComp)
                        {
                            meshComp->SetVisibility(false);
                        }
                        SpawnedInstances.Add(newActor);
                    }
                }
            }
        }
    }
	
	// Check for overlaps and mark instances for removal
	for (int InstanceIndex = SpawnedInstances.Num() - 1; InstanceIndex >= 0; --InstanceIndex)
	{
		for (int CheckIndex = InstanceIndex - 1; CheckIndex >= 0; --CheckIndex)
		{
			float OverlapTolerance = 0.85f;
	
			FBox BoundsA = SpawnedInstances[InstanceIndex]->GetComponentsBoundingBox();
			FBox BoundsB = SpawnedInstances[CheckIndex]->GetComponentsBoundingBox();
	
			// Check for overlap considering the tolerance
			if (BoundsA.Intersect(BoundsB))
			{
				FVector OverlapExtent = BoundsA.GetExtent() * OverlapTolerance;
	
				if (FMath::Abs(BoundsA.Min.X - BoundsB.Max.X) <= OverlapExtent.X
					&& FMath::Abs(BoundsA.Max.X - BoundsB.Min.X) <= OverlapExtent.X
					&& FMath::Abs(BoundsA.Min.Y - BoundsB.Max.Y) <= OverlapExtent.Y
					&& FMath::Abs(BoundsA.Max.Y - BoundsB.Min.Y) <= OverlapExtent.Y
					&& FMath::Abs(BoundsA.Min.Z - BoundsB.Max.Z) <= OverlapExtent.Z
					&& FMath::Abs(BoundsA.Max.Z - BoundsB.Min.Z) <= OverlapExtent.Z)
				{
					InstancesToRemove.Add(SpawnedInstances[InstanceIndex]);
					break; // No need to check further instances for this one
				}
			}
		}
	}
	
	// Remove instances that overlap more than a certain amount
	for (AActor* InstanceToRemove : InstancesToRemove)
	{
		SpawnedInstances.Remove(InstanceToRemove);
		InstanceToRemove->Destroy();
	}
	InstancesToRemove.Empty();
	
	const float GridSpacing = 46.0f;
	TMap<FIntPoint, AActor*> Grid;

	SpawnedInstances.Sort([](const AActor& A, const AActor& B)
	{
		return A.GetActorLocation().X < B.GetActorLocation().X;
	});

	for (AActor* Instance : SpawnedInstances)
	{
		if (!IsValid(Instance))
		{
			continue;
		}

		FVector InstanceLocation = Instance->GetActorLocation();
		FIntPoint GridPosition = FIntPoint(
			FMath::RoundToInt(InstanceLocation.X / GridSpacing),
			FMath::RoundToInt(InstanceLocation.Y / GridSpacing)
		);

		while (Grid.Contains(GridPosition))
		{
			GridPosition.X++;
		}

		Grid.Add(GridPosition, Instance);

		FVector NewInstanceLocation = FVector(
			GridPosition.X * GridSpacing,
			GridPosition.Y * GridSpacing,
			InstanceLocation.Z
		);

		Instance->SetActorLocation(NewInstanceLocation);
		if(Instance->IsA<AARGroundActor>())
		{
			AARGroundActor* GroundActor = Cast<AARGroundActor>(Instance);
			CurrentGroundActors.Add(GroundActor);
		}
		
		
		 AlreadySpawnedActors.Add(Instance);
	}
}
//Simple spawn function for the tracked AR planes
AARPlaneActor* AHelloARManager::SpawnPlaneActor()
{
	
	const FActorSpawnParameters SpawnInfo;
	const FRotator MyRot(0, 0, 0);
	const FVector MyLoc(0, 0, 0);

	AARPlaneActor* CustomPlane = GetWorld()->SpawnActor<AARPlaneActor>(MyLoc, MyRot, SpawnInfo);
	
	return CustomPlane;
}

//Gets the colour to set the plane to when its spawned
FColor AHelloARManager::GetPlaneColor(int Index)
{
	return PlaneColors[Index % PlaneColors.Num()];
}

void AHelloARManager::ResetARCoreSession()
{

	//Get all actors in the level and destroy them as well as emptying the respective arrays
	TArray<AActor*> Planes;
	UGameplayStatics::GetAllActorsOfClass(GetWorld(), AARPlaneActor::StaticClass(), Planes);

	for ( auto& It : Planes)
		It->Destroy();
	
	Planes.Empty();
	PlaneActors.Empty();
}
