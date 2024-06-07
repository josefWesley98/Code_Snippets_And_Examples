using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public struct VertInfo
{
    public Vector3 position;
    public Vector3 normal;
}
public class AnimBaker : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] private ComputeShader infoTextureGen;
    [SerializeField] private Mesh mainMesh;
    [SerializeField] private float animFPS = 60;
    private List<Texture2D> positionsList = new List<Texture2D>();
    private List<Mesh> meshList = new List<Mesh>();
    
    void Start()
    {
        for (int i = 0; i < skinnedMeshRenderer.sharedMesh.subMeshCount; i++)
        {
            meshList.Add(new Mesh());
            positionsList.Add(new Texture2D(0, 0));
        }
        StartCoroutine(BakeAnimation());
    }

    private IEnumerator BakeAnimation()
    {
        var clips = animator.runtimeAnimatorController.animationClips;
  
        int vertsCount = mainMesh.vertexCount;
       
        Mesh mesh = new Mesh();
        animator.speed = 0;
        var textWidth = Mathf.NextPowerOfTwo(vertsCount);
        foreach (var clip in clips)
        {
            string folderName = "";
            var frames = Mathf.NextPowerOfTwo((int)(clip.length * animFPS));
            var info = new List<VertInfo>();

            var pRt = new RenderTexture(textWidth, frames, 0, RenderTextureFormat.ARGBHalf);
            var nRt = new RenderTexture(textWidth, frames, 0, RenderTextureFormat.ARGBHalf);
            
            pRt.name = string.Format("{0}.{1}.posText", name, clip.name);
            nRt.name = string.Format("{0}.{1}.normText", name, clip.name);
            folderName += clip.name + "-";
            foreach (var rt in new[] { pRt, nRt })
            {
                rt.enableRandomWrite = true;
                rt.Create();
                RenderTexture.active = rt;
                GL.Clear(true, true, Color.clear);
            }
            animator.Play(clip.name);
            yield return 0;
            for (var i = 0; i < frames; i++)
            {
                animator.Play(clip.name, 0 , (float)i / frames);
                yield return 0;
                skinnedMeshRenderer.BakeMesh(mesh);
                info.AddRange(System.Linq.Enumerable.Range(0, vertsCount).Select(idx => new VertInfo()
                {
                    position = mesh.vertices[idx],
                    normal = mesh.normals[idx]
                }));
            }
            var buffer = new ComputeBuffer(info.Count, System.Runtime.InteropServices.Marshal.SizeOf<VertInfo>());
            buffer.SetData(info);
            var kernel = infoTextureGen.FindKernel("CSMain");
            uint x, y, z;
            infoTextureGen.GetKernelThreadGroupSizes(kernel, out x,out y,out z);
            
            infoTextureGen.SetInt("VertCount", vertsCount);
            infoTextureGen.SetBuffer(kernel, "meshInfo", buffer);
            infoTextureGen.SetTexture(kernel, "OutPosition", pRt);
            infoTextureGen.SetTexture(kernel, "OutNormal", nRt);
            
            infoTextureGen.Dispatch(kernel, vertsCount / (int)x +1, frames / (int) y + 1, (int) z);
            buffer.Release();

            var posTex = Convert(pRt);
            var normTex = Convert(nRt);
            
            Graphics.CopyTexture(pRt, posTex);
            Graphics.CopyTexture(nRt, normTex);
            folderName += "folder-" + animFPS + "fps";
     
            AssetDatabase.CreateFolder("Assets/AnimsBaked", folderName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            string posTexAssetPath = System.IO.Path.Combine("Assets/AnimsBaked/" + folderName + "/", pRt.name + ".asset");
            string normTexAssetPath = System.IO.Path.Combine("Assets/AnimsBaked/" + folderName +  "/", nRt.name + ".asset");
            
            AssetDatabase.CreateAsset(posTex, posTexAssetPath);
            AssetDatabase.CreateAsset(normTex, normTexAssetPath);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();      
        }
        yield return null;
    }
    
    private Texture2D Convert(RenderTexture rt)
    {
        var texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBAHalf, false);

        RenderTexture.active = rt;
        texture.ReadPixels(Rect.MinMaxRect(0,0,rt.width, rt.height), 0, 0);
        RenderTexture.active = null;
        return texture;
    }
}
