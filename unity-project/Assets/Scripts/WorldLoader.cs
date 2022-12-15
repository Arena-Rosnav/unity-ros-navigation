using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

using RosMessageTypes.Unity;
using RosMessageTypes.Geometry;

using Unity.Robotics.UrdfImporter;

public class WorldLoader : MonoBehaviour
{
    ROSConnection ros;
    public string serviceName = "load_world";

    // Start is called before the first frame update
    void Start() {
        Debug.Log(transform.parent);

        ros = ROSConnection.GetOrCreateInstance();
        ros.ImplementService<LoadWorldRequest, LoadWorldResponse>(serviceName, LoadWorld);
    }

    private LoadWorldResponse LoadWorld(LoadWorldRequest request) {
        string worldPath = request.world_path;
        string worldName = request.world_name;

        float[] origin = request.origin;

        GameObject world = Utils.CreateGameObjectFromUrdfFile(
            worldPath,
            worldName
        );

        // TODO CREATE COLLISION MESHES
        CreateRecursiveMeshCollider(world);

        world.transform.position = new Vector3(
            -origin[0],
            origin[2],
            origin[1]
        );

        // Create Ground Plane
        bool shouldCreateGroundPlane = request.create_ground_plane;

        if ( request.create_ground_plane ) {
            PoseMsg groundPlanePose = request.pose;

            float height = request.height;
            float width = request.width;

            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.localScale = new Vector3(height / 10, 1, width / 10);
            plane.transform.position = new Vector3(
                (float) groundPlanePose.position.x - height / 2 - origin[0], 
                (float) groundPlanePose.position.z + origin[2], 
                (float) groundPlanePose.position.y + width / 2 + origin[1]
            );
            plane.GetComponent<Renderer>().material.color = new Color(100, 100, 100, 1);
            plane.transform.parent = null;
        }

        return new LoadWorldResponse(true, "World created successfully.");
    }

    private void CreateRecursiveMeshCollider(GameObject gameObject) {
        int childCount = 0;

        foreach ( Transform child in gameObject.transform ) {
            childCount++;

            CreateRecursiveMeshCollider(child.gameObject);
        }

        if ( childCount == 0 ) {
            gameObject.AddComponent<MeshCollider>();
        }
    }
}
