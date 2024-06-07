using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fuzzy_Logic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;

public class GPUInstancing : MonoBehaviour
{
    private List<Material[]> materials = new List<Material[]>();
    private List<Mesh> meshes = new List<Mesh>();
    private int groupSize = 0;
    private List<int> numInstances = new List<int>();
    private List<List<Transform>> agentRenderPositions = new List<List<Transform>>();
    //[SerializeField] private Texture2DArray[] texArray;
    private Dictionary<(UnitType, PlayableAnimations), (int, int)> animationInfo = new Dictionary<(UnitType, PlayableAnimations), (int, int)>();
    private ComputeBuffer positionBuffer;
    private ComputeBuffer rotationBuffer;
    private ComputeBuffer animationPositionBuffer;
    private ComputeBuffer ArraySizeBuffer;
    private ComputeBuffer argsBuffer;
    private ComputeBuffer argsBuffer1;
    private ComputeBuffer argsBuffer2;
    private List<ComputeBuffer> positionBuffers = new List<ComputeBuffer>();
    private List<ComputeBuffer> rotationBuffers = new List<ComputeBuffer>();
    private List<ComputeBuffer> animationPositionBuffers = new List<ComputeBuffer>();
    private List<ComputeBuffer> ArraySizeBuffers = new List<ComputeBuffer>();
    private List<List<ComputeBuffer>> groupedArgsBuffers = new List<List<ComputeBuffer>>();
    [SerializeField] private PlayableAnimations currentAnim = PlayableAnimations.Idle;
    
    private List<uint[]> args = new List<uint[]>()
    {
        new uint[] {0,0,0,0,0},
        new uint[] {0,0,0,0,0},
        new uint[] {0,0,0,0,0}
    };

    private void TemporarySetupForAnims()
    {
    // Add a read in function to replace this.
        // warrior
        // 32 height Anims
        animationInfo.Add((UnitType.BaseWarrior, PlayableAnimations.HardBodyImpact),        (32, 0));
        animationInfo.Add((UnitType.BaseWarrior, PlayableAnimations.ShieldBlock),           (32, 1));
        animationInfo.Add((UnitType.BaseWarrior, PlayableAnimations.ShieldBracedImpact),    (32, 2));
        animationInfo.Add((UnitType.BaseWarrior, PlayableAnimations.SmallBodyImpact),       (32, 3));
        animationInfo.Add((UnitType.BaseWarrior, PlayableAnimations.Sprint),                (32, 4));
        
        // 64 height Anims 
        animationInfo.Add((UnitType.BaseWarrior, PlayableAnimations.BraceWalkBackwards),    (64, 0));
        animationInfo.Add((UnitType.BaseWarrior, PlayableAnimations.BraceWalkForwards),     (64, 1));
        animationInfo.Add((UnitType.BaseWarrior, PlayableAnimations.Kick),                  (64, 2));
        animationInfo.Add((UnitType.BaseWarrior, PlayableAnimations.OverheadMediumSlash),   (64, 3));
        animationInfo.Add((UnitType.BaseWarrior, PlayableAnimations.QuickSlash),            (64, 4));
        animationInfo.Add((UnitType.BaseWarrior, PlayableAnimations.ReadyIdle),             (64, 5));
        animationInfo.Add((UnitType.BaseWarrior, PlayableAnimations.ShieldBraceStationary), (64, 6));
        animationInfo.Add((UnitType.BaseWarrior, PlayableAnimations.UnderArmMediumSlash),   (64, 7));
        animationInfo.Add((UnitType.BaseWarrior, PlayableAnimations.WalkBackwards),         (64, 8));
        animationInfo.Add((UnitType.BaseWarrior, PlayableAnimations.WalkForwards),          (64, 9));
        //animationInfo.Add(PlayableAnimations.Death,                 (64, 10));
        
        // 128 height anims
        animationInfo.Add((UnitType.BaseWarrior,PlayableAnimations.Death), (128, 0));
    }

    public void Init(List<Material[]> materials, List<Mesh> meshes, List<int> numInstances, int groupSize)
    {
        // initalised used materials, instanced, meshes and buffers for each group to be spawned.
        TemporarySetupForAnims();
        this.materials = materials;
        this.meshes = meshes;
        this.numInstances = numInstances;
        this.groupSize = groupSize;
        
        List<ComputeBuffer> argsBuffers = new List<ComputeBuffer>();
        for (int i = 0; i < this.groupSize; i++)
        {
            for (int j = 0; j < materials[i].Length; j++)
            {
                argsBuffers.Add(new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments));
            }
            groupedArgsBuffers.Add(argsBuffers);
            
            positionBuffers.Add(null);
            rotationBuffers.Add(null);
            animationPositionBuffers.Add(null);
            ArraySizeBuffers.Add(null);
        }
    }
    public void RenderAI(List<int> _numInstances, List<List<Transform>> _agentPositions, List<int> maxListSize, List<UnitType> unitTypes, List<int> SubMeshes)
    {
        Profiler.BeginSample("RenderAI");
        // initates the drawing a mesh instance for each sub mesh within each mesh 
        numInstances = _numInstances;
        agentRenderPositions = _agentPositions;

        int group = 0;
        foreach(var agentPositions in _agentPositions)
        //for (int group = 0; group < groupSize; group++)
        {
             for (int subMeshIndex = 0; subMeshIndex < SubMeshes[group]; subMeshIndex++)
             {
                UpdatePosition(agentPositions, group , subMeshIndex, maxListSize[group], unitTypes[group]);
                UpdateArgs(subMeshIndex, group);
                Graphics.DrawMeshInstancedIndirect(meshes[group], subMeshIndex , materials[group][subMeshIndex], new Bounds(Vector3.zero, new Vector3(100, 100 ,100)), groupedArgsBuffers[group][subMeshIndex]);
             }
             group++;
        }
        Profiler.EndSample();
    }
    private void UpdatePosition(List<Transform> _agentPositions, int group, int subMeshIndex, int maxListSize,  UnitType unitType)
    {
       
        if (positionBuffers[group] == null || positionBuffers[group].count != numInstances[group] )
        {
            if (positionBuffers[group] != null)
            {
                positionBuffers[group].Release();
            }
            positionBuffers[group] = new ComputeBuffer(numInstances[group], 12, ComputeBufferType.Structured);
        }
        
        if (animationPositionBuffers[group] == null || animationPositionBuffers[group].count != numInstances[group])
        {
            if (animationPositionBuffers[group] != null)
            {
                animationPositionBuffers[group].Release();
            }
            animationPositionBuffers[group] = new ComputeBuffer(numInstances[group], 4, ComputeBufferType.Structured);
        }
        
        if (rotationBuffers[group] == null || rotationBuffers[group].count != numInstances[group])
        {
            if (rotationBuffers[group] != null)
            {
                rotationBuffers[group].Release();
            }
            rotationBuffers[group] = new ComputeBuffer(numInstances[group], 16, ComputeBufferType.Structured);
        }

        if (ArraySizeBuffers[group] == null || ArraySizeBuffers[group].count != numInstances[group])
        {
            if (ArraySizeBuffers[group] != null)
            {
                ArraySizeBuffers[group].Release();
            }
            ArraySizeBuffers[group] = new ComputeBuffer(numInstances[group], 4, ComputeBufferType.Structured);
        }
        
        // setup containers to pass into bufferes and the information for said contrainers.
        
        List<Vector3> agentPositions = new List<Vector3>(maxListSize);
        List<Vector4> agentRotations = new List<Vector4>(maxListSize);
        List<int> AnimPos = new List<int>(maxListSize);
        List<int> ArrayChoice = new List<int>(maxListSize);
       
        foreach (Transform agent in _agentPositions)
        {
            if (agent)
            {
                if (agent.TryGetComponent(out AgentAnimationManager animationMgr))
                {
                    PlayableAnimations agentAnim = animationMgr.GetCurrentAnimation();
                    agentPositions.Add(agent.position);
                    Quaternion rotation = agent.rotation;
                    if (unitType == UnitType.OniSamurai)
                    {
                        
                    }
                    agentRotations.Add(new Vector4(rotation.x, rotation.y , rotation.z, rotation.w));
                    
                    if (animationInfo.ContainsKey((unitType, agentAnim)))
                    {
                        int choice = animationInfo[(unitType, agentAnim)].Item1;
                        int pos = animationInfo[(unitType, agentAnim)].Item2; 
                        AnimPos.Add(pos);
                        ArrayChoice.Add(choice);
                    }
                }
            }
            
        }
        // set the data into the bufferes 
        positionBuffers[group].SetData(agentPositions);
        rotationBuffers[group].SetData(agentRotations);
        ArraySizeBuffers[group].SetData(ArrayChoice);
        animationPositionBuffers[group].SetData(AnimPos);
        
        // set the buffers for the shader.
        materials[group][subMeshIndex].SetBuffer("_Positions",  positionBuffers[group]);
        materials[group][subMeshIndex].SetBuffer("_Rotations",  rotationBuffers[group]);
        materials[group][subMeshIndex].SetBuffer("_Index", ArraySizeBuffers[group]);
        materials[group][subMeshIndex].SetBuffer("_WhichTextureArray", animationPositionBuffers[group]);
    }
    private void UpdateArgs(int subMeshIndex, int group)
    {
        if (groupedArgsBuffers[group][subMeshIndex] == null)
        {
            Debug.LogError("Argument buffer is null.");
            return;
        }
    
        uint[] argsData = new uint[5];
        argsData[0] = (uint)meshes[group].GetIndexCount(subMeshIndex);
        argsData[1] = (uint)numInstances[group];
        argsData[2] = (uint)meshes[group].GetIndexStart(subMeshIndex);
        argsData[3] = (uint)meshes[group].GetBaseVertex(subMeshIndex);
        argsData[4] = (uint)0;
        groupedArgsBuffers[group][subMeshIndex].SetData(argsData);
    }
    void OnDestroy()
    {
        ReleaseArgsBuffer(groupedArgsBuffers);
        ReleaseComputeBuffers(positionBuffers);
        ReleaseComputeBuffers(rotationBuffers);
        ReleaseComputeBuffers(animationPositionBuffers);
        ReleaseComputeBuffers(ArraySizeBuffers);
    }
    private void ReleaseArgsBuffer(List<List<ComputeBuffer>> argsBuffer)
    {
        if (argsBuffer.Count > 0)
        {
            foreach (var t in argsBuffer)
            {
                if (t.Count > 0)
                {
                    for(int j = 0; j < t.Count; j++)
                        if (t[j] != null)
                        {
                            t[j].Release();
                            t[j] = null;
                        }
                }
            }
            argsBuffer.Clear();
        }
    }
    private void ReleaseComputeBuffers(List<ComputeBuffer> buffers)
    {
        if (buffers != null && buffers.Count > 0)
        {
            foreach (var buffer in buffers)
            {
                if (buffer != null)
                {
                    buffer.Release();
                }
            }
            buffers.Clear();
        }
    }
    // setup some remove, and add functions here to dynamically add and remove groups from the rendering pool.
}