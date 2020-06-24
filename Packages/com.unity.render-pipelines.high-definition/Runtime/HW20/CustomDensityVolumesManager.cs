﻿using System;
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

        public static List<CustomDensityVolume> GetVolumesInFrustum(HDCamera hdCamera)
        {
            Vector3 camPosition = hdCamera.camera.transform.position;
            Vector3 camOffset = Vector3.zero;// World-origin-relative

            if (ShaderConfig.s_CameraRelativeRendering != 0)
                camOffset = camPosition; // Camera-relative

            var volumes = GameObject.FindObjectsOfType<CustomDensityVolume>() // Get all the loaded volumes
                .Where( v => // filter the ones that are withing the camera frustum
                {
                    var obb = new OrientedBBox(Matrix4x4.TRS(v.transform.position, v.transform.rotation, v.size));
                    // Handle camera-relative rendering.
                    obb.center -= camOffset;
                    return GeometryUtils.Overlap(obb, hdCamera.frustum, 6, 8);
                    })
                .ToList();

            return volumes;
        }

        // Called to render the custom density volumes in the volumetric buffer
        public static void ApplyCustomDensityVolumes( HDCamera hdCamera, CommandBuffer cmd, int frameIndex)
        {
            var volumes = GetVolumesInFrustum(hdCamera);

            /*
            Debug.Log("Volumes in view : " + volumes.Count);
            foreach (var v in volumes)
                Debug.DrawLine(v.transform.position, v.transform.position + Vector3.up * 0.2f, Color.cyan, 5f);
            // */

#if UNITY_EDITOR
            testingCS = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/CustomDensityVolumeTester.compute");
#endif

            var parameters = HDRenderPipeline.currentPipeline.PrepareVolumetricLightingParameters(hdCamera, frameIndex);

            var kernel = testingCS.FindKernel("CSMain");

            cmd.SetComputeTextureParam(testingCS, kernel, HDShaderIDs._VBufferDensity, HDRenderPipeline.currentPipeline.m_DensityBuffer);
            ConstantBuffer.Push(cmd, parameters.volumetricCB, testingCS, HDShaderIDs._ShaderVariablesVolumetric);

            // cmd.DispatchCompute(testingCS, kernel, ((int)parameters.resolution.x + 7) / 8, ((int)parameters.resolution.y + 7) / 8, parameters.viewCount);

            cmd.SetGlobalVector("VolumeTime", Vector4.one * Time.realtimeSinceStartup);

            foreach( var v in volumes)
            {
                if (v.compute == null) continue;

                kernel = v.compute.FindKernel("CSMain");

                var m = Matrix4x4.TRS(v.transform.position, v.transform.rotation, Vector3.Scale( v.size , v.transform.lossyScale ));

                v.compute.SetMatrix("VolumeMatrix", m);
                v.compute.SetMatrix("InvVolumeMatrix", m.inverse);

                cmd.SetComputeTextureParam(v.compute, kernel, HDShaderIDs._VBufferDensity, HDRenderPipeline.currentPipeline.m_DensityBuffer);
                ConstantBuffer.Push(cmd, parameters.volumetricCB, v.compute, HDShaderIDs._ShaderVariablesVolumetric);
                cmd.DispatchCompute(v.compute, kernel, ((int)parameters.resolution.x + 7) / 8, ((int)parameters.resolution.y + 7) / 8, parameters.viewCount);
            }
        }
    }
}
