/*************************************************************************
 *  Copyright (C), 2017-2018, Mogoson tech. Co., Ltd.
 *  FileName: MeshCombiner.cs
 *  Author: Mogoson   Version: 1.0   Date: 8/31/2017
 *  Version Description:
 *    Internal develop version,mainly to achieve its function.
 *  File Description:
 *    Ignore.
 *  Class List:
 *    <ID>           <name>             <description>
 *     1.         MeshCombiner             Ignore.
 *  Function List:
 *    <class ID>     <name>             <description>
 *     1.
 *  History:
 *    <ID>    <author>      <time>      <version>      <description>
 *     1.     Mogoson     8/31/2017       1.0        Build this file.
 *************************************************************************/

namespace Developer.MeshCombiner
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class MeshCombiner : ScriptableWizard
    {
        #region Property and Field
        [Tooltip("Root gameObject of meshes.")]
        public GameObject meshesRoot;

        [Tooltip("Target gameObject to save new combine meshe.")]
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
            newMesh.Optimize();

            meshSave.AddComponent<MeshFilter>().sharedMesh = newMesh;
            meshSave.AddComponent<MeshCollider>().sharedMesh = newMesh;
            meshSave.AddComponent<MeshRenderer>().sharedMaterials = materialList.ToArray();

            AssetDatabase.CreateAsset(newMesh, newMeshPath);
            AssetDatabase.Refresh();
        }
        #endregion
    }
}