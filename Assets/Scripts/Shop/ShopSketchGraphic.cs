using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IcyFlowers.Shop
{
    // Small vector decorations: no generated textures, cameras, or extra art packages.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ShopSketchGraphic : MaskableGraphic
    {
        public enum Sketch { Paper, Wood, Hill, Cloud, Grass, Flower, Awning }
        public Sketch Style;
        static readonly Color Ink = new Color32(25, 49, 36, 255);

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r = rectTransform.rect;
            if (r.width <= 0 || r.height <= 0) return;
            if (Style == Sketch.Grass) { Grass(vh, r); return; }
            if (Style == Sketch.Flower) { Flower(vh, r); return; }
            if (Style == Sketch.Awning) { Awning(vh, r); return; }
            var points = new List<Vector2>();
            if (Style == Sketch.Hill)
            {
                points.Add(new Vector2(0, 0));
                for (int i = 0; i <= 24; i++)
                {
                    float x = i / 24f;
                    points.Add(new Vector2(x, Mathf.Sin(x * Mathf.PI) * (.90f + .035f * Mathf.Sin(x * 19))));
                }
                points.Add(new Vector2(1, 0));
            }
            else if (Style == Sketch.Cloud)
            {
                points.AddRange(new[] { new Vector2(.04f,.20f),new Vector2(0,.38f),new Vector2(.03f,.55f),new Vector2(.15f,.62f),new Vector2(.20f,.87f),new Vector2(.34f,.98f),new Vector2(.49f,.91f),new Vector2(.56f,.73f),new Vector2(.70f,.79f),new Vector2(.81f,.65f),new Vector2(.82f,.48f),new Vector2(.95f,.46f),new Vector2(1,.29f),new Vector2(.94f,.14f),new Vector2(.55f,.08f),new Vector2(.25f,.13f) });
            }
            else
            {
                points.AddRange(new[] { new Vector2(.012f,.025f),new Vector2(.27f,.012f),new Vector2(.69f,.021f),new Vector2(.985f,.008f),new Vector2(.974f,.39f),new Vector2(.99f,.73f),new Vector2(.98f,.985f),new Vector2(.67f,.974f),new Vector2(.30f,.99f),new Vector2(.005f,.975f),new Vector2(.015f,.62f),new Vector2(.004f,.29f) });
            }
            for (int i = 0; i < points.Count; i++) points[i] = Point(r, points[i]);
            Polygon(vh, points, color);
            Outline(vh, points, Ink, Style == Sketch.Hill ? 2.2f : 2f);
            if (Style == Sketch.Hill)
            {
                Circle(vh, Point(r,new Vector2(.40f,.38f)), 1.6f, Ink);
                Circle(vh, Point(r,new Vector2(.59f,.39f)), 1.6f, Ink);
                var smile = new List<Vector2>();
                for (int i=0; i<=12; i++)
                {
                    float t=i/12f;
                    smile.Add(Point(r,new Vector2(.45f+t*.10f,.29f-Mathf.Sin(t*Mathf.PI)*.025f)));
                }
                Path(vh,smile,Ink,1.6f);
            }
            if (Style == Sketch.Wood)
            {
                for(int i=1;i<=3;i++)
                {
                    var grain = new List<Vector2>();
                    for(int j=0;j<=12;j++) grain.Add(Point(r,new Vector2(.06f+j*.073f,i*.24f+Mathf.Sin(j*1.7f+i)*.014f)));
                    Path(vh,grain,new Color(.29f,.15f,.08f,.20f),1.3f);
                }
            }
        }

        void Grass(VertexHelper vh, Rect r)
        {
            var earth = new List<Vector2> { Point(r,new Vector2(0,0)), Point(r,new Vector2(1,0)), Point(r,new Vector2(1,.42f)), Point(r,new Vector2(0,.42f)) };
            Polygon(vh,earth,color);
            for (int i=0;i<46;i++)
            {
                float x=i/45f;
                float h=.58f+.36f*(.5f+.5f*Mathf.Sin(i*13.7f));
                var blade=new List<Vector2> { Point(r,new Vector2(x-.018f,.36f)),Point(r,new Vector2(x-.008f,h*.83f)),Point(r,new Vector2(x+.004f,h)),Point(r,new Vector2(x+.014f,h*.81f)),Point(r,new Vector2(x+.025f,.36f)) };
                Polygon(vh,blade,i%3==0?new Color32(16,169,54,255):new Color32(18,153,45,255));
            }
        }
        void Flower(VertexHelper vh, Rect r)
        {
            var c=Point(r,new Vector2(.5f,.64f));
            Line(vh,Point(r,new Vector2(.49f,.02f)),c,Ink,4);
            for(int i=0;i<5;i++)
            {
                float a=i*Mathf.PI*2/5;
                var petal=c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r.width*.25f;
                Circle(vh,petal,r.width*.19f,Ink);
                Circle(vh,petal,r.width*.17f,color);
            }
            Circle(vh,c,r.width*.19f,Ink);
            Circle(vh,c,r.width*.16f,new Color32(255,234,71,255));
            Circle(vh,c+new Vector2(-r.width*.05f,0),1,Ink);
            Circle(vh,c+new Vector2(r.width*.05f,0),1,Ink);
        }
        void Awning(VertexHelper vh,Rect r)
        {
            const int count=8;
            for(int i=0;i<count;i++)
            {
                float x=i/(float)count, next=(i+1)/(float)count;
                var p=new List<Vector2> { Point(r,new Vector2(x,.15f)),Point(r,new Vector2(next,.15f)),Point(r,new Vector2(next-.015f,1)),Point(r,new Vector2(x+.015f,1)) };
                Polygon(vh,p,i%2==0?color:new Color32(255,249,219,255));
                Outline(vh,p,Ink,1.5f);
            }
            Line(vh,Point(r,new Vector2(0,.14f)),Point(r,new Vector2(1,.14f)),Ink,3);
        }
        static Vector2 Point(Rect r,Vector2 p) => new Vector2(r.xMin+p.x*r.width,r.yMin+p.y*r.height);
        static void Polygon(VertexHelper vh,List<Vector2> p,Color tint)
        {
            var center=Vector2.zero;
            foreach(var v in p) center+=v;
            center/=p.Count;
            int start=vh.currentVertCount;
            vh.AddVert(center,tint,Vector2.zero);
            foreach(var v in p) vh.AddVert(v,tint,Vector2.zero);
            for(int i=0;i<p.Count;i++) vh.AddTriangle(start,start+1+i,start+1+(i+1)%p.Count);
        }
        static void Circle(VertexHelper vh,Vector2 c,float radius,Color tint)
        {
            var p=new List<Vector2>();
            for(int i=0;i<28;i++) { float a=i*Mathf.PI*2/28; p.Add(c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius); }
            Polygon(vh,p,tint);
        }
        static void Outline(VertexHelper vh,List<Vector2> p,Color tint,float width)
        { for(int i=0;i<p.Count;i++) Line(vh,p[i],p[(i+1)%p.Count],tint,width); }
        static void Path(VertexHelper vh,List<Vector2> p,Color tint,float width)
        { for(int i=1;i<p.Count;i++) Line(vh,p[i-1],p[i],tint,width); }
        static void Line(VertexHelper vh,Vector2 a,Vector2 b,Color tint,float width)
        {
            Vector2 d=(b-a).normalized;
            Vector2 n=new Vector2(-d.y,d.x)*width*.5f;
            int s=vh.currentVertCount;
            vh.AddVert(a+n,tint,Vector2.zero);vh.AddVert(b+n,tint,Vector2.zero);
            vh.AddVert(b-n,tint,Vector2.zero);vh.AddVert(a-n,tint,Vector2.zero);
            vh.AddTriangle(s,s+1,s+2);vh.AddTriangle(s,s+2,s+3);
        }
    }
}
