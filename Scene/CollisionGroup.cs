using System.Collections.Generic;
using UnityEngine;

namespace DoublePreciseCoords
{
    public class CollisionGroup
    {
        // FIELDS
        public List<DPCObject> Bodies = new List<DPCObject>();

        public Vector64 Start;
        public Vector64 End;

        // PROPERTIES
        public Vector64 Size
        {
            get => End - Start;
        }

        private bool _cntr_cached;
        private Vector64 _cntr;
        public Vector64 Center
        {
            get
            {
                if (!_cntr_cached)
                {
                    _cntr = (Start + End) / 2;
                    _cntr_cached = true;
                }
                return _cntr;
            }
        }

        // CONSTRUCTORS
        public CollisionGroup()
        {
            Start = Vector64.zero;
            End = Vector64.zero;

            _cntr_cached = false;
            _cntr = Vector64.zero;
        }

        public CollisionGroup(DPCObject body)
        {
            Bodies = new List<DPCObject> { body };

            Start = body.Position - Vector3.one * body.BoundingRadius;
            End = body.Position + Vector3.one * body.BoundingRadius;

            _cntr_cached = true;
            _cntr = (Start + End) / 2;
        }

        public void RecomputeSize()
        {
            if (Bodies.Count < 1) return;

            Start = Start = Bodies[0].Position - Vector3.one * Bodies[0].BoundingRadius;
            End = Bodies[0].Position + Vector3.one * Bodies[0].BoundingRadius;

            if (Bodies.Count > 1)
            {
                RecomputeAxis(0);
            }
        }

        private void RecomputeAxis(int axis)
        {
            if (axis < 0 || axis > 2) return;

            foreach (var body in Bodies)
            {
                double min = body.Position[axis] - body.BoundingRadius;
                double max = body.Position[axis] + body.BoundingRadius;

                Start[axis] = Start[axis] < min ? Start[axis] : min;
                End[axis] = End[axis] > max ? End[axis] : max;
            }

            RecomputeAxis(axis + 1);
        }


    }
}
