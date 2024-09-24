using System.Collections;
using System.Diagnostics;
using System.Numerics;
using Arch.CommandBuffer;
using Arch.Core;
using Arch.Core.Extensions;
using Zinc.Core;
using Zinc.Core.ImGUI;
using Zinc.Internal.Sokol;

namespace Zinc;

public class DebugOverlaySystem : DSystem, IUpdateSystem
{
    QueryDescription colliderDebug = new QueryDescription().WithAll<ActiveState,Collider,Position,EntityID,AdditionalEntityInfo>();
    float colliderTick = 0.1f;
    float acc = 0f;
    public void Update(double dt)
    {
        acc += (float)dt;
        Engine.ECSWorld.Query(in colliderDebug,
            (Arch.Core.Entity e, ref Position p, ref ActiveState a, ref Collider c, ref EntityID o, ref AdditionalEntityInfo info) =>
            {
                if(!a.Active){return;}
                var activeEntity = Engine.GetEntity(o.ID);
                if(activeEntity == Engine.Cursor){return;}
                var bounds = Utils.GetBounds(o.ID,c);
                float minX = Single.MaxValue, minY = Single.MaxValue, maxX = 0, maxY = 0;

                if(acc > colliderTick)
                {
                    foreach (var b in bounds)
                    {
                        new TemporaryShape(colliderTick,4,4,Palettes.ENDESGA[8]){X = b.X,Y = b.Y, RenderOrder = -1};
                        // minX = b.X < minX ? b.X : minX;
                        // minY = b.Y < minY ? b.Y : minY;
                        
                        // maxX = b.X > maxX ? b.X : maxX;
                        // maxY = b.Y > maxY ? b.Y : maxY;
                    }
                }

                if(Engine.GetEntity(o.ID) is Anchor anchor)
                {
                    //cute scaling but kind of bad without state
                    // ImGUIHelper.Wrappers.SetNextWindowPosition(new(minX, minY));
                    // ImGUIHelper.Wrappers.SetNextWindowSize(maxX-minX, maxY-minY);
                    var winPos = anchor.GetWorldPosition();
                    ImGUIHelper.Wrappers.SetNextWindowPosition(new Vector2(winPos.X,winPos.Y));
                    ImGUIHelper.Wrappers.SetNextWindowSize(100,100);
                    ImGUIHelper.Wrappers.SetNextWindowBGAlpha(0f);
                    ImGUIHelper.Wrappers.Begin($"e{e.Id}", 
                        ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar | 
                        ImGuiWindowFlags_.ImGuiWindowFlags_NoMouseInputs |
                        ImGuiWindowFlags_.ImGuiWindowFlags_NoMove |
                        ImGuiWindowFlags_.ImGuiWindowFlags_NoResize |
                        ImGuiWindowFlags_.ImGuiWindowFlags_NoBringToFrontOnFocus);
                    ImGUIHelper.Wrappers.Text($"{e.Id}\n{winPos.X},{winPos.Y}\n{info.DebugText}");
                    if (Engine.drawDebugColliders)
                    {
                        ImGUIHelper.Wrappers.DrawQuad(bounds);
                    }
                    ImGUIHelper.Wrappers.DrawQuad([new Vector2(0,0),new Vector2(16,0),new Vector2(16,16),new Vector2(0,16)]);
                    ImGUIHelper.Wrappers.End();

                }
            });

            if(acc > colliderTick)
            {
                acc = 0;
            }
    }
}