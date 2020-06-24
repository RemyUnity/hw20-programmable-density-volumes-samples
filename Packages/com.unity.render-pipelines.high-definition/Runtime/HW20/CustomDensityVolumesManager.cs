using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace UnityEngine.Rendering.HighDefinition
{
    class CustomDensityVolumesManager
    {
        public static ComputeShader testingCS;

        // Called to render the custom density volumes in the volumetric buffer
        public static void ApplyCustomDensityVolumes( HDCamera hdCamera, CommandBuffer cmd, int frameIndex)
        {
#if UNITY_EDITOR
            testingCS = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/CustomDensityVolumeTester.compute");
#endif

            var parameters = HDRenderPipeline.currentPipeline.PrepareVolumetricLightingParameters(hdCamera, frameIndex);

            var kernel = testingCS.FindKernel("CSMain");

            cmd.SetComputeTextureParam(testingCS, kernel, HDShaderIDs._VBufferDensity, HDRenderPipeline.currentPipeline.m_DensityBuffer);
            ConstantBuffer.Push(cmd, parameters.volumetricCB, testingCS, HDShaderIDs._ShaderVariablesVolumetric);

            cmd.DispatchCompute(testingCS, kernel, ((int)parameters.resolution.x + 7) / 8, ((int)parameters.resolution.y + 7) / 8, parameters.viewCount);
        }
    }
}
