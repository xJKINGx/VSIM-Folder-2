Shader "PointRendering"
{
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR0;
            };

            StructuredBuffer<int> _Triangles;
            StructuredBuffer<float3> _Positions;
            StructuredBuffer<float3> _VertexPositions;
            uniform uint _StartIndex;
            uniform uint _BaseVertexIndex;
            uniform float4x4 _ObjectToWorld;
            uniform float _NumInstances;

            v2f vert(uint vertexID: SV_VertexID, uint instanceID : SV_InstanceID)
            {
                v2f o;
                float3 pos = _VertexPositions[_Triangles[vertexID + _StartIndex] + _BaseVertexIndex] * 1.1f;
                float4 wpos = mul(_ObjectToWorld, float4(pos + _Positions[instanceID], 1.0f));
                o.pos = mul(UNITY_MATRIX_VP, wpos);
                //o.color = wpos / 100.0f;

                float b{};
                float g{};
                if (wpos.y < -40) {b = 1.0f; g = 0.0f;} else {b = 0.0f; g = 1.0f;}

                o.color = float4(0.0f, g, b, 0.0f);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}