﻿using BabylonExport.Entities;
using GLTFExport.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Max2Babylon
{
    partial class BabylonExporter
    {
        /// <summary>
        /// Store the number of nodes with a specific name
        /// </summary>
        Dictionary<string, int> NbNodesByName;

        private GLTFNode ExportAbstractMesh(ref GLTFNode gltfNode, BabylonAbstractMesh babylonAbstractMesh, GLTF gltf, GLTFNode gltfParentNode, BabylonScene babylonScene)
        {
            RaiseMessage("GLTFExporter.AbstractMesh | Export abstract mesh named: " + babylonAbstractMesh.name, 2);

            // Mesh
            var gltfMesh = gltf.MeshesList.Find(_gltfMesh => _gltfMesh.idGroupInstance == babylonAbstractMesh.idGroupInstance);
            if (gltfMesh != null)
            {
                gltfNode.mesh = gltfMesh.index;
                
                // Skin
                if (gltfMesh.idBabylonSkeleton.HasValue)
                {
                    var babylonSkeleton = babylonScene.skeletons[gltfMesh.idBabylonSkeleton.Value];

                    // if this mesh is sharing a skin with another mesh, use the exported skin
                    if (sharedSkinnedMeshesByOriginal.Values.Any(skinSharingMeshes => skinSharingMeshes.Contains(gltfMesh)))
                    {
                        RaiseMessage("GLTFExporter.Skin | Export skin of node '" + gltfNode.name + "' based on previously exported skeleton '" + babylonSkeleton.name + "'", 2);
                        var skeletonExportData = alreadyExportedSkeletons[babylonSkeleton];
                        gltfNode.skin = skeletonExportData.skinIndex;
                    }
                    else
                    {
                        // Export a new skeleton if necessary and a new skin
                        var gltfSkin = ExportSkin(babylonSkeleton, gltf, gltfNode);
                        gltfNode.skin = gltfSkin.index;
                    }
                }
            }

            return gltfNode;
        }

        /// <summary>
        /// Append a suffix to the specified name if a node already has same name
        /// 
        /// This is used for ThreeJS engine because animations are referenced by names.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetUniqueNodeName(string name)
        {
            if (NbNodesByName.ContainsKey(name))
            {
                string nameSuffix = " (" + NbNodesByName[name] + ")";
                NbNodesByName[name]++;
                name += nameSuffix;
            }
            else
            {
                NbNodesByName.Add(name, 1);
            }
            return name;
        }
    }
}
