using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vuteka
{
    public class Line
    {
        public float[][] linePoints;
    }

    public class Structure
    {
        public Line[] lines;

        public Structure(Line[] _lines)
        {
            lines = _lines;
        }
    }

    public class StructureUndo
    {
        public bool isNew;

        public StructureUndo(bool _isNew)
        {
            isNew = _isNew;
        }

        public StructureUndo(){ }
    }

    public class WorldState
    {
        public Dictionary<string, Structure> structures;
    }

    public class StructureUndoData
    {
        public Dictionary<string, StructureUndo> structures;
    }
}
