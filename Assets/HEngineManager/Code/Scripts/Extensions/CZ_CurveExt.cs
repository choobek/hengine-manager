using HoudiniEngineUnity;
using System.Collections;
using UnityEngine;

namespace CZ.HEngineTools
{
    public static class CZ_CurveExt
    { 
        public static void ProjectToZero(HEU_Curve curve)
        {
            bool bRequiresUpload = false;

            int numPoints = curve.CurveNodeData.Count;
            for (int i = 0; i < numPoints; ++i)
            {
                curve.CurveNodeData[i].position.y = 0.0f;
                bRequiresUpload = true;
            }

            if (bRequiresUpload)
            {
                HEU_ParameterData paramData = curve.Parameters.GetParameter(HEU_Defines.CURVE_COORDS_PARAM);
                if (paramData != null)
                {
                    paramData._stringValues[0] = HEU_Curve.GetPointsString(curve.CurveNodeData);
                }

                curve.SetEditState(HEU_Curve.CurveEditState.REQUIRES_GENERATION);
            }
        }
    }
}