/*************************************************************************
 *  Copyright (C), 2017-2018, Mogoson Tech. Co., Ltd.
 *------------------------------------------------------------------------
 *  File         :  MeshCombiner.cs
 *  Description  :  Draw the extend editor window and combine Meshes.
 *------------------------------------------------------------------------
 *  Author       :  Mogoson
 *  Version      :  0.1.0
 *  Date         :  8/31/2017
 *  Description  :  Initial development version.
 *************************************************************************/

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Developer.MeshCombiner
{
    public class MeshCombiner : ScriptableWizard
    {
        #region Property and Field
        [Tooltip("Root gameobject of meshes.")]
        public GameObject meshesRoot;

        [Tooltip("Gameobject to save new combine mesh.")]
        public GameObject meshSave;
        #endregion

        #region Private Method
        [MenuItem("Tool/Mesh Combiner &M")]
        private static void ShowEditor()
        {
            DisplayWizard("Mesh Combiner", typeof(MeshCombiner), "Combine");
        }

        private void OnWizardUpdate()
        {
            if (meshesRoot && meshSave)
                isValid = true;
            else
                isValid = false;
        }

        private void OnWizardCreate()
        {
            var newMeshPath = EditorUtility.SaveFilePanelInProject(
                "Save New Combine Mesh",
                "NewCombineMesh",
                "asset",
                "Enter a file name to save the new combine mesh.");

            if (newMeshPath == string.Empty)
                return;

            var meshFilters = meshesRoot.GetComponentsInChildren<MeshFilter>();
            var combines = new CombineInstance[meshFilters.Length];
            var materialList = new List<Material>();
            for (int i = 0; i < meshFilters.Length; i++)
            {
                combines[i].mesh = meshFilters[i].sharedMesh;
                combines[i].transform = Matrix4x4.TRS(meshFilters[i].transform.position - meshesRoot.transform.position,
                    meshFilters[i].transform.rotation, meshFilters[i].transform.lossyScale);
                var materials = meshFilters[i].GetComponent<MeshRenderer>().sharedMaterials;
                foreach (var material in materials)
                {
                    materialList.Add(material);
                }
            }
            var newMesh = new Mesh();
            newMesh.CombineMeshes(combines, false);

#if !UNITY_5_5_OR_NEWER
            //Mesh.Optimize was removed in version 5.5.2p4.
            newMesh.Optimize();
#endif
            meshSave.AddComponent<MeshFilter>().sharedMesh = newMesh;
            meshSave.AddComponent<MeshCollider>().sharedMesh = newMesh;
            meshSave.AddComponent<MeshRenderer>().sharedMaterials = materialList.ToArray();

            AssetDatabase.CreateAsset(newMesh, newMeshPath);
            AssetDatabase.Refresh();
            Selection.activeObject = newMesh;
        }
        #endregion
    }
}