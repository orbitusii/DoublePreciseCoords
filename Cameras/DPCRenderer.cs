using System.Collections.Generic;
using UnityEngine;

namespace DoublePreciseCoords.Cameras
{
    public class DPCRenderer : MonoBehaviour
    {
        public bool Visible { get; private set; }
        public List<Renderer> Renderers = new List<Renderer>();

        public void SetVisibility (bool vis)
        {
            if(Visible == vis)
            {
                return;
            }
            
            foreach(Renderer ren in Renderers)
            {
                ren.enabled = vis;
            }

            Visible = vis;
        }
    }
}