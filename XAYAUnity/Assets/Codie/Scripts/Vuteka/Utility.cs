using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Vuteka
{
    public class Utility
    { 
        public static bool ParseLines(ref JObject jsonObj, ref Line[] lines)
        {
            JArray objArray = (JArray)jsonObj["lines"];

            if (objArray == null)
                return false;

            Line[] tempLines = new Line[objArray.Count];
            tempLines = objArray.ToObject<Line[]>();

            if (tempLines == null)
                return false;

            for (int l = 0; l < tempLines.Length; l++)
            {
                if (tempLines[l].linePoints.Length < 2)
                    return false;

                for (int p = 0; p < tempLines[l].linePoints.Length; p++)
                {
                    if (tempLines[l].linePoints[p].Length != 3)
                        return false;
                }
            }

            lines = tempLines;
            return true;
        }

        public static Vector3[] GetVectorPointsFromLine(Line line)
        {
            Vector3[] points = new Vector3[line.linePoints.Length];

            for (int p = 0; p < points.Length; p++)
            {
                points[p] = new Vector3(line.linePoints[p][0], line.linePoints[p][1], line.linePoints[p][2]);
            }

            return points;
        }

        public static void DebugAllElementsInArray<ArrayType>(ArrayType[] array)
        {
            foreach (var a in array)
            {
                Debug.Log(a);
            }
        }

        public static bool IsValidJSON(string s)
        {
            try
            {
                JToken.Parse(s);
                return true;
            }
            catch (JsonReaderException ex)
            {
                return false;
            }
        }

        public static Structure GetStructureFromVectors(Vector3[][] structureLines)
        {
            return new Structure(GetLinesFromVectors(structureLines));
        }

        public static Line[] GetLinesFromVectors(Vector3[][] vectorPoints)
        {
            Line[] lines = new Line[vectorPoints.Length];

            for (int l = 0; l < lines.Length; l++)
            {
                lines[l] = GetLineFromVectorPoints(vectorPoints[l]);
            }

            return lines;
        }

        public static Line GetLineFromVectorPoints(Vector3[] vectorPoints)
        {
            Line line = new Line();
            line.linePoints = new float[vectorPoints.Length][];

            for (int p = 0; p < vectorPoints.Length; p++)
            {
                line.linePoints[p] = new float[] { vectorPoints[p].x, vectorPoints[p].y, vectorPoints[p].z };
            }

            return line;
        }
    }

}
