Shader "Hidden/TexMerge"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RenderTex ("RenderTex", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _RenderTex;

            fixed4 ColorBlend(fixed4 bgCol, fixed4 adCol)
            {
                fixed4 result = fixed4(1, 1, 1, 1);
                result.a = 1 - (1 - adCol.a) * (1 - bgCol.a);
                result.rgb = adCol.rgb * adCol.a / result.a + bgCol.rgb * bgCol.a * (1 - adCol.a) / result.a;

                return result;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 col2 = tex2D(_RenderTex, i.uv);
                // just invert the colors

                return ColorBlend(col, col2);
                /*
                if (col2.a > 0.1)
                { 
                    return col2;
                }
                return col;
                */
            }
            ENDCG
        }
    }
}
