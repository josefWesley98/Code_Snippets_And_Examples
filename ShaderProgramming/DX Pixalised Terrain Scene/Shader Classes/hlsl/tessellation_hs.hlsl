// Tessellation Hull Shader
// Prepares control points for tessellation
cbuffer FactorsBuffer : register(b1)
{
    float values1;
    float values2;
    float values3;
    float values4;
};

struct InputType
{
    float3 position : POSITION;
    float4 colour : COLOR;
};


struct ConstantOutputType
{
    float edges[3] : SV_TessFactor;
    float inside : SV_InsideTessFactor;
};

struct OutputType
{
    float3 position : POSITION;
    float4 colour : COLOR;
};

ConstantOutputType PatchConstantFunction(InputPatch<InputType, 3> inputPatch, uint patchId : SV_PrimitiveID)
{
    ConstantOutputType output;
;

    // Set the tessellation factors for the three edges of the triangle.
    output.edges[0] = values1;
    output.edges[1] = values2;
    output.edges[2] = values3;

    // Set the tessellation factor for tessallating inside the triangle.
    output.inside = values4;
   /*
    output.edges[0] = 3;
    output.edges[1] = 3;
    output.edges[2] = 3;

   //  Set the tessellation factor for tessallating inside the triangle.
    output.inside = 3;*/



    return output;
}

[domain("tri")]
[partitioning("integer")]
[outputtopology("triangle_ccw")]
[outputcontrolpoints(3)]
[patchconstantfunc("PatchConstantFunction")]
OutputType main(InputPatch<InputType, 3> patch, uint pointId : SV_OutputControlPointID, uint patchId : SV_PrimitiveID)
{
    OutputType output;


    // Set the position for this control point as the output position.
    output.position = patch[pointId].position;

    // Set the input colour as the output colour.
    output.colour = patch[pointId].colour;

    return output;
}