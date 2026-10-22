using UnityEngine;
namespace Meridian
{
    public static class WaterVisuals
    {
        public static Material Ocean(EnvironmentResources owner)
        {
            Shader shader=Resources.Load<Shader>("MaritimeOcean");
            if(shader==null)throw new System.InvalidOperationException("Missing MaritimeOcean shader resource");
            Material material=owner.Own(new Material(shader){name="Meridian layered ocean"});
            material.SetTexture("_NormalTex",owner.Own(CreateRippleTexture()));return material;
        }
        public static Material Foam(EnvironmentResources owner)
        {
            Shader shader=Resources.Load<Shader>("MaritimeFoam");
            if(shader==null)throw new System.InvalidOperationException("Missing MaritimeFoam shader resource");
            Material material=owner.Own(new Material(shader){name="Meridian soft surface foam"});
            material.mainTexture=owner.Own(CreateFoamTexture());return material;
        }
        public static Texture2D CreateFoamTexture()
        {
            const int size=128;Texture2D texture=new(size,size,TextureFormat.RGBA32,true){name="Organic feathered foam",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear,anisoLevel=2};
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                Vector2 p=new((x+.5f)/size*2-1,(y+.5f)/size*2-1);
                float noise=Mathf.PerlinNoise(x*.047f+17,y*.047f+29);
                float detail=Mathf.PerlinNoise(x*.17f+7,y*.17f+13);
                float envelope=1-Mathf.SmoothStep(.33f,.96f,p.magnitude+(noise-.5f)*.18f);
                float lace=Mathf.Lerp(.18f,.95f,Mathf.SmoothStep(.2f,.72f,noise*.65f+detail*.35f));
                float alpha=envelope*lace;
                // Absolute transparent border prevents square billboard outlines even at tiny mip levels.
                if(x<2||y<2||x>=size-2||y>=size-2)alpha=0;
                texture.SetPixel(x,y,new Color(.87f,.96f,.97f,alpha));
            }
            texture.Apply(true,false);return texture;
        }
        public static Texture2D CreateRippleTexture()
        {
            const int size=128;Texture2D texture=new(size,size,TextureFormat.RGBA32,true){name="Periodic multi-directional ripple slopes",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear,anisoLevel=4};
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float u=x/(float)size,v=y/(float)size,d=1f/size;
                float dx=(RippleHeight(u+d,v)-RippleHeight(u-d,v))*16f;
                float dz=(RippleHeight(u,v+d)-RippleHeight(u,v-d))*16f;
                texture.SetPixel(x,y,new Color(Mathf.Clamp01(dx*.5f+.5f),Mathf.Clamp01(dz*.5f+.5f),RippleHeight(u,v),1));
            }
            texture.Apply(true,false);return texture;
        }
        private static float RippleHeight(float u,float v)=>TileNoise(u,v,7)*.70f+TileNoise(u,v,15)*.24f+TileNoise(u,v,31)*.06f;
        private static float TileNoise(float u,float v,float period)
        {
            u=Mathf.Repeat(u,1);v=Mathf.Repeat(v,1);float x=u*period+17.31f,y=v*period+43.73f;
            float sx=Mathf.SmoothStep(0,1,u),sy=Mathf.SmoothStep(0,1,v);
            return Mathf.Lerp(Mathf.Lerp(Mathf.PerlinNoise(x,y),Mathf.PerlinNoise(x-period,y),sx),Mathf.Lerp(Mathf.PerlinNoise(x,y-period),Mathf.PerlinNoise(x-period,y-period),sx),sy);
        }
    }
}
