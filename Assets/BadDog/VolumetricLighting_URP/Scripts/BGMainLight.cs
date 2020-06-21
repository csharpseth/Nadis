﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BadDog_URP
{
    [ExecuteInEditMode]
    public class BGMainLight : MonoBehaviour
    {
        [Header("Lighting Params")]

        [Range(0.001f, 0.9999f)]
        public float depthThreshold = 0.995f;
        [Range(0.001f, 1.5f)]
        public float lightingRadius = 1.0f;
        [Range(0.1f, 1)]
        public float lightingSampleWeight = 1.0f;
        [Range(0.1f, 1)]
        public float lightingDecay = 1.0f;
        [Range(0.5f, 20)]
        public float lightingIntensity = 1.0f;
        public Color lightingColor = Color.white;


        private Vector3 m_ViewPosition;
        public static List<BGMainLight> m_MainLightList = new List<BGMainLight>();
        private Material m_Material;


        private void OnEnable()
        {
            if (!m_MainLightList.Contains(this))
            {
                m_MainLightList.Add(this);
            }

            m_Material = null;
        }

        private void OnDisable()
        {
            if(m_MainLightList.Contains(this))
            {
                m_MainLightList.Remove(this);
            }

            if(m_Material != null)
            {
                DestroyImmediate(m_Material);
                m_Material = null;
            }
        }

        public bool UpdateViewPosition(Camera camera)
        {
            m_ViewPosition = camera.WorldToViewportPoint(camera.transform.position - transform.forward);

            if (m_ViewPosition.x < -1 || m_ViewPosition.x > 2 || m_ViewPosition.y < -1 || m_ViewPosition.y > 2)
            {
                return false;
            }

            if (m_ViewPosition.z <= 0)
            {
                return false;
            }

            return true;
        }

        public Material UpdateMaterial(BGVolumetricLightingSetting settings)
        {
            if(m_Material == null)
            {
                m_Material = Instantiate(settings.material);
            }

            m_Material.SetVector(BGShaderIDs._MainLightViewPosition, GetViewPosition());
            m_Material.SetFloat(BGShaderIDs._DepthThreshold, depthThreshold);
            m_Material.SetFloat(BGShaderIDs._LightingRadius, lightingRadius);
            m_Material.SetFloat(BGShaderIDs._LightingSampleWeight, lightingSampleWeight);
            m_Material.SetFloat(BGShaderIDs._LightingDecay, lightingDecay);
            m_Material.SetFloat(BGShaderIDs._LightingIntensity, lightingIntensity);
            m_Material.SetColor(BGShaderIDs._LightingColor, lightingColor);
            m_Material.SetFloat(BGShaderIDs._SampleNum, settings.sampleNum);
            m_Material.SetFloat(BGShaderIDs._SampleDensity, settings.sampleDensity);

            return m_Material;
        }

        public Vector3 GetViewPosition()
        {
            return m_ViewPosition;
        }

        public static List<BGMainLight> GetAllLights()
        {
            return m_MainLightList;
        }
    }
}
